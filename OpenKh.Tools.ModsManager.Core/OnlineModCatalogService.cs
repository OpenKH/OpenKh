using System.Net.Http.Headers;
using System.Text.Json;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class OnlineModCatalogService
{
    private const string FeedUrl = "https://raw.githubusercontent.com/OpenKH/mods-manager-feed/main/downloadable-mods.json";
    private const string DenyListUrl = "https://raw.githubusercontent.com/OpenKH/mods-manager-feed/main/deny.txt";
    private static readonly string[] IconNames = ["icon.png", "Icon.png", "ICON.png", "Icon.PNG", "icon.PNG"];
    private static readonly string[] PreviewNames = ["preview.png", "Preview.png", "PREVIEW.png", "Preview.PNG", "preview.PNG"];
    private static readonly HttpClient SharedHttpClient = CreateHttpClient();
    private readonly HttpClient _httpClient;
    private readonly string _cacheFile;
    private readonly string _denyListCacheFile;
    private readonly string _cacheDirectory;

    public OnlineModCatalogService(
        ModManagerConfigurationService configuration,
        HttpClient? httpClient = null,
        string? cacheDirectory = null)
    {
        _httpClient = httpClient ?? SharedHttpClient;
        _cacheDirectory = cacheDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpenKh",
            "downloadable-mods-cache");
        _cacheFile = Path.Combine(_cacheDirectory, "downloadable-mods.json");
        _denyListCacheFile = Path.Combine(_cacheDirectory, "deny.txt");
    }

    public async Task<IReadOnlyList<OnlineModInfo>> LoadAsync(
        GameInfo game,
        IReadOnlyCollection<string> installedIds,
        IProgress<ModOperationProgress>? progress = null,
        IProgress<OnlineModInfo>? itemProgress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report(new ModOperationProgress($"Downloading available mods for {game.DisplayName}"));
        var json = await DownloadFeedAsync(cancellationToken);
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("mods", out var mods) ||
            !mods.TryGetProperty(game.Id, out var gameMods) ||
            gameMods.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var installed = installedIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var denied = await LoadDenyListAsync(cancellationToken);
        var repositories = gameMods.EnumerateArray()
            .Select(element => element.TryGetProperty("repo", out var repo) ? repo.GetString() : null)
            .Where(repo => !string.IsNullOrWhiteSpace(repo) && !installed.Contains(repo) && !denied.Contains(repo))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToArray();

        var completed = 0;
        using var gate = new SemaphoreSlim(6);
        var tasks = repositories.Select(async repository =>
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                var mod = await LoadDetailsAsync(repository, game.Id, cancellationToken);
                itemProgress?.Report(mod);
                return mod;
            }
            finally
            {
                var current = Interlocked.Increment(ref completed);
                progress?.Report(new ModOperationProgress(
                    $"Loading mod details ({current}/{repositories.Length})",
                    repositories.Length == 0 ? 1 : (double)current / repositories.Length));
                gate.Release();
            }
        });

        var result = await Task.WhenAll(tasks);
        return result.OrderBy(mod => mod.Title, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private async Task<string> DownloadFeedAsync(CancellationToken cancellationToken)
    {
        try
        {
            var json = await _httpClient.GetStringAsync(FeedUrl, cancellationToken);
            Directory.CreateDirectory(Path.GetDirectoryName(_cacheFile)!);
            await File.WriteAllTextAsync(_cacheFile, json, cancellationToken);
            return json;
        }
        catch when (File.Exists(_cacheFile))
        {
            return await File.ReadAllTextAsync(_cacheFile, cancellationToken);
        }
    }

    private async Task<HashSet<string>> LoadDenyListAsync(CancellationToken cancellationToken)
    {
        string text;
        try
        {
            text = await _httpClient.GetStringAsync(DenyListUrl, cancellationToken);
            Directory.CreateDirectory(_cacheDirectory);
            await File.WriteAllTextAsync(_denyListCacheFile, text, cancellationToken);
        }
        catch when (File.Exists(_denyListCacheFile))
        {
            text = await File.ReadAllTextAsync(_denyListCacheFile, cancellationToken);
        }
        catch
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        return text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<OnlineModInfo> LoadDetailsAsync(
        string repository,
        string gameId,
        CancellationToken cancellationToken)
    {
        var parts = repository.Split('/', 2);
        var fallbackTitle = parts.Length == 2 ? parts[1] : repository;
        var fallbackAuthor = parts.Length == 2 ? parts[0] : "Unknown author";
        var title = fallbackTitle;
        var author = fallbackAuthor;
        var description = "Metadata could not be loaded. The repository can still be installed.";
        string? metadataBranch = null;
        var cacheDirectory = Path.Combine(_cacheDirectory, SanitizeRepository(repository));
        var metadataCachePath = Path.Combine(cacheDirectory, "mod.yml");
        foreach (var branch in new[] { "main", "master" })
        {
            try
            {
                var metadataUrl = $"https://raw.githubusercontent.com/{repository}/{branch}/mod.yml";
                var yaml = await _httpClient.GetStringAsync(metadataUrl, cancellationToken);
                using var reader = new StringReader(yaml);
                var metadata = ModMetadata.Read(reader);
                title = string.IsNullOrWhiteSpace(metadata.Title) ? fallbackTitle : metadata.Title;
                author = string.IsNullOrWhiteSpace(metadata.OriginalAuthor) ? fallbackAuthor : metadata.OriginalAuthor;
                description = string.IsNullOrWhiteSpace(metadata.Description)
                    ? "No description is available."
                    : metadata.Description;
                Directory.CreateDirectory(cacheDirectory);
                await File.WriteAllTextAsync(metadataCachePath, yaml, cancellationToken);
                metadataBranch = branch;
                break;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
            }
        }

        if (metadataBranch is null && File.Exists(metadataCachePath))
        {
            try
            {
                var metadata = ModMetadata.Read(metadataCachePath);
                title = string.IsNullOrWhiteSpace(metadata.Title) ? fallbackTitle : metadata.Title;
                author = string.IsNullOrWhiteSpace(metadata.OriginalAuthor) ? fallbackAuthor : metadata.OriginalAuthor;
                description = string.IsNullOrWhiteSpace(metadata.Description)
                    ? "No description is available."
                    : metadata.Description;
            }
            catch
            {
            }
        }

        var branches = metadataBranch is null
            ? new[] { "main", "master" }
            : new[] { metadataBranch }.Concat(new[] { "main", "master" })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        var iconPath = await DownloadImageAsync(
            repository,
            branches,
            IconNames,
            Path.Combine(cacheDirectory, "icon.png"),
            cancellationToken);
        var previewPath = await DownloadImageAsync(
            repository,
            branches,
            PreviewNames,
            Path.Combine(cacheDirectory, "preview.png"),
            cancellationToken);

        return new OnlineModInfo(
            repository,
            title,
            author,
            description,
            gameId,
            iconPath,
            previewPath);
    }

    private async Task<string?> DownloadImageAsync(
        string repository,
        IReadOnlyCollection<string> branches,
        IReadOnlyCollection<string> fileNames,
        string cachePath,
        CancellationToken cancellationToken)
    {
        if (File.Exists(cachePath) && DateTime.UtcNow - File.GetLastWriteTimeUtc(cachePath) < TimeSpan.FromDays(1))
            return cachePath;

        var encodedRepository = string.Join('/', repository.Split('/').Select(Uri.EscapeDataString));
        foreach (var branch in branches)
        {
            foreach (var fileName in fileNames)
            {
                try
                {
                    var url = $"https://raw.githubusercontent.com/{encodedRepository}/{branch}/{fileName}";
            using var response = await _httpClient.GetAsync(url, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                        continue;

                    var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                    Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
                    await File.WriteAllBytesAsync(cachePath, imageBytes, cancellationToken);
                    return cachePath;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                }
            }
        }

        return File.Exists(cachePath) ? cachePath : null;
    }

    private static string SanitizeRepository(string repository)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars().ToHashSet();
        return new string(repository.Select(character =>
            character == '/' || character == '\\' || invalidCharacters.Contains(character) ? '_' : character).ToArray());
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("OpenKH-ModManager", "1.0"));
        return client;
    }
}

public sealed record OnlineModInfo(
    string Repository,
    string Title,
    string Author,
    string Description,
    string GameId,
    string? IconPath,
    string? PreviewPath);
