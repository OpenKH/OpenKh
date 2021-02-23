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
        private string _isoLocation;
        private string _openKhGameEngineLocation;
        private string _pcsx2Location;
        private string _pcReleaseLocation;
        private string _gameDataLocation;

        public string Title => $"Set-up wizard | {ApplicationName}";

        public RelayCommand SelectIsoCommand { get; }
        public string IsoLocation
        {
            get => _isoLocation;
            set
            {
                _isoLocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsIsoSelected));
            }
        }
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameSelected));
            }
        }

        public RelayCommand SelectGameDataLocationCommand { get; }
        public string GameDataLocation
        {
            get => _gameDataLocation;
            set
            {
                _gameDataLocation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(GameDataNotFoundVisibility));
                OnPropertyChanged(nameof(GameDataFoundVisibility));
            }
        }
        public bool IsGameDataFound => File.Exists(Path.Combine(GameDataLocation ?? "", "00objentry.bin"));
        public Visibility GameDataNotFoundVisibility => !IsGameDataFound ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameDataFoundVisibility => IsGameDataFound ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProgressBarVisibility { get; set; }
        public Visibility ExtractionCompleteVisibility { get; set; }

        public int RegionId { get; set; }

        public SetupWizardViewModel()
        {
            ProgressBarVisibility = Visibility.Collapsed;
            ExtractionCompleteVisibility = Visibility.Collapsed;

            SelectIsoCommand = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => IsoLocation = fileName, _isoFilter));
            SelectOpenKhGameEngineCommand = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => OpenKhGameEngineLocation = fileName, _openkhGeFilter));
            SelectPcsx2Command = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => Pcsx2Location = fileName, _pcsx2Filter));
            SelectGameDataLocationCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(path => GameDataLocation = path));
        }
    }
}
