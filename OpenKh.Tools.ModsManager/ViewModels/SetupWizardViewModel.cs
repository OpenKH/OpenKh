using OpenKh.Common;
using OpenKh.Kh1;
using OpenKh.Kh2;
using OpenKh.Tools.Common;
using OpenKh.Tools.ModsManager.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Xe.IO;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public class SetupWizardViewModel : BaseNotifyPropertyChanged
    {
        private const int BufferSize = 65536;
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
                OnPropertyChanged(nameof(IsIsoSelected));
                OnPropertyChanged(nameof(GameId));
                OnPropertyChanged(nameof(GameName));
                OnPropertyChanged(nameof(GameRecognizedVisibility));
                OnPropertyChanged(nameof(GameNotRecognizedVisibility));
                OnPropertyChanged(nameof(IsGameRecognized));
                OnPropertyChanged(nameof(IsGameDataFound));
                OnPropertyChanged(nameof(GameDataFoundVisibility));
                OnPropertyChanged(nameof(GameDataNotFoundVisibility));
            }
        }
        public bool IsIsoSelected => !string.IsNullOrEmpty(IsoLocation) && File.Exists(IsoLocation);
        public bool IsGameRecognized => IsIsoSelected && GameId != null;
        public Visibility GameRecognizedVisibility => IsIsoSelected && GameId != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameNotRecognizedVisibility => IsIsoSelected && GameId == null ? Visibility.Visible : Visibility.Collapsed;

        public bool IsGameSelected
        {
            get
            {
                switch (GameEdition)
                {
                    case 0:
                        return !string.IsNullOrEmpty(OpenKhGameEngineLocation) && File.Exists(OpenKhGameEngineLocation);
                    case 1:
                        return !string.IsNullOrEmpty(Pcsx2Location) && File.Exists(Pcsx2Location);
                    case 2:
                        return !string.IsNullOrEmpty(PcReleaseLocation) &&
                            Directory.Exists(PcReleaseLocation) &&
                            File.Exists(Path.Combine(PcReleaseLocation, "EOSSDK-Win64-Shipping.dll"));
                    default:
                        return false;
                }
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
                OnPropertyChanged(nameof(IsEpicGamesRelease));
            }
        }
        public bool IsEpicGamesRelease => GameEdition == 2;

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

        public bool IsNotExtracting { get; private set; }
        public bool IsGameDataFound => IsNotExtracting && GameService.FolderContainsUniqueFile(GameId, GameDataLocation);
        public Visibility GameDataNotFoundVisibility => !IsGameDataFound ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameDataFoundVisibility => IsGameDataFound ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProgressBarVisibility => IsNotExtracting ? Visibility.Collapsed : Visibility.Visible;
        public Visibility ExtractionCompleteVisibility => ExtractionProgress == 1f ? Visibility.Visible : Visibility.Collapsed;
        public RelayCommand ExtractGameDataCommand { get; set; }
        public float ExtractionProgress { get; set; }

        public int RegionId { get; set; }

        public SetupWizardViewModel()
        {
            IsNotExtracting = true;
            SelectIsoCommand = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => IsoLocation = fileName, _isoFilter));
            SelectOpenKhGameEngineCommand = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => OpenKhGameEngineLocation = fileName, _openkhGeFilter));
            SelectPcsx2Command = new RelayCommand(_ =>
                FileDialog.OnOpen(fileName => Pcsx2Location = fileName, _pcsx2Filter));
            SelectPcReleaseCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(path => PcReleaseLocation = path));
            SelectGameDataLocationCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(path => GameDataLocation = path));
            ExtractGameDataCommand = new RelayCommand(async _ =>
                await ExtractGameData(IsoLocation, GameDataLocation));
        }

        private Task ExtractGameData(string isoLocation, string gameDataLocation)
        {
            return GameId switch
            {
                "kh1" => ExtractKingdomHearts1GameData(isoLocation, gameDataLocation),
                "kh2" => ExtractKingdomHearts2GameData(isoLocation, gameDataLocation),
                _ => Task.CompletedTask,
            };
        }

        private async Task ExtractKingdomHearts2GameData(string isoLocation, string gameDataLocation)
        {
            var fileBlocks = File.OpenRead(isoLocation).Using(stream =>
            {
                var bufferedStream = new BufferedStream(stream);
                var idxBlock = IsoUtility.GetFileOffset(bufferedStream, "KH2.IDX;1");
                var imgBlock = IsoUtility.GetFileOffset(bufferedStream, "KH2.IMG;1");
                return (idxBlock, imgBlock);
            });

            if (fileBlocks.idxBlock == -1 || fileBlocks.imgBlock == -1)
            {
                MessageBox.Show(
                    $"Unable to find the files KH2.IDX and KH2.IMG in the ISO at '{isoLocation}'. The extraction will stop.",
                    "Extraction error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            IsNotExtracting = false;
            ExtractionProgress = 0;
            OnPropertyChanged(nameof(IsNotExtracting));
            OnPropertyChanged(nameof(IsGameDataFound));
            OnPropertyChanged(nameof(ProgressBarVisibility));
            OnPropertyChanged(nameof(ExtractionCompleteVisibility));
            OnPropertyChanged(nameof(ExtractionProgress));

            await Task.Run(() =>
            {
                using var isoStream = File.OpenRead(isoLocation);

                var idxOffset = fileBlocks.idxBlock * 0x800L;
                var idx = Idx.Read(new SubStream(isoStream, idxOffset, isoStream.Length - idxOffset));

                var imgOffset = fileBlocks.imgBlock * 0x800L;
                var imgStream = new SubStream(isoStream, imgOffset, isoStream.Length - imgOffset);
                var img = new Img(imgStream, idx, true);

                var fileCount = img.Entries.Count;
                var fileProcessed = 0;
                foreach (var fileEntry in img.Entries)
                {
                    var fileName = IdxName.Lookup(fileEntry) ?? $"@{fileEntry.Hash32:08X}_{fileEntry.Hash16:04X}";
                    using var stream = img.FileOpen(fileEntry);
                    var fileDestination = Path.Combine(gameDataLocation, fileName);
                    var directoryDestination = Path.GetDirectoryName(fileDestination);
                    if (!Directory.Exists(directoryDestination))
                        Directory.CreateDirectory(directoryDestination);
                    File.Create(fileDestination).Using(dstStream => stream.CopyTo(dstStream, BufferSize));

                    fileProcessed++;
                    ExtractionProgress = (float)fileProcessed / fileCount;
                    OnPropertyChanged(nameof(ExtractionProgress));
                }

                Application.Current.Dispatcher.Invoke(() =>
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
                });
            });
        }

        private async Task ExtractKingdomHearts1GameData(string isoLocation, string gameDataLocation)
        {
            const int IsoBlock = 0x800;
            using var stream = File.OpenRead(isoLocation);
            var firstBlock = IsoUtility.GetFileOffset(stream, "SYSTEM.CNF;1");
            if (firstBlock == -1)
                throw new IOException("The file specified seems to not be a valid PlayStation 2 ISO.");

            var kingdomIdxBlock = IsoUtility.GetFileOffset(stream, "KINGDOM.IDX;1");
            if (kingdomIdxBlock == -1)
                throw new IOException("The file specified seems to not be a Kingdom Hearts 1 ISO");

            var idx = Idx1.Read(stream.SetPosition(kingdomIdxBlock * IsoBlock));
            var img = new Img1(stream, idx, firstBlock);

            IsNotExtracting = false;
            ExtractionProgress = 0;
            OnPropertyChanged(nameof(IsNotExtracting));
            OnPropertyChanged(nameof(IsGameDataFound));
            OnPropertyChanged(nameof(ProgressBarVisibility));
            OnPropertyChanged(nameof(ExtractionCompleteVisibility));
            OnPropertyChanged(nameof(ExtractionProgress));

            await Task.Run(() =>
            {
                var fileCount = img.Entries.Count;
                var fileProcessed = 0;
                foreach (var entry in img.Entries)
                {
                    var fileName = entry.Key;
                    if (fileName == "kingdom.img")
                        continue;

                    if (fileName == null)
                        fileName = $"@noname/{entry.Value.Hash:X08}";

                    var outputFile = Path.Combine(gameDataLocation, fileName);

                    var outputDir = Path.GetDirectoryName(outputFile);
                    if (Directory.Exists(outputDir) == false)
                        Directory.CreateDirectory(outputDir);

                    using var file = File.Create(outputFile);
                    img.FileOpen(entry.Value).CopyTo(file);

                    fileProcessed++;
                    ExtractionProgress = (float)fileProcessed / fileCount;
                    OnPropertyChanged(nameof(ExtractionProgress));
                }

                Application.Current.Dispatcher.Invoke(() =>
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
                });
            });
        }
    }
}
