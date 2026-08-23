namespace OpenKh.Tools.ModsManager.Core;

using OpenKh.Patcher;
using System.Text.Json;

public sealed class ModCatalogService
{
    private const string MetadataFileName = "mod.yml";
    private readonly ModManagerConfigurationService _configuration;

    public ModCatalogService(InstallationLayout layout)
        : this(new ModManagerConfigurationService(layout))
    {
    }

    public ModCatalogService(ModManagerConfigurationService configuration)
    {
        _configuration = configuration;
        _configuration.EnsureDirectories();
    }

    public string InstallationDirectory => _configuration.InstallationDirectory;
    public GameInfo DefaultGame => GameInfo.FromId(_configuration.Current.LaunchGame);

    public Task<IReadOnlyList<ModEntry>> LoadAsync(GameInfo game) => Task.Run(() => Load(game));

    public void SaveEnabledOrder(GameInfo game, IEnumerable<ModEntry> mods)
    {
        var orderedMods = ApplyFormatPriority(mods).ToArray();
        File.WriteAllLines(
            GetEnabledModsPath(game),
            orderedMods.Where(mod => mod.IsEnabled).Select(mod => mod.Id));
        File.WriteAllLines(
            _configuration.GetModOrderFile(game),
            orderedMods.Select(mod => mod.Id));
    }

    public void MoveInstalledModToHighestPriority(GameInfo game, string modId)
    {
        var mods = Load(game);
        var installedMod = mods.FirstOrDefault(mod =>
            mod.Id.Equals(modId, StringComparison.OrdinalIgnoreCase));
        if (installedMod is null)
            return;

        SaveEnabledOrder(
            game,
            new[] { installedMod }.Concat(mods.Where(mod => !ReferenceEquals(mod, installedMod))));
    }

    private IReadOnlyList<ModEntry> Load(GameInfo game)
    {
        var enabledIds = ReadEnabledIds(game);
        var enabledLookup = enabledIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var savedOrder = ReadOrderIds(game);
        var collectionSettings = ReadCollectionSettings(game);
        var locations = EnumerateModLocations(game)
            .GroupBy(location => location.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToDictionary(location => location.Id, StringComparer.OrdinalIgnoreCase);

        var savedOrderLookup = savedOrder.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var orderedIds = (savedOrder.Count > 0 ? savedOrder : enabledIds)
            .Where(locations.ContainsKey)
            .Concat(locations.Keys
                .Where(id => !savedOrderLookup.Contains(id) &&
                    (savedOrder.Count > 0 || !enabledLookup.Contains(id)))
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase));

        var mods = orderedIds
            .Select(id => CreateEntry(
                locations[id],
                enabledLookup.Contains(id),
                game,
                collectionSettings))
            .ToArray();

        return ApplyFormatPriority(mods).ToArray();
    }

    private IEnumerable<ModLocation> EnumerateModLocations(GameInfo game)
    {
        foreach (var location in EnumerateDirectory(GetGameModsDirectory(game)))
            yield return location;

        foreach (var location in EnumerateDirectory(GetCollectionsDirectory()))
            yield return location;
    }

    private static IEnumerable<ModLocation> EnumerateDirectory(string rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            yield break;

        foreach (var firstLevelDirectory in Directory.EnumerateDirectories(rootDirectory))
        {
            var firstLevelName = Path.GetFileName(firstLevelDirectory);
            if (File.Exists(Path.Combine(firstLevelDirectory, MetadataFileName)))
                yield return new ModLocation(firstLevelName, firstLevelDirectory);

            foreach (var secondLevelDirectory in Directory.EnumerateDirectories(firstLevelDirectory))
            {
                if (!File.Exists(Path.Combine(secondLevelDirectory, MetadataFileName)))
                    continue;

                yield return new ModLocation(
                    $"{firstLevelName}/{Path.GetFileName(secondLevelDirectory)}",
                    secondLevelDirectory);
            }
        }
    }

    private static ModEntry CreateEntry(
        ModLocation location,
        bool isEnabled,
        GameInfo game,
        IReadOnlyDictionary<string, Dictionary<string, bool>> collectionSettings)
    {
        ModMetadata? metadata = null;
        try
        {
            metadata = ModMetadata.Read(Path.Combine(location.Directory, MetadataFileName));
        }
        catch
        {
            // An invalid metadata file should not hide an installed mod from the user.
        }

        var idParts = location.Id.Split('/', 2);
        var fallbackName = idParts.Length == 2 ? idParts[1] : idParts[0];
        var fallbackAuthor = idParts.Length == 2 ? idParts[0] : "Local mod";
        var iconPath = Path.Combine(location.Directory, "icon.png");
        var previewPath = Path.Combine(location.Directory, "preview.png");
        var (sourceUrl, reportBugUrl) = GetRepositoryLinks(location.Directory);

        return new ModEntry
        {
            Id = location.Id,
            Name = string.IsNullOrWhiteSpace(metadata?.Title) ? fallbackName : metadata.Title,
            Author = string.IsNullOrWhiteSpace(metadata?.OriginalAuthor) ? fallbackAuthor : metadata.OriginalAuthor,
            Description = string.IsNullOrWhiteSpace(metadata?.Description)
                ? "No description is available for this mod."
                : metadata.Description,
            Directory = location.Directory,
            IconPath = File.Exists(iconPath) ? iconPath : null,
            PreviewPath = File.Exists(previewPath) ? previewPath : null,
            SourceUrl = sourceUrl,
            ReportBugUrl = reportBugUrl,
            FilesToPatch = GetFilesToPatch(metadata, location.Id, game, collectionSettings),
            IsCollection = metadata?.IsCollection == true,
            IsPcPatch = LegacyModFormat.IsPcPatch(location.Directory, metadata),
            IsEnabled = isEnabled
        };
    }

