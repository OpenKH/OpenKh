namespace OpenKh.Tools.ModsManager.Core;

public sealed class ModManagerConfigurationService
{
    private readonly InstallationLayout _layout;

    public ModManagerConfigurationService(InstallationLayout layout)
    {
        _layout = layout;
        Current = ModManagerConfiguration.Load(layout.ConfigurationFile);
        if (Current.GameEdition == 0)
            Current.GameEdition = 2;
        if (string.IsNullOrWhiteSpace(Current.PcVersion))
            Current.PcVersion = "Steam";
    }

    public ModManagerConfiguration Current { get; }
    public string InstallationDirectory => _layout.RootDirectory;
    public string ModCollectionRoot => ResolvePath(Current.ModCollectionPath, _layout.RootDirectory);
    public string CollectionsDirectory => ResolvePath(
        Current.ModCollectionsPath,
        Path.Combine(ModCollectionRoot, "mods", "collections"));
    public string GameDataDirectory => ResolvePath(
        Current.GameDataPath,
        Path.Combine(_layout.RootDirectory, "data"));

    public string GetGameModsDirectory(GameInfo game) =>
        Path.Combine(ModCollectionRoot, "mods", game.Id);

    public string GetGameModOutputDirectory(GameInfo game) =>
        string.IsNullOrWhiteSpace(Current.GameModPath)
            ? Path.Combine(ModCollectionRoot, "mod", game.Id)
            : ResolvePath(Current.GameModPath, Path.Combine(ModCollectionRoot, "mod", game.Id));

    public string GetEnabledModsFile(GameInfo game) =>
        Path.Combine(_layout.RootDirectory, game.EnabledModsFileName);

    public string GetCollectionSettingsFile(GameInfo game) =>
        Path.Combine(_layout.RootDirectory, $"collection-mods-{game.ConfigFileSuffix}.json");

    public void Save() => Current.Save(_layout.ConfigurationFile);

    public void EnsureDirectories()
    {
        Directory.CreateDirectory(ModCollectionRoot);
        Directory.CreateDirectory(CollectionsDirectory);
        Directory.CreateDirectory(GameDataDirectory);
        foreach (var game in GameInfo.SupportedGames)
            Directory.CreateDirectory(GetGameModsDirectory(game));
    }

    private string ResolvePath(string? configuredPath, string fallbackPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            return Path.GetFullPath(fallbackPath);

        return Path.GetFullPath(Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_layout.RootDirectory, configuredPath));
    }
}
