using OpenKh.Common;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using static OpenKh.Tools.ModsManager.Helpers;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public interface IChangeModEnableState
    {
        void ModEnableStateChanged();
    }

    public class MainViewModel : BaseNotifyPropertyChanged, IChangeModEnableState
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        private static Version _version = Assembly.GetEntryAssembly()?.GetName()?.Version;
        private static string ApplicationName = Utilities.GetApplicationName();
        private static string ApplicationVersion = Utilities.GetApplicationVersion();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private GetActiveWindowService _getActiveWindowService = new GetActiveWindowService();

        private DebuggingWindow _debuggingWindow = new DebuggingWindow();
        private ModViewModel _selectedValue;
        private Pcsx2Injector _pcsx2Injector;
        private Process _runningProcess;
        private bool _isBuilding;
        private bool _pc;
        private bool _pcsx2;
        private bool _panaceaInstalled;
        private bool _panaceaConsoleEnabled;
        private bool _panaceaDebugLogEnabled;
        private bool _panaceaSoundDebugEnabled;
        private bool _panaceaCacheEnabled;
        private bool _panaceaQuickMenuEnabled;
        private bool _devView;
        private bool _autoUpdateMods = false;
        private string _launchGame = "kh2";
        private List<string> _supportedGames = new List<string>()
        {
            "kh2",
            "kh1",
            "bbs",
            "Recom",
            "kh3d"
        };
        private List<string> _supportedPCSX2Games = new List<string>()
        {
            "kh2",
            "kh1",
            "Recom"
        };
        private int _wizardVersionNumber = 1;
        private string[] executable = new string[]
        {
            "KINGDOM HEARTS II FINAL MIX.exe",
            "KINGDOM HEARTS FINAL MIX.exe",
            "KINGDOM HEARTS Birth by Sleep FINAL MIX.exe",
            "KINGDOM HEARTS Re_Chain of Memories.exe",
            "KINGDOM HEARTS Dream Drop Distance.exe"
        };
        private int launchExecutable = 0;

        private const string RAW_FILES_FOLDER_NAME = "raw";
        private const string ORIGINAL_FILES_FOLDER_NAME = "original";
        private const string REMASTERED_FILES_FOLDER_NAME = "remastered";

        public static bool overwriteMod = false;
        public string Title => ApplicationName;
        public string CurrentVersion => ApplicationVersion;
        public ObservableCollection<ModViewModel> ModsList { get; set; }
        public ObservableCollection<string> PresetList { get; set; }
        public RelayCommand ExitCommand { get; set; }
        public RelayCommand AddModCommand { get; set; }
        public RelayCommand RemoveModCommand { get; set; }
        public RelayCommand OpenModFolderCommand { get; set; }
        public RelayCommand MoveTop { get; set; }
        public RelayCommand MoveUp { get; set; }
        public RelayCommand MoveDown { get; set; }
        public RelayCommand BuildCommand { get; set; }
        public RelayCommand PatchCommand { get; set; }
        public RelayCommand RestoreCommand { get; set; }
        public RelayCommand RunCommand { get; set; }
        public RelayCommand BuildAndRunCommand { get; set; }
        public RelayCommand StopRunningInstanceCommand { get; set; }
        public RelayCommand WizardCommand { get; set; }
        public ICommand OpenPresetMenuCommand { get; private set; }
        public ICommand CheckForModUpdatesCommand { get; private set; }
        public ICommand OpenLinkCommand { get; private set; }
        public ICommand CheckOpenkhUpdateCommand { get; private set; }
        public ICommand YamlGeneratorCommand { get; private set; }
        public ICommand OpenModSearchCommand { get; private set; }

        public ModViewModel SelectedValue
        {
            get => _selectedValue;
            set
            {
                _selectedValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsModSelected));
                OnPropertyChanged(nameof(IsModInfoVisible));
                OnPropertyChanged(nameof(IsModUnselectedMessageVisible));
                OnPropertyChanged(nameof(MoveUp));
                OnPropertyChanged(nameof(MoveDown));
                OnPropertyChanged(nameof(AddModCommand));
                OnPropertyChanged(nameof(RemoveModCommand));
                OnPropertyChanged(nameof(OpenModFolderCommand));
            }
        }

        public bool IsModSelected => SelectedValue != null;

        public Visibility IsModInfoVisible => IsModSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsModUnselectedMessageVisible => !IsModSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PatchVisible => PC && !PanaceaInstalled || PC && DevView ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ModLoader => !PC || PanaceaInstalled ? Visibility.Visible : Visibility.Collapsed;
        public Visibility notPC => !PC ? Visibility.Visible : Visibility.Collapsed;
        public Visibility isPC => PC ? Visibility.Visible : Visibility.Collapsed;
        public bool GameSelectInteractable => (PC && Directory.Exists(ConfigurationService.PcReleaseLocation)) || (PCSX2 && MultiEmuGames);
        public Visibility GameSelectVisible => PC || PCSX2 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameSelectKH2 => (PC && Directory.Exists(ConfigurationService.PcReleaseLocation)) || (PCSX2 && ConfigurationService.IsoLocationKH2 != null) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameSelectKH1 => (PC && Directory.Exists(ConfigurationService.PcReleaseLocation)) || (PCSX2 && ConfigurationService.IsoLocationKH1 != null) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameSelectBBS => (PC && Directory.Exists(ConfigurationService.PcReleaseLocation)) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameSelectRecom => (PC && Directory.Exists(ConfigurationService.PcReleaseLocation)) || (PCSX2 && ConfigurationService.IsoLocationRecom != null) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameSelectKH3D => (PC && Directory.Exists(ConfigurationService.PcReleaseLocationKH3D)) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PanaceaSettings => PC && PanaceaInstalled ? Visibility.Visible : Visibility.Collapsed;

        public enum GameIDs {
            KH2 = 0,
            KH1 = 1,
            BBS = 2,
            Recom = 3,
            KH3D = 4,
        };

        public bool MultiEmuGames
        {
            get
            {
                int count = 0;
                if (ConfigurationService.IsoLocationKH2 != null)
                {
                    count++;
                }
                if (ConfigurationService.IsoLocationKH1 != null)
                {
                    count++;
                }
                if (ConfigurationService.IsoLocationRecom != null)
                {
                    count++;
                }
                if (count > 1)
                {
                    return true;
                }
                return false;
            }
        }
        public bool PanaceaConsoleEnabled
        {
            get => _panaceaConsoleEnabled;
            set
            {
                _panaceaConsoleEnabled = value;
                ConfigurationService.ShowConsole = _panaceaConsoleEnabled;
                if (_panaceaDebugLogEnabled)
                    PanaceaDebugLogEnabled = false;
                OnPropertyChanged(nameof(PanaceaConsoleEnabled));
                UpdatePanaceaSettings();
            }
        }
        public bool PanaceaDebugLogEnabled
        {
            get => _panaceaDebugLogEnabled;
            set
            {
                _panaceaDebugLogEnabled = value;
                ConfigurationService.DebugLog = _panaceaDebugLogEnabled;
                if (_panaceaSoundDebugEnabled)
                    PanaceaSoundDebugEnabled = false;
                OnPropertyChanged(nameof(PanaceaDebugLogEnabled));
                UpdatePanaceaSettings();
            }
        }
        public bool PanaceaSoundDebugEnabled
        {
            get => _panaceaSoundDebugEnabled;
            set
            {
                _panaceaSoundDebugEnabled = value;
                ConfigurationService.SoundDebug = _panaceaSoundDebugEnabled;
                OnPropertyChanged(nameof(PanaceaSoundDebugEnabled));
                UpdatePanaceaSettings();
            }

        }
        public bool PanaceaCacheEnabled
        {
            get => _panaceaCacheEnabled;
            set
            {
                _panaceaCacheEnabled = value;
                ConfigurationService.EnableCache = _panaceaCacheEnabled;
                UpdatePanaceaSettings();
            }
        }
        public bool PanaceaQuickMenuEnabled
        {
            get => _panaceaQuickMenuEnabled;
            set
            {
                _panaceaQuickMenuEnabled = value;
                ConfigurationService.QuickMenu = _panaceaQuickMenuEnabled;
                UpdatePanaceaSettings();
            }
        }
        public bool DevView
        {
            get => _devView;
            set
            {
                _devView = value;
                ConfigurationService.DevView = DevView;
                OnPropertyChanged(nameof(PatchVisible));
            }
        }
        public bool AutoUpdateMods
        {
            get => _autoUpdateMods;
            set
            {
                _autoUpdateMods = value;
                ConfigurationService.AutoUpdateMods = _autoUpdateMods;
            }
        }
        public bool PanaceaInstalled
        {
            get => _panaceaInstalled;
            set
            {
                _panaceaInstalled = value;
                OnPropertyChanged(nameof(PatchVisible));
                OnPropertyChanged(nameof(ModLoader));
                OnPropertyChanged(nameof(PanaceaSettings));
            }
        }

        public bool PC
        {
            get => _pc;
            set
            {
                _pc = value;
                OnPropertyChanged(nameof(PC));
                OnPropertyChanged(nameof(ModLoader));
                OnPropertyChanged(nameof(PatchVisible));
                OnPropertyChanged(nameof(notPC));
                OnPropertyChanged(nameof(isPC));
                OnPropertyChanged(nameof(GameSelectVisible));
                OnPropertyChanged(nameof(GameSelectInteractable));
                OnPropertyChanged(nameof(PanaceaSettings));
            }
        }

        public bool PCSX2
        {
            get => _pcsx2;
            set
            {
                _pcsx2 = value;
                OnPropertyChanged(nameof(PCSX2));
                OnPropertyChanged(nameof(ModLoader));
                OnPropertyChanged(nameof(PatchVisible));
                OnPropertyChanged(nameof(notPC));
                OnPropertyChanged(nameof(isPC));
                OnPropertyChanged(nameof(GameSelectVisible));
                OnPropertyChanged(nameof(GameSelectInteractable));
                OnPropertyChanged(nameof(PanaceaSettings));
            }
        }

        public int GametoLaunch
        {
            get
            {
                switch (_launchGame)
                {
                    case "kh2":
                        launchExecutable = (int)GameIDs.KH2;
                        break;
                    case "kh1":
                        launchExecutable = (int)GameIDs.KH1;
                        break;
                    case "bbs":
                        launchExecutable = (int)GameIDs.BBS;
                        break;
                    case "Recom":
                        launchExecutable = (int)GameIDs.Recom;
                        break;
                    case "kh3d":
                        launchExecutable = (int)GameIDs.KH3D;
                        break;
                    default:
                        launchExecutable = (int)GameIDs.KH2;
                        break;
                }
                return launchExecutable;
            }
            set
            {
                launchExecutable = value;
                switch ((GameIDs)value)
                {
                    case GameIDs.KH2:
                        _launchGame = "kh2";
                        break;
                    case GameIDs.KH1:
                        _launchGame = "kh1";
                        break;
                    case GameIDs.BBS:
                        _launchGame = "bbs";
                        break;
                    case GameIDs.Recom:
                        _launchGame = "Recom";
                        break;
                    case GameIDs.KH3D:
                        _launchGame = "kh3d";
                        break;
                    default:
                        _launchGame = "kh2";
                        break;
                }
                ConfigurationService.LaunchGame = _launchGame;
                ReloadModsList();
                if (ModsList.Count > 0)
                    _ = FetchUpdates();
            }
        }

        public bool IsBuilding
        {
            get => _isBuilding;
            set
            {
                _isBuilding = value;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(BuildCommand));
                    OnPropertyChanged(nameof(BuildAndRunCommand));
                });
            }
        }

        public bool IsRunning => _runningProcess != null;

        public MainViewModel()
        {
            if (ConfigurationService.GameEdition == SetupWizardViewModel.PC)
            {
                PC = true;
                PCSX2 = false;
                PanaceaInstalled = ConfigurationService.PanaceaInstalled;
                DevView = ConfigurationService.DevView;
                _panaceaConsoleEnabled = ConfigurationService.ShowConsole;
                _panaceaDebugLogEnabled = ConfigurationService.DebugLog;
                _panaceaSoundDebugEnabled = ConfigurationService.SoundDebug;
                _panaceaCacheEnabled = ConfigurationService.EnableCache;
                _panaceaQuickMenuEnabled = ConfigurationService.QuickMenu;

                if (PanaceaInstalled && ConfigurationService.Updated)
                {
                    try
                    {
                        string PanaceaSourceLocation = Path.Combine(AppContext.BaseDirectory, "OpenKH.Panacea.dll");
                        if (ConfigurationService.PcReleaseLocation != null)
                        {
                            string PanaceaDestinationLocation = Path.Combine(ConfigurationService.PcReleaseLocation, "DBGHELP.dll");
                            string PanaceaAlternateLocation = Path.Combine(ConfigurationService.PcReleaseLocation, "version.dll");
                            if (Process.GetProcessesByName("winlogon").Length > 0)
                            {
                                File.Copy(PanaceaSourceLocation, PanaceaDestinationLocation, true);
                                File.Delete(PanaceaAlternateLocation);
                            }
                            else
                            {
                                File.Copy(PanaceaSourceLocation, PanaceaAlternateLocation, true);
                                File.Delete(PanaceaDestinationLocation);
                            }
                        }
                        if (ConfigurationService.PcReleaseLocationKH3D != null)
                        {
                            string PanaceaDestinationLocationkh3d = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "DBGHELP.dll");
                            string PanaceaAlternateLocationkh3d = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "version.dll");
                            if (Process.GetProcessesByName("winlogon").Length > 0)
                            {
                                File.Copy(PanaceaSourceLocation, PanaceaDestinationLocationkh3d, true);
                                File.Delete(PanaceaAlternateLocationkh3d);
                            }
                            else
                            {
                                File.Copy(PanaceaSourceLocation, PanaceaAlternateLocationkh3d, true);
                                File.Delete(PanaceaDestinationLocationkh3d);
                            }
                        }
                        ConfigurationService.Updated = false;
                    }
                    catch
                    {
                        ConfigurationService.Updated = false;
                        MessageBox.Show(
                               $"Unable to automatically update Panacea.\nPlease manually run the setup wizard and reinstall Panacea.",
                               "Error",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                    }
                }
            }
            else if (ConfigurationService.GameEdition == SetupWizardViewModel.PCSX2)
            {
                PC = false;
                PCSX2 = true;
            }
            else
            {
                PC = false;
                PCSX2 = false;
            }
            if (_supportedGames.Contains(ConfigurationService.LaunchGame) && PC || _supportedPCSX2Games.Contains(ConfigurationService.LaunchGame) && PCSX2)
                _launchGame = ConfigurationService.LaunchGame;
            else
                ConfigurationService.LaunchGame = _launchGame;

            AutoUpdateMods = ConfigurationService.AutoUpdateMods;

            try
            {
                Log.OnLogDispatch += (long ms, string tag, string message) =>
                    _debuggingWindow.Log(ms, tag, message);
            }
            catch
            {
                MessageBox.Show(
                       $"Mods Manager had problems starting. Will now force close any instances of Mods Manager to hopefully allow you to re-open the program.",
                       "Error",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error);
                Process[] runningProcesses = Process.GetProcesses();
                foreach (Process process in runningProcesses)
                {
                    if (process.ProcessName == "OpenKh.Tools.ModsManager" && process.Id != Process.GetCurrentProcess().Id)
                    {
                        process.Kill(true);
                    }
                }
                Process.GetCurrentProcess().Kill();
            }

            ReloadModsList();
            SelectedValue = ModsList.FirstOrDefault();
            ReloadPresetList();

            ExitCommand = new RelayCommand(_ => Window.Close());
            AddModCommand = new RelayCommand(_ =>
            {
                var view = new InstallModView();
                if (view.ShowDialog() != true)
                    return;

                Task.Run(async () =>
                {
                    InstallModProgressWindow progressWindow = null;
                    try
                    {
                        var name = view.RepositoryName;
                        var isZipFile = view.IsZipFile;
                        var isLuaFile = view.IsLuaFile;
                        progressWindow = Application.Current.Dispatcher.Invoke(() =>
                        {
                            var progressWindow = new InstallModProgressWindow
                            {
                                ModName = isZipFile ? Path.GetFileName(name) : name,
                                ProgressText = "Initializing",
                                ShowActivated = true
                            };
                            progressWindow.Show();
                            return progressWindow;
                        });
                        await ModsService.InstallMod(name, isZipFile, isLuaFile, progress =>
                        {
                            Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressText = progress);
                        }, nProgress =>
                        {
                            Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressValue = nProgress);
                        });
                        var actualName = isZipFile || isLuaFile ? Path.GetFileNameWithoutExtension(name) : name;
                        var mod = ModsService.GetMods(new string[] { actualName }).First();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            progressWindow.Close();
                            if (overwriteMod)
                            {
                                var modRemove = ModsList.FirstOrDefault(smod => smod.Title == mod.Metadata.Title);
                                if (modRemove != null)
                                    ModsList.RemoveAt(ModsList.IndexOf(modRemove));
                                overwriteMod = false;
                            }
                            ModsList.Insert(0, Map(mod));
                            SelectedValue = ModsList[0];
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("Unable to install the mod `{0}`: {1}\n"
                            , view.RepositoryName
                            , Log.FormatSecondaryLinesWithIndent(ex.ToString(), "  ")
                        );
                        Handle(ex);
                    }
                    finally
                    {
                        Application.Current.Dispatcher.Invoke(() => progressWindow?.Close());
                    }
                });
            }, _ => true);
            RemoveModCommand = new RelayCommand(_ =>
            {
                var mod = SelectedValue;
                if (Question($"Do you want to delete the mod '{mod.Source}'?", $"Remove mod {mod.Source}"))
                {
                    Handle(() =>
                    {
                        ModsService.CleanModFiles(mod.Path);
                        ModsList.RemoveAt(ModsList.IndexOf(SelectedValue));
                    });
                }
            }, _ => IsModSelected);
            OpenModFolderCommand = new RelayCommand(_ =>
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = SelectedValue.Path,
                    UseShellExecute = true
                });
            }, _ => IsModSelected);
            MoveTop = new RelayCommand(_ => MoveSelectedModTop(), _ => CanSelectedModMoveUp());
            MoveUp = new RelayCommand(_ => MoveSelectedModUp(), _ => CanSelectedModMoveUp());
            MoveDown = new RelayCommand(_ => MoveSelectedModDown(), _ => CanSelectedModMoveDown());
            BuildCommand = new RelayCommand(async _ =>
            {
                ResetLogWindow();
                await BuildPatches(false);
                CloseAllWindows();
            }, _ => !IsBuilding);

            PatchCommand = new RelayCommand(async (fastMode) =>
            {
                try
                {
                    ResetLogWindow();
                    await BuildPatches(Convert.ToBoolean(fastMode));
                    await PatchGame(Convert.ToBoolean(fastMode));
                    CloseAllWindows();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }, _ => !IsBuilding);

            RunCommand = new RelayCommand(async _ =>
            {
                CloseRunningProcess();
                ResetLogWindow();
                await RunGame();
            });

            RestoreCommand = new RelayCommand(async (patched) =>
            {
                ResetLogWindow();
                await RestoreGame(Convert.ToBoolean(patched));
                CloseAllWindows();
            });
            BuildAndRunCommand = new RelayCommand(async _ =>
            {
                CloseRunningProcess();
                ResetLogWindow();
                if (await BuildPatches(false))
                    await RunGame();
            }, _ => !IsBuilding);
            StopRunningInstanceCommand = new RelayCommand(_ =>
            {
                CloseRunningProcess();
                ResetLogWindow();
            }, _ => IsRunning);
            WizardCommand = new RelayCommand(_ =>
            {
                var dialog = new SetupWizardWindow();
                if (dialog.ShowDialog() != null)
                {
                    if (ConfigurationService.GameEdition == SetupWizardViewModel.PC)
                    {
                        PC = true;
                        PCSX2 = false;
                        PanaceaInstalled = ConfigurationService.PanaceaInstalled;
                        if (!Directory.Exists(ConfigurationService.PcReleaseLocation))
                        {
                            if (Directory.Exists(ConfigurationService.PcReleaseLocationKH3D))
                            {
                                GametoLaunch = (int)GameIDs.KH3D;
                            }
                            else
                            {
                                MessageBox.Show(
                                "Unable to locate install locations for both KINGDOM HEARTS HD 1.5+2.5 ReMIX and KINGDOM HEARTS HD 2.8 Final Chapter Prologue. They are either missing or corrupted. Please re-run the setup wizard and confirm the install paths are correct.",
                                "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else if (ConfigurationService.GameEdition == SetupWizardViewModel.PCSX2)
                    {
                        PC = false;
                        PCSX2 = true;
                        string? launchIso;
                        switch (_launchGame)
                        {
                            case "kh2":
                                launchIso = ConfigurationService.IsoLocationKH2;
                                break;
                            case "kh1":
                                launchIso = ConfigurationService.IsoLocationKH1;
                                break;
                            case "Recom":
                                launchIso = ConfigurationService.IsoLocationRecom;
                                break;
                            default:
                                launchIso = null;
                                break;
                        }
                        GameInfoModel? game;
                        if (launchIso != null)
                        {
                            game = GameService.DetectGameId(launchIso);
                        }
                        else
                        {
                            game = null;
                        }
                        if (!_supportedPCSX2Games.Contains(_launchGame) || launchIso == null || game == null || game?.Id != _launchGame)
                        {
                            if (ConfigurationService.IsoLocationKH2 != null && (game = GameService.DetectGameId(ConfigurationService.IsoLocationKH2)) != null && game?.Id == "kh2")
                            {
                                GametoLaunch = (int)GameIDs.KH2;
                            }
                            else if (ConfigurationService.IsoLocationKH1 != null && (game = GameService.DetectGameId(ConfigurationService.IsoLocationKH1)) != null && game?.Id == "kh1")
                            {
                                GametoLaunch = (int)GameIDs.KH1;
                            }
                            else if (ConfigurationService.IsoLocationRecom != null && (game = GameService.DetectGameId(ConfigurationService.IsoLocationRecom)) != null && game?.Id == "Recom")
                            {
                                GametoLaunch = (int)GameIDs.Recom;
                            }
                            else
                            {
                                GametoLaunch = (int)GameIDs.KH2;
                            }
                        }
                    }
                    else
                    {
                        PC = false;
                        PCSX2 = false;
                        GametoLaunch = (int)GameIDs.KH2;
                    }
                    ConfigurationService.WizardVersionNumber = _wizardVersionNumber;
                }
                OnPropertyChanged(nameof(GametoLaunch));
                OnPropertyChanged(nameof(GameSelectKH2));
                OnPropertyChanged(nameof(GameSelectKH1));
                OnPropertyChanged(nameof(GameSelectBBS));
                OnPropertyChanged(nameof(GameSelectRecom));
                OnPropertyChanged(nameof(GameSelectKH3D));
            });

            OpenPresetMenuCommand = new RelayCommand(_ =>
            {
                PresetsWindow view = new PresetsWindow(this);
                view.ShowDialog();
            });

            CheckForModUpdatesCommand = new RelayCommand(_ =>
            {
                _ = FetchUpdates();
            });

            OpenLinkCommand = new RelayCommand(url => Process.Start(new ProcessStartInfo(url as string)
            {
                UseShellExecute = true
            }));

            CheckOpenkhUpdateCommand = new RelayCommand(
                _ => _ = UpdateOpenkhAsync()
            );

            YamlGeneratorCommand = new RelayCommand(
                _ =>
                {
                    var window = new YamlGeneratorWindow()
                    {
                        Owner = _getActiveWindowService.GetActiveWindow(),
                    };
                    window.Closing += (_, e) =>
                    {
                        if (!e.Cancel)
                        {
                            window.Owner?.Focus();
                        }
                    };
                    window.Show();
                }
            );

            OpenModSearchCommand = new RelayCommand(
                _ =>
                {
                    var searchWindow = new ModSearchWindow(this)
                    {
                        Owner = _getActiveWindowService.GetActiveWindow(),
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    searchWindow.Closing += (_, e) =>
                    {
                        if (!e.Cancel)
                        {
                            searchWindow.Owner?.Focus();
                            // Reload mods list to show newly installed mods
                            ReloadModsList();
                        }
                    };
                    searchWindow.Show();
                }
            );

            _pcsx2Injector = new Pcsx2Injector(new OperationDispatcher());
            _ = FetchUpdates();

            if (ConfigurationService.WizardVersionNumber < _wizardVersionNumber)
                WizardCommand.Execute(null);
        }

        public void CloseAllWindows()
        {
            CloseRunningProcess();
            Application.Current.Dispatcher.Invoke(_debuggingWindow.Close);
        }

        public void CloseRunningProcess()
        {
            if (_runningProcess == null)
                return;

            _pcsx2Injector.Stop();
            _runningProcess.CloseMainWindow();
            _runningProcess.Kill();
            _runningProcess.Dispose();
            _runningProcess = null;
            OnPropertyChanged(nameof(StopRunningInstanceCommand));
        }

        private void ResetLogWindow()
        {
            if (_debuggingWindow != null)
                Application.Current.Dispatcher.Invoke(_debuggingWindow.Close);
            _debuggingWindow = new DebuggingWindow();
            Application.Current.Dispatcher.Invoke(_debuggingWindow.Show);
            _debuggingWindow.ClearLogs();
        }

        private async Task<bool> BuildPatches(bool fastMode)
        {
            IsBuilding = true;
            var result = await ModsService.RunPatcherAsync(fastMode);
            IsBuilding = false;

            return result;
        }

        private Task RunGame()
        {
            ProcessStartInfo processStartInfo;
            bool isPcsx2 = false;
            switch (ConfigurationService.GameEdition)
            {
                case SetupWizardViewModel.OpenKHGameEngine:
                    Log.Info("Starting OpenKH Game Engine");
                    processStartInfo = new ProcessStartInfo
                    {
                        FileName = ConfigurationService.OpenKhGameEngineLocation,
                        WorkingDirectory = Path.GetDirectoryName(ConfigurationService.OpenKhGameEngineLocation),
                        Arguments = $"--data \"{ConfigurationService.GameDataLocation}\" --modpath \"{ConfigurationService.GameModPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                    };
                    break;
                case SetupWizardViewModel.PCSX2:
                    Log.Info("Starting PCSX2");
                    _pcsx2Injector.RegionId = ConfigurationService.RegionId;
                    _pcsx2Injector.Region = Kh2.Constants.Regions[_pcsx2Injector.RegionId];
                    _pcsx2Injector.Language = Kh2.Constants.Languages[_pcsx2Injector.RegionId];

                    String IsoLocation;
                    switch (_launchGame)
                    {
                        case "kh2":
                            IsoLocation = ConfigurationService.IsoLocationKH2;
                            break;
                        case "kh1":
                            IsoLocation = ConfigurationService.IsoLocationKH1;
                            break;
                        case "Recom":
                            IsoLocation = ConfigurationService.IsoLocationRecom;
                            break;
                        default:
                            IsoLocation = ConfigurationService.IsoLocationKH2;
                            break;
                    }

                    if (IsoLocation != null)
                    {
                        processStartInfo = new ProcessStartInfo
                        {
                            FileName = ConfigurationService.Pcsx2Location,
                            WorkingDirectory = Path.GetDirectoryName(ConfigurationService.Pcsx2Location),
                            Arguments = $"\"{IsoLocation}\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                        };
                        isPcsx2 = true;
                    }
                    else
                    {
                        processStartInfo = null;
                    }
                    break;
                case SetupWizardViewModel.PC:
                    if (ConfigurationService.PCVersion == "EGS" && !(_launchGame == "kh3d"))
                    {
                        if (Directory.Exists(ConfigurationService.PcReleaseLocation))
                        {
                            if (ConfigurationService.PanaceaInstalled)
                            {
                                string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt");
                                if (!File.Exists(panaceaSettings))
                                {
                                    File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt"),
                                    [
                                    $"mod_path={Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath,".."))}",
                                    $"show_console={false}",
                                    ]);
                                }
                                File.AppendAllText(panaceaSettings, "\nquick_launch=" + _launchGame);
                            }
                            processStartInfo = new ProcessStartInfo
                            {
                                FileName = "com.epicgames.launcher://apps/4158b699dd70447a981fee752d970a3e%3A5aac304f0e8948268ddfd404334dbdc7%3A68c214c58f694ae88c2dab6f209b43e4?action=launch&silent=true",
                                UseShellExecute = true,
                            };
                            Process.Start(processStartInfo);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                        else
                        {
                            MessageBox.Show(
                            "Unable to locate KINGDOM HEARTS HD 1.5+2.5 ReMIX install. Please re-run the setup wizard and confirm it is correct.",
                            "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                    }
                    else if (ConfigurationService.PCVersion == "EGS" && _launchGame == "kh3d")
                    {
                        if (Directory.Exists(ConfigurationService.PcReleaseLocationKH3D))
                        {
                            if (ConfigurationService.PanaceaInstalled)
                            {
                                string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt");
                                if (!File.Exists(panaceaSettings))
                                {
                                        File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt"),
                                        [
                                        $"mod_path={Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath,".."))}",
                                        $"show_console={false}",
                                        ]);
                                }
                                File.AppendAllText(panaceaSettings, "\nquick_launch=" + _launchGame);
                            }
                            processStartInfo = new ProcessStartInfo
                            {
                                FileName = "com.epicgames.launcher://apps/c8ff067c1c984cd7ab1998e8a9afc8b6%3Aaa743b9f52e84930b0ba1b701951e927%3Ad1a8f7c478d4439b8c60a5808715dc05?action=launch&silent=true",
                                UseShellExecute = true,
                            };
                            Process.Start(processStartInfo);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                        else
                        {
                            MessageBox.Show(
                            "Unable to locate KINGDOM HEARTS HD 2.8 Final Chapter Prologue install. Please re-run the setup wizard and confirm it is correct.",
                            "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                    }
                    if (ConfigurationService.PCVersion == "Steam" && !(_launchGame == "kh3d") && ConfigurationService.SteamAPITrick1525 == false)
                    {
                        if (Directory.Exists(ConfigurationService.PcReleaseLocation))
                        {
                            if (ConfigurationService.PanaceaInstalled)
                            {
                                string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt");
                                if (!File.Exists(panaceaSettings))
                                {
                                    File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt"),
                                    [
                                    $"mod_path={Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath,".."))}",
                                    $"show_console={false}",
                                    ]);
                                }
                                File.AppendAllText(panaceaSettings, "\nquick_launch=" + _launchGame);
                            }
                            processStartInfo = new ProcessStartInfo
                            {
                                FileName = "steam://rungameid/2552430",
                                UseShellExecute = true,
                            };
                            Process.Start(processStartInfo);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                        else
                        {
                            MessageBox.Show(
                            "Unable to locate KINGDOM HEARTS HD 1.5+2.5 ReMIX install. Please re-run the setup wizard and confirm it is correct.",
                            "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                    }
                    else if (ConfigurationService.PCVersion == "Steam" && _launchGame == "kh3d" && ConfigurationService.SteamAPITrick28 == false)
                    {
                        if (Directory.Exists(ConfigurationService.PcReleaseLocationKH3D))
                        {
                            string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt");
                            if (ConfigurationService.PanaceaInstalled)
                            {
                                if (!File.Exists(panaceaSettings))
                                {
                                    File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt"),
                                    [
                                    $"mod_path={Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath,".."))}",
                                    $"show_console={false}",
                                    ]);
                                }
                                File.AppendAllText(panaceaSettings, "\nquick_launch=" + _launchGame);
                            }
                            processStartInfo = new ProcessStartInfo
                            {
                                FileName = "steam://rungameid/2552440",
                                UseShellExecute = true,
                            };
                            Process.Start(processStartInfo);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                        else
                        {
                            MessageBox.Show(
                            "Unable to locate KINGDOM HEARTS HD 2.8 Final Chapter Prologue install. Please re-run the setup wizard and confirm it is correct.",
                            "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                    }
                    else
                    {
                        string filename = "";

                        try
                        {
                            if (!(_launchGame == "kh3d"))
                            {
                                if (Directory.Exists(ConfigurationService.PcReleaseLocation))
                                {
                                    string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt");
                                    if (ConfigurationService.PanaceaInstalled && !File.Exists(panaceaSettings))
                                    {
                                        File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt"),
                                        [
                                        $"mod_path={Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath,".."))}",
                                        $"show_console={false}",
                                        ]);
                                    }
                                    filename = Path.Combine(ConfigurationService.PcReleaseLocation, executable[launchExecutable]);
                                }
                                else
                                {
                                    MessageBox.Show(
                                    "Unable to locate KINGDOM HEARTS HD 1.5+2.5 ReMIX install. Please re-run the setup wizard and confirm it is correct.",
                                    "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    CloseAllWindows();
                                    return Task.CompletedTask;
                                }
                            }
                            else
                            {
                                if (Directory.Exists(ConfigurationService.PcReleaseLocationKH3D))
                                {
                                    string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt");
                                    if (ConfigurationService.PanaceaInstalled && !File.Exists(panaceaSettings))
                                    {
                                        File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt"),
                                        [
                                        $"mod_path={Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath,".."))}",
                                        $"show_console={false}",
                                        ]);
                                    }
                                    filename = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, executable[launchExecutable]);
                                }
                                else
                                {
                                    MessageBox.Show(
                                    "Unable to locate KINGDOM HEARTS HD 2.8 Final Chapter Prologue install. Please re-run the setup wizard and confirm it is correct.",
                                    "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    CloseAllWindows();
                                    return Task.CompletedTask;
                                }
                            }
                            processStartInfo = new ProcessStartInfo
                            {
                                FileName = filename,
                                WorkingDirectory = Path.GetDirectoryName(filename),
                                UseShellExecute = false,
                            };
                            Process.Start(processStartInfo);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("Unable to locate the game executable `{0}`: {1}\n"
                                , filename
                                , Log.FormatSecondaryLinesWithIndent(ex.ToString(), "  ")
                            );

                            MessageBox.Show(
                           "Unable to locate game executable. Please make sure your Kingdom Hearts executable is correctly named and in the correct folder.",
                           "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                            CloseAllWindows();
                            return Task.CompletedTask;
                        }
                    }
                default:
                    return Task.CompletedTask;
            }

            if (processStartInfo == null || !File.Exists(processStartInfo.FileName))
            {
                MessageBox.Show(
                    "Unable to locate the executable. Please run the Wizard by going to the Settings menu.",
                    "Run error", MessageBoxButton.OK, MessageBoxImage.Error);
                CloseAllWindows();
                return Task.CompletedTask;
            }

            _runningProcess = new Process() { StartInfo = processStartInfo };
            _runningProcess.OutputDataReceived += (sender, e) => CaptureLog(e.Data);
            _runningProcess.ErrorDataReceived += (sender, e) => CaptureLog(e.Data);
            _runningProcess.Start();
            _runningProcess.BeginOutputReadLine();
            _runningProcess.BeginErrorReadLine();
            if (isPcsx2)
                _pcsx2Injector.Run(_runningProcess, _debuggingWindow);

            OnPropertyChanged(nameof(StopRunningInstanceCommand));
            return Task.Run(() =>
            {
                _runningProcess.WaitForExit();
                CloseAllWindows();
            });
        }

        private void CaptureLog(string data)
        {
            if (data == null)
                return;
            else if (data.Contains("err", StringComparison.InvariantCultureIgnoreCase))
                Log.Err(data);
            else if (data.Contains("wrn", StringComparison.InvariantCultureIgnoreCase))
                Log.Warn(data);
            else if (data.Contains("warn", StringComparison.InvariantCultureIgnoreCase))
                Log.Warn(data);
            else
                Log.Info(data);
        }

        public void ReloadModsList()
        {
            ModsList = new ObservableCollection<ModViewModel>(
                ModsService.GetMods(ModsService.Mods).Select(Map));
            OnPropertyChanged(nameof(ModsList));
            OnPropertyChanged(nameof(ModViewModel.CollectionModsList));
        }

        private ModViewModel Map(ModModel mod) => new (mod, this);

        public void ModEnableStateChanged()
        {
            ConfigurationService.EnabledMods = ModsList
                .Where(x => x.Enabled)
                .Select(x => x.Source)
                .ToList();
            OnPropertyChanged(nameof(BuildAndRunCommand));
        }

        private void MoveSelectedModDown()
        {
            var selectedIndex = ModsList.IndexOf(SelectedValue);
            if (selectedIndex < 0)
                return;

            var item = ModsList[selectedIndex];
            ModsList.RemoveAt(selectedIndex);
            ModsList.Insert(++selectedIndex, item);
            SelectedValue = ModsList[selectedIndex];
            ModEnableStateChanged();
        }

        private void MoveSelectedModUp()
        {
            var selectedIndex = ModsList.IndexOf(SelectedValue);
            if (selectedIndex < 0)
                return;

            var item = ModsList[selectedIndex];
            ModsList.RemoveAt(selectedIndex);
            ModsList.Insert(--selectedIndex, item);
            SelectedValue = ModsList[selectedIndex];
            ModEnableStateChanged();
        }
        private void MoveSelectedModTop()
        {
            var selectedIndex = ModsList.IndexOf(SelectedValue);
            if (selectedIndex < 0)
                return;

            var item = ModsList[selectedIndex];
            ModsList.RemoveAt(selectedIndex);
            ModsList.Insert(selectedIndex = 0, item);
            SelectedValue = ModsList[selectedIndex];
            ModEnableStateChanged();
        }

        private async Task PatchGame(bool fastMode)
        {
            await Task.Run(() =>
            {
                if (ConfigurationService.GameEdition == SetupWizardViewModel.PC)
                {
                    // Use the package map file to rearrange the files in the structure needed by the patcher
                    var packageMapLocation = Path.Combine(ConfigurationService.GameModPath, "patch-package-map.txt");
                    var packageMap = File
                        .ReadLines(packageMapLocation)
                        .Select(line => line.Split(" $$$$ "))
                        .ToDictionary(array => array[0], array => array[1]);

                    var patchStagingDir = Path.Combine(ConfigurationService.GameModPath, "patch-staging");
                    if (Directory.Exists(patchStagingDir))
                        Directory.Delete(patchStagingDir, true);
                    Directory.CreateDirectory(patchStagingDir);
                    foreach (var entry in packageMap)
                    {
                        var sourceFile = Path.Combine(ConfigurationService.GameModPath, entry.Key);
                        var destFile = Path.Combine(patchStagingDir, entry.Value);
                        if (File.Exists(sourceFile))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                            File.Move(sourceFile, destFile);
                        }
                    }

                    foreach (var directory in Directory.GetDirectories(Path.Combine(ConfigurationService.GameModPath)))
                        if (!"patch-staging".Equals(Path.GetFileName(directory)))
                            Directory.Delete(directory, true);

                    var stagingDirs = Directory.GetDirectories(patchStagingDir).Select(directory => Path.GetFileName(directory)).ToHashSet();

                    string[] specialDirs = Array.Empty<string>();
                    var specialStagingDir = Path.Combine(patchStagingDir, "special");
                    if (Directory.Exists(specialStagingDir))
                        specialDirs = Directory.GetDirectories(specialStagingDir).Select(directory => Path.GetFileName(directory)).ToArray();

                    foreach (var packageName in stagingDirs)
                        Directory.Move(Path.Combine(patchStagingDir, packageName), Path.Combine(ConfigurationService.GameModPath, packageName));
                    foreach (var specialDir in specialDirs)
                        Directory.Move(Path.Combine(ConfigurationService.GameModPath, "special", specialDir), Path.Combine(ConfigurationService.GameModPath, specialDir));

                    stagingDirs.Remove("special"); // Since it's not actually a real game package
                    Directory.Delete(patchStagingDir, true);

                    var specialModDir = Path.Combine(ConfigurationService.GameModPath, "special");
                    if (Directory.Exists(specialModDir))
                        Directory.Delete(specialModDir, true);

                    foreach (var directory in stagingDirs.Select(packageDir => Path.Combine(ConfigurationService.GameModPath, packageDir)))
                    {
                        if (specialDirs.Contains(Path.GetDirectoryName(directory)))
                            continue;

                        var patchFiles = new List<string>();
                        var _dirPart = new DirectoryInfo(directory).Name;

                        var _orgPath = Path.Combine(directory, ORIGINAL_FILES_FOLDER_NAME);
                        var _rawPath = Path.Combine(directory, RAW_FILES_FOLDER_NAME);

                        if (Directory.Exists(_orgPath))
                            patchFiles = OpenKh.Egs.Helpers.GetAllFiles(_orgPath).ToList();

                        if (Directory.Exists(_rawPath))
                            patchFiles.AddRange(OpenKh.Egs.Helpers.GetAllFiles(_rawPath).ToList());

                        string _pkgSoft;
                        switch (_launchGame)
                        {
                            case "kh1":
                                _pkgSoft = fastMode ? "kh1_first" : _dirPart;
                                break;
                            case "bbs":
                                _pkgSoft = fastMode ? "bbs_first" : _dirPart;
                                break;
                            case "Recom":
                                _pkgSoft = "Recom";
                                break;
                            case "kh3d":
                                _pkgSoft = fastMode ? "kh3d_first" : _dirPart;
                                break;
                            default:
                                _pkgSoft = fastMode ? "kh2_first" : _dirPart;
                                break;

                        }
                        string _pkgName = null;
                        string _backupDir = null;
                        if (_launchGame != "kh3d" && ConfigurationService.PcReleaseLocation != null)
                        {
                            _pkgName = Path.Combine(ConfigurationService.PcReleaseLocation, "Image", ConfigurationService.PCVersion == "Steam" && ConfigurationService.PcReleaseLanguage == "en" ? "dt" : ConfigurationService.PcReleaseLanguage, _pkgSoft + ".pkg");
                            _backupDir = Path.Combine(ConfigurationService.PcReleaseLocation, "BackupImage");
                        }
                        else if (ConfigurationService.PcReleaseLocationKH3D != null)
                        {
                            _pkgName = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "Image", ConfigurationService.PCVersion == "Steam" && ConfigurationService.PcReleaseLanguage == "en" ? "dt" : ConfigurationService.PcReleaseLanguage, _pkgSoft + ".pkg");
                            _backupDir = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "BackupImage");
                        }
                        else
                        {
                            Log.Warn("Game Location for selected game cannot be found! Re-run th setup wizard to confirm the path is correct and/or confirm launchgame in the top right is correct.");
                            break;
                        }

                        if (!Directory.Exists(_backupDir))
                            Directory.CreateDirectory(_backupDir);

                        var outputDir = "patchedpkgs";
                        var hedFile = Path.ChangeExtension(_pkgName, "hed");

                        if (!File.Exists(_backupDir + "/" + _pkgSoft + ".pkg"))
                        {
                            Log.Info($"Backing Up Package File {_pkgSoft}");

                            File.Copy(_pkgName, _backupDir + "/" + _pkgSoft + ".pkg");
                            File.Copy(hedFile, _backupDir + "/" + _pkgSoft + ".hed");
                        }

                        else
                        {
                            Log.Info($"Restoring Package File {_pkgSoft}");

                            File.Delete(hedFile);
                            File.Delete(_pkgName);

                            File.Copy(_backupDir + "/" + _pkgSoft + ".pkg", _pkgName);
                            File.Copy(_backupDir + "/" + _pkgSoft + ".hed", hedFile);
                        }

                        using var hedStream = File.OpenRead(hedFile);
                        using var pkgStream = File.OpenRead(_pkgName);
                        var hedHeaders = OpenKh.Egs.Hed.Read(hedStream).ToList();

                        if (!Directory.Exists(outputDir))
                            Directory.CreateDirectory(outputDir);

                        using var patchedHedStream = File.Create(Path.Combine(outputDir, Path.GetFileName(hedFile)));
                        using var patchedPkgStream = File.Create(Path.Combine(outputDir, Path.GetFileName(_pkgName)));

                        foreach (var hedHeader in hedHeaders)
                        {
                            var hash = OpenKh.Egs.Helpers.ToString(hedHeader.MD5);

                            // We don't know this filename, we ignore it
                            if (!OpenKh.Egs.EgsTools.Names.TryGetValue(hash, out var filename))
                                continue;

                            var asset = new OpenKh.Egs.EgsHdAsset(pkgStream.SetPosition(hedHeader.Offset));

                            if (patchFiles.Contains(filename))
                            {
                                patchFiles.Remove(filename);

                                if (hedHeader.DataLength > 0)
                                {
                                    OpenKh.Egs.EgsTools.ReplaceFile(directory, filename, patchedHedStream, patchedPkgStream, asset, hedHeader);
                                    Log.Info($"Replacing File {filename} in {_pkgSoft}");
                                }
                            }

                            else
                            {
                                OpenKh.Egs.EgsTools.ReplaceFile(directory, filename, patchedHedStream, patchedPkgStream, asset, hedHeader);
                                Log.Info($"Skipped File {filename} in {_pkgSoft}");
                            }
                        }

                        // Add all files that are not in the original HED file and inject them in the PKG stream too
                        foreach (var filename in patchFiles)
                        {
                            OpenKh.Egs.EgsTools.AddFile(directory, filename, patchedHedStream, patchedPkgStream);
                            Log.Info($"Adding File {filename} to {_pkgSoft}");
                        }

                        hedStream.Close();
                        pkgStream.Close();

                        patchedHedStream.Close();
                        patchedPkgStream.Close();

                        File.Delete(hedFile);
                        File.Delete(_pkgName);

                        File.Move(Path.Combine(outputDir, Path.GetFileName(hedFile)), hedFile);
                        File.Move(Path.Combine(outputDir, Path.GetFileName(_pkgName)), _pkgName);
                    }
                }
            });
        }

        private async Task RestoreGame(bool patched)
        {
            await Task.Run(() =>
            {
                if (ConfigurationService.GameEdition == SetupWizardViewModel.PC)
                {
                    if (patched && _launchGame != "kh3d")
                    {
                        if (ConfigurationService.PcReleaseLocation == null || !Directory.Exists(Path.Combine(ConfigurationService.PcReleaseLocation, "BackupImage")))
                        {
                            Log.Warn("backup folder cannot be found! Cannot restore the game.");
                        }
                        else
                        {
                            foreach (var file in Directory.GetFiles(Path.Combine(ConfigurationService.PcReleaseLocation, "BackupImage")).Where(x => x.Contains(".pkg") && (x.Contains(_launchGame))))
                            {
                                Log.Info($"Restoring Package File {file.Replace(".pkg", "")}");

                                var _fileBare = Path.GetFileName(file);
                                var _trueName = Path.Combine(ConfigurationService.PcReleaseLocation, "Image", ConfigurationService.PCVersion == "Steam" && ConfigurationService.PcReleaseLanguage == "en" ? "dt" : ConfigurationService.PcReleaseLanguage, _fileBare);

                                File.Delete(Path.ChangeExtension(_trueName, "hed"));
                                File.Delete(_trueName);

                                File.Copy(file, _trueName);
                                File.Copy(Path.ChangeExtension(file, "hed"), Path.ChangeExtension(_trueName, "hed"));
                            }
                        }

                    }
                    else if (patched)
                    {
                        if (ConfigurationService.PcReleaseLocationKH3D == null || !Directory.Exists(Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "BackupImage")))
                        {
                            Log.Warn("backup folder cannot be found! Cannot restore the game.");
                        }
                        else
                        {
                            foreach (var file in Directory.GetFiles(Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "BackupImage")).Where(x => x.Contains(".pkg") && (x.Contains(_launchGame))))
                            {
                                Log.Info($"Restoring Package File {file.Replace(".pkg", "")}");

                                var _fileBare = Path.GetFileName(file);
                                var _trueName = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "Image", ConfigurationService.PCVersion == "Steam" && ConfigurationService.PcReleaseLanguage == "en" ? "dt" : ConfigurationService.PcReleaseLanguage, _fileBare);

                                File.Delete(Path.ChangeExtension(_trueName, "hed"));
                                File.Delete(_trueName);

                                File.Copy(file, _trueName);
                                File.Copy(Path.ChangeExtension(file, "hed"), Path.ChangeExtension(_trueName, "hed"));
                            }
                        }

                    }
                    if (Directory.Exists(ConfigurationService.GameModPath))
                    {
                        try
                        {
                            Directory.Delete(ConfigurationService.GameModPath, true);
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("Unable to fully clean the mod directory:\n{0}", ex.Message);
                        }
                    }
                }
            });
        }

        private bool CanSelectedModMoveDown() =>
            SelectedValue != null && ModsList.IndexOf(SelectedValue) < ModsList.Count - 1;

        private bool CanSelectedModMoveUp() =>
            SelectedValue != null && ModsList.IndexOf(SelectedValue) > 0;

        private async Task FetchUpdates()
        {
            await Task.Delay(50); // fixes a bug where the UI wanted to refresh too soon
            await foreach (var modUpdate in ModsService.FetchUpdates())
            {
                var mod = ModsList.FirstOrDefault(x => x.Source == modUpdate.Name);
                if (mod == null)
                    continue;

                Application.Current.Dispatcher.Invoke(() =>
                    mod.UpdateCount = modUpdate.UpdateCount);
            }
            if (AutoUpdateMods)
            {
                foreach (var mod in ModsList)
                {
                    if (mod.UpdateCount > 0)
                        await ModsService.Update(mod.Source);
                }
                ReloadModsList();
            }
        }

        private async Task UpdateOpenkhAsync()
        {
            var progressWindowService = new ProgressWindowService();

            var checkResult = await progressWindowService.ShowAsync(
                async monitor =>
                {
                    monitor.SetTitle("Checking update from github.com");
                    var result = await new OpenkhUpdateCheckerService().CheckAsync(monitor.Cancellation);
                    monitor.Cancellation.ThrowIfCancellationRequested();
                    return result;
                }
            );
            if (checkResult.HasUpdate)
            {
                var message = "A new version of OpenKh has been detected!\n" +
                    $"[Current: {checkResult.CurrentVersion}, Latest: {checkResult.NewVersion}]\n\n" +
                    "Do you wish to update the game?";

                if (MessageBox.Show(message, "OpenKh", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await progressWindowService.ShowAsync(
                        async monitor =>
                        {
                            monitor.SetTitle("Updating");

                            await new OpenkhUpdateProceederService().UpdateAsync(
                                checkResult.DownloadZipUrl,
                                rate => monitor.SetProgress(rate),
                                monitor.Cancellation
                            );
                        }
                    );
                    ConfigurationService.Updated = true;
                    // quit app
                    Window?.Close();
                }
            }
            else
            {
                var message = $"The latest version '{checkResult.CurrentVersion}' is already installed!";

                MessageBox.Show(message, "OpenKh");
            }
        }

        public void UpdatePanaceaSettings()
        {
            if (PanaceaInstalled)
            {
                string configTargetPath;

                if (_launchGame != "kh3d" && ConfigurationService.PcReleaseLocation != null)
                    configTargetPath = ConfigurationService.PcReleaseLocation;
                else if (ConfigurationService.PcReleaseLocationKH3D != null)
                    configTargetPath = ConfigurationService.PcReleaseLocationKH3D;
                else
                {
                    Log.Warn("Unable to update Panacea settings! Game installation directory is not configured or is invalid.");

                    return;
                }

                string panaceaSettings = Path.Combine(configTargetPath, "panacea_settings.txt");
                string[] lines = File.Exists(panaceaSettings) ? File.ReadAllLines(panaceaSettings) : Array.Empty<string>();
                string textToWrite = $"mod_path={Path.GetFullPath(Path.Combine(ConfigurationService.GameModPath,".."))}\r\n";
                foreach (string entry in lines)
                {
                    if (entry.Contains("dev_path"))
                    {
                        textToWrite += entry;
                        break;
                    }
                }
                textToWrite += $"\r\nshow_console={_panaceaConsoleEnabled}\r\n" +
                    $"debug_log={_panaceaDebugLogEnabled}\r\nsound_debug={_panaceaSoundDebugEnabled}\r\n" +
                    $"enable_cache={_panaceaCacheEnabled}\r\nquick_menu={_panaceaQuickMenuEnabled}";
                File.WriteAllText(panaceaSettings, textToWrite);
            }
        }

        // PRESETS
        public void SavePreset(string presetName)
        {
            string name = string.Join("+", presetName.Split(Path.GetInvalidFileNameChars()));
            List<string> enabledMods = ModsList
            .Where(x => x.Enabled)
            .Select(x => x.Source)
            .ToList();
            File.WriteAllLines(Path.Combine(ConfigurationService.PresetPath, name + ".txt"), enabledMods);
            if (!PresetList.Contains(name))
            {
                PresetList.Add(name);
            }
        }
        public void RemovePreset(string presetName)
        {
            File.Delete(Path.Combine(ConfigurationService.PresetPath, presetName + ".txt"));
            PresetList.Remove(presetName);
        }

        public void LoadPreset(string presetName)
        {
            string filename = Path.Combine(ConfigurationService.PresetPath, presetName + ".txt");
            if (File.Exists(filename))
            {
                ConfigurationService.EnabledMods = File.ReadAllLines(filename);
                ReloadModsList();
                if (ModsList.Count > 0)
                    _ = FetchUpdates();
            }
            else
            {
                MessageBox.Show("Cannot find preset", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void ReloadPresetList()
        {
            if (PresetList == null)
                PresetList = new ObservableCollection<string>();

            PresetList.Clear();
            foreach (string presetFilePath in Directory.GetFiles(ConfigurationService.PresetPath))
            {
                PresetList.Add(Path.GetFileNameWithoutExtension(presetFilePath));
            }
        }
    }
}
