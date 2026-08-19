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
        var enabledMods = mods.Where(mod => mod.IsEnabled).Select(mod => mod.Id);
        File.WriteAllLines(GetEnabledModsPath(game), enabledMods);
    }

    private IReadOnlyList<ModEntry> Load(GameInfo game)
    {
        var enabledIds = ReadEnabledIds(game);
        var enabledLookup = enabledIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var collectionSettings = ReadCollectionSettings(game);
        var locations = EnumerateModLocations(game)
            .GroupBy(location => location.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToDictionary(location => location.Id, StringComparer.OrdinalIgnoreCase);

        var orderedIds = enabledIds
            .Where(locations.ContainsKey)
            .Concat(locations.Keys
                .Where(id => !enabledLookup.Contains(id))
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase));

        return orderedIds
            .Select(id => CreateEntry(
                locations[id],
                enabledLookup.Contains(id),
                game,
                collectionSettings))
            .ToArray();
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
            FilesToPatch = GetFilesToPatch(metadata, location.Id, game, collectionSettings),
            IsCollection = metadata?.IsCollection == true,
            IsEnabled = isEnabled
        };
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

    private IReadOnlyList<string> ReadEnabledIds(GameInfo game)
    {
        var path = GetEnabledModsPath(game);
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
