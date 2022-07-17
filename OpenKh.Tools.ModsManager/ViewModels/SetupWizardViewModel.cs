using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            .AddExtensions("OpenKH Game Engine Executable", "*Game.exe");
        private static List<FileDialogFilter> _pcsx2Filter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("PCSX2 Emulator", "exe");
        private static List<FileDialogFilter> _pcFilter = FileDialogFilterComposer
           .Compose()
           .AddExtensions("PC Release Executable", "exe");

        private int _gameEdition;
        private string _isoLocation;
        private string _openKhGameEngineLocation;
        private string _pcsx2Location;
        private string _pcReleaseLocation;
        private string _gameDataLocation;

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
                OnPropertyChanged(nameof(IsIsoSelected));
                OnPropertyChanged(nameof(GameId));
                OnPropertyChanged(nameof(GameName));
                OnPropertyChanged(nameof(GameRecognizedVisibility));
                OnPropertyChanged(nameof(GameNotRecognizedVisibility));
                OnPropertyChanged(nameof(IsGameRecognized));
            }
        }
        public bool IsIsoSelected => (!string.IsNullOrEmpty(IsoLocation) && File.Exists(IsoLocation)) || GameEdition == 2;
        public bool IsGameRecognized => (IsIsoSelected && GameId != null) || GameEdition == 2;
        public Visibility GameRecognizedVisibility => IsIsoSelected && GameId != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility GameNotRecognizedVisibility => IsIsoSelected && GameId == null ? Visibility.Visible : Visibility.Collapsed;

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

        public bool IsNotExtracting { get; private set; }
        public bool IsGameDataFound => IsNotExtracting && File.Exists(Path.Combine(GameDataLocation ?? "", "00objentry.bin"));
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
                FileDialog.OnOpen(fileName => PcReleaseLocation = fileName, _pcFilter));
            SelectGameDataLocationCommand = new RelayCommand(_ =>
                FileDialog.OnFolder(path => GameDataLocation = path));
            ExtractGameDataCommand = new RelayCommand(async _ => 
            { 
                BEGIN:
                try
                {
                    await ExtractGameData(IsoLocation, GameDataLocation);
                }

                catch (IOException _ex)
                {
                    var _sysMessage = MessageBox.Show(_ex.Message + "\n\nWould you like to try again?", "An Exception was Caught!", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    switch (_sysMessage)
                    {
                        case MessageBoxResult.Yes:
                            goto BEGIN;
                            break;
                    }
                }
            });
        }

        private async Task ExtractGameData(string isoLocation, string gameDataLocation)
        {
            switch (GameEdition)
            {

                default:
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
                break;

                case 2:
                {
                    IsNotExtracting = false;
                    ExtractionProgress = 0;
                    OnPropertyChanged(nameof(IsNotExtracting));
                    OnPropertyChanged(nameof(IsGameDataFound));
                    OnPropertyChanged(nameof(ProgressBarVisibility));
                    OnPropertyChanged(nameof(ExtractionCompleteVisibility));
                    OnPropertyChanged(nameof(ExtractionProgress));

                    await Task.Run(() =>
                    {
                        var _nameList = new string[]
                        {
                            "first",
                            "second",
                            "third",
                            "fourth",
                            "fifth",
                            "sixth"
                        };

                        var _totalFiles = 0;
                        var _procTotalFiles = 0;

                        for (int i = 0; i < 6; i++)
                        {
                            using var _stream = new FileStream(Path.GetDirectoryName(_pcReleaseLocation) + "/Image/en/kh2_" + _nameList[i] + ".hed", FileMode.Open);
                            var _hedFile = OpenKh.Egs.Hed.Read(_stream);
                            _totalFiles += _hedFile.Count();
                        }

                        for (int i = 0; i < 6; i++)
                        {
                            var outputDir = gameDataLocation;
                            using var hedStream = File.OpenRead(Path.GetDirectoryName(_pcReleaseLocation) + "/Image/en/kh2_" + _nameList[i] + ".hed");
                            using var img = File.OpenRead(Path.GetDirectoryName(_pcReleaseLocation) + "/Image/en/kh2_" + _nameList[i] + ".pkg");

                            foreach (var entry in OpenKh.Egs.Hed.Read(hedStream))
                            {
                                var hash = OpenKh.Egs.Helpers.ToString(entry.MD5);
                                if (!OpenKh.Egs.EgsTools.Names.TryGetValue(hash, out var fileName))
                                    fileName = $"{hash}.dat";

                                var outputFileName = Path.Combine(outputDir, fileName);

                                OpenKh.Egs.EgsTools.CreateDirectoryForFile(outputFileName);

                                var hdAsset = new OpenKh.Egs.EgsHdAsset(img.SetPosition(entry.Offset));

                                File.Create(outputFileName).Using(stream => stream.Write(hdAsset.OriginalData));

                                outputFileName = Path.Combine(outputDir, REMASTERED_FILES_FOLDER_NAME, fileName);

                                foreach (var asset in hdAsset.Assets)
                                {
                                    var outputFileNameRemastered = Path.Combine(OpenKh.Egs.EgsTools.GetHDAssetFolder(outputFileName), asset);
                                    OpenKh.Egs.EgsTools.CreateDirectoryForFile(outputFileNameRemastered);

                                    var assetData = hdAsset.RemasteredAssetsDecompressedData[asset];
                                    File.Create(outputFileNameRemastered).Using(stream => stream.Write(assetData));
                                    _procTotalFiles++;

                                    ExtractionProgress = (float)_procTotalFiles / _totalFiles;
                                    OnPropertyChanged(nameof(ExtractionProgress));
                                }
                            }
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
                break;
            }
        }
    }
}
