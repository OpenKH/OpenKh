using System.IO.Compression;
using OpenKh.Patcher;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class LocalModInstaller
{
    private const string MetadataFileName = "mod.yml";
    private readonly ModManagerConfigurationService _configuration;

    public LocalModInstaller(InstallationLayout layout)
        : this(new ModManagerConfigurationService(layout))
    {
    }

    public LocalModInstaller(ModManagerConfigurationService configuration)
    {
        _configuration = configuration;
        _configuration.EnsureDirectories();
    }

    public Task<ModInstallResult> InstallAsync(string packagePath, GameInfo game) =>
        InstallAsync(packagePath, game, false, CancellationToken.None);

    public Task<ModInstallResult> InstallAsync(
        string packagePath,
        GameInfo game,
        bool overwrite,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Install(packagePath, game, overwrite, cancellationToken), cancellationToken);

    private ModInstallResult Install(
        string packagePath,
        GameInfo game,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(packagePath))
            throw new FileNotFoundException("The selected mod package does not exist.", packagePath);

        using var archive = ZipFile.OpenRead(packagePath);
        var patchArchive = PatchArchiveInfo.FromFileName(packagePath);
        var packageLayout = patchArchive is null ? GetPackageLayout(archive) : new PackageLayout(string.Empty);
        ModMetadata? metadata = null;
        if (patchArchive is null)
        {
            var metadataEntry = archive.Entries.First(entry =>
                NormalizeEntryName(entry.FullName).Equals(
                    $"{packageLayout.Prefix}{MetadataFileName}",
                    StringComparison.OrdinalIgnoreCase));
            using var metadataReader = new StreamReader(metadataEntry.Open());
            metadata = ModMetadata.Read(metadataReader);
        }

        var packageName = CreateSafeDirectoryName(Path.GetFileNameWithoutExtension(packagePath));
        var destinationRoot = metadata?.IsCollection == true
            ? GetCollectionsDirectory()
            : GetGameModsDirectory(game);
        var destinationDirectory = Path.Combine(destinationRoot, packageName);

        if (Directory.Exists(destinationDirectory))
        {
            if (!overwrite)
            {
                throw new IOException(
                    $"A mod named '{packageName}' is already installed. Enable replacement to install it again.");
            }

            foreach (var file in Directory.EnumerateFiles(destinationDirectory, "*", SearchOption.AllDirectories))
                File.SetAttributes(file, FileAttributes.Normal);
            Directory.Delete(destinationDirectory, true);
        }

        Directory.CreateDirectory(destinationDirectory);
        try
        {
            if (patchArchive is not null)
            {
                ExtractPatchArchive(archive, destinationDirectory, patchArchive, cancellationToken);
            }
            else
            {
                foreach (var entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ExtractEntry(entry, packageLayout.Prefix, destinationDirectory);
                }
            }

            if (!File.Exists(Path.Combine(destinationDirectory, MetadataFileName)))
                throw new InvalidDataException("The package did not extract a mod.yml file.");
        }
        catch
        {
            Directory.Delete(destinationDirectory, true);
            throw;
        }

        return new ModInstallResult(
            packageName,
            string.IsNullOrWhiteSpace(metadata?.Title) ? patchArchive?.DisplayName ?? packageName : metadata.Title,
            destinationDirectory);
    }

    private static void ExtractPatchArchive(
        ZipArchive archive,
        string destinationDirectory,
        PatchArchiveInfo patchArchive,
        CancellationToken cancellationToken)
    {
        var assets = new List<AssetFile>();
        foreach (var entry in archive.Entries.Where(entry => !string.IsNullOrEmpty(entry.Name)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalizedName = NormalizeEntryName(entry.FullName);
            var parts = normalizedName.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                continue;

            var package = parts[0];
            var originalIndex = Array.FindIndex(parts, part => part.Equals("original", StringComparison.OrdinalIgnoreCase));
            var relativeParts = originalIndex >= 0 ? parts.Skip(originalIndex + 1) : parts.Skip(1);
            var relativeName = string.Join('/', relativeParts);
            if (string.IsNullOrWhiteSpace(relativeName))
                continue;

            ExtractFile(entry, relativeName, destinationDirectory, true);
            assets.Add(new AssetFile
            {
                Method = "copy",
                Name = relativeName,
                Package = package,
                Platform = "pc",
                Source = [new AssetFile { Name = relativeName }]
            });
        }

        var metadata = new Metadata
        {
            Title = patchArchive.DisplayName,
            OriginalAuthor = "Unknown",
            Description = $"Metadata generated for an imported {patchArchive.ExtensionName} modification.",
            Game = patchArchive.GameId,
            Assets = assets
        };
        using var metadataStream = File.Create(Path.Combine(destinationDirectory, MetadataFileName));
        metadata.Write(metadataStream);
    }

    private static PackageLayout GetPackageLayout(ZipArchive archive)
    {
        var fileNames = archive.Entries
            .Where(entry => !string.IsNullOrEmpty(entry.Name))
            .Select(entry => NormalizeEntryName(entry.FullName))
            .ToArray();

        if (fileNames.Any(name => name.Equals(MetadataFileName, StringComparison.OrdinalIgnoreCase)))
            return new PackageLayout(string.Empty);

        var metadataEntry = fileNames
            .Where(name => name.EndsWith($"/{MetadataFileName}", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name.Count(character => character == '/'))
            .FirstOrDefault();

        if (metadataEntry is null)
            throw new InvalidDataException("This package is not a valid OpenKH mod because mod.yml is missing.");

        var prefix = metadataEntry[..^MetadataFileName.Length];
        if (fileNames.Any(name => !name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidDataException(
                "The package contains files outside the directory that contains mod.yml.");
        }

        return new PackageLayout(prefix);
    }

    private static void ExtractEntry(ZipArchiveEntry entry, string prefix, string destinationDirectory)
    {
        var entryName = NormalizeEntryName(entry.FullName);
        if (!entryName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return;

        var relativeName = entryName[prefix.Length..];
        if (string.IsNullOrWhiteSpace(relativeName) || string.IsNullOrEmpty(entry.Name))
            return;

        ExtractFile(entry, relativeName, destinationDirectory, false);
    }

    private static void ExtractFile(
        ZipArchiveEntry entry,
        string relativeName,
        string destinationDirectory,
        bool overwrite)
    {
        // Canonicalize both paths before the prefix check so archive entries cannot escape the install directory.
        var destinationRoot = Path.GetFullPath(destinationDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var destinationPath = Path.GetFullPath(Path.Combine(
            destinationRoot,
            relativeName.Replace('/', Path.DirectorySeparatorChar)));
        var pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!destinationPath.StartsWith(destinationRoot, pathComparison))
        {
            throw new InvalidDataException("The package contains a file outside its destination directory.");
        }

        var parentDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(parentDirectory))
            Directory.CreateDirectory(parentDirectory);
        entry.ExtractToFile(destinationPath, overwrite);
    }

    private string GetGameModsDirectory(GameInfo game)
    {
        return _configuration.GetGameModsDirectory(game);
    }

    private string GetCollectionsDirectory() => _configuration.CollectionsDirectory;

    private static string NormalizeEntryName(string name) => name.Replace('\\', '/').TrimStart('/');

    private static string CreateSafeDirectoryName(string name)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars().ToHashSet();
        var safeName = new string(name.Select(character =>
            invalidCharacters.Contains(character) ? '_' : character).ToArray()).Trim();
        return string.IsNullOrEmpty(safeName) ? "InstalledMod" : safeName;
    }

    private sealed record PackageLayout(string Prefix);

    private sealed record PatchArchiveInfo(string GameId, string ExtensionName, string DisplayName)
    {
        public static PatchArchiveInfo? FromFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var name = Path.GetFileNameWithoutExtension(fileName);
            return extension switch
            {
                ".kh2pcpatch" => new("kh2", "KH2PCPATCH", $"{name} (KH2PCPATCH)"),
                ".kh1pcpatch" => new("kh1", "KH1PCPATCH", $"{name} (KH1PCPATCH)"),
                ".compcpatch" => new("Recom", "COMPCPATCH", $"{name} (COMPCPATCH)"),
                ".bbspcpatch" => new("bbs", "BBSPCPATCH", $"{name} (BBSPCPATCH)"),
                ".dddpcpatch" => new("kh3d", "DDDPCPATCH", $"{name} (DDDPCPATCH)"),
                _ => null
            };
        }
    }
}

public sealed record ModInstallResult(string Id, string DisplayName, string Directory);
