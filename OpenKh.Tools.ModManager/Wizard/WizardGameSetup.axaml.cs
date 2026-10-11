using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using OpenKh.Common;
using OpenKh.Tools.ModManager.Classes;
using OpenKh.Tools.ModManager.Services;
using OpenKh.Tools.ModManager.ViewModels;
using OpenKh.Tools.ModManager.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModManager.Wizard
{
    public partial class WizardGameSetup : ContentPage
    {
        SolidColorBrush _successBrush = SolidColorBrush.Parse("#40F040");
        SolidColorBrush _failBrush = SolidColorBrush.Parse("#F04040");

        public WizardGameSetup()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<PageRequestMessage>(this, (registrar, message) =>
            {
                if (message.Self != this)
                    return;

                message.Reply(true);
            });
        }

        private async void OnFolderClickFirst(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchTopLevel = TopLevel.GetTopLevel(this);
            var _storageProvider = _fetchTopLevel != null ? _fetchTopLevel.StorageProvider : null;

            if (_storageProvider != null)
            {
                var _fetchFolder = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select a Folder for 1.5+2.5...",
                    AllowMultiple = false
                });

                if (_fetchFolder.Count >= 1)
                    PathCollectionFirst.Text = _fetchFolder[0].Path.LocalPath;
            }
        }

        private async void OnFolderClickSecond(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchTopLevel = TopLevel.GetTopLevel(this);
            var _storageProvider = _fetchTopLevel != null ? _fetchTopLevel.StorageProvider : null;

            if (_storageProvider != null)
            {
                var _fetchFolder = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select a Folder for 2.8...",
                    AllowMultiple = false
                });

                if (_fetchFolder.Count >= 1)
                    PathCollectionSecond.Text = _fetchFolder[0].Path.LocalPath;
            }
        }

        private async void OnFolderClickEmuFirst(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchTopLevel = TopLevel.GetTopLevel(this);
            var _storageProvider = _fetchTopLevel != null ? _fetchTopLevel.StorageProvider : null;

            if (_storageProvider != null)
            {
                var _fetchFolder = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select a PCSX2 Installation...",
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        OperatingSystem.IsWindows() ? new FilePickerFileType("Executable File") { Patterns = ["*.exe"] }: new FilePickerFileType("AppImage Archive") { Patterns = ["*.AppImage"] }
                    ]
                });

                if (_fetchFolder.Count >= 1)
                    PathEmulator.Text = _fetchFolder[0].Path.LocalPath;
            }
        }

        private async void OnFileClickKH1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext.CurrentConfig;

            var _fetchTopLevel = TopLevel.GetTopLevel(this);
            var _storageProvider = _fetchTopLevel != null ? _fetchTopLevel.StorageProvider : null;

            if (_storageProvider != null)
            {
                var _fetchFile = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select ISO File for Kingdom Hearts...",
                    AllowMultiple = false,
                    FileTypeFilter = [new FilePickerFileType("ISO Image") { Patterns = ["*.iso"] }]
                });

                if (_fetchFile.Count >= 1)
                {
                    using var _fetchStream = new FileStream(_fetchFile[0x00].Path.LocalPath, FileMode.Open);
                    var _fetchIDX = IsoUtility.GetFileOffset(_fetchStream, "KINGDOM.IDX;1");

                    if (_fetchIDX == -1)
                    {
                        _fetchConfig.Emulator.RomPath[0x00] = null;
                        await DialogService.ShowMessage(_fetchTopLevel as Window, "Invalid ISO for Kingdom Hearts!", "This file does not seem to be an ISO for Kingdom Hearts. Please check the file and try again.", Dialogs.MessageType.ERROR);

                        IconMissingKH1.IsVisible = true;
                        IconFoundKH1.IsVisible = false;
                    }

                    else
                    {
                        _fetchConfig.Emulator.RomPath[0x00] = _fetchFile[0x00].Path.LocalPath;

                        IconMissingKH1.IsVisible = false;
                        IconFoundKH1.IsVisible = true;
                    }

                    OnPathChanged(null, null);
                }
            }
        }

        private async void OnFileClickKH2(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext.CurrentConfig;

            var _fetchTopLevel = TopLevel.GetTopLevel(this);
            var _storageProvider = _fetchTopLevel != null ? _fetchTopLevel.StorageProvider : null;

            if (_storageProvider != null)
            {
                var _fetchFile = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select ISO File for Kingdom Hearts II...",
                    AllowMultiple = false,
                    FileTypeFilter = [new FilePickerFileType("ISO Image") { Patterns = ["*.iso"] }]
                });

                if (_fetchFile.Count >= 1)
                {
                    using var _fetchStream = new FileStream(_fetchFile[0x00].Path.LocalPath, FileMode.Open);
                    var _fetchIDX = IsoUtility.GetFileOffset(_fetchStream, "KH2.IDX;1");

                    if (_fetchIDX == -1)
                    {
                        _fetchConfig.Emulator.RomPath[0x01] = null;
                        await DialogService.ShowMessage(_fetchTopLevel as Window, "Invalid ISO for Kingdom Hearts II!", "This file does not seem to be an ISO for Kingdom Hearts II. Please check the file and try again.", Dialogs.MessageType.ERROR);

                        IconMissingKH2.IsVisible = true;
                        IconFoundKH2.IsVisible = false;
                    }

                    else
                    {
                        _fetchConfig.Emulator.RomPath[0x01] = _fetchFile[0x00].Path.LocalPath;

                        IconMissingKH2.IsVisible = false;
                        IconFoundKH2.IsVisible = true;
                    }

                    OnPathChanged(null, null);
                }
            }
        }

        private async void OnFileClickCOM(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext.CurrentConfig;

            var _fetchTopLevel = TopLevel.GetTopLevel(this);
            var _storageProvider = _fetchTopLevel != null ? _fetchTopLevel.StorageProvider : null;

            if (_storageProvider != null)
            {
                var _fetchFile = await _storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select ISO File for Kingdom Hearts Re:Chain of Memories...",
                    AllowMultiple = false,
                    FileTypeFilter = [new FilePickerFileType("ISO Image") { Patterns = ["*.iso"] }]
                });

                if (_fetchFile.Count >= 1)
                {
                    _fetchConfig.Emulator.RomPath[0x02] = _fetchFile[0x00].Path.LocalPath;

                    IconMissingCOM.IsVisible = false;
                    IconFoundCOM.IsVisible = true;

                    OnPathChanged(null, null);
                }
            }
        }

        private async void OnDetectClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext != null ? _fetchContext.CurrentConfig : null;

            if (_fetchConfig != null)
            {
                if (_fetchConfig.Frontend.TargetPlatform == Platform.STEAM)
                {
                    var _fetchFolders = PathService.FetchSteamLibraries();

                    if (_fetchFolders == null)
                        return;

                    var _fetchPath1525 = "";
                    var _fetchPath28 = "";

                    _fetchFolders.AsParallel().ForAll(_fetchFolder =>
                    {
                        var _fetchSteamApps = Path.Combine(_fetchFolder, "steamapps");

                        if (Directory.Exists(_fetchSteamApps))
                        {
                            var _manifestPath1525 = Path.Combine(_fetchSteamApps, "appmanifest_2552430.acf");
                            var _manifestPath28 = Path.Combine(_fetchSteamApps, "appmanifest_2552440.acf");

                            var _installDirRegex = new Regex("installdir[^\"]*\"\\s*\"([^\"]*)\"", RegexOptions.None, TimeSpan.FromMilliseconds(500));

                            if (File.Exists(_manifestPath1525))
                            {
                                var _fetchManifest = File.ReadAllLines(_manifestPath1525);

                                _fetchManifest.AsParallel().ForAll(_fetchLine =>
                                {
                                    var _fetchMatch = _installDirRegex.Match(_fetchLine);

                                    if (_fetchMatch.Success)
                                    {
                                        var _fetchValue = Regex.Unescape(_fetchMatch.Groups[1].Value);
                                        var _fetchLibraryPath = Path.Combine(_fetchSteamApps, "common", _fetchValue);
                                        _fetchPath1525 = _fetchLibraryPath.Replace("\\", "/");
                                    }
                                });
                            }

                            if (File.Exists(_manifestPath28))
                            {
                                var _fetchManifest = File.ReadAllLines(_manifestPath28);

                                _fetchManifest.AsParallel().ForAll(_fetchLine =>
                                {
                                    var _fetchMatch = _installDirRegex.Match(_fetchLine);

                                    if (_fetchMatch.Success)
                                    {
                                        var _fetchValue = Regex.Unescape(_fetchMatch.Groups[1].Value);
                                        var _fetchLibraryPath = Path.Combine(_fetchSteamApps, "common", _fetchValue);
                                        _fetchPath28 = _fetchLibraryPath.Replace("\\", "/");
                                    }
                                });
                            }
                        }
                    });

                    _fetchConfig.Frontend.GamePath[0] = !String.IsNullOrEmpty(_fetchPath1525) ? _fetchPath1525 : "";
                    _fetchConfig.Frontend.GamePath[1] = !String.IsNullOrEmpty(_fetchPath28) ? _fetchPath28 : "";

                    PathCollectionFirst.Text = _fetchConfig.Frontend.GamePath[0];
                    PathCollectionSecond.Text = _fetchConfig.Frontend.GamePath[1];

                    PathCollectionFirst.BorderBrush = !String.IsNullOrEmpty(_fetchPath1525) ? _successBrush : _failBrush;
                    PathCollectionSecond.BorderBrush = !String.IsNullOrEmpty(_fetchPath28) ? _successBrush : _failBrush;
                }

                else if (_fetchConfig.Frontend.TargetPlatform == Platform.EPIC_GAMES_STORE)
                {
                    if (OperatingSystem.IsWindows())
                    {
                        var _fetchProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                        var _fetchEpicManifestPath = Path.Combine(_fetchProgramData, "Epic", "EpicGamesLauncher", "Data", "Manifests");

                        var _fetchAllFiles = Directory.GetFiles(_fetchEpicManifestPath);

                        var _fetchPath1525 = "";
                        var _fetchPath28 = "";

                        _fetchAllFiles.AsParallel().ForAll(_fetchFile =>
                        {
                            if (Path.GetExtension(_fetchFile) == ".item")
                            {
                                var _fetchFileRAW = File.ReadAllLines(_fetchFile);
                                var _catalogRegex = new Regex("\"CatalogNamespace\":\\s+\"([^\"]*)\"", RegexOptions.None, TimeSpan.FromMilliseconds(500));
                                var _installDirRegex = new Regex("\"InstallLocation\":\\s+\"([^\"]*)\"", RegexOptions.None, TimeSpan.FromMilliseconds(500));

                                var _isManifest1525 = false;
                                var _isManifest28 = false;

                                var _fetchDirectory = "";

                                _fetchFileRAW.AsParallel().ForAll(_fetchLine =>
                                {
                                    var _fetchCatalogMatch = _catalogRegex.Match(_fetchLine);
                                    var _fetchDirectoryMatch = _installDirRegex.Match(_fetchLine);

                                    if (_fetchCatalogMatch.Success)
                                    {
                                        var _fetchValue = Regex.Unescape(_fetchCatalogMatch.Groups[1].Value);

                                        _isManifest1525 = _fetchValue == "4158b699dd70447a981fee752d970a3e";
                                        _isManifest28 = _fetchValue == "c8ff067c1c984cd7ab1998e8a9afc8b6";
                                    }

                                    if (_fetchDirectoryMatch.Success)
                                        _fetchDirectory = Regex.Unescape(_fetchDirectoryMatch.Groups[1].Value);
                                });

                                if (_fetchDirectory != "" && _isManifest1525)
                                    _fetchPath1525 = _fetchDirectory.Replace("\\", "/");

                                if (_fetchDirectory != "" && _isManifest28)
                                    _fetchPath28 = _fetchDirectory.Replace("\\", "/");
                            }
                        });


                        _fetchConfig.Frontend.GamePath[0] = !String.IsNullOrEmpty(_fetchPath1525) ? _fetchPath1525 : "";
                        _fetchConfig.Frontend.GamePath[1] = !String.IsNullOrEmpty(_fetchPath28) ? _fetchPath28 : "";

                        PathCollectionFirst.Text = _fetchConfig.Frontend.GamePath[0];
                        PathCollectionSecond.Text = _fetchConfig.Frontend.GamePath[1];

                        PathCollectionFirst.BorderBrush = !String.IsNullOrEmpty(_fetchPath1525) ? _successBrush : _failBrush;
                        PathCollectionSecond.BorderBrush = !String.IsNullOrEmpty(_fetchPath28) ? _successBrush : _failBrush;
                    }

                    else
                    {
                        PathCollectionFirst.BorderBrush = _failBrush;
                        PathCollectionSecond.BorderBrush = _failBrush;
                    }
                }
            }
        }

        private void OnPathChanged(object? sender, TextChangedEventArgs? e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext != null ? _fetchContext.CurrentConfig : null;

            if (PlatformBox.SelectedItem != null && _fetchConfig != null)
            {
                if (PlatformBox.SelectedIndex == 0x00)
                {
                    var _fetchPathEmulator = PathEmulator.Text;

                    var _fetchPathKH1 = _fetchConfig.Emulator.RomPath[0x00];
                    var _fetchPathKH2 = _fetchConfig.Emulator.RomPath[0x01];

                    var _isValidEmulator = !String.IsNullOrEmpty(_fetchPathEmulator) && File.Exists(_fetchPathEmulator);
                    var _isValidIsoFile = (!String.IsNullOrEmpty(_fetchPathKH1) || !String.IsNullOrEmpty(_fetchPathKH2)) && (File.Exists(_fetchPathKH1) || File.Exists(_fetchPathKH2));

                    if (!_isValidEmulator || !_isValidIsoFile)
                        WeakReferenceMessenger.Default.Send(new BlockNextRequest());

                    else
                        WeakReferenceMessenger.Default.Send(new UnblockNextRequest());
                }

                else
                {
                    var _fetchPath1525 = PathCollectionFirst.Text;
                    var _fetchPath28 = PathCollectionSecond.Text;

                    var _isValid1525 = !String.IsNullOrEmpty(_fetchPath1525) && File.Exists(Path.Combine(_fetchPath1525, "KINGDOM HEARTS HD 1.5+2.5 ReMIX.exe"));
                    var _isValid28 = !String.IsNullOrEmpty(_fetchPath28) && File.Exists(Path.Combine(_fetchPath28, "KINGDOM HEARTS HD 2.8 Final Chapter Prologue.exe"));

                    if (!_isValid1525 && !_isValid28)
                        WeakReferenceMessenger.Default.Send(new BlockNextRequest());

                    else
                        WeakReferenceMessenger.Default.Send(new UnblockNextRequest());
                }
            }
        }

        private void OnGameSelectChanged(object? sender, SelectionChangedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext != null ? _fetchContext.CurrentConfig : null;

            if (PlatformBox.SelectedItem != null && _fetchConfig != null)
            {
                if (PlatformBox.SelectedIndex == 0x03)
                {
                    var _fetchSysDrive = Path.GetPathRoot(Environment.SystemDirectory);

                    var _fetchPath1525 = Path.Combine(_fetchSysDrive, "XboxGames", "KINGDOM HEARTS -HD 1.5+2.5 ReMIX");
                    var _fetchPath28 = Path.Combine(_fetchSysDrive, "XboxGames", "KINGDOM HEARTS 2.8 FINAL CHAPTER PROLOGUE");

                    var _isValid1525 = !String.IsNullOrEmpty(_fetchPath1525) && File.Exists(Path.Combine(_fetchPath1525, "KINGDOM HEARTS HD 1.5+2.5 ReMIX.exe"));
                    var _isValid28 = !String.IsNullOrEmpty(_fetchPath28) && File.Exists(Path.Combine(_fetchPath28, "KINGDOM HEARTS HD 2.8 Final Chapter Prologue.exe"));

                    _fetchConfig.Frontend.GamePath[0] = _fetchPath1525;
                    _fetchConfig.Frontend.GamePath[1] = _fetchPath28;

                    PathCollectionFirst.Text = _fetchConfig.Frontend.GamePath[0];
                    PathCollectionSecond.Text = _fetchConfig.Frontend.GamePath[1];

                    PathCollectionFirst.BorderBrush = _isValid1525 ? _successBrush : _failBrush;
                    PathCollectionSecond.BorderBrush = _isValid28 ? _successBrush : _failBrush;
                }
            }
        }
    }
}
