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
        private static string ApplicationName = Utilities.GetApplicationName();
        private DebuggingWindow _debuggingWindow = new DebuggingWindow();
        private ModViewModel _selectedValue;
        private Pcsx2Injector _pcsx2Injector;
        private Process _runningProcess;

        public string Title => ApplicationName;
        public ObservableCollection<ModViewModel> ModsList { get; set; }
        public RelayCommand AddModCommand { get; set; }
        public RelayCommand RemoveModCommand { get; set; }
        public RelayCommand MoveUp { get; set; }
        public RelayCommand MoveDown { get; set; }
        public RelayCommand BuildCommand { get; set; }
        public RelayCommand WizardCommand { get; set; }

        public ModViewModel SelectedValue
        {
            get => _selectedValue;
            set
            {
                _selectedValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MoveUp));
                OnPropertyChanged(nameof(MoveDown));
                OnPropertyChanged(nameof(AddModCommand));
                OnPropertyChanged(nameof(RemoveModCommand));
            }
        }

        public MainViewModel()
        {
            Log.OnLogDispatch += (long ms, string tag, string message) =>
                _debuggingWindow.Log(ms, tag, message);

            ReloadModsList();
            SelectedValue = ModsList.FirstOrDefault();
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
            }, _ => SelectedValue != null);
            MoveUp = new RelayCommand(_ => MoveSelectedModUp(), _ => CanSelectedModMoveUp());
            MoveDown = new RelayCommand(_ => MoveSelectedModDown(), _ => CanSelectedModMoveDown());
            BuildCommand = new RelayCommand(async _ =>
            {
                CloseRunningProcess();
                Application.Current.Dispatcher.Invoke(_debuggingWindow.Close);
                _debuggingWindow = new DebuggingWindow();
                Application.Current.Dispatcher.Invoke(_debuggingWindow.Show);
                _debuggingWindow.ClearLogs();

                await ModsService.RunPacherAsync();
                switch (ConfigurationService.GameEdition)
                {
                    case 0:
                        _runningProcess = Process.Start(new ProcessStartInfo
                        {
                            FileName = ConfigurationService.OpenKhGameEngineLocation,
                            WorkingDirectory = Path.GetDirectoryName(ConfigurationService.OpenKhGameEngineLocation),
                            Arguments = $"--data \"{ConfigurationService.GameDataLocation}\" --modpath \"{ConfigurationService.GameModPath}\""
                        });
                        break;
                    case 1:
                        if (ConfigurationService.RegionId > 0)
                        {
                            _pcsx2Injector.RegionId = ConfigurationService.RegionId - 1;
                            _pcsx2Injector.Region = Kh2.Constants.Regions[_pcsx2Injector.RegionId];
                            _pcsx2Injector.Language = _pcsx2Injector.Region switch
                            {
                                "uk" => "us",
                                "fm" => "jp",
                                _ => _pcsx2Injector.Region,
                            };
                        }
                        else
                            _pcsx2Injector.RegionId = -1;

                        _runningProcess = Process.Start(new ProcessStartInfo
                        {
                            FileName = ConfigurationService.Pcsx2Location,
                            WorkingDirectory = Path.GetDirectoryName(ConfigurationService.Pcsx2Location),
                            Arguments = $"\"{ConfigurationService.IsoLocation}\""
                        });
                        _pcsx2Injector.Run(_runningProcess, _debuggingWindow);
                        break;
                    case 2:
                        break;
                }

            });
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
                }
            });

            _pcsx2Injector = new Pcsx2Injector(new OperationDispatcher());
        }

        public void CloseAllWindows()
        {
            CloseRunningProcess();
            _debuggingWindow.Close();
        }

        public void CloseRunningProcess()
        {
            if (_runningProcess == null)
                return;

            _pcsx2Injector.Stop();
            _runningProcess.CloseMainWindow();
            _runningProcess.Kill();
            _runningProcess.Dispose();
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
            OnPropertyChanged(nameof(BuildCommand));
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
    }
}
