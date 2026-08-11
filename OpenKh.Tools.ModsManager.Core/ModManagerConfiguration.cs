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
    public string? ModCollectionPath { get; set; }
    public string? ModCollectionsPath { get; set; }
    public string? GameModPath { get; set; }
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
    public bool DevView { get; set; }
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
    public List<CreatorPreference> CreatorPreferences { get; set; } = [];

    public static ModManagerConfiguration Load(string fileName)
    {
        if (!File.Exists(fileName))
            return new ModManagerConfiguration();

        using var reader = File.OpenText(fileName);
        var configuration = Deserializer.Deserialize<ModManagerConfiguration>(reader) ?? new ModManagerConfiguration();
        configuration.GamesToExtract ??= [];
        configuration.CreatorPreferences ??= [];
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
}

public sealed class CreatorPreference
{
    public string Label { get; set; } = string.Empty;
    public string ModDirectory { get; set; } = string.Empty;
    public string GameDataPath { get; set; } = string.Empty;
    public string DiffToolPath { get; set; } = string.Empty;
}
