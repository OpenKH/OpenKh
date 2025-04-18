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
        private DownloadableModViewModel _selectedDownloadableMod;
        private Pcsx2Injector _pcsx2Injector;
        private Process _runningProcess;
        private bool _isBuilding;
        private bool _pc;
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
        private bool _isLoadingDownloadableMods;

        private const string RAW_FILES_FOLDER_NAME = "raw";
        private const string ORIGINAL_FILES_FOLDER_NAME = "original";
        private const string REMASTERED_FILES_FOLDER_NAME = "remastered";

        public static bool overwriteMod = false;
        public string Title => ApplicationName;
        public string CurrentVersion => ApplicationVersion;
        public ObservableCollection<ModViewModel> ModsList { get; set; }
        public ObservableCollection<DownloadableModViewModel> DownloadableModsList { get; set; }
        public ObservableCollection<DownloadableModViewModel> FilteredDownloadableModsList { get; set; }
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
        public RelayCommand OpenLinkCommand { get; set; }
        public RelayCommand CheckOpenkhUpdateCommand { get; set; }
        public RelayCommand OpenPresetMenuCommand { get; set; }
        public RelayCommand CheckForModUpdatesCommand { get; set; }
        public RelayCommand YamlGeneratorCommand { get; set; }
        public RelayCommand RefreshDownloadableModsCommand { get; set; }

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

        public DownloadableModViewModel SelectedDownloadableMod
        {
            get => _selectedDownloadableMod;
            set
            {
                _selectedDownloadableMod = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDownloadableModSelected));
                OnPropertyChanged(nameof(IsDownloadableModInfoVisible));
                OnPropertyChanged(nameof(IsDownloadableModUnselectedMessageVisible));
            }
        }

        public bool IsModSelected => SelectedValue != null;

        public bool IsDownloadableModSelected => SelectedDownloadableMod != null;

        public Visibility IsModInfoVisible => IsModSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsModUnselectedMessageVisible => !IsModSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsDownloadableModInfoVisible => IsDownloadableModSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsDownloadableModUnselectedMessageVisible => !IsDownloadableModSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsLoadingDownloadableModsVisibility => _isLoadingDownloadableMods ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PatchVisible => PC && !PanaceaInstalled || PC && DevView ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ModLoader => !PC || PanaceaInstalled ? Visibility.Visible : Visibility.Collapsed;
        public Visibility notPC => !PC ? Visibility.Visible : Visibility.Collapsed;
        public Visibility isPC => PC ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PanaceaSettings => PC && PanaceaInstalled ? Visibility.Visible : Visibility.Collapsed;

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
                        launchExecutable = 0;
                        return 0;
                    case "kh1":
                        launchExecutable = 1;
                        return 1;
                    case "bbs":
                        launchExecutable = 2;
                        return 2;
                    case "Recom":
                        launchExecutable = 3;
                        return 3;
                    case "kh3d":
                        launchExecutable = 4;
                        return 4;
                    default:
                        launchExecutable = 0;
                        return 0;
                }
            }
            set
            {
                launchExecutable = value;
                switch (value)
                {
                    case 0:
                        _launchGame = "kh2";
                        ConfigurationService.LaunchGame = "kh2";
                        break;
                    case 1:
                        _launchGame = "kh1";
                        ConfigurationService.LaunchGame = "kh1";
                        break;
                    case 2:
                        _launchGame = "bbs";
                        ConfigurationService.LaunchGame = "bbs";
                        break;
                    case 3:
                        _launchGame = "Recom";
                        ConfigurationService.LaunchGame = "Recom";
                        break;
                    case 4:
                        _launchGame = "kh3d";
                        ConfigurationService.LaunchGame = "kh3d";
                        break;
                    default:
                        _launchGame = "kh2";
                        ConfigurationService.LaunchGame = "kh2";
                        break;
                }
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

        public bool IsLoadingDownloadableMods
        {
            get => _isLoadingDownloadableMods;
            set
            {
                _isLoadingDownloadableMods = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLoadingDownloadableModsVisibility));
            }
        }

        private string _searchQuery;
        
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                FilterDownloadableMods();
            }
        }

        public MainViewModel()
        {
            DownloadableModsList = new ObservableCollection<DownloadableModViewModel>();
            FilteredDownloadableModsList = new ObservableCollection<DownloadableModViewModel>();
            PresetList = new ObservableCollection<string>();
            
            if (ConfigurationService.GameEdition == 2)
            {
                PC = true;
                PanaceaInstalled = ConfigurationService.PanaceaInstalled;
                DevView = ConfigurationService.DevView;
                _panaceaConsoleEnabled = ConfigurationService.ShowConsole;
                _panaceaDebugLogEnabled = ConfigurationService.DebugLog;
                _panaceaSoundDebugEnabled = ConfigurationService.SoundDebug;
                _panaceaCacheEnabled = ConfigurationService.EnableCache;
                _panaceaQuickMenuEnabled = ConfigurationService.QuickMenu;
            }
            else
                PC = false;
            if (_supportedGames.Contains(ConfigurationService.LaunchGame) && PC)
                _launchGame = ConfigurationService.LaunchGame;
            else
                ConfigurationService.LaunchGame = _launchGame;

            AutoUpdateMods = ConfigurationService.AutoUpdateMods;

            Log.OnLogDispatch += (long ms, string tag, string message) =>
                _debuggingWindow.Log(ms, tag, message);

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
                        foreach (var filePath in Directory.GetFiles(mod.Path, "*", SearchOption.AllDirectories))
                        {
                            var attributes = File.GetAttributes(filePath);
                            if (attributes.HasFlag(FileAttributes.ReadOnly))
                                File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                        }

                        Directory.Delete(mod.Path, true);
                        ModsList.RemoveAt(ModsList.IndexOf(SelectedValue));
                        RefreshDownloadableMods(); // Refrescar la lista de mods descargables después de eliminar un mod
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
                    if (ConfigurationService.GameEdition == 2)
                    {
                        PC = true;
                        PanaceaInstalled = ConfigurationService.PanaceaInstalled;
                    }
                    else
                        PC = false;
                    ConfigurationService.WizardVersionNumber = _wizardVersionNumber;
                }
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

            RefreshDownloadableModsCommand = new RelayCommand(x => RefreshDownloadableMods());

            _pcsx2Injector = new Pcsx2Injector(new OperationDispatcher());
            _ = FetchUpdates();

            if (ConfigurationService.WizardVersionNumber < _wizardVersionNumber)
                WizardCommand.Execute(null);

            ConfigurationService.OnGameNameChanged += (sender, e) =>
            {
                ReloadModsList();
                RefreshDownloadableMods();
            };

            ReloadModsList();
            RefreshDownloadableMods();
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
            var result = await ModsService.RunPacherAsync(fastMode);
            IsBuilding = false;

            return result;
        }

        private Task RunGame()
        {
            ProcessStartInfo processStartInfo;
            bool isPcsx2 = false;
            switch (ConfigurationService.GameEdition)
            {
                case 0:
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
                case 1:
                    Log.Info("Starting PCSX2");
                    _pcsx2Injector.RegionId = ConfigurationService.RegionId;
                    _pcsx2Injector.Region = Kh2.Constants.Regions[_pcsx2Injector.RegionId];
                    _pcsx2Injector.Language = Kh2.Constants.Languages[_pcsx2Injector.RegionId];

                    processStartInfo = new ProcessStartInfo
                    {
                        FileName = ConfigurationService.Pcsx2Location,
                        WorkingDirectory = Path.GetDirectoryName(ConfigurationService.Pcsx2Location),
                        Arguments = $"\"{ConfigurationService.IsoLocation}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                    };
                    isPcsx2 = true;
                    break;
                case 2:
                    if (ConfigurationService.PCVersion == "EGS" && !(_launchGame == "kh3d"))
                    {
                        if (ConfigurationService.PcReleaseLocation != null)
                        {
                            if (ConfigurationService.PanaceaInstalled)
                            {
                                string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt");
                                if (!File.Exists(panaceaSettings))
                                {
                                    File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt"),
                                    new string[]
                                    {
                                $"mod_path={ConfigurationService.GameModPath}",
                                $"show_console={false}",
                                    });
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
                        if (ConfigurationService.PcReleaseLocationKH3D != null)
                        {
                            string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt");
                            if (ConfigurationService.PanaceaInstalled)
                            {
                                if (!File.Exists(panaceaSettings))
                                {
                                    if (Directory.Exists(ConfigurationService.PcReleaseLocationKH3D))
                                    {
                                        File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt"),
                                        new string[]
                                        {
                                    $"mod_path={ConfigurationService.GameModPath}",
                                    $"show_console={false}",
                                        });
                                    }
                                    else
                                        File.Create(panaceaSettings);
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
                        if (ConfigurationService.PcReleaseLocation != null)
                        {
                            if (ConfigurationService.PanaceaInstalled)
                            {
                                string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt");
                                if (!File.Exists(panaceaSettings))
                                {
                                    File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt"),
                                    new string[]
                                    {
                                $"mod_path={ConfigurationService.GameModPath}",
                                $"show_console={false}",
                                    });
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
                        if (ConfigurationService.PcReleaseLocationKH3D != null)
                        {
                            string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt");
                            if (ConfigurationService.PanaceaInstalled)
                            {
                                if (!File.Exists(panaceaSettings))
                                {
                                    if (Directory.Exists(ConfigurationService.PcReleaseLocationKH3D))
                                    {
                                        File.WriteAllLines(Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt"),
                                        new string[]
                                        {
                                    $"mod_path={ConfigurationService.GameModPath}",
                                    $"show_console={false}",
                                        });
                                    }
                                    else
                                        File.Create(panaceaSettings);
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
                                filename = Path.Combine(ConfigurationService.PcReleaseLocation, executable[launchExecutable]);
                            }
                            else
                            {
                                filename = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, executable[launchExecutable]);
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
        }

        private ModViewModel Map(ModModel mod) => new ModViewModel(mod, this);

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
                if (ConfigurationService.GameEdition == 2)
                {
                    // Use the package map file to rearrange the files in the structure needed by the patcher
                    var packageMapLocation = Path.Combine(ConfigurationService.GameModPath, _launchGame, "patch-package-map.txt");
                    var packageMap = File
                        .ReadLines(packageMapLocation)
                        .Select(line => line.Split(" $$$$ "))
                        .ToDictionary(array => array[0], array => array[1]);

                    var patchStagingDir = Path.Combine(ConfigurationService.GameModPath, _launchGame, "patch-staging");
                    if (Directory.Exists(patchStagingDir))
                        Directory.Delete(patchStagingDir, true);
                    Directory.CreateDirectory(patchStagingDir);
                    foreach (var entry in packageMap)
                    {
                        var sourceFile = Path.Combine(ConfigurationService.GameModPath, _launchGame, entry.Key);
                        var destFile = Path.Combine(patchStagingDir, entry.Value);
                        if (File.Exists(sourceFile))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                            File.Move(sourceFile, destFile);
                        }
                    }

                    foreach (var directory in Directory.GetDirectories(Path.Combine(ConfigurationService.GameModPath, _launchGame)))
                        if (!"patch-staging".Equals(Path.GetFileName(directory)))
                            Directory.Delete(directory, true);

                    var stagingDirs = Directory.GetDirectories(patchStagingDir).Select(directory => Path.GetFileName(directory)).ToHashSet();

                    string[] specialDirs = Array.Empty<string>();
                    var specialStagingDir = Path.Combine(patchStagingDir, "special");
                    if (Directory.Exists(specialStagingDir))
                        specialDirs = Directory.GetDirectories(specialStagingDir).Select(directory => Path.GetFileName(directory)).ToArray();

                    foreach (var packageName in stagingDirs)
                        Directory.Move(Path.Combine(patchStagingDir, packageName), Path.Combine(ConfigurationService.GameModPath, _launchGame, packageName));
                    foreach (var specialDir in specialDirs)
                        Directory.Move(Path.Combine(ConfigurationService.GameModPath, _launchGame, "special", specialDir), Path.Combine(ConfigurationService.GameModPath, _launchGame, specialDir));

                    stagingDirs.Remove("special"); // Since it's not actually a real game package
                    Directory.Delete(patchStagingDir, true);

                    var specialModDir = Path.Combine(ConfigurationService.GameModPath, _launchGame, "special");
                    if (Directory.Exists(specialModDir))
                        Directory.Delete(specialModDir, true);

                    foreach (var directory in stagingDirs.Select(packageDir => Path.Combine(ConfigurationService.GameModPath, _launchGame, packageDir)))
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
                if (ConfigurationService.GameEdition == 2)
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
                            Directory.Delete(Path.Combine(ConfigurationService.GameModPath, _launchGame), true);
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
                if (_launchGame != "kh3d" && ConfigurationService.PcReleaseLocation != null)
                {
                    string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocation, "panacea_settings.txt");
                    string[] lines = File.ReadAllLines(panaceaSettings);
                    string textToWrite = $"mod_path={ConfigurationService.GameModPath}\r\n";
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
                else if (ConfigurationService.PcReleaseLocationKH3D != null)
                {
                    string panaceaSettings = Path.Combine(ConfigurationService.PcReleaseLocationKH3D, "panacea_settings.txt");
                    string[] lines = File.ReadAllLines(panaceaSettings);
                    string textToWrite = $"mod_path={ConfigurationService.GameModPath}\r\n";
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

        public async void RefreshDownloadableMods()
        {
            try
            {
                IsLoadingDownloadableMods = true;
                DownloadableModsList.Clear();
                FilteredDownloadableModsList.Clear();
                SelectedDownloadableMod = null;

                var mods = await DownloadableModsService.GetDownloadableModsForGame(ConfigurationService.LaunchGame);

                // Filter out installed mods
                var installedModPaths = ModsList
                    .Select(x => x.Source.ToLower())
                    .ToList();

                var downloadableMods = mods
                    .Where(x => 
                    {
                        string modPath = DownloadableModsService.GetModPath(x.Repository);
                        return !string.IsNullOrEmpty(modPath) && !installedModPaths.Contains(modPath.ToLower());
                    })
                    .Select(x => new DownloadableModViewModel(x, this))
                    .ToList();

                foreach (var mod in downloadableMods.OrderBy(x => x.Name))
                {
                    DownloadableModsList.Add(mod);
                }

                // Apply search filter if there's a search query
                FilterDownloadableMods();

                if (FilteredDownloadableModsList.Count > 0)
                {
                    SelectedDownloadableMod = FilteredDownloadableModsList[0];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing downloadable mods: {ex.Message}");
            }
            finally
            {
                IsLoadingDownloadableMods = false;
            }
        }

        private void FilterDownloadableMods()
        {
            FilteredDownloadableModsList.Clear();
            foreach (var mod in DownloadableModsList)
            {
                if (string.IsNullOrEmpty(_searchQuery) || mod.Name.ToLower().Contains(_searchQuery.ToLower()))
                {
                    FilteredDownloadableModsList.Add(mod);
                }
            }
        }
    }
}