    private static IEnumerable<ModEntry> ApplyFormatPriority(IEnumerable<ModEntry> mods)
    {
        var orderedMods = mods.ToArray();

        // PC Patch packages replace complete files, so OpenKH mods must always take priority over them.
        return orderedMods
            .Where(mod => !mod.IsPcPatch)
            .Concat(orderedMods.Where(mod => mod.IsPcPatch));
    }

    private static IReadOnlyList<string> GetFilesToPatch(
        ModMetadata? metadata,
        string modId,
        GameInfo game,
        IReadOnlyDictionary<string, Dictionary<string, bool>> collectionSettings)
    {
        collectionSettings.TryGetValue(modId, out var enabledOptionalAssets);
        var files = new List<string>();
        foreach (var asset in metadata?.Assets ?? [])
        {
            if (metadata?.IsCollection == true &&
                !string.IsNullOrWhiteSpace(asset.Game) &&
                !asset.Game.Equals(game.Id, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (asset.CollectionOptional)
            {
                var enabled = !string.IsNullOrWhiteSpace(asset.Name) &&
                    enabledOptionalAssets?.TryGetValue(asset.Name, out var isEnabled) == true &&
                    isEnabled;
                if (!enabled)
                    continue;
            }

            if (!string.IsNullOrWhiteSpace(asset.Name))
                files.Add(asset.CollectionOptional ? $"{asset.Name} (optional, enabled)" : asset.Name);
            if (asset.Multi is not null)
                files.AddRange(asset.Multi.Select(entry => entry.Name).Where(name => !string.IsNullOrWhiteSpace(name)));
        }

        return files.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static (string? SourceUrl, string? ReportBugUrl) GetRepositoryLinks(string directory)
    {
        try
        {
            var configFile = Path.Combine(directory, ".git", "config");
            if (!File.Exists(configFile))
                return (null, null);

            var sourceUrl = NormalizeRepositoryUrl(ReadOriginRemoteUrl(configFile));
            return sourceUrl is null ? (null, null) : (sourceUrl, $"{sourceUrl}/issues");
        }
        catch (IOException)
        {
            return (null, null);
        }
        catch (UnauthorizedAccessException)
        {
            return (null, null);
        }
    }

    private static string? ReadOriginRemoteUrl(string configFile)
    {
        var isOrigin = false;
        foreach (var line in File.ReadLines(configFile))
        {
            var value = line.Trim();
            if (value.StartsWith('['))
            {
                isOrigin = value.Equals("[remote \"origin\"]", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!isOrigin || !value.StartsWith("url", StringComparison.OrdinalIgnoreCase))
                continue;

            var separator = value.IndexOf('=');
            if (separator >= 0)
                return value[(separator + 1)..].Trim();
        }

        return null;
    }

    private static string? NormalizeRepositoryUrl(string? remoteUrl)
    {
        if (string.IsNullOrWhiteSpace(remoteUrl))
            return null;

        var value = remoteUrl.Trim();
        if (value.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
        {
            var separator = value.IndexOf(':');
            if (separator <= 4 || separator == value.Length - 1)
                return null;
            value = $"https://{value[4..separator]}/{value[(separator + 1)..]}";
        }
        else if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme is not ("http" or "https" or "ssh"))
                return null;
            value = $"https://{uri.Host}{uri.AbsolutePath}";
        }
        else
        {
            return null;
        }

        value = value.TrimEnd('/');
        return value.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? value[..^4]
            : value;
    }

    private IReadOnlyList<string> ReadEnabledIds(GameInfo game)
    {
        var path = GetEnabledModsPath(game);
        return File.Exists(path)
            ? File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray()
            : [];
    }

    private IReadOnlyList<string> ReadOrderIds(GameInfo game)
    {
        var path = _configuration.GetModOrderFile(game);
        return File.Exists(path)
            ? File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray()
            : [];
    }

    private Dictionary<string, Dictionary<string, bool>> ReadCollectionSettings(GameInfo game)
    {
        var fileName = _configuration.GetCollectionSettingsFile(game);
        if (!File.Exists(fileName) || string.IsNullOrWhiteSpace(File.ReadAllText(fileName)))
            return new Dictionary<string, Dictionary<string, bool>>(StringComparer.OrdinalIgnoreCase);
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(File.ReadAllText(fileName))
                ?? new Dictionary<string, Dictionary<string, bool>>(StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, Dictionary<string, bool>>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private string GetEnabledModsPath(GameInfo game) => _configuration.GetEnabledModsFile(game);

    private string GetGameModsDirectory(GameInfo game)
    {
        return _configuration.GetGameModsDirectory(game);
    }

    private string GetCollectionsDirectory() => _configuration.CollectionsDirectory;

    private sealed record ModLocation(string Id, string Directory);
}
