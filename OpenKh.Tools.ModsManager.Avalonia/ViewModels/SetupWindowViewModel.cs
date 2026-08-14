using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.ViewModels;

public sealed class SetupWindowViewModel : ObservableObject
{
    private readonly ModManagerConfigurationService _service;
    private int _gameEdition;
    private int _pcVersionIndex;
    private string? _pcReleaseLocation;
    private string? _pcReleaseLocationKh3D;

    public SetupWindowViewModel(ModManagerConfigurationService service)
    {
        _service = service;
        var configuration = service.Current;
        GameEdition = configuration.GameEdition == 1 ? 1 : 0;
        ModCollectionPath = configuration.ModCollectionPath;
        GameDataPath = configuration.GameDataPath;
        PcReleaseLocation = configuration.PcReleaseLocation;
        PcReleaseLocationKh3D = configuration.PcReleaseLocationKh3D;
        PcVersionIndex = configuration.PcVersion.ToLowerInvariant() switch
        {
            "egs" => 0,
            "steam" => 1,
            "other" => 2,
            _ => 1
        };
        LanguageIndex = configuration.PcReleaseLanguage.Equals("jp", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        RegionId = Math.Clamp(configuration.RegionId, 0, 8);
        SteamApiTrick1525 = configuration.SteamApiTrick1525;
        SteamApiTrick28 = configuration.SteamApiTrick28;
        Pcsx2Location = configuration.Pcsx2Location;
        IsoLocationKh2 = configuration.IsoLocationKh2;
        IsoLocationKh1 = configuration.IsoLocationKh1;
        IsoLocationRecom = configuration.IsoLocationRecom;
        AutoUpdateMods = configuration.AutoUpdateMods;
        SkipRemastered = configuration.SkipRemastered;
        EnableCache = configuration.EnableCache;
        ShowConsole = configuration.ShowConsole;
        DebugLog = configuration.DebugLog;
        var gamesToExtract = configuration.GamesToExtract.ToHashSet(StringComparer.OrdinalIgnoreCase);
        ExtractKh1 = gamesToExtract.Contains("kh1");
        ExtractKh2 = gamesToExtract.Contains("kh2");
        ExtractBbs = gamesToExtract.Contains("bbs");
        ExtractRecom = gamesToExtract.Contains("Recom");
        ExtractKh3D = gamesToExtract.Contains("kh3d");
    }

    public int GameEdition
    {
        get => _gameEdition;
        set
        {
            if (!SetProperty(ref _gameEdition, value))
                return;
            OnPropertyChanged(nameof(IsPcMode));
            OnPropertyChanged(nameof(IsPcsx2Mode));
        }
    }
    public bool IsPcMode => GameEdition == 0;
    public bool IsPcsx2Mode => GameEdition == 1;
    public string? ModCollectionPath { get; set; }
    public string? GameDataPath { get; set; }
    public string? PcReleaseLocation
    {
        get => _pcReleaseLocation;
        set => SetProperty(ref _pcReleaseLocation, value);
    }

    public string? PcReleaseLocationKh3D
    {
        get => _pcReleaseLocationKh3D;
        set => SetProperty(ref _pcReleaseLocationKh3D, value);
    }

    public int PcVersionIndex
    {
        get => _pcVersionIndex;
        set
        {
            if (SetProperty(ref _pcVersionIndex, value))
                OnPropertyChanged(nameof(IsSteamPlatform));
        }
    }

    public bool IsSteamPlatform => PcVersionIndex == 1;
    public int LanguageIndex { get; set; }
    public int RegionId { get; set; }
    public bool SteamApiTrick1525 { get; set; }
    public bool SteamApiTrick28 { get; set; }
    public string? Pcsx2Location { get; set; }
    public string? IsoLocationKh2 { get; set; }
    public string? IsoLocationKh1 { get; set; }
    public string? IsoLocationRecom { get; set; }
    public bool AutoUpdateMods { get; set; }
    public bool SkipRemastered { get; set; }
    public bool EnableCache { get; set; }
    public bool ShowConsole { get; set; }
    public bool DebugLog { get; set; }
    public bool ExtractKh1 { get; set; }
    public bool ExtractKh2 { get; set; }
    public bool ExtractBbs { get; set; }
    public bool ExtractRecom { get; set; }
    public bool ExtractKh3D { get; set; }

    public void Save()
    {
        var configuration = _service.Current;
        configuration.GameEdition = IsPcsx2Mode ? 1 : 2;
        configuration.ModCollectionPath = EmptyToNull(ModCollectionPath);
        configuration.GameDataPath = EmptyToNull(GameDataPath);
        configuration.PcReleaseLocation = EmptyToNull(PcReleaseLocation);
        configuration.PcReleaseLocationKh3D = EmptyToNull(PcReleaseLocationKh3D);
        configuration.PcVersion = PcVersionIndex switch
        {
            1 => "Steam",
            2 => "Other",
            _ => "EGS"
        };
        configuration.PcReleaseLanguage = LanguageIndex == 1 ? "jp" : "en";
        configuration.RegionId = RegionId;
        configuration.SteamApiTrick1525 = SteamApiTrick1525;
        configuration.SteamApiTrick28 = SteamApiTrick28;
        configuration.Pcsx2Location = EmptyToNull(Pcsx2Location);
        configuration.IsoLocationKh2 = EmptyToNull(IsoLocationKh2);
        configuration.IsoLocationKh1 = EmptyToNull(IsoLocationKh1);
        configuration.IsoLocationRecom = EmptyToNull(IsoLocationRecom);
        configuration.AutoUpdateMods = AutoUpdateMods;
        configuration.SkipRemastered = SkipRemastered;
        configuration.EnableCache = EnableCache;
        configuration.ShowConsole = ShowConsole;
        configuration.DebugLog = DebugLog;
        configuration.GamesToExtract = GetGamesToExtract().ToList();
        _service.Save();
        _service.EnsureDirectories();
    }

    public IReadOnlyCollection<GameInfo> GetSelectedGames() =>
        GetGamesToExtract().Select(GameInfo.FromId).ToArray();

    private IEnumerable<string> GetGamesToExtract()
    {
        if (ExtractKh1)
            yield return "kh1";
        if (ExtractKh2)
            yield return "kh2";
        if (ExtractBbs && IsPcMode)
            yield return "bbs";
        if (ExtractRecom)
            yield return "Recom";
        if (ExtractKh3D && IsPcMode)
            yield return "kh3d";
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
