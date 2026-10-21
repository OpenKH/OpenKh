#pragma warning disable S4790
#pragma warning disable CS4014

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using OpenKh.Patcher;
using OpenKh.Tools.ModManager.Classes;
using OpenKh.Tools.ModManager.Dialogs;
using OpenKh.Tools.ModManager.Models;
using OpenKh.Tools.ModManager.Services;
using OpenKh.Tools.ModManager.Views;
using SharpYaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Ard.AreaDataScript;

namespace OpenKh.Tools.ModManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region EVENT HANDLERS

    protected void OnModPropertyChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Fetch the modlist from the sender.
        var _fetchModList = sender as ObservableCollection<ModModel>;

        if (CurrentConfig != null && _fetchModList != null)
        {
            // Fetch the mod memory path.
            var _fetchMemoryPath = Path.Combine(PathService.ResolveMod(CurrentConfig), "mod_memory.yml");

            // Create the actual mod memory collection.
            var _modMemoryList = new ObservableCollection<MemoryModel>();

            // For every mod that exists:
            foreach (var _fetchMod in _fetchModList)
            {
                // If the mod is invalid do not memorize it.
                if (!_fetchMod.ModValid)
                    continue;

                // Construct the memory structure (Hash, Is Active, Current Index)
                _modMemoryList.Add
                (
                    new MemoryModel
                    {
                        ModHash = ModService.ResolveMD5(_fetchMod, CurrentConfig),
                        ModActive = _fetchMod.ModActive,
                        ModIndex = _fetchModList.IndexOf(_fetchMod)
                    }
                );
            }

            // Serialize and commit the mod memory.
            var _fetchSerial = YamlSerializer.Serialize(_modMemoryList);
            File.WriteAllText(_fetchMemoryPath, _fetchSerial);
        }
    }

    protected void OnPresetChanged(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        if (CurrentConfig != null)
        {
            var _fetchTargetGame = CurrentConfig.Frontend.TargetGame;
            var _fetchPresetConfig = CurrentConfig.Frontend.TargetPreset;

            if (_fetchPresetConfig != null)
            {
                var _fetchCurrent = _fetchPresetConfig[(int)_fetchTargetGame];

                var _fetchPresetPath = Path.Combine(AppContext.BaseDirectory, "profile.yml");

                var _fetchPresetList = sender as ObservableCollection<PresetModel>;
                var _fetchGameList = _fetchPresetList.Where(x => x.TargetGame == CurrentConfig.Frontend.TargetGame);

                if (PresetMenu == null)
                {
                    PresetMenu = new ObservableCollection<object>()
                {
                    new MenuItem()
                    {
                        Header = "Create New Profile...",
                        Command = CreatePresetCommand,
                        InputGesture = new KeyGesture(Key.N, KeyModifiers.Alt)
                    },

                    new Separator(),

                    new MenuItem()
                    {
                        Header = "No Profiles Available",
                        IsEnabled = false
                    },
                };
                }

                PresetMenu = new ObservableCollection<object>(PresetMenu.Take(0x03));

                var _fetchMenu = PresetMenu.OfType<MenuItem>();
                var _doesDefaultExist = _fetchMenu.FirstOrDefault(x => x.Header as string == "Default") != null;

                if (_fetchGameList.Count() > 0x00)
                {
                    if (!_doesDefaultExist)
                    {
                        PresetMenu[0x02] = new MenuItem
                        {
                            Header = "Default",
                            ToggleType = MenuItemToggleType.Radio,
                            IsChecked = _fetchCurrent == "",
                            Command = SwitchPresetCommand,
                            CommandParameter = null,
                            InputGesture = new KeyGesture(Key.D, KeyModifiers.Alt)
                        };
                    }

                    for (int i = 0; i < _fetchGameList.Count(); i++)
                    {
                        var _fetchPreset = _fetchGameList.ElementAt(i);

                        PresetMenu.Add(new MenuItem
                        {
                            Header = _fetchPreset.Name,
                            ToggleType = MenuItemToggleType.Radio,
                            Command = SwitchPresetCommand,
                            IsChecked = _fetchCurrent == _fetchPreset.Name,
                            CommandParameter = _fetchPreset,
                            InputGesture = new KeyGesture(Key.D0 + i, KeyModifiers.Alt),
                            ContextMenu = new ContextMenu
                            {
                                ItemsSource = new[]
                                {
                                new MenuItem
                                {
                                    Header = "Rename Profile...",
                                    Command = RenamePresetCommand,
                                    CommandParameter = _fetchPreset
                                },

                                new MenuItem
                                {
                                    Header = "Delete Profile...",
                                    Command = DeletePresetCommand,
                                    CommandParameter = _fetchPreset
                                }
                            }
                            }
                        });
                    }
                }

                else
                {
                    PresetMenu[0x02] = new MenuItem
                    {
                        Header = "No Profiles Available",
                        IsEnabled = false
                    };
                }

                var _fetchSerial = YamlSerializer.Serialize(_fetchPresetList);
                File.WriteAllText(_fetchPresetPath, _fetchSerial);
            }
        }
    }

    #endregion

    #region PROPERTIES

    [ObservableProperty]
    private bool _initialized = false;

    [ObservableProperty]
    private ModModel? _currentMod = null;

    [ObservableProperty]
    private ObservableCollection<ModModel>? _installedMods = null;

    [ObservableProperty]
    private bool _configurationValid = true;

    [ObservableProperty]
    private Config? _currentConfig = null;

    [ObservableProperty]
    private bool _doesConfigExist = false;

    [ObservableProperty]
    private bool _hasModsInstalled = false;

    [ObservableProperty]
    private ObservableCollection<object>? _presetMenu;

    [ObservableProperty]
    private ObservableCollection<PresetModel>? _presetMemory;

    #endregion

    #region HELPER FUNCTIONS

    public MainView? FetchMainWindow()
    {
        var _fetchApplication = Avalonia.Application.Current;

        if (_fetchApplication == null)
            return null;

        var _fetchLifetime = _fetchApplication.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var _fetchMainView = _fetchLifetime != null ? _fetchLifetime.MainWindow : null;

        return _fetchMainView as MainView;
    }

    #endregion

    #region VIEWMODEL INITIALIZATION

    public MainViewModel() => InitViewModel();

    public void InitViewModel()
    {
        Initialized = false;

        CurrentMod = null;
        InstalledMods = null;
        HasModsInstalled = false;
        ConfigurationValid = true;

        PresetMenu = null;
        PresetMemory = null;

        // === Configuration Parsing and Verification === //

        // If the config is null, parse it.
        // If it does not exist, pop-up the setup wizard.

        if (CurrentConfig == null)
        {
            var CurrentConfigFile = Path.Combine(AppContext.BaseDirectory, "config.yml");

            if (File.Exists(CurrentConfigFile))
            {
                var CurrentConfigRAW = File.ReadAllText(CurrentConfigFile);

                DoesConfigExist = true;
                CurrentConfig = Config.Load();
            }

            else
            {
                DoesConfigExist = false;

                CurrentConfig = new Config
                {
                    Panacea = new Panacea(),

                    Frontend = new Frontend()
                    {
                        GamePath = new string[2],
                        TargetPlatform = Platform.STEAM,
                        ModBuildType = BuildType.PANACEA,
                        TargetGame = Game.KINGDOM_HEARTS_II,
                        TargetPreset = new string[5]
                    },

                    Emulator = new Emulator()
                    {
                        EmuPath = new string[1],
                        RomPath = new string[5],
                    }
                };

                CurrentConfig.Commit();
            }
        }

        ConfigurationValid = CurrentConfig.IsValid();

        // === Preset Parsing and Verification === //

        var _fetchPresetConfig = CurrentConfig.Frontend.TargetPreset;

        if (_fetchPresetConfig != null)
        {
            var _fetchTargetGame = CurrentConfig.Frontend.TargetGame;
            var _fetchTargetPreset = _fetchPresetConfig[(int)_fetchTargetGame];

            var _fetchPresetPath = Path.Combine(AppContext.BaseDirectory, "profile.yml");

            if (File.Exists(_fetchPresetPath))
            {
                var _fetchPresetRAW = File.ReadAllText(_fetchPresetPath);
                var _fetchPresetList = YamlSerializer.Deserialize<List<PresetModel>>(_fetchPresetRAW);

                if (_fetchPresetList != null)
                {
                    PresetMemory = new ObservableCollection<PresetModel>();
                    PresetMemory.CollectionChanged += OnPresetChanged;

                    OnPresetChanged(PresetMemory, null);

                    foreach (var _fetchPreset in _fetchPresetList)
                        PresetMemory.Add(_fetchPreset);

                    if (_fetchTargetPreset != null && PresetMenu != null)
                    {
                        var _fetchMenu = PresetMenu.OfType<MenuItem>();
                        var _fetchPresetItem = _fetchMenu.FirstOrDefault(x => x.Header as string == _fetchTargetPreset);

                        if (_fetchPresetItem == null)
                            _fetchPresetConfig[(int)_fetchTargetGame] = "";
                    }
                }
            }

            else
                File.WriteAllText(_fetchPresetPath, "[]");
        }

        IterateModlist();

        // Initialization is complete.
        Initialized = true;
    }

    #endregion

    #region MOD COMMANDS

    [RelayCommand]
    public async Task<bool> IterateModlist()
    {
        if (CurrentConfig != null)
        {
            InstalledMods = null;
            HasModsInstalled = false;

            // Construct the mod folder path for the specified game.

            var _fetchMods = new ObservableCollection<ModModel>();
            var _fetchModsPath = PathService.ResolveMod(CurrentConfig);

            // For each directory that exists in the mod folder:

            foreach (var _fetchDirectory in Directory.EnumerateDirectories(_fetchModsPath))
            {
                foreach (var _fetchChild in Directory.EnumerateDirectories(_fetchDirectory))
                {
                    var _fetchPath = _fetchChild;

                    // Construct the paths to YAML and PNG files.
                    var _fetchPathYaml = Path.Combine(_fetchPath, "mod.yml");
                    var _fetchPathIcon = Path.Combine(_fetchPath, "icon.png");

                    var _fetchPathGit = Path.Combine(_fetchPath, "git_info.yml");

                    GitModel? _fetchGitModel = null;

                    PresetModel _fetchLinkPreset = null;

                    // YAML don't do it? Don't do it!
                    if (!File.Exists(_fetchPathYaml))
                    {
                        if (PresetMemory != null)
                        {
                            var _fetchPathLink = Path.Combine(_fetchPath, "mod_link.yml");

                            if (!File.Exists(_fetchPathLink))
                                continue;

                            var _fetchLinkRAW = await File.ReadAllTextAsync(_fetchPathLink);
                            var _fetchLinkSerial = YamlSerializer.Deserialize<PresetModel>(_fetchLinkRAW);

                            if (_fetchLinkSerial != null)
                            {
                                var _tryFetchPreset = PresetMemory.FirstOrDefault(x => x.Name == _fetchLinkSerial.Name);

                                if (_tryFetchPreset != null || _fetchLinkSerial.Name == "Default")
                                {
                                    var _fetchPresetPath = PathService.ResolvePreset(CurrentConfig);
                                    var _fetchBareModPath = PathService.ResolveMod(CurrentConfig, true);

                                    var _fetchLinkedPathMain = _tryFetchPreset == null ? Path.Combine(_fetchBareModPath, Config.GameShorthand[CurrentConfig.Frontend.TargetGame], Path.GetFileName(_fetchDirectory), Path.GetFileName(_fetchChild))
                                                                                       : Path.Combine(_fetchPresetPath, _tryFetchPreset.FolderName, Path.GetFileName(_fetchDirectory), Path.GetFileName(_fetchChild));

                                    if (!Directory.Exists(_fetchLinkedPathMain))
                                        continue;

                                    else
                                    {
                                        _fetchPath = _fetchLinkedPathMain;

                                        _fetchPathYaml = Path.Combine(_fetchPath, "mod.yml");
                                        _fetchPathIcon = Path.Combine(_fetchPath, "icon.png");

                                        _fetchPathGit = Path.Combine(_fetchPath, ".git");

                                        _fetchLinkPreset = _tryFetchPreset ?? _fetchLinkSerial;
                                    }
                                }

                                else
                                    continue;
                            }

                            else
                                continue;
                        }

                        else
                            continue;
                    }

                     var _metadata = Metadata.Read(_fetchPathYaml);

                    // If the metadata is valid, parse the mod and push it to the ViewModel.

                    if (_metadata.IsValid)
                    {
                        var _modModel = new ModModel
                        {
                            ModTitle = _metadata.Title,
                            ModAuthor = _metadata.OriginalAuthor,
                            ModDescription = _metadata.Description,
                            ModPath = _fetchPath,
                            ModLinkPreset = _fetchLinkPreset,
                            ModFilesList = _metadata.Assets.Select(x => x.Name).ToArray(),
                            ModIcon = File.Exists(_fetchPathIcon) ? new Bitmap(_fetchPathIcon) : null,
                            ModActive = true,
                            ModValid = true
                        };

                        if (_metadata.Preferences != null)
                        {
                            _modModel.ModPreferences = new List<PreferenceModel>();

                            foreach (var _preference in _metadata.Preferences)
                            {
                                var _fetchPref = new PreferenceModel
                                {
                                    Title = _preference.Title,
                                    Key = _preference.Key,
                                    Description = _preference.Description,
                                    Type = _preference.Type,
                                    Options = _preference.Options,
                                    Value = _preference.Value
                                };

                                _modModel.ModPreferences.Add(_fetchPref);
                            }
                        }

                        // We have found a Git Repository, let's see what's up.

                        if (File.Exists(_fetchPathGit))
                        {
                            var _fetchGitRAW = await File.ReadAllTextAsync(_fetchPathGit);
                            _fetchGitModel = YamlSerializer.Deserialize<GitModel>(_fetchGitRAW);

                            if (_fetchGitModel != null)
                            {
                                _modModel.ModTitle = String.IsNullOrEmpty(_modModel.ModTitle) ? _fetchGitModel.Repository : _modModel.ModTitle;
                                _modModel.ModAuthor = String.IsNullOrEmpty(_modModel.ModAuthor) ? _fetchGitModel.Author : _modModel.ModAuthor;

                                _modModel.ModSource = new Uri($"https://{_fetchGitModel.Platform}/{_fetchGitModel.Author}/{_fetchGitModel.Repository}");
                                _modModel.ModIssues = new Uri($"https://{_fetchGitModel.Platform}/{_fetchGitModel.Author}/{_fetchGitModel.Repository}/issues");

                                _modModel.ModPlatform = _fetchGitModel.Platform;
                                _modModel.ModGitAddress = $"{Path.GetFileName(_fetchDirectory)}/{Path.GetFileName(_fetchChild)}@{_fetchGitModel.Platform}";

                                _modModel.ModLatestCommit = _fetchGitModel.LatestCommit ?? new DateTime();
                            }
                        }

                        _fetchMods.Add(_modModel);
                    }

                    // Otherwise, make it known that the mod sucks ASS and is no good for us, but still push it to the ViewModel so we know about it :D

                    else
                    {
                        var uri = new Uri("avares://OpenKh.Tools.ModManager/Assets/invalid_mod.png");

                        var _modModel = new ModModel
                        {
                            ModTitle = _metadata.Title,
                            ModAuthor = "This mod is invalid!",
                            ModDescription = "This mod contains errors within its YAML file. Please check the formatting!",
                            ModIcon = new Bitmap(AssetLoader.Open(uri)),
                            ModPath = _fetchChild,
                            ModActive = false,
                            ModValid = false
                        };

                        _fetchMods.Add(_modModel);
                    }
                }
            }

            // When all mods are loaded, start processing the "mod memory" for the targeted game.\
            // If the mod memory does not exist, assume default order and mark all active.

            var _fetchModMemoryPath = Path.Combine(_fetchModsPath, "mod_memory.yml");

            if (File.Exists(_fetchModMemoryPath))
            {
                // Fetch the raw YAML data and deserialize it.
                var _fetchRawYaml = await File.ReadAllTextAsync(_fetchModMemoryPath);
                var _fetchModMemory = YamlSerializer.Deserialize<ObservableCollection<MemoryModel>>(_fetchRawYaml);

                // Make a temporary array for us to order the mods.
                // And a temporary list for us to commit the mods.

                var _tempModArray = new ModModel[_fetchModMemory.Count];
                var _tempModList = new List<ModModel>();

                foreach (var _fetchMemory in _fetchModMemory)
                {
                    var _fetchMod = _fetchMods.FirstOrDefault(x => ModService.ResolveMD5(x, CurrentConfig) == _fetchMemory.ModHash);

                    if (_fetchMod == null || !_fetchMod.ModValid)
                        continue;

                    _fetchMod.ModActive = _fetchMemory.ModActive;
                    _tempModArray[_fetchMemory.ModIndex] = _fetchMod;

                    _fetchMods.Remove(_fetchMod);
                }

                // Add all the existing mods from the array.
                _tempModList.AddRange(_tempModArray.Where(x => x != null));

                // Add all the valid mods from the original list which didn't exist on the mod memory.
                _tempModList.AddRange(_fetchMods.Where(x => x.ModValid));

                // Add all the invalid mods from the original list which didn't exist on the mod memory.
                _tempModList.AddRange(_fetchMods.Where(x => !x.ModValid));

                // Sync to the actual installed mods collection.
                InstalledMods = new ObservableCollection<ModModel>(_tempModList);
            }

            else
                InstalledMods = _fetchMods;

            // Register mod property event to handle mod memory.
            InstalledMods.CollectionChanged += OnModPropertyChanged;

            // If there is at least one mod, select the first mod and declare we have mods.
            if (InstalledMods.Count > 0)
            {
                HasModsInstalled = true;
                CurrentMod = InstalledMods.First();
            }

            Task.Run(async () =>
            {
                foreach (var _fetchMod in InstalledMods)
                {
                    var _fetchGitPath = Path.Combine(_fetchMod.ModPath, "git_info.yml");

                    if (File.Exists(_fetchGitPath))
                    {
                        var _fetchGitRAW = File.ReadAllText(_fetchGitPath);
                        var _fetchGitSerial = YamlSerializer.Deserialize<GitModel>(_fetchGitRAW);

                        var _fetchLatest = await GitService.FetchLatestCommit(_fetchGitSerial.Platform, _fetchGitSerial.Author, _fetchGitSerial.Repository, _fetchGitSerial.Branch);
                        var _fetchCompare = DateTime.Compare(_fetchMod.ModLatestCommit, _fetchLatest ?? _fetchMod.ModLatestCommit);

                        if (_fetchCompare < 0x00)
                            _fetchMod.ModUpdate = true;
                    }
                }
            });

            return true;
        }

        return false;
    }

    [RelayCommand]
    private async Task<bool> Install(string? inputParameter)
    {
        var _fetchErroredList = new List<string>();
        var _fetchSuccessList = new List<string>();
        var _fetchLinkedList = new Dictionary<string, PresetModel>();

        PresetModel _fetchPresetTarget = null;

        var _fetchTargetGame = CurrentConfig.Frontend.TargetGame;
        var _fetchPresetArray = CurrentConfig.Frontend.TargetPreset;

        if (CurrentConfig != null && InstalledMods != null)
        {
            var _fetchMainView = FetchMainWindow();

            if (_fetchMainView == null)
                return false;

            var _fetchResult = inputParameter ?? await DialogService.ShowInput(_fetchMainView, "Install a new Mod", "Enter the name of the repository to install.", "Install", "Ex. OpenKH/a-very-cool-mod@github.com", null, "Select and Install an Archive or Script", async (inputParent) =>
            {
                var _fetchFiles = await _fetchMainView.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select an Archive or Script File...",
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("OpenKH Mod Archive") { Patterns = ["*.zip"] },
                        new FilePickerFileType("PCPatch Package") { Patterns = ["*.kh1pcpatch", "*.kh2pcpatch", "*.bbspcpatch", "*.compcpatch", "*.dddpcpatch"] },
                        new FilePickerFileType("LuaBackend Script") { Patterns = ["*.lua"] },
                        new FilePickerFileType("All Files") { Patterns = ["*"] },
                    ]
                });

                if (_fetchFiles.Count >= 1)
                    return _fetchFiles[0].Path.LocalPath;

                else
                    return null;
            });

            var _fetchModPath = PathService.ResolveMod(CurrentConfig);
            var _fetchInstallResult = 0x00;

            if (_fetchResult != null && !string.IsNullOrEmpty(_fetchResult))
            {
                var _fetchFileInfo = new FileInfo(@$"{_fetchResult}");

                if (_fetchFileInfo.Exists)
                {
                    SafePtr _progressCurrentPtr = 0x00;
                    SafePtr _progressMaximumPtr = 0x00;

                    SafePtr _progressTextPtr = "Processing Local Files: {0} / {3}";

                    var _singleProgress = DialogService.ShowProgress(_fetchMainView, "Installing Mod...", "Installing declared Mod... Please be patient...", ModService.CancelTokenSource, _progressCurrentPtr, _progressMaximumPtr, _progressTextPtr);

                    _fetchInstallResult =
                    await ModService.InstallLocal
                    (
                        _fetchResult,
                        CurrentConfig,
                        (int processed, int total) =>
                        {
                           _progressCurrentPtr /= processed;
                           _progressMaximumPtr /= total;

                            if (ModService.CancelToken.IsCancellationRequested)
                                return false;

                            return true;
                        }
                    );

                    // Free all the pointers allocated.

                    _progressTextPtr.Dispose();

                    _progressMaximumPtr.Dispose();
                    _progressCurrentPtr.Dispose();
                }

                else
                {
                    var _fetchMultiInstall = _fetchResult.Contains(';') ? _fetchResult.Split(';') : null;

                    if (_fetchMultiInstall == null)
                    {
                        var _fetchModAuthor = _fetchResult.Split('/').First();
                        var _fetchModName = _fetchResult.Split('/').Last();

                        var _fetchModNormalized = _fetchResult.Split(':').First()
                                                              .Split('@').First();

                        var _fetchBytes = Encoding.ASCII.GetBytes(_fetchModNormalized);
                        var _fetchModHash = MD5.HashData(_fetchBytes);

                        if (_fetchPresetArray != null && PresetMemory != null)
                        {
                            var _fetchExistsList = new List<PresetModel>();
                            var _fetchPresetCurrent = _fetchPresetArray[(int)_fetchTargetGame];

                            if (_fetchPresetCurrent != "")
                            {
                                var _fetchBareModPath = PathService.ResolveMod(CurrentConfig, true);
                                var _fetchMemoryPath = Path.Combine(_fetchBareModPath, Config.GameShorthand[_fetchTargetGame], "mod_memory.yml");

                                if (File.Exists(_fetchMemoryPath))
                                {
                                    var _fetchMemoryRAW = await File.ReadAllTextAsync(_fetchMemoryPath);
                                    var _fetchMemorySerial = YamlSerializer.Deserialize<List<MemoryModel>>(_fetchMemoryRAW);

                                    var _fetchExists = _fetchMemorySerial.FirstOrDefault(x => x.ModHash == Convert.ToHexString(_fetchModHash));

                                    if (_fetchExists != null)
                                        _fetchExistsList.Add(new PresetModel { Name = "Default", TargetGame = _fetchTargetGame });
                                }
                            }

                            foreach (var _fetchPreset in PresetMemory)
                            {
                                if (_fetchPreset.Name == _fetchPresetCurrent)
                                    continue;

                                var _fetchPresetPath = PathService.ResolvePreset(CurrentConfig);
                                var _fetchMemoryPath = Path.Combine(_fetchPresetPath, _fetchPreset.FolderName, "mod_memory.yml");

                                if (!File.Exists(_fetchMemoryPath))
                                    continue;

                                var _fetchMemoryRAW = await File.ReadAllTextAsync(_fetchMemoryPath);
                                var _fetchMemorySerial = YamlSerializer.Deserialize<List<MemoryModel>>(_fetchMemoryRAW);

                                var _fetchExists = _fetchMemorySerial.FirstOrDefault(x => x.ModHash == Convert.ToHexString(_fetchModHash));

                                if (_fetchExists != null)
                                    _fetchExistsList.Add(_fetchPreset);
                            }

                            if (_fetchExistsList.Count == 1)
                            {
                                var _fetchPreset = _fetchExistsList.First();
                                var _fetchQuestion = await DialogService.ShowQuestion(_fetchMainView, "Mod Exists in Preset", $"The Mod you are trying to install currently exists in the Preset: \"{_fetchPreset.Name}\".\n" +
                                                                                                                              $"Would you like to link this install to that Preset?\n" +
                                                                                                                              $"Saying \"No\" will install a new instance of this Mod.");

                                if (_fetchQuestion)
                                    _fetchPresetTarget = _fetchPreset;
                            }

                            else if (_fetchExistsList.Count >= 2)
                            {
                                var _fetchDialog = await DialogService.ShowOptions(_fetchMainView, "Mod Exists in Multiple Presets", $"The Mod you are trying to install currently exists in multiple Presets.\n" +
                                                                                                                                     $"If you would like to link this Mod to a Preset, please select a Preset from below and click \"Link\"\n" +
                                                                                                                                     $"Otherwise, click \"Install\" to install a new instance of this Mod.", "Link", _fetchExistsList.Select(x => x.Name).ToArray(), "Install");

                                if (_fetchDialog != null)
                                    _fetchPresetTarget = _fetchExistsList.FirstOrDefault(x => x.Name == _fetchDialog);
                            }

                            if (_fetchPresetTarget != null)
                                goto SKIP_INSTALL;
                        }

                        // Allocate the pointers necessary.

                        SafePtr _progressCurrentPtr = 0.00;
                        SafePtr _progressMaximumPtr = -1.00;

                        SafePtr _progressTextPtr = "Sending REST Request... Please wait...";

                        var _singleProgress = DialogService.ShowProgress(_fetchMainView, "Installing Mod...", "Installing declared Mod... Please be patient...", ModService.CancelTokenSource, _progressCurrentPtr, _progressMaximumPtr, _progressTextPtr);

                        _fetchInstallResult =
                        await ModService.InstallGit
                        (
                            _fetchResult,
                            CurrentConfig,
                            (progress, totalLength, isLocal) =>
                            {
                                if (!isLocal)
                                {
                                    var _isGibibyte = totalLength > 1073741824;
                                    var _isMebibyte = totalLength > 1048576;

                                    _progressTextPtr /= "Downloading Zipball: {0:0.00} / {3:0.00} " + (_isGibibyte ? "GiB" : (_isMebibyte ? "MiB" : "KiB"));

                                    var _fetchCurrByte = (double)progress / Math.Pow(1024, Convert.ToInt32(_isMebibyte) + Convert.ToInt32(_isGibibyte) + 1);
                                    var _fetchTotalByte = (double)totalLength / Math.Pow(1024, Convert.ToInt32(_isMebibyte) + Convert.ToInt32(_isGibibyte) + 1);

                                    _progressCurrentPtr /= _fetchCurrByte;
                                    _progressMaximumPtr /= _fetchTotalByte;
                                }

                                else
                                {
                                    _progressTextPtr /= "Extracting Zipball: {0} / {3}";

                                    _progressCurrentPtr /= Convert.ToDouble(progress);
                                    _progressMaximumPtr /= Convert.ToDouble(totalLength);
                                }

                                if (ModService.CancelToken.IsCancellationRequested)
                                    return false;

                                return true;
                            },
                            inputParameter != null
                        );

                        // Free all the pointers allocated.

                        _progressTextPtr.Dispose();

                        _progressMaximumPtr.Dispose();
                        _progressCurrentPtr.Dispose();
                    }

                    else
                    {
                        // Allocate all the pointers to use for the progress bars.

                        SafePtr _firstCurrentPtr = 0x00;
                        SafePtr _firstMaximumPtr = 0x00;

                        SafePtr _secondCurrentPtr = 0.00;
                        SafePtr _secondMaximumPtr = -1.00;

                        SafePtr _firstTextPtr = $"Processing Mod: N/A";
                        SafePtr _secondTextPtr = "Sending REST Request... Please wait...";

                        // Show the dialog.

                        var _multiProgress = DialogService.ShowProgress(_fetchMainView, "Installing Mod...", "Installing declared Mod... Please be patient...", ModService.CancelTokenSource, _firstCurrentPtr, _firstMaximumPtr, _firstTextPtr, _secondCurrentPtr, _secondMaximumPtr, _secondTextPtr);

                        for (int i = 0; i < _fetchMultiInstall.Length; i++)
                        {
                            var _fetchMod = _fetchMultiInstall[i];

                            var _fetchModAuthor = _fetchMod.Split('/').First();
                            var _fetchModName = _fetchMod.Split('/').Last();

                            var _fetchModNormalized = _fetchMod.Split(':').First()
                                                               .Split('@').First();

                            var _fetchBytes = Encoding.ASCII.GetBytes(_fetchModNormalized);
                            var _fetchModHash = MD5.HashData(_fetchBytes);

                            if (_fetchPresetArray != null && PresetMemory != null)
                            {
                                var _fetchExistsList = new List<PresetModel>();
                                var _fetchPresetCurrent = _fetchPresetArray[(int)_fetchTargetGame];

                                if (_fetchPresetCurrent != "")
                                {
                                    var _fetchBareModPath = PathService.ResolveMod(CurrentConfig, true);
                                    var _fetchMemoryPath = Path.Combine(_fetchBareModPath, Config.GameShorthand[_fetchTargetGame], "mod_memory.yml");

                                    if (File.Exists(_fetchMemoryPath))
                                    {
                                        var _fetchMemoryRAW = await File.ReadAllTextAsync(_fetchMemoryPath);
                                        var _fetchMemorySerial = YamlSerializer.Deserialize<List<MemoryModel>>(_fetchMemoryRAW);

                                        var _fetchExists = _fetchMemorySerial.FirstOrDefault(x => x.ModHash == Convert.ToHexString(_fetchModHash));

                                        if (_fetchExists != null)
                                            _fetchExistsList.Add(new PresetModel { Name = "Default", TargetGame = _fetchTargetGame });
                                    }
                                }

                                foreach (var _fetchPreset in PresetMemory)
                                {
                                    if (_fetchPreset.Name == _fetchPresetCurrent)
                                        continue;

                                    var _fetchPresetPath = PathService.ResolvePreset(CurrentConfig);
                                    var _fetchMemoryPath = Path.Combine(_fetchPresetPath, _fetchPreset.FolderName, "mod_memory.yml");

                                    if (!File.Exists(_fetchMemoryPath))
                                        continue;

                                    var _fetchMemoryRAW = await File.ReadAllTextAsync(_fetchMemoryPath);
                                    var _fetchMemorySerial = YamlSerializer.Deserialize<List<MemoryModel>>(_fetchMemoryRAW);

                                    var _fetchExists = _fetchMemorySerial.FirstOrDefault(x => x.ModHash == Convert.ToHexString(_fetchModHash));

                                    if (_fetchExists != null)
                                        _fetchExistsList.Add(_fetchPreset);
                                }

                                if (_fetchExistsList.Count == 1)
                                {
                                    var _fetchPreset = _fetchExistsList.First();
                                    var _fetchQuestion = await DialogService.ShowQuestion(_fetchMainView, "Mod Exists in Preset", $"The Mod named \"{_fetchModNormalized}\" currently exists in the Preset: \"{_fetchPreset.Name}\".\n" +
                                                                                                                                  $"Would you like to link this install to that Preset?\n" +
                                                                                                                                  $"Saying \"No\" will install a new instance of this Mod.");

                                    if (_fetchQuestion)
                                    {
                                        _fetchSuccessList.Add(_fetchModNormalized);
                                        _fetchLinkedList.Add(_fetchModNormalized, _fetchPreset);
                                        continue;
                                    }
                                }

                                else if (_fetchExistsList.Count >= 2)
                                {
                                    var _fetchDialog = await DialogService.ShowOptions(_fetchMainView, "Mod Exists in Multiple Presets", $"The Mod named \"{_fetchModNormalized}\" currently exists in multiple Presets.\n" +
                                                                                                                                         $"If you would like to link this Mod to a Preset, please select a Preset from below and click \"Link\"\n" +
                                                                                                                                         $"Otherwise, click \"Install\" to install a new instance of this Mod.", "Link", _fetchExistsList.Select(x => x.Name).ToArray(), "Install");

                                    if (_fetchDialog != null)
                                    {
                                        _fetchSuccessList.Add(_fetchModNormalized);
                                        _fetchLinkedList.Add(_fetchModNormalized, _fetchExistsList.FirstOrDefault(x => x.Name == _fetchDialog));
                                        continue;
                                    }
                                }
                            }




                            var _fetchInstallStatus =
                            await ModService.InstallGit
                            (
                                _fetchMod,
                                CurrentConfig,
                                (progress, totalLength, isLocal) =>
                                {
                                    _firstTextPtr /= $"Processing Mod: {_fetchMod}";

                                    _firstCurrentPtr /= Convert.ToDouble(i + 0x01);
                                    _firstMaximumPtr /= Convert.ToDouble(_fetchMultiInstall.Length);

                                    if (!isLocal)
                                    {
                                        var _isGibibyte = totalLength > 1073741824;
                                        var _isMebibyte = totalLength > 1048576;

                                        _secondTextPtr /= "Downloading Zipball: {0:0.00} / {3:0.00} " + (_isGibibyte ? "GiB" : (_isMebibyte ? "MiB" : "KiB"));

                                        var _fetchCurrByte = (double)progress / Math.Pow(1024, Convert.ToInt32(_isMebibyte) + Convert.ToInt32(_isGibibyte) + 1);
                                        var _fetchTotalByte = (double)totalLength / Math.Pow(1024, Convert.ToInt32(_isMebibyte) + Convert.ToInt32(_isGibibyte) + 1);

                                        _secondCurrentPtr /= _fetchCurrByte;
                                        _secondMaximumPtr /= _fetchTotalByte;
                                    }

                                    else
                                    {
                                        _secondTextPtr /= "Extracting Zipball: {0} / {3}";

                                        _secondCurrentPtr /= Convert.ToDouble(progress);
                                        _secondMaximumPtr /= Convert.ToDouble(totalLength);
                                    }

                                    if (ModService.CancelToken.IsCancellationRequested)
                                        return false;

                                    return true;
                                }
                            );

                            if (_fetchInstallStatus == 0x01)
                                _fetchErroredList.Add(_fetchMod);

                            else
                                _fetchSuccessList.Add(_fetchMod);

                            if (ModService.CancelToken.IsCancellationRequested)
                                break;
                        }

                        // Free all the pointers allocated.

                        _secondTextPtr.Dispose();
                        _firstTextPtr.Dispose();

                        _secondMaximumPtr.Dispose();
                        _secondCurrentPtr.Dispose();

                        _firstMaximumPtr.Dispose();
                        _firstCurrentPtr.Dispose();

                        // Return the result.

                        _fetchInstallResult = 0x02;
                    }
                }

            SKIP_INSTALL:

                if (ModService.CancelToken.IsCancellationRequested || _fetchInstallResult == 0x03)
                    return false;

                var _fetchGameNames = _fetchMainView.GameBox.Items.OfType<ComboBoxItem>()
                                                                  .Select(x => x.Content as string)
                                                                  .ToList();

                switch (_fetchInstallResult)
                {
                    case 0x00:
                    {
                        var _fetchModFolder = "";

                        var _fetchModAuthor = "";
                        var _fetchModName = "";

                        var _fetchGitMod = false;

                        if (_fetchFileInfo.Exists)
                            _fetchModFolder = Path.Combine(_fetchModPath, $"local/{Path.GetFileNameWithoutExtension(_fetchResult)}");

                        else
                        {
                            _fetchModAuthor = _fetchResult.Split('/').First();
                            _fetchModName = _fetchResult.Split('/').Last();

                            _fetchModName = _fetchModName.Split(':').First();
                            _fetchModName = _fetchModName.Split('@').First();

                            _fetchModFolder = Path.Combine(_fetchModPath, _fetchModAuthor, _fetchModName);
                            _fetchGitMod = true;
                        }

                        var _fetchPresetPath = PathService.ResolvePreset(CurrentConfig);
                        var _fetchBareModPath = PathService.ResolveMod(CurrentConfig, true);

                        if (_fetchPresetTarget != null)
                            _fetchModFolder = _fetchPresetTarget.Name == "Default" ? Path.Combine(_fetchBareModPath, Config.GameShorthand[_fetchTargetGame], _fetchModAuthor, _fetchModName) : Path.Combine(_fetchPresetPath, _fetchPresetTarget.FolderName, _fetchModAuthor, _fetchModName);

                        var _fetchPathGit = Path.Combine(_fetchModFolder, "git_info.yml");
                        var _fetchYamlName = Path.Combine(_fetchModFolder, "mod.yml");
                        var _fetchPathIcon = Path.Combine(_fetchModFolder, "icon.png");

                        var _fetchMetadata = Metadata.Read(_fetchYamlName);

                        if (_fetchMetadata.Game != null)
                        { 
                            var _fetchMetadataGame = Config.GameShorthand.FirstOrDefault(x => x.Value == _fetchMetadata.Game).Key;

                            var _fullNameMetadata = _fetchGameNames[(int)_fetchMetadataGame];
                            var _fullNameCurrent = _fetchGameNames[(int)CurrentConfig.Frontend.TargetGame];

                            if (_fetchMetadataGame != CurrentConfig.Frontend.TargetGame)
                            {
                                if (Directory.Exists(_fetchModFolder))
                                    Directory.Delete(_fetchModFolder, true);

                                await DialogService.ShowMessage(_fetchMainView, "Unsupported Game", $"This mod is made for {_fullNameMetadata} but you are trying to install it for {_fullNameCurrent}!\nPlease check to make sure the selected game matches the mod's requirements.", MessageType.ERROR);

                                return false;
                            }
                        }

                        if (_fetchMetadata.Dependencies != null)
                        {
                            var _fetchDependencies = _fetchMetadata.Dependencies;
                            var _fetchMissingDeps = new List<string>();

                            foreach (var _fetchDependency in _fetchDependencies)
                            {
                                var _doesHaveDependency = InstalledMods.FirstOrDefault(x => x.ModGitAddress != null && x.ModGitAddress.ToLower() == _fetchDependency.ToLower());

                                if (_doesHaveDependency == null)
                                    _fetchMissingDeps.Add(_fetchDependency);
                            }

                            if (_fetchMissingDeps.Count > 0)
                            {
                                var _fetchDepString = String.Join("\n- ", _fetchMissingDeps);
                                var _fetchDepQuestion = await DialogService.ShowQuestion(_fetchMainView, "Missing Dependencies", $"This Mod has some dependencies that are not installed.\nTo proceed, all the following dependencies must be installed:\n\n- {_fetchDepString}\n\nDo you want to proceed? [Not doing so will cancel this mod's install.]");

                                if (!_fetchDepQuestion)
                                {
                                    if (Directory.Exists(_fetchModFolder))
                                        Directory.Delete(_fetchModFolder, true);

                                    return false;
                                }

                                else
                                {
                                    var _fetchInstallStr = String.Join(";", _fetchMissingDeps);

                                    var _fetchInstall = await Install(_fetchInstallStr);

                                    if (!_fetchInstall)
                                    {
                                        if (Directory.Exists(_fetchModFolder))
                                            Directory.Delete(_fetchModFolder, true);

                                        return false;
                                    }
                                }
                            }
                        }

                        if (_fetchMetadata.IsValid)
                        {
                            var _modModel = new ModModel
                            {
                                ModTitle = !String.IsNullOrEmpty(_fetchMetadata.Title) ? _fetchMetadata.Title : _fetchModName,
                                ModAuthor = !String.IsNullOrEmpty(_fetchMetadata.OriginalAuthor) ? _fetchMetadata.OriginalAuthor : _fetchModAuthor,
                                ModGitAddress = _fetchGitMod ? _fetchResult : null,
                                ModLinkPreset = _fetchPresetTarget,
                                ModDescription = !String.IsNullOrEmpty(_fetchMetadata.Description) ? _fetchMetadata.Description : "This mod does not have a description, but we believe it's pretty cool.",
                                ModPath = _fetchModFolder,
                                ModFilesList = _fetchMetadata.Assets.Select(x => x.Name).ToArray(),
                                ModIcon = File.Exists(_fetchPathIcon) ? new Bitmap(_fetchPathIcon) : null,
                                ModActive = true,
                                ModValid = true
                            };

                            if (_fetchMetadata.Preferences != null)
                            {
                                _modModel.ModPreferences = new List<PreferenceModel>();

                                foreach (var _preference in _fetchMetadata.Preferences)
                                {
                                    var _fetchPref = new PreferenceModel
                                    {
                                        Title = _preference.Title,
                                        Key = _preference.Key,
                                        Description = _preference.Description,
                                        Type = _preference.Type,
                                        Options = _preference.Options,
                                        Value = _preference.Value
                                    };

                                    _modModel.ModPreferences.Add(_fetchPref);
                                }
                            }

                            if (File.Exists(_fetchPathGit))
                            {
                                var _fetchGitRAW = await File.ReadAllTextAsync(_fetchPathGit);
                                var _fetchSerial = YamlSerializer.Deserialize<GitModel>(_fetchGitRAW);

                                if (_fetchSerial != null)
                                {
                                    _modModel.ModTitle = String.IsNullOrEmpty(_modModel.ModTitle) ? _fetchSerial.Repository : _modModel.ModTitle;
                                    _modModel.ModAuthor = String.IsNullOrEmpty(_modModel.ModAuthor) ? _fetchSerial.Author : _modModel.ModAuthor;

                                    _modModel.ModSource = new Uri($"https://{_fetchSerial.Platform}/{_fetchSerial.Author}/{_fetchSerial.Repository}");
                                    _modModel.ModIssues = new Uri($"https://{_fetchSerial.Platform}/{_fetchSerial.Author}/{_fetchSerial.Repository}/issues");

                                    _modModel.ModPlatform = _fetchSerial.Platform;
                                    _modModel.ModLatestCommit = _fetchSerial.LatestCommit ?? new DateTime();
                                }
                            }

                            var _fetchExistingMod = InstalledMods.FirstOrDefault(x => x.ModPath == _modModel.ModPath);

                            if (_fetchExistingMod != null)
                                InstalledMods.Remove(_fetchExistingMod);

                            var _fetchFirstInvalid = InstalledMods.FirstOrDefault(x => !x.ModValid);
                            var _fetchModIndex = _fetchFirstInvalid != null ? InstalledMods.IndexOf(_fetchFirstInvalid) : 0x00;

                            InstalledMods.Insert(_fetchModIndex, _modModel);
                            HasModsInstalled = true;

                            if (_fetchPresetTarget != null)
                            {
                                var _fetchTargetFolder = Path.Combine(_fetchModPath, _fetchModAuthor, _fetchModName);

                                if (!Directory.Exists(_fetchTargetFolder))
                                    Directory.CreateDirectory(_fetchTargetFolder);

                                var _fetchLinkPath = Path.Combine(_fetchTargetFolder, "mod_link.yml");
                                var _fetchLinkSerial = YamlSerializer.Serialize(_fetchPresetTarget);

                                File.WriteAllText(_fetchLinkPath, _fetchLinkSerial);
                            }
                        }

                        else
                        {
                            var uri = new Uri("avares://OpenKh.Tools.ModManager/Assets/invalid_mod.png");

                            var _modModel = new ModModel
                            {
                                ModTitle = !String.IsNullOrEmpty(_fetchMetadata.Title) ? _fetchMetadata.Title : _fetchModName,
                                ModAuthor = "This mod is invalid!",
                                ModDescription = "This mod contains errors within its YAML file. Please check the formatting!",
                                ModIcon = new Bitmap(AssetLoader.Open(uri)),
                                ModPath = _fetchModFolder,
                                ModActive = false,
                                ModValid = false
                            };

                            var _fetchExistingMod = InstalledMods.FirstOrDefault(x => x.ModPath == _modModel.ModPath);

                            if (_fetchExistingMod != null)
                                InstalledMods.Remove(_fetchExistingMod);

                            InstalledMods.Add(_modModel);
                        }

                        return true;
                    }

                    case 0x01:
                        await DialogService.ShowMessage(_fetchMainView, "ERROR - Invalid Mod", "This is NOT a valid/compliant Mod Manager Mod. Please make sure it exists and it is valid.", MessageType.ERROR);
                        return false;

                    case 0x02:
                    {
                        var _fetchIncompatibleList = new List<Tuple<string, string>>();
                        var _fetchNoDependencyList = new List<string>();

                        foreach (var _fetchSuccess in _fetchSuccessList)
                        {
                            var _tryFetchPreset = _fetchLinkedList.FirstOrDefault(x => x.Key == _fetchSuccess).Value;

                            var _fetchPresetPath = PathService.ResolvePreset(CurrentConfig);
                            var _fetchBareModPath = PathService.ResolveMod(CurrentConfig, true);

                            var _fetchModAuthor = _fetchSuccess.Split('/').First();
                            var _fetchModName = _fetchSuccess.Split('/').Last();

                            // Just in case we have platform and branch info, lose them.
                            _fetchModName = _fetchModName.Split(':').First();
                            _fetchModName = _fetchModName.Split('@').First();

                            var _fetchCurrentPath = Path.Combine(_fetchModPath, _fetchModAuthor, _fetchModName);

                            if (_tryFetchPreset != null)
                                _fetchCurrentPath = _tryFetchPreset.Name == "Default" ? Path.Combine(_fetchBareModPath, Config.GameShorthand[_fetchTargetGame], _fetchSuccess) : Path.Combine(_fetchPresetPath, _tryFetchPreset.FolderName, _fetchSuccess);

                            var _fetchPathGit = Path.Combine(_fetchCurrentPath, "git_info.yml");
                            var _fetchYamlName = Path.Combine(_fetchCurrentPath, "mod.yml");
                            var _fetchPathIcon = Path.Combine(_fetchCurrentPath, "icon.png");

                            var _fetchMetadata = Metadata.Read(_fetchYamlName);

                            if (_fetchMetadata.Game != null)
                            {
                                var _fetchMetadataGame = Config.GameShorthand.FirstOrDefault(x => x.Value == _fetchMetadata.Game).Key;

                                var _fullNameMetadata = _fetchGameNames[(int)_fetchMetadataGame];
                                var _fullNameCurrent = _fetchGameNames[(int)CurrentConfig.Frontend.TargetGame];

                                if (_fetchMetadataGame != CurrentConfig.Frontend.TargetGame)
                                {
                                    if (Directory.Exists(_fetchCurrentPath))
                                        Directory.Delete(_fetchCurrentPath, true);

                                    var _fetchGameTuple = new Tuple<string, string>(_fetchSuccess, _fullNameMetadata);
                                    _fetchIncompatibleList.Add(_fetchGameTuple);

                                    continue;
                                }
                            }

                            if (_fetchMetadata.Dependencies != null)
                            {
                                var _fetchDependencies = _fetchMetadata.Dependencies;
                                var _fetchMissingDeps = new List<string>();

                                foreach (var _fetchDependency in _fetchDependencies)
                                {
                                    var _doesHaveDependency = InstalledMods.FirstOrDefault(x => x.ModGitAddress != null && x.ModGitAddress.ToLower() == _fetchDependency.ToLower());

                                    if (_doesHaveDependency == null)
                                        _fetchMissingDeps.Add(_fetchDependency);
                                }

                                if (_fetchMissingDeps.Count > 0)
                                {
                                    var _fetchDepString = String.Join("\n- ", _fetchMissingDeps);
                                    var _fetchDepQuestion = await DialogService.ShowQuestion(_fetchMainView, "Missing Dependencies", $"This Mod has some dependencies that are not installed.\nTo proceed, all the following dependencies must be installed:\n\n- {_fetchDepString}\n\nDo you want to proceed? [Not doing so will cancel this mod's install.]");

                                    if (!_fetchDepQuestion)
                                    {
                                        if (Directory.Exists(_fetchCurrentPath))
                                            Directory.Delete(_fetchCurrentPath, true);

                                        _fetchNoDependencyList.Add(_fetchSuccess);
                                        continue;
                                    }

                                    else
                                    {
                                        var _fetchInstallStr = String.Join(";", _fetchMissingDeps);
                                        var _fetchInstall = await Install(_fetchInstallStr);

                                        if (!_fetchInstall)
                                        {
                                            if (Directory.Exists(_fetchCurrentPath))
                                                Directory.Delete(_fetchCurrentPath, true);

                                            _fetchNoDependencyList.Add(_fetchSuccess);
                                            continue;
                                        }
                                    }
                                }
                            }

                            if (_fetchMetadata.IsValid)
                            {
                                var _modModel = new ModModel
                                {
                                    ModTitle = _fetchMetadata.Title,
                                    ModAuthor = _fetchMetadata.OriginalAuthor,
                                    ModDescription = _fetchMetadata.Description,
                                    ModGitAddress = _fetchSuccess,
                                    ModLinkPreset = _tryFetchPreset,
                                    ModPath = _fetchCurrentPath,
                                    ModFilesList = _fetchMetadata.Assets.Select(x => x.Name).ToArray(),
                                    ModIcon = File.Exists(_fetchPathIcon) ? new Bitmap(_fetchPathIcon) : null,
                                    ModActive = true,
                                    ModValid = true
                                };

                                if (_fetchMetadata.Preferences != null)
                                {
                                    _modModel.ModPreferences = new List<PreferenceModel>();

                                    foreach (var _preference in _fetchMetadata.Preferences)
                                    {
                                        var _fetchPref = new PreferenceModel
                                        {
                                            Title = _preference.Title,
                                            Key = _preference.Key,
                                            Description = _preference.Description,
                                            Type = _preference.Type,
                                            Options = _preference.Options,
                                            Value = _preference.Value
                                        };

                                        _modModel.ModPreferences.Add(_fetchPref);
                                    }
                                }

                                if (File.Exists(_fetchPathGit))
                                {
                                    var _fetchGitRAW = await File.ReadAllTextAsync(_fetchPathGit);
                                    var _fetchSerial = YamlSerializer.Deserialize<GitModel>(_fetchGitRAW);

                                    if (_fetchSerial != null)
                                    {
                                        _modModel.ModTitle = String.IsNullOrEmpty(_modModel.ModTitle) ? _fetchSerial.Repository : _modModel.ModTitle;
                                        _modModel.ModAuthor = String.IsNullOrEmpty(_modModel.ModAuthor) ? _fetchSerial.Author : _modModel.ModAuthor;

                                        _modModel.ModSource = new Uri($"https://{_fetchSerial.Platform}/{_fetchSerial.Author}/{_fetchSerial.Repository}");
                                        _modModel.ModIssues = new Uri($"https://{_fetchSerial.Platform}/{_fetchSerial.Author}/{_fetchSerial.Repository}/issues");

                                        _modModel.ModPlatform = _fetchSerial.Platform;
                                        _modModel.ModLatestCommit = _fetchSerial.LatestCommit ?? new DateTime();
                                    }
                                }

                                var _fetchExistingMod = InstalledMods.FirstOrDefault(x => x.ModPath == _modModel.ModPath);

                                if (_fetchExistingMod != null)
                                    InstalledMods.Remove(_fetchExistingMod);

                                var _fetchFirstInvalid = InstalledMods.FirstOrDefault(x => !x.ModValid);
                                var _fetchModIndex = _fetchFirstInvalid != null ? InstalledMods.IndexOf(_fetchFirstInvalid) : 0x00;

                                InstalledMods.Insert(_fetchModIndex, _modModel);
                                HasModsInstalled = true;

                                if (_tryFetchPreset != null)
                                {
                                    var _fetchTargetFolder = Path.Combine(_fetchModPath, _fetchModAuthor, _fetchModName);

                                    if (!Directory.Exists(_fetchTargetFolder))
                                        Directory.CreateDirectory(_fetchTargetFolder);

                                    var _fetchLinkPath = Path.Combine(_fetchTargetFolder, "mod_link.yml");
                                    var _fetchLinkSerial = YamlSerializer.Serialize(_tryFetchPreset);

                                    File.WriteAllText(_fetchLinkPath, _fetchLinkSerial);
                                }
                            }

                            else
                            {
                                var uri = new Uri("avares://OpenKh.Tools.ModManager/Assets/invalid_mod.png");

                                var _modModel = new ModModel
                                {
                                    ModTitle = _fetchMetadata.Title,
                                    ModAuthor = "This mod is invalid!",
                                    ModDescription = "This mod contains errors within its YAML file. Please check the formatting!",
                                    ModIcon = new Bitmap(AssetLoader.Open(uri)),
                                    ModPath = _fetchCurrentPath,
                                    ModActive = false,
                                    ModValid = false
                                };

                                var _fetchExistingMod = InstalledMods.FirstOrDefault(x => x.ModPath == _modModel.ModPath);

                                if (_fetchExistingMod != null)
                                    InstalledMods.Remove(_fetchExistingMod);

                                InstalledMods.Add(_modModel);
                            }
                        }

                        if (_fetchErroredList.Count > 0)
                        {
                            var _fetchError = String.Join('\n', _fetchErroredList);
                            await DialogService.ShowMessage(_fetchMainView, "Some Mods were invalid!", $"The following mods were invalid and thus were not installed:\n {_fetchError}", MessageType.WARNING);
                        }

                        return true;
                    }

                    default:
                        return false;
                }
            }

            else
                return false;
        }

        else
            return false;
    }

    [RelayCommand]
    private async Task<bool> Remove()
    {
        if (CurrentMod != null && InstalledMods != null)
        {
            var _fetchMainView = FetchMainWindow() as Window;

            if (_fetchMainView == null)
                return false;

            bool? _fetchResult = await DialogService.ShowQuestion(_fetchMainView, "Remove Mod?", "Are you sure you want to remove the selected mod?\n" +
                                                                                                 "Please do note that doing so will also remove it from all presets the mod is linked to.");

            // If they indeed do:

            if (_fetchResult == true)
            {
                var _fetchModPath = CurrentMod.ModPath;
                var _fetchGitAddress = CurrentMod.ModGitAddress;

                var _isModLinked = CurrentMod.ModLinkPreset != null;

                // Remove the mod from the list.
                InstalledMods.Remove(CurrentMod);

                // If we still have mods, select one.
                if (InstalledMods.Count > 0)
                    CurrentMod = InstalledMods.First();

                // If we do not, nullify selection.
                else
                {
                    HasModsInstalled = false;
                    CurrentMod = null;
                }

                // Remove the mod directory.
                // This is a task for a reason. This isn't being awaited for a reason. Do not listen to IntelliSense on this one.
                Task.Run(() =>
                {
                    var _fetchTargetGame = CurrentConfig.Frontend.TargetGame;
                    var _fetchPresetArray = CurrentConfig.Frontend.TargetPreset;

                    if (!_isModLinked)
                    {
                        if (Directory.Exists(_fetchModPath))
                            Directory.Delete(_fetchModPath, true);
                    }

                    else
                    {
                        // Only Git mods can be linked.
                        var _fetchNormalized = _fetchGitAddress.Split(':').First()
                                                               .Split('@').First();

                        var _fetchCurrentPath = PathService.ResolveMod(CurrentConfig);
                        var _fetchActualPath = Path.Combine(_fetchCurrentPath, _fetchNormalized);

                        if (Directory.Exists(_fetchActualPath))
                            Directory.Delete(_fetchActualPath, true);
                    }

                    if (_fetchGitAddress != null && _fetchPresetArray != null && PresetMemory != null)
                    {
                        var _fetchNormalized = _fetchGitAddress.Split(':').First()
                                                               .Split('@').First();

                        var _fetchPresetName = _fetchPresetArray[(int)_fetchTargetGame];
                        var _fetchTargetName = String.IsNullOrEmpty(_fetchPresetName) ? "Default" : _fetchPresetName;

                        foreach (var _fetchPreset in PresetMemory)
                        {
                            if (_fetchPreset.Name == _fetchTargetName)
                                continue;

                            var _fetchPresetPath = PathService.ResolvePreset(CurrentConfig, true);
                            var _fetchModBarePath = PathService.ResolveMod(CurrentConfig, true);

                            var _fetchTargetPath = Path.Combine(_fetchPresetPath, Config.GameShorthand[_fetchTargetGame], _fetchPreset.FolderName, _fetchNormalized);
                            
                            if (Directory.Exists(_fetchTargetPath))
                            {
                                var _fetchFilePath = Path.Combine(_fetchTargetPath, "mod_link.yml");

                                if (File.Exists(_fetchFilePath))
                                {
                                    var _fetchLinkRAW = File.ReadAllText(_fetchFilePath);
                                    var _fetchLinkSerial = YamlSerializer.Deserialize<PresetModel>(_fetchLinkRAW);

                                    if (_fetchLinkSerial != null && _fetchLinkSerial.Name == _fetchTargetName)
                                        Directory.Delete(_fetchTargetPath, true);
                                }
                            }
                        }
                    }
                });

                return true;
            }

            else
                return false;
        }

        else
            return false;
    }

    [RelayCommand]
    private async Task<bool> Update()
    {
        if (CurrentMod != null && InstalledMods != null)
        {
            var _fetchMainView = FetchMainWindow() as Window;

            if (_fetchMainView == null)
                return false;

            bool? _fetchResult = await DialogService.ShowQuestion(_fetchMainView, "Update Mod?", "Are you sure you want to update the selected mod?");

            // If they indeed do:

            if (_fetchResult == true)
            {
                var _fetchGitPath = Path.Combine(CurrentMod.ModPath, "git_info.yml");

                var _fetchGitRAW = File.ReadAllText(_fetchGitPath);
                var _fetchGitSerial = YamlSerializer.Deserialize<GitModel>(_fetchGitRAW);

                var _fetchAddress = $"{_fetchGitSerial.Author}/{_fetchGitSerial.Repository}";

                if (_fetchGitSerial.Branch != null)
                    _fetchAddress += $":{_fetchGitSerial.Branch}";

                _fetchAddress += $"@{_fetchGitSerial.Platform}";

                await Install(_fetchAddress);
                return true;
            }

            else
                return false;
        }

        else
            return false;
    }

    [RelayCommand]
    private void Toggle(Tuple<ModModel, bool> inputParameter)
    {
        if (CurrentConfig != null && InstalledMods != null)
        {
            var _fetchMod = inputParameter.Item1;
            var _fetchIsChecked = inputParameter.Item2;

            var _fetchMemoryPath = Path.Combine(PathService.ResolveMod(CurrentConfig), "mod_memory.yml");

            var _fetchMemoryRAW = File.Exists(_fetchMemoryPath) ? File.ReadAllText(_fetchMemoryPath) : "[]";
            var _fetchMemory = YamlSerializer.Deserialize<ObservableCollection<MemoryModel>>(_fetchMemoryRAW);

            if (_fetchMemory == null)
                return;

            var _fetchSenderHash = ModService.ResolveMD5(_fetchMod, CurrentConfig);
            var _fetchMemoryItem = _fetchMemory.FirstOrDefault(x => x.ModHash == _fetchSenderHash);

            if (_fetchMemoryItem == null)
            {
                _fetchMemoryItem = new MemoryModel
                {
                    ModHash = _fetchSenderHash,
                    ModActive = _fetchIsChecked,
                    ModIndex = InstalledMods.IndexOf(inputParameter.Item1)
                };

                _fetchMemory.Add(_fetchMemoryItem);
            }

            else
            {
                var _fetchMemoryIndex = _fetchMemory.IndexOf(_fetchMemoryItem);
                _fetchMemory[_fetchMemoryIndex].ModActive = _fetchIsChecked;
            }

            var _fetchSerial = YamlSerializer.Serialize(_fetchMemory);
            File.WriteAllText(_fetchMemoryPath, _fetchSerial);
        }
    }

    [RelayCommand]
    private void MoveUtmost()
    {
        if (InstalledMods != null && CurrentMod != null)
        {
            var _fetchModIndex = InstalledMods.IndexOf(CurrentMod);
            InstalledMods.Move(_fetchModIndex, 0);

            CurrentMod = InstalledMods[0];
        }
    }

    [RelayCommand]
    private void MoveUp()
    {
        if (InstalledMods != null && CurrentMod != null)
        {
            var _fetchModIndex = InstalledMods.IndexOf(CurrentMod);

            if (_fetchModIndex != 0 && CurrentMod.ModValid)
            {
                InstalledMods.Move(_fetchModIndex, _fetchModIndex - 1);
                CurrentMod = InstalledMods[_fetchModIndex - 1];
            }
        }
    }

    [RelayCommand]
    private void MoveDown()
    {
        if (InstalledMods != null && CurrentMod != null)
        {
            var _fetchModIndex = InstalledMods.IndexOf(CurrentMod);

            if ( _fetchModIndex != InstalledMods.Count - 1 && CurrentMod.ModValid)
            {
                InstalledMods.Move(_fetchModIndex, _fetchModIndex + 1);
                CurrentMod = InstalledMods[_fetchModIndex + 1];
            }
        }
    }

    [RelayCommand]
    private async Task<bool> OpenFolder()
    {
        if (CurrentMod != null)
        {
            var _fetchMainView = FetchMainWindow();

            if (_fetchMainView == null)
                return false;

            var _fetchDirectoryInfo = new DirectoryInfo(CurrentMod.ModPath);

            if (_fetchMainView?.Launcher != null)
                await _fetchMainView.Launcher.LaunchDirectoryInfoAsync(_fetchDirectoryInfo);

            return true;
        }

        else
            return false;
    }

    #endregion

    #region BUILD COMMANDS

    [RelayCommand]
    private async Task<bool> Run()
    {
        if (CurrentConfig != null)
        {
            var _fetchMainView = FetchMainWindow();

            if (_fetchMainView == null)
                return false;

            return await ModService.Run(CurrentConfig, _fetchMainView);
        }

        else
            return false;
    }

    [RelayCommand]
    private async Task<bool> Build()
    {
        if (CurrentConfig != null && InstalledMods != null)
        {
            var _fetchMainView = FetchMainWindow() as Window;

            if (_fetchMainView == null)
                return false;

            // Allocate all the pointers to use for the progress bars.

            SafePtr _firstCurrentPtr = 0.00;
            SafePtr _firstMaximumPtr = 0.00;

            SafePtr _secondCurrentPtr = 0.00;
            SafePtr _secondMaximumPtr = 0.00;

            SafePtr _firstTextPtr = "Currently building: N/A";
            SafePtr _secondTextPtr = "Processing Files: {0} / {3}";

            // Show the dialog.

            var _multiProgress = DialogService.ShowProgress(_fetchMainView, "Installing Mod...", "Installing declared Mod... Please be patient...", ModService.CancelTokenSource, _firstCurrentPtr, _firstMaximumPtr, _firstTextPtr, _secondCurrentPtr, _secondMaximumPtr, _secondTextPtr);

            string _currentModName = "N/A";

            var _buildResult =
                await ModService.Build
                (
                    InstalledMods,
                    CurrentConfig,

                    (string currModName, int procMod, int totalMod) =>
                    {
                        _currentModName = currModName;

                        _firstTextPtr /= $"Currently building: {currModName}";

                        _firstCurrentPtr /= Convert.ToDouble(procMod);
                        _firstMaximumPtr /= Convert.ToDouble(totalMod);

                        if (ModService.CancelToken.IsCancellationRequested)
                            return false;

                        return true;
                    },

                    (int processed, int total) =>
                    {
                        _secondCurrentPtr /= Convert.ToDouble(processed);
                        _secondMaximumPtr /= Convert.ToDouble(total);

                        if (ModService.CancelToken.IsCancellationRequested)
                            return false;

                        return true;
                    }
                );

            // Free all the pointers allocated.

            _secondTextPtr.Dispose();
            _firstTextPtr.Dispose();

            _secondMaximumPtr.Dispose();
            _secondCurrentPtr.Dispose();

            _firstMaximumPtr.Dispose();
            _firstCurrentPtr.Dispose();

            if (_buildResult == 0x00)
                return true;

            else if (_buildResult == 0x01)
                await DialogService.ShowMessage(_fetchMainView, "ERROR - Failed to build Mod", string.Format("Building failed on this Mod: {0}\nPlease re-install the Mod and try again!", _currentModName), MessageType.ERROR);
            return false;
        }

        else
            return false;
    }

    [RelayCommand]
    private async Task<bool> Restore()
    {
        if (CurrentConfig != null)
        {
            var _fetchMainView = FetchMainWindow() as Window;

            if (_fetchMainView == null)
                return false;

            var _fetchBuildDir = PathService.ResolveBuild(CurrentConfig);
            Directory.Delete(_fetchBuildDir, true);

            await DialogService.ShowMessage(_fetchMainView, "Restore Completed!", "Restoration complete! The game should run as if it isn't modded!", MessageType.INFO);

            return true;
        }

        else
            return false;
    }

    [RelayCommand]
    private async Task<bool> BuildRun()
    {
        if (CurrentConfig != null && InstalledMods != null)
        {
            var _buildResult = await Build();

            if (!_buildResult)
                return false;

            var _runResult = await Run();

            if (!_runResult)
                return false;

            return true;
        }

        else
            return false;
    }

    #endregion

    #region ASSEMBLY COMMANDS

    [RelayCommand]
    private async Task<bool> InstallPanacea()
    {
        if (CurrentConfig != null)
        {
            var _fetchMainView = FetchMainWindow() as Window;

            if (_fetchMainView == null)
                return false;

            PackageService.InstallPanacea(CurrentConfig);
            ConfigurationValid = CurrentConfig.IsValid();

            await DialogService.ShowMessage(_fetchMainView, "Panacea Installed!", "Panacea has been installed in accordance to current settings.", MessageType.INFO);
            return true;
        }

        else
            return false;
    }

    [RelayCommand]
    private async Task<bool> InstallBackend()
    {
        if (CurrentConfig != null)
        {
            var _fetchMainView = FetchMainWindow() as Window;

            if (_fetchMainView == null)
                return false;

            PackageService.InstallBackend(CurrentConfig);
            ConfigurationValid = CurrentConfig.IsValid();

            await DialogService.ShowMessage(_fetchMainView, "LuaBackend Installed!", "LuaBackend has been installed in accordance to current settings.", MessageType.INFO);
            return true;
        }

        else
            return false;
    }

    #endregion

    #region POPUP COMMANDS

    [RelayCommand]
    private async Task<bool> LaunchSetup()
    {
        if (CurrentConfig != null && InstalledMods != null)
        {
            var _fetchMainView = FetchMainWindow() as Window;

            if (_fetchMainView == null)
                return false;

            await new SetupWizardView().ShowDialog(_fetchMainView);
            return true;
        }

        else
            return false;
    }

    [RelayCommand]
    private async Task<bool> LaunchCatalog()
    {
        if (CurrentConfig != null && InstalledMods != null)
        {
            var _fetchMainView = FetchMainWindow() as Window;

            if (_fetchMainView == null)
                return false;

            var _catalogWindow = new CatalogView();
            var _fetchResult = await _catalogWindow.ShowDialog<string?>(_fetchMainView);

            if (!String.IsNullOrEmpty(_fetchResult) && InstallCommand.CanExecute(_fetchResult))
            {
                InstallCommand.Execute(_fetchResult);
                return true;
            }

            else
                return false;
        }

        else
            return false;
    }

    [RelayCommand]
    private async Task<bool> PromptArguments()
    {
        if (CurrentConfig != null)
        {
            var _fetchMainView = FetchMainWindow() as Window;

            if (_fetchMainView == null)
                return false;

            var _fetchResult = await DialogService.ShowInput(_fetchMainView, "Declare Launch Aruguments", "Enter the launch arguments to use when launching on Steam.", "Done", "Ex. -fastboot -noaspect", CurrentConfig.Frontend.LaunchArguments);

            if (!String.IsNullOrEmpty(_fetchResult))
                CurrentConfig.Frontend.LaunchArguments = _fetchResult;

            return true;
        }

        else
            return false;
    }

    #endregion

    #region PRESET COMMANDS

    [RelayCommand]
    private async Task<bool> CreatePreset()
    {
        if (CurrentConfig != null)
        {
            var _fetchTargetGame = (int)CurrentConfig.Frontend.TargetGame;

            if (PresetMemory != null)
            {
                var _fetchMainView = FetchMainWindow() as Window;

                if (_fetchMainView == null)
                    return false;

                var _fetchResult = await DialogService.ShowInput(_fetchMainView, "Create New Profile...", "Please enter a name for the new Profile.", "OK", "Ex. \"Some Very Cool Profile\"");

                if (_fetchResult != null)
                {
                    PresetMemory.Add(new PresetModel
                    {
                        Name = _fetchResult,
                        TargetGame = CurrentConfig.Frontend.TargetGame
                    });
                }
            }
        }

        return false;
    }

    [RelayCommand]
    private async Task<bool> SwitchPreset(PresetModel input)
    {
        if (CurrentConfig != null)
        {
            var _fetchTargetGame = CurrentConfig.Frontend.TargetGame;
            var _fetchPresetList = CurrentConfig.Frontend.TargetPreset;

            if (_fetchPresetList != null)
            {
                var _fetchPreset = _fetchPresetList[(int)_fetchTargetGame];
                var _fetchName = input == null ? "" : input.Name;

                if (_fetchPreset != _fetchName)
                {
                    _fetchPresetList[(int)_fetchTargetGame] = _fetchName;
                    IterateModlist();
                }
            }
        }

        return false;
    }

    [RelayCommand]
    private async Task<bool> DeletePreset(PresetModel input)
    {
        if (CurrentConfig != null && PresetMemory != null)
        {
            var _fetchTargetGame = CurrentConfig.Frontend.TargetGame;
            var _fetchPresetList = CurrentConfig.Frontend.TargetPreset;

            if (_fetchPresetList != null)
            {
                var _fetchPreset = _fetchPresetList[(int)_fetchTargetGame];
                var _fetchName = input == null ? "" : input.Name;

                var _fetchMainView = FetchMainWindow() as Window;

                if (_fetchMainView == null)
                    return false;

                var _fetchResult = await DialogService.ShowQuestion(_fetchMainView, "Delete this Profile?", $"Are you sure you want to delete the Profile \"{_fetchName}\"?\n" +
                                                                                                           $"If it is currently active, the default Profile will be loaded.");

                if (_fetchResult)
                {
                    if (_fetchPreset == _fetchName)
                    {
                        _fetchPresetList[(int)_fetchTargetGame] = "";
                        IterateModlist();
                    }

                    PresetMemory.Remove(input);

                    Task.Run(() =>
                    {
                        var _fetchPresetPath = PathService.ResolvePreset(CurrentConfig);
                        var _fetchTargetFolder = Path.Combine(_fetchPresetPath, input.FolderName);

                        if (Directory.Exists(_fetchTargetFolder))
                            Directory.Delete(_fetchTargetFolder, true);
                    });

                    return true;
                }
            }
        }

        return false;
    }

    [RelayCommand]
    private async Task<bool> RenamePreset(PresetModel input)
    {
        if (CurrentConfig != null && PresetMemory != null)
        {
            var _fetchTargetGame = CurrentConfig.Frontend.TargetGame;
            var _fetchPresetList = CurrentConfig.Frontend.TargetPreset;

            if (_fetchPresetList != null)
            {
                var _fetchPreset = _fetchPresetList[(int)_fetchTargetGame];

                var _fetchName = input.Name;
                var _fetchFolder = input.FolderName;

                var _fetchMainView = FetchMainWindow() as Window;

                if (_fetchMainView == null)
                    return false;

                var _fetchResult = await DialogService.ShowInput(_fetchMainView, "Rename Profile", "Please enter a new name for this Profile.", "Rename", $"Current Name: \"{_fetchName}\"");

                if (_fetchResult != null)
                {
                    var _fetchIndex = PresetMemory.IndexOf(input);
                    PresetMemory.Remove(input);

                    if (_fetchPreset == _fetchName)
                        _fetchPresetList[(int)_fetchTargetGame] = _fetchResult;

                    input.Name = _fetchResult;
                    PresetMemory.Insert(_fetchIndex, input);

                    Task.Run(() =>
                    {
                        var _fetchPresetPath = PathService.ResolvePreset(CurrentConfig);

                        var _fetchFolderOLD = Path.Combine(_fetchPresetPath, _fetchFolder);
                        var _fetchFolderNEW = Path.Combine(_fetchPresetPath, input.FolderName);

                        if (Directory.Exists(_fetchFolderOLD))
                            Directory.Move(_fetchFolderOLD, _fetchFolderNEW);
                    });
                }
            }
        }

        return false;
    }

    #endregion
}

#pragma warning restore CS4014
#pragma warning restore S4790
