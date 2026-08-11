namespace OpenKh.Tools.ModsManager.Core;

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
            .Select(id => CreateEntry(locations[id], enabledLookup.Contains(id)))
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

    private static ModEntry CreateEntry(ModLocation location, bool isEnabled)
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
            IsCollection = metadata?.IsCollection == true,
            IsEnabled = isEnabled
        };
    }

    private IReadOnlyList<string> ReadEnabledIds(GameInfo game)
    {
        var path = GetEnabledModsPath(game);
        return File.Exists(path)
            ? File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray()
            : [];
    }

    private string GetEnabledModsPath(GameInfo game) => _configuration.GetEnabledModsFile(game);

    private string GetGameModsDirectory(GameInfo game)
    {
        return _configuration.GetGameModsDirectory(game);
    }

    private string GetCollectionsDirectory() => _configuration.CollectionsDirectory;

    private sealed record ModLocation(string Id, string Directory);
}
