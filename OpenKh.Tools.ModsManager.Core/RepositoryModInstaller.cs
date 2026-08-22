using LibGit2Sharp;
using System.IO.Compression;
using System.Net.Http.Headers;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class RepositoryModInstaller
{
    private const string MetadataFileName = "mod.yml";
    private static readonly HttpClient HttpClient = CreateHttpClient();
    private readonly ModManagerConfigurationService _configuration;
    private readonly LocalModInstaller _localInstaller;

    public RepositoryModInstaller(
        ModManagerConfigurationService configuration,
        LocalModInstaller localInstaller)
    {
        _configuration = configuration;
        _localInstaller = localInstaller;
    }

    public async Task<ModInstallResult> InstallAsync(
        string source,
        GameInfo game,
        string? branch = null,
        bool overwrite = false,
        IProgress<ModOperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Enter a repository, ZIP URL, or local file.", nameof(source));

        source = source.Trim();
        if (File.Exists(source))
            return await InstallLocalFileAsync(source, game, overwrite, progress, cancellationToken);

        if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && IsArchiveUri(uri))
            return await DownloadArchiveAsync(uri, game, overwrite, progress, cancellationToken);

        var repository = RepositoryAddress.Parse(source, branch);
        return await CloneAsync(repository, game, overwrite, progress, cancellationToken);
    }

    public string? FindInstalledMod(string source, GameInfo game, string? branch = null)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Enter a repository, ZIP URL, or local file.", nameof(source));

        source = source.Trim();
        if (File.Exists(source))
        {
            var extension = Path.GetExtension(source);
            if (extension.Equals(".lua", StringComparison.OrdinalIgnoreCase))
            {
                var id = Path.GetFileNameWithoutExtension(source);
                return Directory.Exists(Path.Combine(_configuration.GetGameModsDirectory(game), id))
                    ? id
                    : null;
            }

            return _localInstaller.FindInstalledMod(source, game);
        }

        if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && IsArchiveUri(uri))
            return null;

        var repository = RepositoryAddress.Parse(source, branch);
        return Directory.Exists(Path.Combine(_configuration.GetGameModsDirectory(game), repository.Id)) ||
               Directory.Exists(Path.Combine(_configuration.CollectionsDirectory, repository.Id))
            ? repository.Id
            : null;
    }

    private async Task<ModInstallResult> InstallLocalFileAsync(
        string fileName,
        GameInfo game,
        bool overwrite,
        IProgress<ModOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        if (extension.Equals(".lua", StringComparison.OrdinalIgnoreCase))
            return await InstallLuaAsync(fileName, game, overwrite, cancellationToken);

        progress?.Report(new ModOperationProgress($"Opening {Path.GetFileName(fileName)}"));
        return await _localInstaller.InstallAsync(fileName, game, overwrite, cancellationToken);
    }

    private async Task<ModInstallResult> DownloadArchiveAsync(
        Uri uri,
        GameInfo game,
        bool overwrite,
        IProgress<ModOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report(new ModOperationProgress("Downloading mod package", 0));
        var temporaryFile = Path.Combine(Path.GetTempPath(), $"openkh-mod-{Guid.NewGuid():N}.zip");
        try
        {
            using var response = await HttpClient.GetAsync(
                uri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var output = File.Create(temporaryFile);
            var buffer = new byte[81920];
            long receivedBytes = 0;
            while (true)
            {
                var count = await input.ReadAsync(buffer, cancellationToken);
                if (count == 0)
                    break;

                await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
                receivedBytes += count;
                progress?.Report(new ModOperationProgress(
                    "Downloading mod package",
                    totalBytes > 0 ? (double)receivedBytes / totalBytes.Value : null));
            }

            var archiveName = GetArchiveName(uri);
            var namedTemporaryFile = Path.Combine(Path.GetDirectoryName(temporaryFile)!, archiveName);
            File.Move(temporaryFile, namedTemporaryFile, true);
            temporaryFile = namedTemporaryFile;
            return await _localInstaller.InstallAsync(temporaryFile, game, overwrite, cancellationToken);
        }
        finally
        {
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }
    }

    private async Task<ModInstallResult> CloneAsync(
        RepositoryAddress repository,
        GameInfo game,
        bool overwrite,
        IProgress<ModOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var destination = Path.Combine(_configuration.GetGameModsDirectory(game), repository.Id);
        var collectionDestination = Path.Combine(_configuration.CollectionsDirectory, repository.Id);
        PrepareDestination(destination, overwrite);
        PrepareDestination(collectionDestination, overwrite);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        try
        {
            progress?.Report(new ModOperationProgress($"Cloning {repository.Id}", 0));
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var options = new CloneOptions
                {
                    BranchName = repository.Branch,
                    RecurseSubmodules = true
                };
                options.FetchOptions.OnProgress = message =>
                {
                    progress?.Report(new ModOperationProgress(message.Trim()));
                    return !cancellationToken.IsCancellationRequested;
                };
                options.FetchOptions.OnTransferProgress = transfer =>
                {
                    var percentage = transfer.TotalObjects == 0
                        ? null
                        : (double?)transfer.ReceivedObjects / transfer.TotalObjects;
                    progress?.Report(new ModOperationProgress("Receiving repository objects", percentage));
                    return !cancellationToken.IsCancellationRequested;
                };

                Repository.Clone(repository.CloneUrl, destination, options);
                cancellationToken.ThrowIfCancellationRequested();
            }, cancellationToken);

            var metadataPath = Path.Combine(destination, MetadataFileName);
            if (!File.Exists(metadataPath))
                throw new InvalidDataException("The repository is not an OpenKH mod because mod.yml is missing.");

            var metadata = ModMetadata.Read(metadataPath);
            var finalDestination = destination;
            if (metadata.IsCollection)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(collectionDestination)!);
                Directory.Move(destination, collectionDestination);
                finalDestination = collectionDestination;
            }

            progress?.Report(new ModOperationProgress($"Installed {metadata.Title ?? repository.Id}", 1));
            return new ModInstallResult(
                repository.Id,
                string.IsNullOrWhiteSpace(metadata.Title) ? repository.Id : metadata.Title,
                finalDestination);
        }
        catch
        {
            DeleteDirectoryIfPresent(destination);
            throw;
        }
    }

    private async Task<ModInstallResult> InstallLuaAsync(
        string fileName,
        GameInfo game,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        var id = Path.GetFileNameWithoutExtension(fileName);
        var destination = Path.Combine(_configuration.GetGameModsDirectory(game), id);
        PrepareDestination(destination, overwrite);
        Directory.CreateDirectory(destination);

        try
        {
            var lines = await File.ReadAllLinesAsync(fileName, cancellationToken);
            var title = ReadLuaHeader(lines, "LUAGUI_NAME") ?? id;
            var author = ReadLuaHeader(lines, "LUAGUI_AUTH") ?? "Unknown author";
            var description = ReadLuaHeader(lines, "LUAGUI_DESC") ??
                "This metadata was generated from a Lua mod.";
            var scriptName = Path.GetFileName(fileName);
            File.Copy(fileName, Path.Combine(destination, scriptName));
            var metadata = $"title: {QuoteYaml(title)}\noriginalAuthor: {QuoteYaml(author)}\n" +
                $"description: {QuoteYaml(description)}\nassets:\n" +
                $"- name: scripts/{scriptName}\n  method: copy\n  source:\n  - name: {scriptName}\n";
            await File.WriteAllTextAsync(Path.Combine(destination, MetadataFileName), metadata, cancellationToken);
            return new ModInstallResult(id, title, destination);
        }
        catch
        {
            DeleteDirectoryIfPresent(destination);
            throw;
        }
    }

    private static string? ReadLuaHeader(IEnumerable<string> lines, string key)
    {
        var prefix = $"{key}=";
        var line = lines.FirstOrDefault(line =>
            line.Replace(" ", string.Empty).StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        if (line is null)
            return null;

        return line[(line.IndexOf('=') + 1)..].Trim().Trim('\'', '"');
    }

    private static string QuoteYaml(string value) =>
        $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ")}\"";

    private static void PrepareDestination(string destination, bool overwrite)
    {
        if (!Directory.Exists(destination))
            return;
        if (!overwrite)
            throw new ModAlreadyInstalledException(Path.GetFileName(destination));
        DeleteDirectoryIfPresent(destination);
    }

    private static void DeleteDirectoryIfPresent(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);
        Directory.Delete(directory, true);
    }

    private static bool IsArchiveUri(Uri uri) =>
        uri.AbsolutePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
        uri.AbsolutePath.Contains("/archive/", StringComparison.OrdinalIgnoreCase) ||
        uri.AbsolutePath.Contains("/releases/download/", StringComparison.OrdinalIgnoreCase);

    private static string GetArchiveName(Uri uri)
    {
        var name = Path.GetFileName(uri.AbsolutePath);
        return name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? name : "DownloadedMod.zip";
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("OpenKH-ModManager", "1.0"));
        return client;
    }

    internal sealed record RepositoryAddress(string Id, string CloneUrl, string? Branch)
    {
        internal static RepositoryAddress Parse(string source, string? branch)
        {
            if (Uri.TryCreate(source, UriKind.Absolute, out var uri))
            {
                var parts = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    throw new ArgumentException("The repository URL must include an owner and repository name.");

                var repositoryName = parts[1].EndsWith(".git", StringComparison.OrdinalIgnoreCase)
                    ? parts[1][..^4]
                    : parts[1];
                var detectedBranch = branch;
                if (parts.Length >= 4 && parts[2].Equals("tree", StringComparison.OrdinalIgnoreCase))
                    detectedBranch ??= string.Join('/', parts.Skip(3));
                var baseUrl = $"{uri.Scheme}://{uri.Authority}/{parts[0]}/{repositoryName}.git";
                return new RepositoryAddress($"{parts[0]}/{repositoryName}", baseUrl, detectedBranch);
            }

            var repositorySource = source.Trim().Trim('/');
            var repositoryHost = "github.com";
            var hostSeparator = repositorySource.LastIndexOf('@');
            if (hostSeparator > 0)
            {
                repositoryHost = repositorySource[(hostSeparator + 1)..].Trim();
                repositorySource = repositorySource[..hostSeparator].TrimEnd('/');
                if (!Uri.TryCreate($"https://{repositoryHost}", UriKind.Absolute, out var hostUri) ||
                    !hostUri.AbsolutePath.Equals("/", StringComparison.Ordinal))
                {
                    throw new ArgumentException("The repository host is not valid.");
                }

                repositoryHost = hostUri.Authority;
            }

            var sourceParts = repositorySource.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (sourceParts.Length < 2)
                throw new ArgumentException("Use owner/repository or paste a full repository URL.");
            var shorthandRepositoryName = sourceParts[1].EndsWith(".git", StringComparison.OrdinalIgnoreCase)
                ? sourceParts[1][..^4]
                : sourceParts[1];
            var id = $"{sourceParts[0]}/{shorthandRepositoryName}";
            var detectedSourceBranch = branch ?? (sourceParts.Length > 2 ? string.Join('/', sourceParts.Skip(2)) : null);
            return new RepositoryAddress(id, $"https://{repositoryHost}/{id}.git", detectedSourceBranch);
        }
    }
}
