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
        private ModViewModel _selectedValue;
        private Pcsx2Injector _pcsx2Injector;

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
                        var repositoryName = view.RepositoryName;
                        progressWindow = Application.Current.Dispatcher.Invoke(() =>
                        {
                            var progressWindow = new InstallModProgressWindow
                            {
                                ModName = repositoryName,
                                ProgressText = "Initializing",
                                ShowActivated = true
                            };
                            progressWindow.Show();
                            return progressWindow;
                        });

                        await ModsService.InstallModFromGithub(repositoryName, progress =>
                        {
                            Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressText = progress);
                        }, nProgress =>
                        {
                            Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressValue = nProgress);
                        });

                        var mod = ModsService.GetMods(new string[] { repositoryName }).First();
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
                        Directory.Delete(mod.Path, true);
                        ReloadModsList();
                    });
                }
            }, _ => SelectedValue != null);
            MoveUp = new RelayCommand(_ => MoveSelectedModUp(), _ => CanSelectedModMoveUp());
            MoveDown = new RelayCommand(_ => MoveSelectedModDown(), _ => CanSelectedModMoveDown());
            BuildCommand = new RelayCommand(_ =>
            {
                ModsService.RunPacherAsync();
                switch (ConfigurationService.GameEdition)
                {
                    case 0:
                        Process.Start(new ProcessStartInfo
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

                        _pcsx2Injector.Run(Process.Start(new ProcessStartInfo
                        {
                            FileName = ConfigurationService.Pcsx2Location,
                            WorkingDirectory = Path.GetDirectoryName(ConfigurationService.Pcsx2Location),
                            Arguments = ConfigurationService.IsoLocation
                        }));
                        break;
                    case 2:
                        break;
                }

            }, _ => ModsList.Any(x => x.Enabled));
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

        private void ReloadModsList()
        {
            ModsList = new ObservableCollection<ModViewModel>(
                ModsService.GetMods(ModsService.Mods).Select(Map));
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
        }

        private bool CanSelectedModMoveDown() =>
            SelectedValue != null && ModsList.IndexOf(SelectedValue) < ModsList.Count - 1;

        private bool CanSelectedModMoveUp() =>
            SelectedValue != null && ModsList.IndexOf(SelectedValue) > 0;
    }
}
