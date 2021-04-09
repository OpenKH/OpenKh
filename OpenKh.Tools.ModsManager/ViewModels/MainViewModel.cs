using OpenKh.Common;
using OpenKh.Tools.Common;
using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.Views;
using System;
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
        private static Version _version = Assembly.GetEntryAssembly()?.GetName()?.Version;
        private static string ApplicationName = Utilities.GetApplicationName();
        private static string ApplicationVersion = Utilities.GetApplicationVersion();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        private DebuggingWindow _debuggingWindow = new DebuggingWindow();
        private ModViewModel _selectedValue;
        private Pcsx2Injector _pcsx2Injector;
        private Process _runningProcess;
        private bool _isBuilding;

        public string Title => ApplicationName;
        public string CurrentVersion => ApplicationVersion;
        public ObservableCollection<ModViewModel> ModsList { get; set; }
        public RelayCommand ExitCommand { get; set; }
        public RelayCommand AddModCommand { get; set; }
        public RelayCommand RemoveModCommand { get; set; }
        public RelayCommand OpenModFolderCommand { get; set; }
        public RelayCommand MoveUp { get; set; }
        public RelayCommand MoveDown { get; set; }
        public RelayCommand BuildCommand { get; set; }
        public RelayCommand RunCommand { get; set; }
        public RelayCommand BuildAndRunCommand { get; set; }
        public RelayCommand StopRunningInstanceCommand { get; set; }
        public RelayCommand WizardCommand { get; set; }
        public RelayCommand OpenLinkCommand { get; set; }

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
            Log.OnLogDispatch += (long ms, string tag, string message) =>
                _debuggingWindow.Log(ms, tag, message);

            ReloadModsList();
            SelectedValue = ModsList.FirstOrDefault();

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

                        await ModsService.InstallMod(name, isZipFile, progress =>
                        {
                            Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressText = progress);
                        }, nProgress =>
                        {
                            Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressValue = nProgress);
                        });

                        var actualName = isZipFile ? Path.GetFileNameWithoutExtension(name) : name;
                        var mod = ModsService.GetMods(new string[] { actualName }).First();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            progressWindow.Close();
                            ModsList.Insert(0, Map(mod));
                            SelectedValue = ModsList[0];
                        });
                    }
                    catch (Exception ex)
                    {
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
                        ReloadModsList();
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
            MoveUp = new RelayCommand(_ => MoveSelectedModUp(), _ => CanSelectedModMoveUp());
            MoveDown = new RelayCommand(_ => MoveSelectedModDown(), _ => CanSelectedModMoveDown());
            BuildCommand = new RelayCommand(async _ =>
            {
                ResetLogWindow();
                await BuildPatches();
                CloseAllWindows();
            }, _ => !IsBuilding);
            RunCommand = new RelayCommand(async _ =>
            {
                CloseRunningProcess();
                ResetLogWindow();
                await RunGame();
            });
            BuildAndRunCommand = new RelayCommand(async _ =>
            {
                CloseRunningProcess();
                ResetLogWindow();
                if (await BuildPatches())
                    await RunGame();
            }, _ => !IsBuilding);
            StopRunningInstanceCommand = new RelayCommand(_ =>
            {
                CloseRunningProcess();
                ResetLogWindow();
            }, _ => IsRunning);
            WizardCommand = new RelayCommand(_ =>
            {
                var dialog = new SetupWizardWindow()
                {
                    ConfigGameEdition = ConfigurationService.GameEdition,
                    ConfigGameDataLocation = ConfigurationService.GameDataLocation,
                    ConfigIsoLocation = ConfigurationService.IsoLocation,
                    ConfigOpenKhGameEngineLocation = ConfigurationService.OpenKhGameEngineLocation,
                    ConfigPcsx2Location = ConfigurationService.Pcsx2Location,
                    ConfigPcReleaseLocation = ConfigurationService.PcReleaseLocation,
                    ConfigRegionId = ConfigurationService.RegionId,
                };
                if (dialog.ShowDialog() == true)
                {
                    ConfigurationService.GameEdition = dialog.ConfigGameEdition;
                    ConfigurationService.GameDataLocation = dialog.ConfigGameDataLocation;
                    ConfigurationService.IsoLocation = dialog.ConfigIsoLocation;
                    ConfigurationService.OpenKhGameEngineLocation = dialog.ConfigOpenKhGameEngineLocation;
                    ConfigurationService.Pcsx2Location = dialog.ConfigPcsx2Location;
                    ConfigurationService.PcReleaseLocation = dialog.ConfigPcReleaseLocation;
                    ConfigurationService.RegionId = dialog.ConfigRegionId;
                    ConfigurationService.IsFirstRunComplete = true;
                }
            });
            OpenLinkCommand = new RelayCommand(url => Process.Start(new ProcessStartInfo(url as string)
            {
                UseShellExecute = true
            }));

            _pcsx2Injector = new Pcsx2Injector(new OperationDispatcher());
            FetchUpdates();

            if (!ConfigurationService.IsFirstRunComplete)
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

        private async Task<bool> BuildPatches()
        {
            IsBuilding = true;
            var result = await ModsService.RunPacherAsync();
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
                default:
                    Log.Info("Starting Kingdom Hearts II: Final Mix");
                    processStartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(ConfigurationService.PcReleaseLocation, "KINGDOM HEARTS II FINAL MIX.exe"),
                        WorkingDirectory = ConfigurationService.PcReleaseLocation,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                    };
                    // TODO copy OpenKh.Research.Panacea.dll as DINPUT8.dll
                    break;
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

        private void ReloadModsList()
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
        }
    }
}
