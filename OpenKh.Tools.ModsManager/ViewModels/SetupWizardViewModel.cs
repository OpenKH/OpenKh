using OpenKh.Tools.Common;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public class SetupWizardViewModel : BaseNotifyPropertyChanged
    {
        private static string ApplicationName = Utilities.GetApplicationName();
        private static List<FileDialogFilter> _isoFilter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("PlayStation 2 game ISO", "iso");
        private static List<FileDialogFilter> _openkhGeFilter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("OpenKH Game Engine executable", "*Game.exe");
        private static List<FileDialogFilter> _pcsx2Filter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("PCSX2 emulator", "exe");

        private int _gameEdition;
        private string _openKhGameEngineLocation;
        private string _pcsx2Location;
        private string _pcReleaseLocation;

        public string Title => $"Set-up wizard | {ApplicationName}";

        public RelayCommand SelectIsoCommand { get; }
        public string IsoLocation { get; set; }
        public bool IsIsoSelected => !string.IsNullOrEmpty(IsoLocation) && File.Exists(IsoLocation);

        public bool IsGameSelected
        {
            get
            {
                var location = GameEdition switch
                {
                    0 => OpenKhGameEngineLocation,
                    1 => Pcsx2Location,
                    2 => PcReleaseLocation,
                    _ => string.Empty,
                };
                return !string.IsNullOrEmpty(location) && File.Exists(location);
            }
        }
        public int GameEdition
        {
            get => _gameEdition;
            set
            {
                _gameEdition = value;
                OnPropertyChanged(nameof(IsGameSelected));
                OnPropertyChanged(nameof(OpenKhGameEngineConfigVisibility));
                OnPropertyChanged(nameof(Pcsx2ConfigVisibility));
                OnPropertyChanged(nameof(PcReleaseConfigVisibility));
            }
        }

        public RelayCommand SelectOpenKhGameEngineCommand { get; }
        public Visibility OpenKhGameEngineConfigVisibility => GameEdition == 0 ? Visibility.Visible : Visibility.Collapsed;
        public string OpenKhGameEngineLocation
        {
            get => _openKhGameEngineLocation;
            set
            {
                _openKhGameEngineLocation = value;
                OnPropertyChanged(nameof(IsGameSelected));
            }
        }

        public RelayCommand SelectPcsx2Command { get; }
        public Visibility Pcsx2ConfigVisibility => GameEdition == 1 ? Visibility.Visible : Visibility.Collapsed;
        public string Pcsx2Location
        {
            get => _pcsx2Location;
            set
            {
                _pcsx2Location = value;
                OnPropertyChanged(nameof(IsGameSelected));
            }
        }

        public RelayCommand SelectPcReleaseCommand { get; }
        public Visibility PcReleaseConfigVisibility => GameEdition == 2 ? Visibility.Visible : Visibility.Collapsed;
        public string PcReleaseLocation
        {
            get => _pcReleaseLocation;
            set
            {
                _pcReleaseLocation = value;
                OnPropertyChanged(nameof(IsGameSelected));
            }
        }

        public RelayCommand SelectGameDataLocationCommand { get; }
        public string GameDataLocation { get; set; }
        public bool IsGameDataFound => File.Exists(Path.Combine(GameDataLocation ?? "", "00objentry.bin"));
        public Visibility GameDataNotFoundVisibility => !IsGameDataFound ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameDataFoundVisibility => IsGameDataFound ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProgressBarVisibility { get; set; }
        public Visibility ExtractionCompleteVisibility { get; set; }

        public SetupWizardViewModel()
        {
            ProgressBarVisibility = Visibility.Collapsed;
            ExtractionCompleteVisibility = Visibility.Collapsed;

            SelectIsoCommand = new RelayCommand(_ =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    IsoLocation = fileName;
                    OnPropertyChanged(nameof(IsoLocation));
                    OnPropertyChanged(nameof(IsIsoSelected));
                }, _isoFilter);
            });
            SelectOpenKhGameEngineCommand = new RelayCommand(_ =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    OpenKhGameEngineLocation = fileName;
                    OnPropertyChanged(nameof(OpenKhGameEngineLocation));
                    OnPropertyChanged(nameof(IsGameSelected));
                }, _openkhGeFilter);
            });
            SelectPcsx2Command = new RelayCommand(_ =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    Pcsx2Location = fileName;
                    OnPropertyChanged(nameof(Pcsx2Location));
                    OnPropertyChanged(nameof(IsGameSelected));
                }, _pcsx2Filter);
            });
            SelectGameDataLocationCommand = new RelayCommand(_ =>
            {
                FileDialog.OnFolder(path =>
                {
                    GameDataLocation = path;
                    OnPropertyChanged(nameof(GameDataLocation));
                    OnPropertyChanged(nameof(IsGameDataFound));
                    OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                    OnPropertyChanged(nameof(GameDataFoundVisibility));
                });
            });
        }
    }
}
