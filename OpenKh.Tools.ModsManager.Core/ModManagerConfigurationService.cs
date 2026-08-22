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
    public string ModCollectionRoot => GetConfiguredRoot(
        Current.ModCollectionPath,
        Current.GameModPath,
        "mods");
    public string CompiledModsRoot => GetConfiguredRoot(
        Current.GameModPath,
        Current.ModCollectionPath,
        "mod");
    public string CollectionsDirectory => GetCollectionsDirectory();
    public string GameDataDirectory => ResolvePath(
        Current.GameDataPath,
        Path.Combine(_layout.RootDirectory, "data"));

    public string GetGameModsDirectory(GameInfo game) =>
        Path.Combine(ModCollectionRoot, game.Id);

    public string GetGameModOutputDirectory(GameInfo game) =>
        Path.Combine(CompiledModsRoot, game.Id);

    public string GetEnabledModsFile(GameInfo game) =>
        Path.Combine(_layout.RootDirectory, game.EnabledModsFileName);

    public string GetModOrderFile(GameInfo game) =>
        Path.Combine(_layout.RootDirectory, $"mod-order-{game.ConfigFileSuffix}.txt");

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

    private string GetCollectionsDirectory()
    {
        if (string.IsNullOrWhiteSpace(Current.ModCollectionsPath))
            return Path.Combine(_layout.RootDirectory, "mods", "collections");

        var configuredPath = ResolvePath(Current.ModCollectionsPath, _layout.RootDirectory);
        if (!PathsMatch(Current.ModCollectionsPath, Current.ModCollectionPath))
            return configuredPath;

        return Path.Combine(configuredPath, "collections");
    }

    private string GetConfiguredRoot(string? configuredPath, string? conflictingPath, string fallbackName)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath) && !PathsMatch(configuredPath, conflictingPath))
            return ResolvePath(configuredPath, _layout.RootDirectory);

        var parent = ResolvePath(configuredPath, _layout.RootDirectory);
        return Path.Combine(parent, fallbackName);
    }

    private bool PathsMatch(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            return false;

        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return ResolvePath(left, _layout.RootDirectory)
            .Equals(ResolvePath(right, _layout.RootDirectory), comparison);
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
