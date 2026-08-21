using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class ModManagerConfiguration
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .IgnoreFields()
        .IgnoreUnmatchedProperties()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .IgnoreFields()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public int WizardVersionNumber { get; set; }
    [YamlMember(Alias = "installedModsPath")]
    public string? ModCollectionPath { get; set; }

    [YamlMember(Alias = "installedCollectionsPath")]
    public string? ModCollectionsPath { get; set; }

    [YamlMember(Alias = "compiledModPath")]
    public string? GameModPath { get; set; }

    [YamlMember(Alias = "extractedGameDataPath")]
    public string? GameDataPath { get; set; }
    public int GameEdition { get; set; } = 2;

    [YamlMember(Alias = "isoLocationKH2")]
    public string? IsoLocationKh2 { get; set; }

    [YamlMember(Alias = "isoLocationKH1")]
    public string? IsoLocationKh1 { get; set; }

    public string? IsoLocationRecom { get; set; }
    public string? OpenKhGameEngineLocation { get; set; }
    public string? Pcsx2Location { get; set; }
    public string? PcReleaseLocation { get; set; }

    [YamlMember(Alias = "pcReleaseLocationKH3D")]
    public string? PcReleaseLocationKh3D { get; set; }

    public string PcReleaseLanguage { get; set; } = "en";
    public int RegionId { get; set; }
    public bool PanaceaInstalled { get; set; }
    public bool ShowConsole { get; set; }
    public bool DebugLog { get; set; }
    public bool SoundDebug { get; set; }
    public bool EnableCache { get; set; } = true;
    public bool QuickMenu { get; set; }
    public bool EnablePatching { get; set; }
    public bool AutoUpdateMods { get; set; }

    [YamlMember(Alias = "pcVersion")]
    public string PcVersion { get; set; } = "Steam";

    [YamlMember(Alias = "steamAPITrick1525")]
    public bool SteamApiTrick1525 { get; set; }

    [YamlMember(Alias = "steamAPITrick28")]
    public bool SteamApiTrick28 { get; set; }

    public List<string> GamesToExtract { get; set; } = [];
    public bool SkipRemastered { get; set; }
    public string LaunchGame { get; set; } = "kh2";
    public bool DarkMode { get; set; } = true;
    public bool Updated { get; set; }
    [YamlIgnore]
    public List<CreatorPreference> CreatorPreferences { get; set; } = [];

    [YamlMember(Alias = "yamlGenPrefs")]
    public List<YamlGeneratorPreference> YamlGeneratorPreferences
    {
        get => CreatorPreferences.Select(YamlGeneratorPreference.FromCreatorPreference).ToList();
        set => CreatorPreferences = value?
            .Select(preference => preference.ToCreatorPreference())
            .ToList() ?? [];
    }

    public static ModManagerConfiguration Load(string fileName)
    {
        if (!File.Exists(fileName))
            return new ModManagerConfiguration();

        var yaml = File.ReadAllText(fileName);
        var configuration = Deserializer.Deserialize<ModManagerConfiguration>(yaml) ?? new ModManagerConfiguration();
        var interim = Deserializer.Deserialize<InterimConfiguration>(yaml) ?? new InterimConfiguration();
        var migrated = configuration.ApplyInterimConfiguration(interim);
        configuration.GamesToExtract ??= [];
        configuration.CreatorPreferences ??= [];
        if (migrated)
            configuration.Save(fileName);
        return configuration;
    }

    public void Save(string fileName)
    {
        var directory = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var temporaryFile = $"{fileName}.tmp";
        using (var writer = File.CreateText(temporaryFile))
            Serializer.Serialize(writer, this);

        File.Move(temporaryFile, fileName, true);
    }

    private bool ApplyInterimConfiguration(InterimConfiguration interim)
    {
        var migrated = false;
        if (IsMissing(ModCollectionPath, interim.ModCollectionPath))
        {
            ModCollectionPath = interim.ModCollectionPath;
            migrated = true;
        }
        if (IsMissing(ModCollectionsPath, interim.ModCollectionsPath))
        {
            ModCollectionsPath = interim.ModCollectionsPath;
            migrated = true;
        }
        if (IsMissing(GameModPath, interim.GameModPath))
        {
            GameModPath = interim.GameModPath;
            migrated = true;
        }
        if (IsMissing(GameDataPath, interim.GameDataPath))
        {
            GameDataPath = interim.GameDataPath;
            migrated = true;
        }
        if (CreatorPreferences.Count == 0 && interim.CreatorPreferences is { Count: > 0 })
        {
            CreatorPreferences = interim.CreatorPreferences;
            migrated = true;
        }
        if (!EnablePatching && interim.DevView == true)
        {
            EnablePatching = true;
            migrated = true;
        }

        return migrated;
    }

    private static bool IsMissing(string? destination, string? source) =>
        string.IsNullOrWhiteSpace(destination) && !string.IsNullOrWhiteSpace(source);

    private sealed class InterimConfiguration
    {
        public string? ModCollectionPath { get; set; }
        public string? ModCollectionsPath { get; set; }
        public string? GameModPath { get; set; }
        public string? GameDataPath { get; set; }
        public bool? DevView { get; set; }
        public List<CreatorPreference>? CreatorPreferences { get; set; }
    }
}

public sealed class CreatorPreference
{
    public string Label { get; set; } = string.Empty;
    public string ModDirectory { get; set; } = string.Empty;
    public string GameDataPath { get; set; } = string.Empty;
    public string DiffToolPath { get; set; } = string.Empty;
}

public sealed class YamlGeneratorPreference
{
    public string Label { get; set; } = string.Empty;
    public string GameDataPath { get; set; } = string.Empty;
    public string ModYmlFilePath { get; set; } = string.Empty;
    public string DiffToolPath { get; set; } = string.Empty;

    public CreatorPreference ToCreatorPreference() => new()
    {
        Label = Label,
        GameDataPath = GameDataPath,
        ModDirectory = GetModDirectory(ModYmlFilePath),
        DiffToolPath = DiffToolPath,
    };

    public static YamlGeneratorPreference FromCreatorPreference(CreatorPreference preference) => new()
    {
        Label = preference.Label,
        GameDataPath = preference.GameDataPath,
        ModYmlFilePath = GetModYmlPath(preference.ModDirectory),
        DiffToolPath = preference.DiffToolPath,
    };

    private static string GetModDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        return Path.GetExtension(path).Equals(".yml", StringComparison.OrdinalIgnoreCase)
            ? Path.GetDirectoryName(path) ?? string.Empty
            : path;
    }

    private static string GetModYmlPath(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            return string.Empty;

        return Path.GetExtension(directory).Equals(".yml", StringComparison.OrdinalIgnoreCase)
            ? directory
            : Path.Combine(directory, "mod.yml");
    }
}
