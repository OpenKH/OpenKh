using Octokit;
using OpenKh.Common;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO.Compression;
using System.Diagnostics;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public class SetupWizardViewModel : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        private const int BufferSize = 65536;
        private readonly GameDataExtractionService _gameDataExtractionService = new GameDataExtractionService();
        private static readonly string PanaceaDllName = "OpenKH.Panacea.dll";
        private static string ApplicationName = Utilities.GetApplicationName();
        private static List<FileDialogFilter> _isoFilter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("PlayStation 2 game ISO", "iso");
        private static List<FileDialogFilter> _openkhGeFilter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("OpenKH Game Engine Executable", "*Game.exe");
        private static List<FileDialogFilter> _pcsx2Filter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("PCSX2 Emulator", "exe");

        public const int OpenKHGameEngine = 0;
        public const int PCSX2 = 1;
        public const int PC = 2;

        private int _gameEdition;
        private string _isoLocation = null;
        private string _isoLocationKH2 = ConfigurationService.IsoLocationKH2;
        private string _isoLocationKH1 = ConfigurationService.IsoLocationKH1;
        private string _isoLocationRecom = ConfigurationService.IsoLocationRecom;
        private string _openKhGameEngineLocation = ConfigurationService.OpenKhGameEngineLocation;
        private string _pcsx2Location = ConfigurationService.Pcsx2Location;
        private string _pcReleaseLocation = ConfigurationService.PcReleaseLocation;
        private string _pcReleaseLocationKH3D = ConfigurationService.PcReleaseLocationKH3D;
        private int _gameCollection = 0;
        private string _pcReleaseLanguage = ConfigurationService.PcReleaseLanguage;
        private string _gameDataLocation = ConfigurationService.GameDataLocation;
        private List<string> LuaScriptPaths = new List<string>();
        private bool _overrideGameDataFound = false;

        private Xceed.Wpf.Toolkit.WizardPage _wizardPageAfterIntro;
        public Xceed.Wpf.Toolkit.WizardPage WizardPageAfterIntro
        {
            get => _wizardPageAfterIntro;
            private set
            {
                _wizardPageAfterIntro = value;
                OnPropertyChanged();
            }
        }
        private Xceed.Wpf.Toolkit.WizardPage _wizardPageAfterGameData;
        public Xceed.Wpf.Toolkit.WizardPage WizardPageAfterGameData
        {
            get => _wizardPageAfterGameData;
            private set
            {
                _wizardPageAfterGameData = value;
                OnPropertyChanged();
            }
        }
        private Xceed.Wpf.Toolkit.WizardPage _wizardPageAfterLuaBackend;
        public Xceed.Wpf.Toolkit.WizardPage WizardPageAfterLuaBackend
        {
            get => _wizardPageAfterLuaBackend;
            private set
            {
                _wizardPageAfterLuaBackend = value;
                OnPropertyChanged();
            }
        }
        public Xceed.Wpf.Toolkit.WizardPage PageIsoSelection { get; internal set; }
        public Xceed.Wpf.Toolkit.WizardPage PageEosInstall { get; internal set; }
        public Xceed.Wpf.Toolkit.WizardPage PageSteamAPITrick { get; internal set; }
        public Xceed.Wpf.Toolkit.WizardPage PageRegion { get; internal set; }
        public Xceed.Wpf.Toolkit.WizardPage PageGameData { get; internal set; }
        public Xceed.Wpf.Toolkit.WizardPage LastPage { get; internal set; }

        public WizardPageStackService PageStack { get; set; } = new WizardPageStackService();

        private const string RAW_FILES_FOLDER_NAME = "raw";
        private const string ORIGINAL_FILES_FOLDER_NAME = "original";
        private const string REMASTERED_FILES_FOLDER_NAME = "remastered";

        public string Title => $"Set-up wizard | {ApplicationName}";

        public RelayCommand SelectIsoCommand { get; }
        public string GameId { get; set; }
        public string GameName { get; set; }
        public string IsoLocation
        {
            get => _isoLocation;
            set
            {
                _isoLocation = value;
                if (File.Exists(_isoLocation))
                {
                    var game = GameService.DetectGameId(_isoLocation);
                    GameId = game?.Id;
                    GameName = game?.Name;
                }
                else
                {
                    GameId = null;
                    GameName = null;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(GameName));
            }
        }
        public void ValidateIsoLocations()
        {
            if (!File.Exists(_isoLocationKH2) || GameService.DetectGameId(_isoLocationKH2)?.Id != "kh2")
            {
                _isoLocationKH2 = null;
                ConfigurationService.IsoLocationKH2 = _isoLocationKH2;
            }
            if (!File.Exists(_isoLocationKH1) || GameService.DetectGameId(_isoLocationKH1)?.Id != "kh1")
            {
                _isoLocationKH1 = null;
                ConfigurationService.IsoLocationKH1 = _isoLocationKH1;
            }
            if (!File.Exists(_isoLocationRecom) || GameService.DetectGameId(_isoLocationRecom)?.Id != "Recom")
            {
                _isoLocationRecom = null;
                ConfigurationService.IsoLocationRecom = _isoLocationRecom;
            }
            OnPropertyChanged(nameof(IsGameDataFound));
            OnPropertyChanged(nameof(GameDataFoundVisibility));
            OnPropertyChanged(nameof(GameDataNotFoundVisibility));
            OnPropertyChanged(nameof(KH2RecognizedVisibility));
            OnPropertyChanged(nameof(KH1RecognizedVisibility));
            OnPropertyChanged(nameof(RecomRecognizedVisibility));
        }
        public string IsoLocationKH2
        {
            get => _isoLocationKH2;
            set
            {
                _isoLocationKH2 = value;
                if (File.Exists(_isoLocationKH2))
                {
                    if (GameService.DetectGameId(_isoLocationKH2)?.Id == "kh2")
                    {
                        WizardPageAfterGameData = PageRegion;
                    }
                    else
                    {
                        WizardPageAfterGameData = LastPage;
                        _isoLocationKH2 = null;
                    }
                }
                else
                {
                    WizardPageAfterGameData = LastPage;
                    _isoLocationKH2 = null;
                }
                ConfigurationService.IsoLocationKH2 = _isoLocationKH2;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(GameDataFoundVisibility));
                OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                OnPropertyChanged(nameof(KH2RecognizedVisibility));
                OnPropertyChanged(nameof(WizardPageAfterGameData));
            }
        }
        public string IsoLocationKH1
        {
            get => _isoLocationKH1;
            set
            {
                _isoLocationKH1 = value;
                if (File.Exists(_isoLocationKH1))
                {
                    var game = GameService.DetectGameId(_isoLocationKH1);
                    if (game?.Id != "kh1")
                    {
                        _isoLocationKH1 = null;
                    }
                }
                else
                {
                    _isoLocationKH1 = null;
                }
                ConfigurationService.IsoLocationKH1 = _isoLocationKH1;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(GameDataFoundVisibility));
                OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                OnPropertyChanged(nameof(KH1RecognizedVisibility));
            }
        }
        public string IsoLocationRecom
        {
            get => _isoLocationRecom;
            set
            {
                _isoLocationRecom = value;
                if (File.Exists(_isoLocationRecom))
                {
                    var game = GameService.DetectGameId(_isoLocationRecom);
                    if (game?.Id != "Recom")
                    {
                        _isoLocationRecom = null;
                    }
                }
                else
                {
                    _isoLocationRecom = null;
                }
                ConfigurationService.IsoLocationRecom = _isoLocationRecom;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(GameDataFoundVisibility));
                OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                OnPropertyChanged(nameof(RecomRecognizedVisibility));
            }
        }
        public bool IsIsoSelected => (!string.IsNullOrEmpty(IsoLocation) && File.Exists(IsoLocation));
        public bool IsGameRecognized => (IsIsoSelected && GameId != null);
        public Visibility GameRecognizedVisibility => IsIsoSelected && GameId != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameNotRecognizedVisibility => IsIsoSelected && GameId == null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility KH1RecognizedVisibility => _isoLocationKH1 != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility KH2RecognizedVisibility => _isoLocationKH2 != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility RecomRecognizedVisibility => _isoLocationRecom != null ? Visibility.Visible : Visibility.Collapsed;

        public bool IsGameSelected
        {
            get
            {
                return GameEdition switch
                {
                    OpenKHGameEngine => !string.IsNullOrEmpty(OpenKhGameEngineLocation) && File.Exists(OpenKhGameEngineLocation),
                    PCSX2 => !string.IsNullOrEmpty(Pcsx2Location) && File.Exists(Pcsx2Location),
                    PC => (!string.IsNullOrEmpty(PcReleaseLocation) &&
                        Directory.Exists(PcReleaseLocation) &&
                        (File.Exists(Path.Combine(PcReleaseLocation, "EOSSDK-Win64-Shipping.dll")) ||
                        File.Exists(Path.Combine(PcReleaseLocation, "steam_api64.dll"))))||
                        (!string.IsNullOrEmpty(PcReleaseLocationKH3D) &&
                        Directory.Exists(PcReleaseLocationKH3D) &&
                        (File.Exists(Path.Combine(PcReleaseLocationKH3D, "EOSSDK-Win64-Shipping.dll")) ||
                        File.Exists(Path.Combine(PcReleaseLocationKH3D, "steam_api64.dll")))),
                    _ => false,
                };
            }
        }

        public int GameEdition
        {
            get
            {
                _gameEdition = ConfigurationService.GameEdition;
                WizardPageAfterIntro = _gameEdition switch
                {
                    OpenKHGameEngine => LastPage,
                    PCSX2 => PageIsoSelection,
                    PC => PageEosInstall,
                    _ => null,
                };
                WizardPageAfterGameData = _gameEdition switch
                {
                    OpenKHGameEngine => LastPage,
                    PCSX2 => _isoLocationKH2 != null ? PageRegion : LastPage,
                    PC => LastPage,
                    _ => null,
                };
                return _gameEdition;
            }
            set
            {
                _gameEdition = value;
                ConfigurationService.GameEdition = _gameEdition;
                WizardPageAfterIntro = _gameEdition switch
                {
                    OpenKHGameEngine => LastPage,
                    PCSX2 => PageIsoSelection,
                    PC => PageEosInstall,
                    _ => null,
                };
                WizardPageAfterGameData = _gameEdition switch
                {
                    OpenKHGameEngine => LastPage,
                    PCSX2 => _isoLocationKH2!= null ? PageRegion : LastPage,
                    PC => LastPage,
                    _ => null,
                };

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameSelected));
                OnPropertyChanged(nameof(OpenKhGameEngineConfigVisibility));
                OnPropertyChanged(nameof(Pcsx2ConfigVisibility));
                OnPropertyChanged(nameof(PcReleaseConfigVisibility));
                OnPropertyChanged(nameof(BothPcReleaseSelected));
                OnPropertyChanged(nameof(PcRelease1525Selected));
                OnPropertyChanged(nameof(PcRelease28Selected));
            }
        }
        public int PCReleaseLanguage
        {
            get
            {
                switch (_pcReleaseLanguage)
                {
                    case "jp":
                        return 1;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (value)
                {
                    case 1:
                        ConfigurationService.PcReleaseLanguage = "jp";
                        _pcReleaseLanguage = "jp";
                        break;
                    default:
                        ConfigurationService.PcReleaseLanguage = "en";
                        _pcReleaseLanguage = "en";
                        break;
                }
            }
        }

        public int LaunchOption
        {
            get
            {
                switch (ConfigurationService.PCVersion)
                {
                    case "Steam":
                        WizardPageAfterLuaBackend = PageSteamAPITrick;
                        return 1;
                    case "Other":
                        WizardPageAfterLuaBackend = PageGameData;
                        return 2;
                    default:
                        WizardPageAfterLuaBackend = PageGameData;
                        return 0;
                }
            }
            set
            {
                switch (value)
                {
                    case 1:
                        ConfigurationService.PCVersion = "Steam";
                        WizardPageAfterLuaBackend = PageSteamAPITrick;
                        break;
                    case 2:
                        ConfigurationService.PCVersion = "Other";
                        WizardPageAfterLuaBackend = PageGameData;
                        break;
                    default:
                        ConfigurationService.PCVersion = "EGS";
                        WizardPageAfterLuaBackend = PageGameData;
                        break;
                }
            }
        }
        public RelayCommand SelectOpenKhGameEngineCommand { get; }
        public Visibility OpenKhGameEngineConfigVisibility => GameEdition == OpenKHGameEngine ? Visibility.Visible : Visibility.Collapsed;
        public string OpenKhGameEngineLocation
        {
            get => _openKhGameEngineLocation;
            set
            {
                _openKhGameEngineLocation = value;
                ConfigurationService.OpenKhGameEngineLocation = _openKhGameEngineLocation;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameSelected));
            }
        }

        public RelayCommand SelectPcsx2Command { get; }
        public Visibility Pcsx2ConfigVisibility => GameEdition == PCSX2 ? Visibility.Visible : Visibility.Collapsed;
        public string Pcsx2Location
        {
            get => _pcsx2Location;
            set
            {
                _pcsx2Location = value;
                ConfigurationService.Pcsx2Location = _pcsx2Location;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameSelected));
            }
        }

        public RelayCommand SelectPcReleaseCommand { get; }
        public RelayCommand SelectPcReleaseKH3DCommand { get; }
        public Visibility PcReleaseConfigVisibility => GameEdition == PC  ? Visibility.Visible : Visibility.Collapsed;
        public Visibility BothPcReleaseSelected => PcReleaseSelections == "both" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PcRelease1525Selected => PcReleaseSelections == "1.5+2.5" ? Visibility.Visible: Visibility.Collapsed;
        public Visibility PcRelease28Selected => PcReleaseSelections == "2.8" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility InstallForPc1525 => GameCollection == 0 && (PcReleaseSelections == "both" || PcReleaseSelections == "1.5+2.5") ? Visibility.Visible : Visibility.Collapsed;
        public Visibility InstallForPc28 => GameCollection == 1 && (PcReleaseSelections == "both" || PcReleaseSelections == "2.8") ? Visibility.Visible :Visibility.Collapsed;
        public string PcReleaseLocation
        {
            get => _pcReleaseLocation;
            set
            {
                _pcReleaseLocation = value;
                ConfigurationService.PcReleaseLocation = _pcReleaseLocation;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLastPanaceaVersionInstalled));
                OnPropertyChanged(nameof(PanaceaInstalledVisibility));
                OnPropertyChanged(nameof(PanaceaNotInstalledVisibility));
                OnPropertyChanged(nameof(IsGameSelected));
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(LuaBackendFoundVisibility));
                OnPropertyChanged(nameof(LuaBackendNotFoundVisibility));
                OnPropertyChanged(nameof(BothPcReleaseSelected));
                OnPropertyChanged(nameof(PcRelease1525Selected));
                OnPropertyChanged(nameof(PcRelease28Selected));
                OnPropertyChanged(nameof(InstallForPc1525));
                OnPropertyChanged(nameof(SteamAPIFileFound));
                OnPropertyChanged(nameof(SteamAPIFileNotFound));
            }
        }

        public string PcReleaseLocationKH3D
        {
            get => _pcReleaseLocationKH3D;
            set
            {
                _pcReleaseLocationKH3D = value;
                ConfigurationService.PcReleaseLocationKH3D = _pcReleaseLocationKH3D;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLastPanaceaVersionInstalled));
                OnPropertyChanged(nameof(PanaceaInstalledVisibility));
                OnPropertyChanged(nameof(PanaceaNotInstalledVisibility));
                OnPropertyChanged(nameof(IsGameSelected));
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(LuaBackendFoundVisibility));
                OnPropertyChanged(nameof(LuaBackendNotFoundVisibility));
                OnPropertyChanged(nameof(BothPcReleaseSelected));
                OnPropertyChanged(nameof(PcRelease1525Selected));
                OnPropertyChanged(nameof(PcRelease28Selected));
                OnPropertyChanged(nameof(InstallForPc28));
                OnPropertyChanged(nameof(SteamAPIFileFound));
                OnPropertyChanged(nameof(SteamAPIFileNotFound));
            }
        }

        public string PcReleaseSelections
        {
            get
            {
                if (Directory.Exists(PcReleaseLocation) && (File.Exists(Path.Combine(PcReleaseLocation, "EOSSDK-Win64-Shipping.dll")) ||
                    File.Exists(Path.Combine(PcReleaseLocation, "steam_api64.dll"))) &&
                    Directory.Exists(PcReleaseLocationKH3D) && (File.Exists(Path.Combine(PcReleaseLocationKH3D, "EOSSDK-Win64-Shipping.dll")) ||
                    File.Exists(Path.Combine(PcReleaseLocationKH3D, "steam_api64.dll"))) && _gameEdition == PC)
                {
                    return "both";
                }
                else if (Directory.Exists(PcReleaseLocation) && (File.Exists(Path.Combine(PcReleaseLocation, "EOSSDK-Win64-Shipping.dll")) ||
                    File.Exists(Path.Combine(PcReleaseLocation, "steam_api64.dll"))) && _gameEdition == PC)
                {

                    return "1.5+2.5";
                }
                else if (Directory.Exists(PcReleaseLocationKH3D) && (File.Exists(Path.Combine(PcReleaseLocationKH3D, "EOSSDK-Win64-Shipping.dll")) ||
                    File.Exists(Path.Combine(PcReleaseLocationKH3D, "steam_api64.dll"))) && _gameEdition == PC)
                {

                    return "2.8";
                }
                return "";
            }
            set { }
        }

        public int GameCollection
        {
            get => _gameCollection;
            set
            {
                _gameCollection = value;
                OnPropertyChanged(nameof(GameCollection));
                OnPropertyChanged(nameof(InstallForPc1525));
                OnPropertyChanged(nameof(InstallForPc28));
                OnPropertyChanged(nameof(LuaBackendFoundVisibility));
                OnPropertyChanged(nameof(LuaBackendNotFoundVisibility));
                OnPropertyChanged(nameof(IsLastPanaceaVersionInstalled));
                OnPropertyChanged(nameof(PanaceaInstalledVisibility));
                OnPropertyChanged(nameof(PanaceaNotInstalledVisibility));
                OnPropertyChanged(nameof(SteamAPIFileFound));
                OnPropertyChanged(nameof(SteamAPIFileNotFound));
            }
        }
        public bool Extractkh1
        {
            get => ConfigurationService.Extractkh1;
            set => ConfigurationService.Extractkh1 = value;
        }
        public bool Extractkh2
        {
            get => ConfigurationService.Extractkh2;
            set => ConfigurationService.Extractkh2 = value;
        }
        public bool Extractbbs
        {
            get => ConfigurationService.Extractbbs;
            set => ConfigurationService.Extractbbs = value;
        }
        public bool Extractrecom
        {
            get => ConfigurationService.Extractrecom;
            set => ConfigurationService.Extractrecom = value;
        }
        public bool Extractkh3d
        {
            get => ConfigurationService.Extractkh3d;
            set => ConfigurationService.Extractkh3d = value;
        }
        public bool SkipRemastered
        {
            get => ConfigurationService.SkipRemastered;
            set => ConfigurationService.SkipRemastered = value;
        }
        public bool LuaConfigkh1
        {
            get => LuaScriptPaths.Contains("kh1");
            set
            {
                if (value)
                {
                    LuaScriptPaths.Add("kh1");
                }
                else
                {
                    LuaScriptPaths.Remove("kh1");
                }
            }
        }
        public bool LuaConfigkh2
        {
            get => LuaScriptPaths.Contains("kh2");
            set
            {
                if (value)
                {
                    LuaScriptPaths.Add("kh2");
                }
                else
                {
                    LuaScriptPaths.Remove("kh2");
                }
            }
        }
        public bool LuaConfigbbs
        {
            get => LuaScriptPaths.Contains("bbs");
            set
            {
                if (value)
                {
                    LuaScriptPaths.Add("bbs");
                }
                else
                {
                    LuaScriptPaths.Remove("bbs");
                }
            }
        }
        public bool LuaConfigrecom
        {
            get => LuaScriptPaths.Contains("Recom");
            set
            {
                if (value)
                {
                    LuaScriptPaths.Add("Recom");
                }
                else
                {
                    LuaScriptPaths.Remove("Recom");
                }
            }
        }
        public bool LuaConfigkh3d
        {
            get => LuaScriptPaths.Contains("kh3d");
            set
            {
                if (value)
                {
                    LuaScriptPaths.Add("kh3d");
                }
                else
                {
                    LuaScriptPaths.Remove("kh3d");
                }
            }
        }
        public bool OverrideGameDataFound
        {
            get => _overrideGameDataFound;
            set
            {
                _overrideGameDataFound = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                OnPropertyChanged(nameof(GameDataFoundVisibility));
            }

        }

        public RelayCommand SelectGameDataLocationCommand { get; }
        public string GameDataLocation
        {
            get => ConfigurationService.GameDataLocation = _gameDataLocation;
            set
            {
                _gameDataLocation = value;
                ConfigurationService.GameDataLocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                OnPropertyChanged(nameof(GameDataFoundVisibility));
            }
        }

        public bool IsNotExtracting { get; private set; }
        public bool IsGameDataFound => IsNotExtracting &&
            ((GameEdition == PCSX2 && (GameService.FolderContainsUniqueFile("kh2", Path.Combine(GameDataLocation, "kh2")) ||
            GameService.FolderContainsUniqueFile("kh1", Path.Combine(GameDataLocation, "kh1")) ||
            GameService.FolderContainsUniqueFile("Recom", Path.Combine(GameDataLocation, "Recom")))) ||
            (GameEdition == PC && (GameService.FolderContainsUniqueFile("kh2", Path.Combine(GameDataLocation, "kh2")) ||
            GameService.FolderContainsUniqueFile("kh1", Path.Combine(GameDataLocation, "kh1")) ||
            Directory.Exists(Path.Combine(GameDataLocation, "bbs", "message")) ||
            Directory.Exists(Path.Combine(GameDataLocation, "Recom", "SYS"))||
            Directory.Exists(Path.Combine(GameDataLocation, "kh3d","setdata"))))||
            OverrideGameDataFound);
        public Visibility GameDataNotFoundVisibility => !IsGameDataFound ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameDataFoundVisibility => IsGameDataFound ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProgressBarVisibility => IsNotExtracting ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ExtractionCompleteVisibility => ExtractionProgress == 1f ? Visibility.Visible : Visibility.Collapsed;
        public RelayCommand ExtractGameDataCommand { get; set; }
        public float ExtractionProgress { get; set; }
        public int RegionId
        {
            get
            {
                return ConfigurationService.RegionId;
            }
            set
            {
                ConfigurationService.RegionId = value;
            }
        }
        public bool IsLuaBackendInstalled
        {
            get
            {
                if (PcReleaseLocation != null && GameCollection == 0)
                {
                    return File.Exists(Path.Combine(PcReleaseLocation, "LuaBackend.dll")) && File.Exists(Path.Combine(PcReleaseLocation, "LuaBackend.toml"));
                }
                else if (PcReleaseLocationKH3D != null && GameCollection == 1)
                {
                    return File.Exists(Path.Combine(PcReleaseLocationKH3D, "LuaBackend.dll")) && File.Exists(Path.Combine(PcReleaseLocationKH3D, "LuaBackend.toml"));
                }
                else
                    return false;
            }
        }
        public bool IsSteamAPIFileInstalled
        {
            get
            {
                if (PcReleaseLocation != null && GameCollection == 0 && ConfigurationService.PCVersion == "Steam")
                {
                    if (File.Exists(Path.Combine(PcReleaseLocation, "steam_appid.txt")))
                    {
                        ConfigurationService.SteamAPITrick1525 = true;
                        return true;
                    }
                    return false;
                }
                else if (PcReleaseLocationKH3D != null && GameCollection == 1)
                {
                    if (File.Exists(Path.Combine(PcReleaseLocationKH3D, "steam_appid.txt")))
                    {
                        ConfigurationService.SteamAPITrick28 = true;
                        return true;
                    }
                    return false;
                }
                else
                return false;
            }
        }
        public RelayCommand InstallSteamAPIFile {  get; set; }
        public RelayCommand RemoveSteamAPIFile { get; set; }
        public RelayCommand InstallLuaBackendCommand { get; set; }
        public RelayCommand RemoveLuaBackendCommand { get; set; }
        public Visibility LuaBackendFoundVisibility => IsLuaBackendInstalled ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LuaBackendNotFoundVisibility => IsLuaBackendInstalled ? Visibility.Collapsed : Visibility.Visible;
        public Visibility SteamAPIFileFound => IsSteamAPIFileInstalled ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SteamAPIFileNotFound => IsSteamAPIFileInstalled ? Visibility.Collapsed : Visibility.Visible;
        public RelayCommand InstallPanaceaCommand { get; }
        public RelayCommand DetectInstallsCommand { get; }
        public RelayCommand RemovePanaceaCommand { get; }
        public bool PanaceaInstalled { get; set; }
        private string PanaceaSourceLocation => Path.Combine(AppContext.BaseDirectory, PanaceaDllName);
        public Visibility PanaceaNotInstalledVisibility => !IsLastPanaceaVersionInstalled ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PanaceaInstalledVisibility => IsLastPanaceaVersionInstalled ? Visibility.Visible : Visibility.Collapsed;
        public bool IsLastPanaceaVersionInstalled
        {
            get
            {
                if (PcReleaseLocation == null && GameCollection == 0)
                {
                    // Won't be able to find the source location
                    PanaceaInstalled = false;
                    ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                    return false;
                }

                if (PcReleaseLocationKH3D == null && GameCollection == 1)
                {
                    // Won't be able to find the source location
                    PanaceaInstalled = false;
                    ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                    return false;
                }

                if (!File.Exists(PanaceaSourceLocation))
                {
                    // While debugging it is most likely to not have the compiled
                    // DLL into the right place. So don't bother.
                    PanaceaInstalled = true;
                    ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                    return true;
                }

                byte[] CalculateChecksum(string fileName) =>
                    System.Security.Cryptography.MD5.Create().Using(md5 =>
                        File.OpenRead(fileName).Using(md5.ComputeHash));
                bool IsEqual(byte[] left, byte[] right)
                {
                    for (var i = 0; i < left.Length; i++)
                        if (left[i] != right[i])
                        {
                            PanaceaInstalled = false;
                            ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                            return false;
                        }
                    PanaceaInstalled = true;
                    ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                    return true;
                }
                string PanaceaDestinationLocation = null;
                string PanaceaAlternateLocation = null;
                if (GameCollection == 0)
                {
                    PanaceaDestinationLocation = Path.Combine(PcReleaseLocation, "DBGHELP.dll");
                    PanaceaAlternateLocation = Path.Combine(PcReleaseLocation, "version.dll");
                }
                else
                {
                    PanaceaDestinationLocation = Path.Combine(PcReleaseLocationKH3D, "DBGHELP.dll");
                    PanaceaAlternateLocation = Path.Combine(PcReleaseLocationKH3D, "version.dll");
                }

                if (File.Exists(PanaceaDestinationLocation) && !File.Exists(PanaceaAlternateLocation))
                {
                    if(IsEqual(CalculateChecksum(PanaceaSourceLocation),CalculateChecksum(PanaceaDestinationLocation)))
                    {
                        PanaceaInstalled = true;
                        ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                        return true;
                    }
                    return false;
                }
                else if (File.Exists(PanaceaAlternateLocation) && !File.Exists(PanaceaDestinationLocation))
                {
                    if (IsEqual(CalculateChecksum(PanaceaSourceLocation),CalculateChecksum(PanaceaAlternateLocation)))
                    {
                        PanaceaInstalled = true;
                        ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                        return true;
                    }
                    return false;
                }
                else if (File.Exists(PanaceaDestinationLocation) && File.Exists(PanaceaAlternateLocation))
                {
                    if (IsEqual(CalculateChecksum(PanaceaSourceLocation),CalculateChecksum(PanaceaDestinationLocation)) ||
                        IsEqual(CalculateChecksum(PanaceaSourceLocation),CalculateChecksum(PanaceaAlternateLocation)))
                    {
                        PanaceaInstalled = true;
                        ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                        return true;
                    }
                    return false;
                }
                else
                {
                    PanaceaInstalled = false;
                    ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                    return false;
                }
            }
        }

        public SetupWizardViewModel()
        {
            IsNotExtracting = true;
            ValidateIsoLocations();
            SelectIsoCommand = new RelayCommand(param => {
                string gameId = param as string;
                FileDialog.OnOpen(
                    fileName => {
                        IsoLocation = fileName;
                        if (gameId == GameId)
                        {
                            switch (gameId)
                            {
                                case "kh2":
                                    IsoLocationKH2 = fileName;
                                    break;
                                case "kh1":
                                    IsoLocationKH1 = fileName;
                                    break;
                                case "Recom":
                                    IsoLocationRecom = fileName;
                                    break;
                            }
                        }
                        else
                        {
                            GameId = null;
                            GameName = null;
                        }
                        OnPropertyChanged(nameof(GameName));
                        OnPropertyChanged(nameof(IsIsoSelected));
                        OnPropertyChanged(nameof(GameRecognizedVisibility));
                        OnPropertyChanged(nameof(GameNotRecognizedVisibility));
                        OnPropertyChanged(nameof(IsGameRecognized));
                    },
                    _isoFilter
                );
            });
            SelectOpenKhGameEngineCommand = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => OpenKhGameEngineLocation = fileName, _openkhGeFilter));
            SelectPcsx2Command = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => Pcsx2Location = fileName, _pcsx2Filter));
            SelectPcReleaseCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(path => PcReleaseLocation = path));
            SelectPcReleaseKH3DCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(path => PcReleaseLocationKH3D = path));
            SelectGameDataLocationCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(path => GameDataLocation = path));
            ExtractGameDataCommand = new RelayCommand(async _ =>
            {
            BEGIN:
                try
                {
                    if (GameEdition == PCSX2)
                    {
                        if (Extractkh2 && !string.IsNullOrEmpty(IsoLocationKH2) && GameService.DetectGameId(IsoLocationKH2)?.Id == "kh2")
                        {
                            await ExtractGameData(IsoLocationKH2, GameDataLocation);
                        }
                        if (Extractkh1 && !string.IsNullOrEmpty(IsoLocationKH1) && GameService.DetectGameId(IsoLocationKH1)?.Id == "kh1")
                        {
                            await ExtractGameData(IsoLocationKH1, GameDataLocation);
                        }
                        if (Extractrecom && !string.IsNullOrEmpty(IsoLocationRecom) && GameService.DetectGameId(IsoLocationRecom)?.Id == "Recom")
                        {
                            await ExtractGameData(IsoLocationRecom, GameDataLocation);
                        }
                    }
                    else if (GameEdition == PC)
                    {
                        await ExtractGameData(null, GameDataLocation);
                    }
                }
                catch (OperationCanceledException)
                {
                    // user closed the dialog
                }
                catch (GameDataExtractionService.BadConfigurationException _ex)
                {
                    MessageBox.Show(
                        _ex.Message,
                        "Extraction error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                catch (Exception _ex)
                {
                    var _sysMessage = MessageBox.Show(_ex.Message + "\n\nWould you like to try again?", "An Exception was Caught!", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    switch (_sysMessage)
                    {
                        case MessageBoxResult.Yes:
                            goto BEGIN;
                    }
                }
            });
            DetectInstallsCommand = new RelayCommand(_ =>
            {
                if (ConfigurationService.PCVersion == "EGS")
                {
                    // Get ProgramData Folder Location
                    string programDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    string directoryPath = Path.Combine(programDataFolder, "Epic\\EpicGamesLauncher\\Data\\Manifests");

                    if (!Directory.Exists(directoryPath))
                    {
                        MessageBox.Show("No Game Install Locations Found\nPlease Manually Browse To Your Game Install Directory", "Failure", MessageBoxButton.OK);
                        return;
                    }

                    // Get List of .item Files in C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests\
                    IEnumerable<string> itemFiles = Directory.EnumerateFiles(directoryPath, "*.item");

                    if (!itemFiles.Any())
                    {
                        MessageBox.Show("No Game Install Locations Found\nPlease Manually Browse To Your Game Install Directory", "Failure", MessageBoxButton.OK);
                        return;
                    }

                    bool installLocationFoundRemix = false;
                    bool installLocationFound3D = false;
                    try
                    {
                        foreach (string itemFile in itemFiles)
                        {
                            // Read Each .item File and Locate Install Location For Games
                            using StreamReader sr = new StreamReader(itemFile);
                            string line;
                            if (sr != null)
                            {
                                while ((line = sr.ReadLine()) is not null)
                                {
                                    if (line.Contains("\"LaunchExecutable\": \"KINGDOM HEARTS HD 1.5+2.5 ReMIX.exe\","))
                                    {
                                        while ((line = sr.ReadLine()) is not null)
                                        {
                                            if (line.Contains("\"InstallLocation\": \""))
                                            {
                                                int startIndex = line.IndexOf("\": \"") + 4;
                                                int endIndex = line.IndexOf("\",");
                                                string parsedText = line[startIndex..endIndex];
                                                parsedText = parsedText.Replace("\\\\", "\\");
                                                PcReleaseLocation = parsedText;
                                                if (File.Exists(Path.Combine(PcReleaseLocation, "EOSSDK-Win64-Shipping.dll")))
                                                {
                                                    installLocationFoundRemix = true;
                                                }
                                            }
                                        }
                                    }
                                    else if (line.Contains("\"LaunchExecutable\": \"KINGDOM HEARTS HD 2.8 Final Chapter Prologue.exe\","))
                                    {
                                        while ((line = sr.ReadLine()) is not null)
                                        {
                                            if (line.Contains("\"InstallLocation\": \""))
                                            {
                                                int startIndex = line.IndexOf("\": \"") + 4;
                                                int endIndex = line.IndexOf("\",");
                                                string parsedText = line[startIndex..endIndex];
                                                parsedText = parsedText.Replace("\\\\", "\\");
                                                PcReleaseLocationKH3D = parsedText;
                                                if (File.Exists(Path.Combine(PcReleaseLocationKH3D, "EOSSDK-Win64-Shipping.dll")))
                                                {
                                                    installLocationFound3D = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!installLocationFoundRemix && !installLocationFound3D)
                        {
                            MessageBox.Show("No Game Install Locations Found\nThis may be caused by missing files in the game install folder. If you have an installation verify your game files through Epic Games Store to get the missing files and try again." +
                                "\nPlease Manually Browse To Your Game Install Directory", "Failure", MessageBoxButton.OK);
                        }
                        else if (!installLocationFoundRemix && installLocationFound3D)
                        {
                            MessageBox.Show("Kingdom Hearts HD 1.5+2.5: MISSING\n(This may be caused by missing files in the game install folder. If you have an installation verify your game files through Epic Games Store to get the missing files and try again.)" +
                                "\nKingdom Hearts HD 2.8: FOUND", "Success", MessageBoxButton.OK);
                        }
                        else if (installLocationFoundRemix && !installLocationFound3D)
                        {
                            MessageBox.Show("Kingdom Hearts HD 1.5+2.5: FOUND\nKingdom Hearts HD 2.8: MISSING" +
                                "\n(This may be caused by missing files in the game install folder. If you have an installation verify your game files through Epic Games Store to get the missing files and try again.)", "Success", MessageBoxButton.OK);
                        }
                        else
                        {
                            MessageBox.Show("Kingdom Hearts HD 1.5+2.5: FOUND\nKingdom Hearts HD 2.8: FOUND", "Success", MessageBoxButton.OK);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\nPlease Manually Browse To Your Game Install Directory");
                        return;
                    }
                }
                else if (ConfigurationService.PCVersion == "Steam")
                {
                    // Get ProgramFilesX86 Location
                    string programDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                    string directoryPath = Path.Combine(programDataFolder, "Steam\\steamapps");

                    if (!Directory.Exists(directoryPath))
                    {
                        MessageBox.Show("No Game Install Locations Found\nPlease Manually Browse To Your Game Install Directory", "Failure", MessageBoxButton.OK);
                        return;
                    }

                    bool installLocationFoundRemix = false;
                    bool installLocationFound3D = false;
                    // Read the entire content of the VDF file
                    string vdfContent;
                    try
                    {
                        vdfContent = File.ReadAllText(Path.Combine(directoryPath, "libraryfolders.vdf"));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\nPlease Manually Browse To Your Game Install Directory");
                        return;
                    }
                    try
                    {
                        // Define a regular expression to match "path" values
                        Regex regex = new Regex(@"""path""\s*""([^""]*)""", RegexOptions.IgnoreCase);
                        // MatchCollection to store all matches found
                        MatchCollection matches = regex.Matches(vdfContent);
                        // Iterate through matches and print out the "path" values
                        if (Directory.Exists(directoryPath))
                        {
                            foreach (Match match in matches)
                            {
                                string pathValue = match.Groups[1].Value; // Group 1 is the path raw, without the key
                                Console.WriteLine($"Path: {pathValue}");
                                string parsedText = pathValue.Replace(@"\\", @"\");
                                string commonGamesDirectory = Path.Combine(parsedText, "steamapps\\common");
                                if (Directory.Exists(commonGamesDirectory))
                                {
                                    string kH1525Path = Path.Combine(commonGamesDirectory, @"KINGDOM HEARTS -HD 1.5+2.5 ReMIX-");
                                    string kH28Path = Path.Combine(commonGamesDirectory, @"KINGDOM HEARTS HD 2.8 Final Chapter Prologue");
                                    if (File.Exists(Path.Combine(kH1525Path, "steam_api64.dll")))
                                    {
                                        installLocationFoundRemix = true;
                                        PcReleaseLocation = kH1525Path;
                                    }
                                    if (File.Exists(Path.Combine(kH28Path, "steam_api64.dll")))
                                    {
                                        installLocationFound3D = true;
                                        PcReleaseLocationKH3D = kH28Path;
                                    }
                                }
                            }
                        }
                        if (!installLocationFoundRemix && !installLocationFound3D)
                        {
                            MessageBox.Show("No Game Install Locations Found\nThis may be caused by missing files in the game install folder. If you have an installation verify your game files through Steam to get the missing files and try again." +
                                "\nPlease Manually Browse To Your Game Install Directory", "Failure", MessageBoxButton.OK);
                        }
                        else if (!installLocationFoundRemix && installLocationFound3D)
                        {
                            MessageBox.Show("Kingdom Hearts HD 1.5+2.5: MISSING (This may be caused by missing files in the game install folder. If you have an installation verify your game files through Steam to get the missing files and try again.)" +
                                "\nKingdom Hearts HD 2.8: FOUND", "Success", MessageBoxButton.OK);
                        }
                        else if (installLocationFoundRemix && !installLocationFound3D)
                        {
                            MessageBox.Show("Kingdom Hearts HD 1.5+2.5: FOUND\nKingdom Hearts HD 2.8: MISSING" +
                                "(This may be caused by missing files in the game install folder. If you have an installation verify your game files through Steam to get the missing files and try again.)", "Success", MessageBoxButton.OK);
                        }
                        else
                        {
                            MessageBox.Show("Kingdom Hearts HD 1.5+2.5: FOUND\nKingdom Hearts HD 2.8: FOUND", "Success", MessageBoxButton.OK);
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Launcher \"Other\" does not support auto detect game installation. If you wish to use this feature select either EGS or Steam on the dropdown above", "Unsupported", MessageBoxButton.OK);
                }

            });
            InstallPanaceaCommand = new RelayCommand(AlternateName =>
            {
                if (File.Exists(PanaceaSourceLocation))
                {
                    // Again, do not bother in debug mode
                    string PanaceaDestinationLocation = null;
                    string PanaceaAlternateLocation = null;
                    string PanaceaDependenciesLocation = null;
                    if (GameCollection == 0 && PcReleaseLocation != null)
                    {
                        PanaceaDestinationLocation = Path.Combine(PcReleaseLocation, "DBGHELP.dll");
                        PanaceaAlternateLocation = Path.Combine(PcReleaseLocation, "version.dll");
                        PanaceaDependenciesLocation = Path.Combine(PcReleaseLocation, "dependencies");
                    }
                    else if (GameCollection == 1 && PcReleaseLocationKH3D != null)
                    {
                        PanaceaDestinationLocation = Path.Combine(PcReleaseLocationKH3D, "DBGHELP.dll");
                        PanaceaAlternateLocation = Path.Combine(PcReleaseLocationKH3D, "version.dll");
                        PanaceaDependenciesLocation = Path.Combine(PcReleaseLocationKH3D, "dependencies");
                    }
                    else if (PanaceaDestinationLocation == null || PanaceaAlternateLocation == null || PanaceaDestinationLocation == null)
                    {
                        MessageBox.Show(
                                $"At least one filepath is invalid check you selected a correct filepath for the collection you are trying to install panacea for.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        return;
                    }
                    if (!Directory.Exists(PanaceaDependenciesLocation))
                    {
                        Directory.CreateDirectory(PanaceaDependenciesLocation);
                    }
                    try
                    {
                        if(Process.GetProcessesByName("winlogon").Length > 0)
                        {
                            File.Copy(PanaceaSourceLocation, PanaceaDestinationLocation, true);
                            File.Delete(PanaceaAlternateLocation);
                        }
                        else
                        {
                            File.Copy(PanaceaSourceLocation, PanaceaAlternateLocation, true);
                            File.Delete(PanaceaDestinationLocation);
                        }
                        if (Directory.Exists(PcReleaseLocation))
                        {
                            File.WriteAllLines(Path.Combine(PcReleaseLocation, "panacea_settings.txt"),
                                [
                                $"mod_path={Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath,".."))}",
                                $"show_console={false}",
                                ]);
                        }
                        if (Directory.Exists(PcReleaseLocationKH3D))
                        {
                            File.WriteAllLines(Path.Combine(PcReleaseLocationKH3D, "panacea_settings.txt"),
                                [
                                $"mod_path={Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath,".."))}",
                                $"show_console={false}",
                                ]);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        PanaceaInstalled = false;
                        ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                        return;
                    }
                    try
                    {
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "avcodec-vgmstream-59.dll"), Path.Combine(PanaceaDependenciesLocation, "avcodec-vgmstream-59.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "avformat-vgmstream-59.dll"), Path.Combine(PanaceaDependenciesLocation, "avformat-vgmstream-59.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "avutil-vgmstream-57.dll"), Path.Combine(PanaceaDependenciesLocation, "avutil-vgmstream-57.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "bass.dll"), Path.Combine(PanaceaDependenciesLocation, "bass.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "bass_vgmstream.dll"), Path.Combine(PanaceaDependenciesLocation, "bass_vgmstream.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "libatrac9.dll"), Path.Combine(PanaceaDependenciesLocation, "libatrac9.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "libcelt-0061.dll"), Path.Combine(PanaceaDependenciesLocation, "libcelt-0061.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "libcelt-0110.dll"), Path.Combine(PanaceaDependenciesLocation, "libcelt-0110.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "libg719_decode.dll"), Path.Combine(PanaceaDependenciesLocation, "libg719_decode.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "libmpg123-0.dll"), Path.Combine(PanaceaDependenciesLocation, "libmpg123-0.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "libspeex-1.dll"), Path.Combine(PanaceaDependenciesLocation, "libspeex-1.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "libvorbis.dll"), Path.Combine(PanaceaDependenciesLocation, "libvorbis.dll"), true);
                        File.Copy(Path.Combine(AppContext.BaseDirectory, "swresample-vgmstream-4.dll"), Path.Combine(PanaceaDependenciesLocation, "swresample-vgmstream-4.dll"), true);
                    }
                    catch
                    {
                        MessageBox.Show(
                            $"Missing panacea dependencies. Unable to fully install panacea.",
                            "Extraction error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        File.Delete(PanaceaDestinationLocation);
                        File.Delete(PanaceaAlternateLocation);
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "avcodec-vgmstream-59.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "avformat-vgmstream-59.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "avutil-vgmstream-57.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "bass.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "bass_vgmstream.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libatrac9.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libcelt-0061.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libcelt-0110.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libg719_decode.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libmpg123-0.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libspeex-1.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libvorbis.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "swresample-vgmstream-4.dll"));
                        File.Delete(Path.Combine(PcReleaseLocation, "panacea_settings.txt"));
                        File.Delete(Path.Combine(PcReleaseLocationKH3D, "panacea_settings.txt"));
                        PanaceaInstalled = false;
                        ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                        return;
                    }
                    OnPropertyChanged(nameof(IsLastPanaceaVersionInstalled));
                    OnPropertyChanged(nameof(PanaceaInstalledVisibility));
                    OnPropertyChanged(nameof(PanaceaNotInstalledVisibility));
                    PanaceaInstalled = true;
                    ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                }
            });
            RemovePanaceaCommand = new RelayCommand(_ =>
            {
                string PanaceaDestinationLocation = null;
                string PanaceaAlternateLocation = null;
                string PanaceaDependenciesLocation = null;
                if (GameCollection == 0 && PcReleaseLocation != null)
                {
                    PanaceaDestinationLocation = Path.Combine(PcReleaseLocation, "DBGHELP.dll");
                    PanaceaAlternateLocation = Path.Combine(PcReleaseLocation, "version.dll");
                    PanaceaDependenciesLocation = Path.Combine(PcReleaseLocation, "dependencies");
                }
                else if (GameCollection == 1 && PcReleaseLocationKH3D != null)
                {
                    PanaceaDestinationLocation = Path.Combine(PcReleaseLocationKH3D, "DBGHELP.dll");
                    PanaceaAlternateLocation = Path.Combine(PcReleaseLocationKH3D, "version.dll");
                    PanaceaDependenciesLocation = Path.Combine(PcReleaseLocationKH3D, "dependencies");
                }
                else if (PanaceaDestinationLocation == null || PanaceaAlternateLocation == null || PanaceaDestinationLocation == null)
                {
                    MessageBox.Show(
                            $"At least one filepath is invalid check you selected a correct filepath for the collection you are trying to uninstall panacea for.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    return;
                }
                try
                {
                    if (File.Exists(PanaceaDestinationLocation) || File.Exists(PanaceaAlternateLocation))
                    {
                        File.Delete(PanaceaDestinationLocation);
                        File.Delete(PanaceaAlternateLocation);
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "avcodec-vgmstream-59.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "avformat-vgmstream-59.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "avutil-vgmstream-57.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "bass.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "bass_vgmstream.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libatrac9.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libcelt-0061.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libcelt-0110.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libg719_decode.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libmpg123-0.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libspeex-1.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "libvorbis.dll"));
                        File.Delete(Path.Combine(PanaceaDependenciesLocation, "swresample-vgmstream-4.dll"));
                        if (GameCollection == 0 && PcReleaseLocation != null)
                        {
                            File.Delete(Path.Combine(PcReleaseLocation, "panacea_settings.txt"));
                        }
                        if (GameCollection == 1 && PcReleaseLocationKH3D != null)
                        {
                            File.Delete(Path.Combine(PcReleaseLocationKH3D, "panacea_settings.txt"));
                        }
                    }
                    OnPropertyChanged(nameof(IsLastPanaceaVersionInstalled));
                    OnPropertyChanged(nameof(PanaceaInstalledVisibility));
                    OnPropertyChanged(nameof(PanaceaNotInstalledVisibility));
                    PanaceaInstalled = false;
                    ConfigurationService.PanaceaInstalled = PanaceaInstalled;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            });
            InstallLuaBackendCommand = new RelayCommand(installed =>
            {
                try
                {
                    if (!Convert.ToBoolean(installed))
                    {
                        var gitClient = new GitHubClient(new ProductHeaderValue("LuaBackend"));
                        var releases = gitClient.Repository.Release.GetLatest(owner: "Sirius902", name: "LuaBackend").Result;
                        string DownPath = Path.GetTempPath() + "LuaBackend" + Path.GetExtension(releases.Assets[0].Name);
                        string TempExtractionLocation = Path.GetTempPath() + "LuaBackend";
                        #pragma warning disable SYSLIB0014 // Type or member is obsolete
                        var _client = new WebClient();
                        #pragma warning restore SYSLIB0014 // Type or member is obsolete
                        _client.DownloadFile(new System.Uri(releases.Assets[0].BrowserDownloadUrl), DownPath);
                        try
                        {
                            using (var zip = ZipFile.OpenRead(DownPath))
                            {
                                zip.ExtractToDirectory(TempExtractionLocation, true);
                            }
                        }
                        catch
                        {
                            MessageBox.Show(
                                    $"Unable to extract \"{Path.GetFileName(DownPath)}\" as it is not a zip file. You may have to install it manually.",
                                    "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                            File.Delete(DownPath);
                            File.Delete(TempExtractionLocation);
                            return;
                        }
                        string DestinationCollection = null;
                        if (GameCollection == 0)
                        {
                            DestinationCollection = PcReleaseLocation;
                        }
                        else
                        {
                            DestinationCollection = PcReleaseLocationKH3D;
                        }
                        if (DestinationCollection == null)
                        {
                            MessageBox.Show(
                                    $"At least one filepath is invalid check you selected a correct filepath for the collection you are trying to install Lua Backend for.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            File.Delete(DownPath);
                            Directory.Delete(TempExtractionLocation, true);
                            return;
                        }
                        else
                        {
                            if (File.Exists(Path.Combine(TempExtractionLocation, "DBGHELP.dll")))
                            {
                                File.Move(Path.Combine(TempExtractionLocation, "DBGHELP.dll"), Path.Combine(DestinationCollection, "LuaBackend.dll"), true);
                            }
                            if (File.Exists(Path.Combine(TempExtractionLocation, "LuaBackend.toml")))
                            {
                                File.Move(Path.Combine(TempExtractionLocation, "LuaBackend.toml"), Path.Combine(DestinationCollection, "LuaBackend.toml"), true);
                                string config = File.ReadAllText(Path.Combine(DestinationCollection, "LuaBackend.toml")).Replace("\\", "/").Replace("\\\\", "/");
                                if (LuaScriptPaths.Contains("kh1") && GameCollection == 0)
                                {
                                    int index = config.IndexOf("true }", config.IndexOf("[kh1]")) + 6;
                                    config = config.Insert(index, ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "kh1/scripts\" , relative = false}").Replace("\\", "/"));
                                }
                                if (LuaScriptPaths.Contains("kh2") && GameCollection == 0)
                                {
                                    int index = config.IndexOf("true }", config.IndexOf("[kh2]")) + 6;
                                    config = config.Insert(index, ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "kh2/scripts\" , relative = false}").Replace("\\", "/"));
                                }
                                if (LuaScriptPaths.Contains("bbs") && GameCollection == 0)
                                {
                                    int index = config.IndexOf("true }", config.IndexOf("[bbs]")) + 6;
                                    config = config.Insert(index, ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "bbs/scripts\" , relative = false}").Replace("\\", "/"));
                                }
                                if (LuaScriptPaths.Contains("Recom") && GameCollection == 0)
                                {
                                    int index = config.IndexOf("true }", config.IndexOf("[recom]")) + 6;
                                    config = config.Insert(index, ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "Recom/scripts\" , relative = false}").Replace("\\", "/"));
                                }
                                if (LuaScriptPaths.Contains("kh3d") && GameCollection == 1)
                                {
                                    int index = config.IndexOf("true }", config.IndexOf("[kh3d]")) + 6;
                                    config = config.Insert(index, ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "kh3d/scripts\" , relative = false}").Replace("\\", "/"));
                                }
                                if (ConfigurationService.PCVersion == "Steam")
                                {
                                    int indexEGS = 0;
                                    int indexSteam = 0;
                                    //KH3D
                                    indexEGS = config.IndexOf("game_docs = \"KINGDOM HEARTS HD 2.8 Final Chapter Prologue\"", config.IndexOf("[kh3d]"));
                                    indexSteam = config.IndexOf("# game_docs = \"My Games/KINGDOM HEARTS HD 2.8 Final Chapter Prologue\"", config.IndexOf("[kh3d]"));
                                    if (indexEGS > 0 && indexSteam > 0)
                                    {
                                        config = config.Remove(indexSteam, 2);
                                        config = config.Insert(indexEGS, "# ");
                                    }
                                    //ReCoM
                                    indexEGS = config.IndexOf("game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[recom]"));
                                    indexSteam = config.IndexOf("# game_docs = \"My Games/KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[recom]"));
                                    if (indexEGS > 0 && indexSteam > 0)
                                    {
                                        config = config.Remove(indexSteam, 2);
                                        config = config.Insert(indexEGS, "# ");
                                    }
                                    //BBS
                                    indexEGS = config.IndexOf("game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[bbs]"));
                                    indexSteam = config.IndexOf("# game_docs = \"My Games/KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[bbs]"));
                                    if (indexEGS > 0 && indexSteam > 0)
                                    {
                                        config = config.Remove(indexSteam, 2);
                                        config = config.Insert(indexEGS, "# ");
                                    }
                                    //KH2
                                    indexEGS = config.IndexOf("game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[kh2]"));
                                    indexSteam = config.IndexOf("# game_docs = \"My Games/KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[kh2]"));
                                    if (indexEGS > 0 && indexSteam > 0)
                                    {
                                        config = config.Remove(indexSteam, 2);
                                        config = config.Insert(indexEGS, "# ");
                                    }
                                    //KH1
                                    indexEGS = config.IndexOf("game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[kh1]"));
                                    indexSteam = config.IndexOf("# game_docs = \"My Games/KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[kh1]"));
                                    if (indexEGS > 0 && indexSteam > 0)
                                    {
                                        config = config.Remove(indexSteam, 2);
                                        config = config.Insert(indexEGS, "# ");
                                    }
                                }
                                File.WriteAllText(Path.Combine(DestinationCollection, "LuaBackend.toml"), config);
                            }
                            File.Delete(DownPath);
                            Directory.Delete(TempExtractionLocation);
                            OnPropertyChanged(nameof(IsLuaBackendInstalled));
                            OnPropertyChanged(nameof(LuaBackendFoundVisibility));
                            OnPropertyChanged(nameof(LuaBackendNotFoundVisibility));
                        }
                    }
                    else
                    {
                        string DestinationCollection = null;
                        if (GameCollection == 0)
                        {
                            DestinationCollection = PcReleaseLocation;
                        }
                        else
                        {
                            DestinationCollection = PcReleaseLocationKH3D;
                        }
                        if (DestinationCollection == null)
                        {
                            MessageBox.Show(
                                    $"At least one filepath is invalid check you selected a correct filepath for the collection you are trying to configure Lua Backend for.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            return;
                        }
                        else
                        {
                            if (File.Exists(Path.Combine(DestinationCollection, "LuaBackend.toml")))
                            {
                                string config = File.ReadAllText(Path.Combine(DestinationCollection, "LuaBackend.toml")).Replace("\\", "/").Replace("\\\\", "/");
                                if (LuaScriptPaths.Contains("kh1") && GameCollection == 0)
                                {
                                    if (config.Contains("mod/kh1/scripts"))
                                    {
                                        var errorMessage = MessageBox.Show($"Your Lua Backend is already configured to run Lua scripts for KH1 from an OpenKH Mod Manager." +
                                            $" Do you want to change Lua Backend to run scripts for KH1 from this version of OpenKH Mod Manager instead?", "Warning",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);

                                        switch (errorMessage)
                                        {
                                            case MessageBoxResult.Yes:
                                            {
                                                int index = config.IndexOf("scripts", config.IndexOf("[kh1]"));
                                                config = config.Remove(index, config.IndexOf("]", index) - index + 1);
                                                config = config.Insert(index, "scripts = [{ path = \"scripts/kh1/\", relative = true }" +
                                                    ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "kh1/scripts\" , relative = false}]").Replace("\\", "/"));
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int index = config.IndexOf("scripts", config.IndexOf("[kh1]"));
                                        config = config.Remove(index, config.IndexOf("]", index) - index + 1);
                                        config = config.Insert(index, "scripts = [{ path = \"scripts/kh1/\", relative = true }" +
                                            ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "kh1/scripts\" , relative = false}]").Replace("\\", "/"));
                                    }
                                }
                                if (LuaScriptPaths.Contains("kh2") && GameCollection == 0)
                                {
                                    if (config.Contains("mod/kh2/scripts"))
                                    {
                                        var errorMessage = MessageBox.Show($"Your Lua Backend is already configured to run Lua scripts for KH2 from an OpenKH Mod Manager." +
                                            $" Do you want to change Lua Backend to run scripts for KH2 from this version of OpenKH Mod Manager instead?", "Warning",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);

                                        switch (errorMessage)
                                        {
                                            case MessageBoxResult.Yes:
                                            {
                                                int index = config.IndexOf("scripts", config.IndexOf("[kh2]"));
                                                config = config.Remove(index, config.IndexOf("]", index) - index + 1);
                                                config = config.Insert(index, "scripts = [{ path = \"scripts/kh2/\", relative = true }" +
                                                    ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "kh2/scripts\" , relative = false}]").Replace("\\", "/"));
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int index = config.IndexOf("scripts", config.IndexOf("[kh2]"));
                                        config = config.Remove(index, config.IndexOf("]", index) - index + 1);
                                        config = config.Insert(index, "scripts = [{ path = \"scripts/kh2/\", relative = true }" +
                                            ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "kh2/scripts\" , relative = false}]").Replace("\\", "/"));
                                    }
                                }
                                if (LuaScriptPaths.Contains("bbs") && GameCollection == 0)
                                {
                                    if (config.Contains("mod/bbs/scripts"))
                                    {
                                        var errorMessage = MessageBox.Show($"Your Lua Backend is already configured to run Lua scripts for BBS from an OpenKH Mod Manager." +
                                            $" Do you want to change Lua Backend to run scripts for BBS from this version of OpenKH Mod Manager instead?", "Warning",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);

                                        switch (errorMessage)
                                        {
                                            case MessageBoxResult.Yes:
                                            {
                                                int index = config.IndexOf("scripts", config.IndexOf("[bbs]"));
                                                config = config.Remove(index, config.IndexOf("]", index) - index + 1);
                                                config = config.Insert(index, "scripts = [{ path = \"scripts/bbs/\", relative = true }" +
                                                    ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "bbs/scripts\" , relative = false}]").Replace("\\", "/"));
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int index = config.IndexOf("scripts", config.IndexOf("[bbs]"));
                                        config = config.Remove(index, config.IndexOf("]", index) - index + 1);
                                        config = config.Insert(index, "scripts = [{ path = \"scripts/bbs/\", relative = true }" +
                                            ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "bbs/scripts\" , relative = false}]").Replace("\\", "/"));
                                    }
                                }
                                if (LuaScriptPaths.Contains("Recom") && GameCollection == 0)
                                {
                                    if (config.Contains("mod/Recom/scripts"))
                                    {
                                        var errorMessage = MessageBox.Show($"Your Lua Backend is already configured to run Lua scripts for ReCoM from an OpenKH Mod Manager." +
                                            $" Do you want to change Lua Backend to run scripts for ReCoM from this version of OpenKH Mod Manager instead?", "Warning",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);

                                        switch (errorMessage)
                                        {
                                            case MessageBoxResult.Yes:
                                            {
                                                int index = config.IndexOf("scripts", config.IndexOf("[recom]"));
                                                config = config.Remove(index, config.IndexOf("]", index) - index + 1);
                                                config = config.Insert(index, "scripts = [{ path = \"scripts/recom/\", relative = true }" +
                                                    ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "Recom/scripts\" , relative = false}]").Replace("\\", "/"));
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int index = config.IndexOf("scripts", config.IndexOf("[recom]"));
                                        config = config.Remove(index, config.IndexOf("]", index) - index + 1);
                                        config = config.Insert(index, "scripts = [{ path = \"scripts/recom/\", relative = true }" +
                                            ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "Recom/scripts\" , relative = false}]").Replace("\\", "/"));
                                    }
                                }
                                if (LuaScriptPaths.Contains("kh3d") && GameCollection == 1)
                                {
                                    if (config.Contains("mod/kh3d/scripts"))
                                    {
                                        var errorMessage = MessageBox.Show($"Your Lua Backend is already configured to run Lua scripts for KH3D from an OpenKH Mod Manager." +
                                            $" Do you want to change Lua Backend to run scripts for KH3D from this version of OpenKH Mod Manager instead?", "Warning",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);

                                        switch (errorMessage)
                                        {
                                            case MessageBoxResult.Yes:
                                            {
                                                int index = config.IndexOf("scripts", config.IndexOf("[kh3d]"));
                                                config = config.Remove(index, config.IndexOf("]", index) - index + 1);
                                                config = config.Insert(index, "scripts = [{ path = \"scripts/kh3d/\", relative = true }" +
                                                    ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "kh3d/scripts\" , relative = false}]").Replace("\\", "/"));
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int index = config.IndexOf("scripts", config.IndexOf("[kh3d]"));
                                        config = config.Remove(index, config.IndexOf("]", index) - index + 1);
                                        config = config.Insert(index, "scripts = [{ path = \"scripts/kh3d/\", relative = true }" +
                                            ", {path = \"" + Path.Combine(Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath, "..")), "kh3d/scripts\" , relative = false}]").Replace("\\", "/"));
                                    }
                                }
                                if (ConfigurationService.PCVersion == "Steam")
                                {
                                    int indexEGS = 0;
                                    int indexSteam = 0;
                                    //KH3D
                                    indexEGS = config.IndexOf("game_docs = \"KINGDOM HEARTS HD 2.8 Final Chapter Prologue\"", config.IndexOf("[kh3d]"));
                                    indexSteam = config.IndexOf("# game_docs = \"My Games/KINGDOM HEARTS HD 2.8 Final Chapter Prologue\"", config.IndexOf("[kh3d]"));
                                    if (indexEGS > 0 && indexSteam > 0)
                                    {
                                        config = config.Remove(indexSteam, 2);
                                        config = config.Insert(indexEGS, "# ");
                                    }
                                    //ReCoM
                                    indexEGS = config.IndexOf("game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[recom]"));
                                    indexSteam = config.IndexOf("# game_docs = \"My Games/KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[recom]"));
                                    if (indexEGS > 0 && indexSteam > 0)
                                    {
                                        config = config.Remove(indexSteam, 2);
                                        config = config.Insert(indexEGS, "# ");
                                    }
                                    //BBS
                                    indexEGS = config.IndexOf("game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[bbs]"));
                                    indexSteam = config.IndexOf("# game_docs = \"My Games/KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[bbs]"));
                                    if (indexEGS > 0 && indexSteam > 0)
                                    {
                                        config = config.Remove(indexSteam, 2);
                                        config = config.Insert(indexEGS, "# ");
                                    }
                                    //KH2
                                    indexEGS = config.IndexOf("game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[kh2]"));
                                    indexSteam = config.IndexOf("# game_docs = \"My Games/KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[kh2]"));
                                    if (indexEGS > 0 && indexSteam > 0)
                                    {
                                        config = config.Remove(indexSteam, 2);
                                        config = config.Insert(indexEGS, "# ");
                                    }
                                    //KH1
                                    indexEGS = config.IndexOf("game_docs = \"KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[kh1]"));
                                    indexSteam = config.IndexOf("# game_docs = \"My Games/KINGDOM HEARTS HD 1.5+2.5 ReMIX\"", config.IndexOf("[kh1]"));
                                    if (indexEGS > 0 && indexSteam > 0)
                                    {
                                        config = config.Remove(indexSteam, 2);
                                        config = config.Insert(indexEGS, "# ");
                                    }
                                }
                                File.WriteAllText(Path.Combine(DestinationCollection, "LuaBackend.toml"), config);
                                OnPropertyChanged(nameof(IsLuaBackendInstalled));
                                OnPropertyChanged(nameof(LuaBackendFoundVisibility));
                                OnPropertyChanged(nameof(LuaBackendNotFoundVisibility));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            });
            RemoveLuaBackendCommand = new RelayCommand(_ =>
            {
                try
                {
                    if (GameCollection == 0 && PcReleaseLocation == null || GameCollection == 1 && PcReleaseLocationKH3D == null)
                    {
                        MessageBox.Show(
                                $"At least one filepath is invalid check you selected a correct filepath for the collection you are trying to uninstall Lua Backend for.",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        if (GameCollection == 0)
                        {
                            File.Delete(Path.Combine(PcReleaseLocation, "LuaBackend.dll"));
                            File.Delete(Path.Combine(PcReleaseLocation, "LuaBackend.toml"));
                        }
                        else if (GameCollection == 1)
                        {
                            File.Delete(Path.Combine(PcReleaseLocationKH3D, "LuaBackend.dll"));
                            File.Delete(Path.Combine(PcReleaseLocationKH3D, "LuaBackend.toml"));
                        }
                        OnPropertyChanged(nameof(IsLuaBackendInstalled));
                        OnPropertyChanged(nameof(LuaBackendFoundVisibility));
                        OnPropertyChanged(nameof(LuaBackendNotFoundVisibility));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            });
            InstallSteamAPIFile = new RelayCommand(_ =>
            {
                try
                {
                    if (GameCollection == 0)
                    {
                        if (Directory.Exists(PcReleaseLocation))
                        {
                            File.WriteAllText(Path.Combine(PcReleaseLocation, "steam_appid.txt"), "2552430");
                            ConfigurationService.SteamAPITrick1525 = true;
                        }
                    }
                    else if (GameCollection == 1)
                    {
                        if (Directory.Exists(PcReleaseLocationKH3D))
                        {
                            File.WriteAllText(Path.Combine(PcReleaseLocationKH3D, "steam_appid.txt"), "2552440");
                            ConfigurationService.SteamAPITrick28 = true;
                        }
                    }
                    OnPropertyChanged(nameof(SteamAPIFileFound));
                    OnPropertyChanged(nameof(SteamAPIFileNotFound));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            });
            RemoveSteamAPIFile = new RelayCommand(_ =>
            {
                try
                {
                    if (GameCollection == 0)
                    {
                        if (PcReleaseLocation != null && File.Exists(Path.Combine(PcReleaseLocation, "steam_appid.txt")))
                        {
                            File.Delete(Path.Combine(PcReleaseLocation, "steam_appid.txt"));
                            ConfigurationService.SteamAPITrick1525 = false;
                        }
                    }
                    else if (GameCollection == 1)
                    {
                        if (PcReleaseLocationKH3D != null && File.Exists(Path.Combine(PcReleaseLocationKH3D, "steam_appid.txt")))
                        {
                            File.Delete(Path.Combine(PcReleaseLocationKH3D, "steam_appid.txt"));
                            ConfigurationService.SteamAPITrick28 = false;
                        }
                    }
                    OnPropertyChanged(nameof(SteamAPIFileFound));
                    OnPropertyChanged(nameof(SteamAPIFileNotFound));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            });
        }

        private async Task ExtractGameData(string isoLocation, string gameDataLocation)
        {
            void MarkStarted()
            {
                IsNotExtracting = false;
                ExtractionProgress = 0;
                OnPropertyChanged(nameof(IsNotExtracting));
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(ProgressBarVisibility));
                OnPropertyChanged(nameof(ExtractionCompleteVisibility));
                OnPropertyChanged(nameof(ExtractionProgress));
            }

            void MarkSuccessful()
            {
                IsNotExtracting = true;
                ExtractionProgress = 1.0f;
                OnPropertyChanged(nameof(IsNotExtracting));
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                OnPropertyChanged(nameof(GameDataFoundVisibility));
                OnPropertyChanged(nameof(ProgressBarVisibility));
                OnPropertyChanged(nameof(ExtractionCompleteVisibility));
                OnPropertyChanged(nameof(ExtractionProgress));
            }

            void MarkFailure()
            {
                IsNotExtracting = true;
                OnPropertyChanged(nameof(IsNotExtracting));
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(ProgressBarVisibility));
                OnPropertyChanged(nameof(ExtractionCompleteVisibility));
                OnPropertyChanged(nameof(ExtractionProgress));
            }

            Action<float> CreateOnProgressProcessor()
            {
                var lastProgress = 0f;

                return progress =>
                {
                    if (progress == 0)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => MarkStarted());
                    }
                    else if (progress == 1)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => MarkSuccessful());
                    }
                    else
                    {
                        if (0.01f <= progress - lastProgress)
                        {
                            lastProgress = progress;

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                ExtractionProgress = progress;
                                OnPropertyChanged(nameof(ExtractionProgress));
                            });
                        }
                    }
                };
            }

            try
            {
                switch (GameEdition)
                {
                    default:
                        break;

                    case PCSX2:
                    {
                        if (isoLocation != null)
                        {
                            var game = GameService.DetectGameId(isoLocation);
                            if (game != null)
                            {
                                switch (game?.Id)
                                {
                                    case "kh1":
                                        await _gameDataExtractionService.ExtractKh1Ps2EditionAsync(
                                            isoLocation: isoLocation,
                                            gameDataLocation: gameDataLocation,
                                            onProgress: CreateOnProgressProcessor()
                                        );
                                        break;
                                    case "kh2":
                                        await _gameDataExtractionService.ExtractKh2Ps2EditionAsync(
                                            isoLocation: isoLocation,
                                            gameDataLocation: gameDataLocation,
                                            onProgress: CreateOnProgressProcessor()
                                        );
                                        break;
                                    case "Recom":
                                        await _gameDataExtractionService.ExtractRecomPs2EditionAsync(
                                            isoLocation: isoLocation,
                                            gameDataLocation: gameDataLocation,
                                            onProgress: CreateOnProgressProcessor()
                                        );
                                        break;
                                }
                            }
                        }
                        break;
                    }

                    case PC:
                    {
                        var langFolder = ConfigurationService.PCVersion == "Steam" ? "dt" : _pcReleaseLanguage == "jp" ? "jp" : "en";

                        await _gameDataExtractionService.ExtractKhPcEditionAsync(
                            gameDataLocation: gameDataLocation,
                            onProgress: CreateOnProgressProcessor(),
                            getKhFilePath: fileName
                                => Path.Combine(
                                    _pcReleaseLocation,
                                    "Image",
                                    langFolder,
                                    fileName
                                ),
                            getKh3dFilePath: fileName
                                => Path.Combine(
                                    _pcReleaseLocationKH3D,
                                    "Image",
                                    langFolder,
                                    fileName
                                ),
                            extractkh1: ConfigurationService.Extractkh1,
                            extractkh2: ConfigurationService.Extractkh2,
                            extractbbs: ConfigurationService.Extractbbs,
                            extractrecom: ConfigurationService.Extractrecom,
                            extractkh3d: ConfigurationService.Extractkh3d,
                            ifRetry: async ex =>
                            {
                                var delayedResult = new TaskCompletionSource<bool>();

                                await System.Windows.Application.Current.Dispatcher.InvokeAsync(
                                    () =>
                                    {
                                        var selection = MessageBox.Show(ex.Message + "\n\nWould you like to retry again?", "An Exception was Caught!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                                        switch (selection)
                                        {
                                            case MessageBoxResult.Yes:
                                                delayedResult.SetResult(true);
                                                return;
                                            case MessageBoxResult.No:
                                                delayedResult.SetResult(false);
                                                return;
                                            default:
                                                delayedResult.SetException(ex);
                                                return;
                                        }
                                    }
                                )
                                    .Task;

                                return await delayedResult.Task;
                            },
                            cancellationToken: Abort
                        );
                        break;
                    }
                }
            }
            catch
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => MarkFailure());
                throw;
            }
        }

        private CancellationTokenSource _abort = new CancellationTokenSource();
        public CancellationToken Abort => _abort.Token;
        public void SetAborted() => _abort.Cancel();
    }
}
