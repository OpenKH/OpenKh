using OpenKh.Tools.ModsManager.Core;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Threading;

namespace OpenKh.Tools.ModsManager.Avalonia.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly ModCatalogService _catalogService;
    private readonly RepositoryModInstaller _modInstaller;
    private readonly ModMaintenanceService _maintenanceService;
    private readonly IModInstallPrompt _modInstallPrompt;
    private readonly IOnlineModsPrompt _onlineModsPrompt;
    private readonly IPresetsPrompt _presetsPrompt;
    private readonly ISettingsPrompt _settingsPrompt;
    private readonly IInfoPrompt _infoPrompt;
    private readonly ICreatorPrompt _creatorPrompt;
    private readonly IUserDialogService _dialogs;
    private readonly ISetupPrompt _setupPrompt;
    private readonly ModManagerConfigurationService _configuration;
    private readonly ModBuildService _buildService;
    private readonly GameLaunchService _launchService;
    private readonly ICollectionSettingsPrompt _collectionSettingsPrompt;
    private readonly PcPackagePatchService _packagePatchService;
    private readonly IControllerInputService _controllerInput;
    private readonly List<ModListItemViewModel> _allMods = [];
    private GameInfo _selectedGame;
    private ModListItemViewModel? _selectedMod;
    private string _searchText = string.Empty;
    private string _statusText = "Loading your mods";
    private string _statusForeground = "#91A2BA";
    private bool _isBusy;
    private bool _showAdvancedOptions;
    private bool _startupUpdateCheckCompleted;

    public MainWindowViewModel(
        ModCatalogService catalogService,
        RepositoryModInstaller modInstaller,
        ModMaintenanceService maintenanceService,
        IModInstallPrompt modInstallPrompt,
        IOnlineModsPrompt onlineModsPrompt,
        IPresetsPrompt presetsPrompt,
        ISettingsPrompt settingsPrompt,
        IInfoPrompt infoPrompt,
        ICreatorPrompt creatorPrompt,
        IUserDialogService dialogs,
        ISetupPrompt setupPrompt,
        ModManagerConfigurationService configuration,
        ModBuildService buildService,
        GameLaunchService launchService,
        ICollectionSettingsPrompt collectionSettingsPrompt,
        PcPackagePatchService packagePatchService,
        IControllerInputService controllerInput)
    {
        _catalogService = catalogService;
        _modInstaller = modInstaller;
        _maintenanceService = maintenanceService;
        _modInstallPrompt = modInstallPrompt;
        _onlineModsPrompt = onlineModsPrompt;
        _presetsPrompt = presetsPrompt;
        _settingsPrompt = settingsPrompt;
        _infoPrompt = infoPrompt;
        _creatorPrompt = creatorPrompt;
        _dialogs = dialogs;
        _setupPrompt = setupPrompt;
        _configuration = configuration;
        _buildService = buildService;
        _launchService = launchService;
        _collectionSettingsPrompt = collectionSettingsPrompt;
        _packagePatchService = packagePatchService;
        _controllerInput = controllerInput;
        _selectedGame = catalogService.DefaultGame;
        Games = GameInfo.SupportedGames;
        Mods = [];

        RefreshCommand = new RelayCommand(() => _ = RefreshAsync(), () => !IsBusy);
        MoveUpCommand = new RelayCommand(() => MoveSelected(-1), () => CanMoveSelected(-1));
        MoveDownCommand = new RelayCommand(() => MoveSelected(1), () => CanMoveSelected(1));
        MoveTopCommand = new RelayCommand(MoveSelectedToTop, () => CanMoveSelectedToTop());
        OpenFolderCommand = new RelayCommand(OpenSelectedFolder, () => SelectedMod is not null);
        OpenSourceCommand = new RelayCommand(
            () => OpenUrl(SelectedMod?.SourceUrl),
            () => SelectedMod?.HasSource == true);
        ReportBugCommand = new RelayCommand(
            () => OpenUrl(SelectedMod?.ReportBugUrl),
            () => SelectedMod?.CanReportBug == true);
        InstallCommand = new AsyncRelayCommand(InstallPackageAsync, () => !IsBusy);
        BrowseModsCommand = new AsyncRelayCommand(BrowseOnlineModsAsync, () => !IsBusy);
        PresetsCommand = new AsyncRelayCommand(OpenPresetsAsync, () => !IsBusy);
        SettingsCommand = new AsyncRelayCommand(OpenSettingsAsync, () => !IsBusy);
        InfoCommand = new AsyncRelayCommand(OpenInfoAsync, () => !IsBusy);
        CreatorCommand = new AsyncRelayCommand(OpenCreatorAsync, () => !IsBusy);
        CheckUpdatesCommand = new AsyncRelayCommand(CheckUpdatesAsync, () => !IsBusy && TotalCount > 0);
        UpdateSelectedCommand = new AsyncRelayCommand(UpdateSelectedAsync, () => !IsBusy && SelectedMod?.HasUpdate == true);
        RemoveSelectedCommand = new AsyncRelayCommand(RemoveSelectedAsync, () => !IsBusy && SelectedMod is not null);
        SetupCommand = new AsyncRelayCommand(OpenSetupAsync, () => !IsBusy);
        BuildCommand = new AsyncRelayCommand(BuildAsync, () => !IsBusy);
        BuildAndPlayCommand = new AsyncRelayCommand(BuildAndPlayAsync, () => !IsBusy);
        PlayCommand = new AsyncRelayCommand(PlayAsync, () => !IsBusy);
        StopGameCommand = new RelayCommand(StopGame, () => IsGameRunning);
        CollectionSettingsCommand = new AsyncRelayCommand(OpenCollectionSettingsAsync, () => !IsBusy && SelectedMod?.IsCollection == true);
        ApplyToGameCommand = new AsyncRelayCommand(ApplyToGameAsync, () => !IsBusy && EnabledCount > 0);
        FastPatchCommand = new AsyncRelayCommand(FastPatchAsync, () => !IsBusy && EnabledCount > 0);
        RestoreGameCommand = new AsyncRelayCommand(RestoreGameAsync, () => !IsBusy);
        ClearBuiltModsCommand = new AsyncRelayCommand(ClearBuiltModsAsync, () => !IsBusy);
        ToggleAdvancedOptionsCommand = new RelayCommand(() => ShowAdvancedOptions = !ShowAdvancedOptions);
        _controllerInput.StatusChanged += ControllerConnectionChanged;
        _launchService.RunningStateChanged += LaunchRunningStateChanged;

        _ = RefreshAsync();
    }

    public IReadOnlyList<GameInfo> Games { get; }
    public ObservableCollection<ModListItemViewModel> Mods { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand MoveUpCommand { get; }
    public RelayCommand MoveDownCommand { get; }
    public RelayCommand MoveTopCommand { get; }
    public RelayCommand OpenFolderCommand { get; }
    public RelayCommand OpenSourceCommand { get; }
    public RelayCommand ReportBugCommand { get; }
    public AsyncRelayCommand InstallCommand { get; }
    public AsyncRelayCommand BrowseModsCommand { get; }
    public AsyncRelayCommand PresetsCommand { get; }
    public AsyncRelayCommand SettingsCommand { get; }
    public AsyncRelayCommand InfoCommand { get; }
    public AsyncRelayCommand CreatorCommand { get; }
    public AsyncRelayCommand CheckUpdatesCommand { get; }
    public AsyncRelayCommand UpdateSelectedCommand { get; }
    public AsyncRelayCommand RemoveSelectedCommand { get; }
    public AsyncRelayCommand SetupCommand { get; }
    public AsyncRelayCommand BuildCommand { get; }
    public AsyncRelayCommand BuildAndPlayCommand { get; }
    public AsyncRelayCommand PlayCommand { get; }
    public RelayCommand StopGameCommand { get; }
    public AsyncRelayCommand CollectionSettingsCommand { get; }
    public AsyncRelayCommand ApplyToGameCommand { get; }
    public AsyncRelayCommand FastPatchCommand { get; }
    public AsyncRelayCommand RestoreGameCommand { get; }
    public AsyncRelayCommand ClearBuiltModsCommand { get; }
    public RelayCommand ToggleAdvancedOptionsCommand { get; }
    public string InstallationDirectory => _catalogService.InstallationDirectory;
    public string ControllerStatusText => _controllerInput.StatusText;
    public string ControllerHelpText => _controllerInput.NavigationHelpText;
    public bool IsControllerConnected => _controllerInput.IsConnected;
    public bool ShowControllerHelp => IsControllerConnected || !string.IsNullOrWhiteSpace(ControllerHelpText);
    public bool IsGameRunning => _launchService.IsRunning;

    public GameInfo SelectedGame
    {
        get => _selectedGame;
        set
        {
            if (!SetProperty(ref _selectedGame, value) || value is null)
                return;

            _configuration.Current.LaunchGame = value.Id;
            _configuration.Save();
            _ = RefreshAsync();
        }
    }

    public ModListItemViewModel? SelectedMod
    {
        get => _selectedMod;
        set
        {
            if (!SetProperty(ref _selectedMod, value))
                return;

            OnPropertyChanged(nameof(HasSelection));
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
            MoveTopCommand.NotifyCanExecuteChanged();
            OpenFolderCommand.NotifyCanExecuteChanged();
            OpenSourceCommand.NotifyCanExecuteChanged();
            ReportBugCommand.NotifyCanExecuteChanged();
            UpdateSelectedCommand.NotifyCanExecuteChanged();
            RemoveSelectedCommand.NotifyCanExecuteChanged();
            CollectionSettingsCommand.NotifyCanExecuteChanged();
            ApplyToGameCommand.NotifyCanExecuteChanged();
            FastPatchCommand.NotifyCanExecuteChanged();
            RestoreGameCommand.NotifyCanExecuteChanged();
            ClearBuiltModsCommand.NotifyCanExecuteChanged();
            SetupCommand.NotifyCanExecuteChanged();
            BuildCommand.NotifyCanExecuteChanged();
            BuildAndPlayCommand.NotifyCanExecuteChanged();
            PlayCommand.NotifyCanExecuteChanged();
            CollectionSettingsCommand.NotifyCanExecuteChanged();
        }
    }

    public bool HasSelection => SelectedMod is not null;

    public bool ShowAdvancedOptions
    {
        get => _showAdvancedOptions;
        set => SetProperty(ref _showAdvancedOptions, value);
    }

    public bool EnablePatching => _configuration.Current.EnablePatching;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilter();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set
        {
            StatusForeground = "#91A2BA";
            SetProperty(ref _statusText, value);
        }
    }

    public string StatusForeground
    {
        get => _statusForeground;
        private set => SetProperty(ref _statusForeground, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value))
                return;

            RefreshCommand.NotifyCanExecuteChanged();
            InstallCommand.NotifyCanExecuteChanged();
            BrowseModsCommand.NotifyCanExecuteChanged();
            PresetsCommand.NotifyCanExecuteChanged();
            SettingsCommand.NotifyCanExecuteChanged();
            InfoCommand.NotifyCanExecuteChanged();
            CreatorCommand.NotifyCanExecuteChanged();
            CheckUpdatesCommand.NotifyCanExecuteChanged();
            UpdateSelectedCommand.NotifyCanExecuteChanged();
            RemoveSelectedCommand.NotifyCanExecuteChanged();
            SetupCommand.NotifyCanExecuteChanged();
            BuildCommand.NotifyCanExecuteChanged();
            BuildAndPlayCommand.NotifyCanExecuteChanged();
            PlayCommand.NotifyCanExecuteChanged();
            CollectionSettingsCommand.NotifyCanExecuteChanged();
            ApplyToGameCommand.NotifyCanExecuteChanged();
            FastPatchCommand.NotifyCanExecuteChanged();
            RestoreGameCommand.NotifyCanExecuteChanged();
            ClearBuiltModsCommand.NotifyCanExecuteChanged();
        }
    }

    public int TotalCount => _allMods.Count;
    public int EnabledCount => _allMods.Count(mod => mod.IsEnabled);
    public string LibrarySummary => TotalCount == 1 ? "1 installed mod" : $"{TotalCount} installed mods";
    public string EnabledSummary => EnabledCount == 1 ? "1 enabled" : $"{EnabledCount} enabled";

    private async Task RefreshAsync()
    {
        try
        {
            IsBusy = true;
            StatusText = $"Loading {SelectedGame.DisplayName}";
            var entries = await _catalogService.LoadAsync(SelectedGame);

            _allMods.Clear();
            _allMods.AddRange(entries.Select(entry =>
                new ModListItemViewModel(entry, SaveEnabledOrder)));
            ApplyFilter();
            StatusText = TotalCount == 0
                ? "No mods were found for this game"
                : $"{LibrarySummary}, {EnabledSummary}";
            NotifySummaryChanged();
            if (!_startupUpdateCheckCompleted && _configuration.Current.AutoUpdateMods && TotalCount > 0)
            {
                _startupUpdateCheckCompleted = true;
                await CheckUpdatesAsync();
            }
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not load mods: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var selectedId = SelectedMod?.Id;
        var query = SearchText.Trim();
        var filteredMods = string.IsNullOrEmpty(query)
            ? _allMods
            : _allMods.Where(mod =>
                mod.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                mod.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                mod.Id.Contains(query, StringComparison.OrdinalIgnoreCase));

        Mods.Clear();
        foreach (var mod in filteredMods)
            Mods.Add(mod);

        SelectedMod = Mods.FirstOrDefault(mod => mod.Id == selectedId) ?? Mods.FirstOrDefault();
    }

    private void SaveEnabledOrder()
    {
        _catalogService.SaveEnabledOrder(SelectedGame, _allMods.Select(mod => mod.Model));
        StatusText = $"Mod selection saved, {EnabledSummary}";
        NotifySummaryChanged();
    }

    private bool CanMoveSelected(int offset)
    {
        if (SelectedMod is null || !string.IsNullOrWhiteSpace(SearchText))
            return false;

        var index = _allMods.IndexOf(SelectedMod);
        var targetIndex = index + offset;
        return index >= 0 &&
            targetIndex >= GetHighestAllowedIndex(SelectedMod) &&
            targetIndex <= GetLowestAllowedIndex(SelectedMod);
    }

    private void MoveSelected(int offset)
    {
        if (!CanMoveSelected(offset) || SelectedMod is null)
            return;

        var oldIndex = _allMods.IndexOf(SelectedMod);
        var newIndex = oldIndex + offset;
        var selectedId = SelectedMod.Id;
        _allMods.RemoveAt(oldIndex);
        _allMods.Insert(newIndex, SelectedMod);
        var selected = Mods[oldIndex];
        Mods.RemoveAt(oldIndex);
        Mods.Insert(newIndex, selected);
        SelectedMod = Mods.First(mod => mod.Id == selectedId);
        SaveEnabledOrder();
        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
        MoveTopCommand.NotifyCanExecuteChanged();
    }

    private bool CanMoveSelectedToTop() =>
        SelectedMod is not null &&
        string.IsNullOrWhiteSpace(SearchText) &&
        _allMods.IndexOf(SelectedMod) > GetHighestAllowedIndex(SelectedMod);

    private void MoveSelectedToTop()
    {
        if (!CanMoveSelectedToTop() || SelectedMod is null)
            return;
        var selectedId = SelectedMod.Id;
        var oldIndex = _allMods.IndexOf(SelectedMod);
        var targetIndex = GetHighestAllowedIndex(SelectedMod);
        _allMods.Remove(SelectedMod);
        _allMods.Insert(targetIndex, SelectedMod);
        var selected = Mods[oldIndex];
        Mods.RemoveAt(oldIndex);
        Mods.Insert(targetIndex, selected);
        SelectedMod = Mods.First(mod => mod.Id == selectedId);
        SaveEnabledOrder();
        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
        MoveTopCommand.NotifyCanExecuteChanged();
    }

    private int GetHighestAllowedIndex(ModListItemViewModel mod)
    {
        if (!mod.IsPcPatch)
            return 0;

        var firstPcPatch = _allMods.FindIndex(item => item.IsPcPatch);
        return firstPcPatch < 0 ? _allMods.Count : firstPcPatch;
    }

    private int GetLowestAllowedIndex(ModListItemViewModel mod)
    {
        if (mod.IsPcPatch)
            return _allMods.Count - 1;

        var firstPcPatch = _allMods.FindIndex(item => item.IsPcPatch);
        return firstPcPatch < 0 ? _allMods.Count - 1 : firstPcPatch - 1;
    }

    private void OpenSelectedFolder()
    {
        if (SelectedMod is null)
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = SelectedMod.Directory,
            UseShellExecute = true
        });
    }

    private static void OpenUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private async Task InstallPackageAsync()
    {
        var request = await _modInstallPrompt.ShowAsync();
        if (request is null)
            return;

        try
        {
            if (!request.Overwrite)
            {
                var existingMod = _modInstaller.FindInstalledMod(
                    request.Source,
                    SelectedGame,
                    request.Branch);
                if (existingMod is not null)
                {
                    var replace = await ConfirmReplacementAsync(existingMod);
                    if (!replace)
                        return;

                    request = request with { Overwrite = true };
                }
            }

            IsBusy = true;
            StatusText = "Preparing mod installation";
            var progress = new Progress<ModOperationProgress>(value =>
            {
                StatusText = value.Percentage is { } percentage
                    ? $"{value.Message} ({percentage:P0})"
                    : value.Message;
            });
            ModInstallResult result;
            while (true)
            {
                try
                {
                    result = await _modInstaller.InstallAsync(
                        request.Source,
                        SelectedGame,
                        request.Branch,
                        request.Overwrite,
                        progress);
                    break;
                }
                catch (ModAlreadyInstalledException exception) when (!request.Overwrite)
                {
                    IsBusy = false;
                    var replace = await ConfirmReplacementAsync(exception.ModName);
                    if (!replace)
                        return;

                    request = request with { Overwrite = true };
                    IsBusy = true;
                    StatusText = $"Replacing {exception.ModName}";
                }
            }
            await RefreshAsync();
            SelectedMod = Mods.FirstOrDefault(mod =>
                mod.Id.Equals(result.Id, StringComparison.OrdinalIgnoreCase));
            SetSuccessStatus($"{result.DisplayName} was installed successfully");
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not install mod: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CheckUpdatesAsync()
    {
        try
        {
            IsBusy = true;
            var updateCount = 0;
            var checkedCount = 0;
            foreach (var mod in _allMods)
            {
                StatusText = $"Checking {mod.Name} for updates";
                var behindBy = await _maintenanceService.CheckForUpdateAsync(mod.Model);
                mod.SetUpdateCount(behindBy);
                if (behindBy > 0)
                    updateCount++;
                checkedCount++;
                StatusText = $"Checked {checkedCount} of {TotalCount} mods";
            }

            StatusText = updateCount switch
            {
                0 => "All repository mods are up to date",
                1 => "1 mod update is available",
                _ => $"{updateCount} mod updates are available"
            };
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not check for updates: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenSetupAsync()
    {
        if (!await _setupPrompt.ShowAsync())
            return;

        _configuration.Current.WizardVersionNumber = 1;
        _configuration.Save();
        await RefreshAsync();
        StatusText = "Setup was saved";
    }

    private async Task BrowseOnlineModsAsync()
    {
        if (!await _onlineModsPrompt.ShowAsync(
                SelectedGame,
                _allMods.Select(mod => mod.Id).ToArray(),
                RefreshAsync))
            return;

        await RefreshAsync();
        SetSuccessStatus("Online mod installation completed");
    }

    private async Task OpenPresetsAsync()
    {
        var enabledIds = _allMods.Where(mod => mod.IsEnabled).Select(mod => mod.Id).ToArray();
        var presetIds = await _presetsPrompt.ShowAsync(enabledIds);
        if (presetIds is null)
            return;

        var enabled = presetIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var order = presetIds
            .Select(id => _allMods.FirstOrDefault(mod => mod.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            .Where(mod => mod is not null)
            .Cast<ModListItemViewModel>()
            .Concat(_allMods.Where(mod => !enabled.Contains(mod.Id)))
            .ToArray();
        foreach (var mod in _allMods)
            mod.Model.IsEnabled = enabled.Contains(mod.Id);
        _catalogService.SaveEnabledOrder(SelectedGame, order.Select(mod => mod.Model));
        await RefreshAsync();
        SetSuccessStatus("Preset applied");
    }

    private async Task OpenSettingsAsync()
    {
        if (await _settingsPrompt.ShowAsync())
        {
            if (!EnablePatching)
                ShowAdvancedOptions = false;
            OnPropertyChanged(nameof(EnablePatching));
            SetSuccessStatus("Settings were saved");
        }
    }

    private async Task<bool> ConfirmReplacementAsync(string modName)
    {
        var replace = await _dialogs.ConfirmAsync(
            "Replace Existing Mod?",
            $"A mod named '{modName}' is already installed. Replace it with the selected mod?",
            "Replace Mod");
        if (!replace)
            StatusText = "Mod installation was cancelled";
        return replace;
    }

    private Task OpenInfoAsync() => _infoPrompt.ShowAsync();

    private Task OpenCreatorAsync() => _creatorPrompt.ShowAsync();

    public async Task RunInitialSetupAsync()
    {
        if (_configuration.Current.WizardVersionNumber >= 1)
            return;

        StatusText = "Complete Setup before using the Mod Manager";
        if (!await _setupPrompt.ShowAsync())
        {
            StatusText = "Setup was not completed";
            return;
        }

        _configuration.Current.WizardVersionNumber = 1;
        _configuration.Save();
        await RefreshAsync();
        SetSuccessStatus("Setup was completed");
    }

    private async Task BuildAsync()
    {
        try
        {
            IsBusy = true;
            var progress = CreateStatusProgress();
            var outputDirectory = await _buildService.BuildAsync(
                SelectedGame,
                _allMods.Select(mod => mod.Model).ToArray(),
                progress: progress);
            SetSuccessStatus($"Mods built in {outputDirectory}");
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not build mods: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenCollectionSettingsAsync()
    {
        if (SelectedMod is null || !SelectedMod.IsCollection)
            return;
        if (await _collectionSettingsPrompt.ShowAsync(SelectedMod.Model, SelectedGame))
        {
            var selectedId = SelectedMod.Id;
            var selectedName = SelectedMod.Name;
            await RefreshAsync();
            SelectedMod = Mods.FirstOrDefault(mod => mod.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
            SetSuccessStatus($"{selectedName} collection settings were saved");
        }
    }

    private Task ApplyToGameAsync() => PatchGameAsync(false);

    private Task FastPatchAsync() => PatchGameAsync(true);

    private async Task PatchGameAsync(bool fastMode)
    {
        var confirmed = await _dialogs.ConfirmAsync(
            $"{(fastMode ? "Fast patch" : "Full patch")} {SelectedGame.DisplayName}?",
            fastMode
                ? "OpenKH will build a fast patch, create backups when needed, and replace the first game package."
                : "OpenKH will create backups in BackupImage, build the enabled mods, and replace every affected HED/PKG file.",
            fastMode ? "Fast patch" : "Full patch");
        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            var progress = CreateStatusProgress();
            await _buildService.BuildAsync(
                SelectedGame,
                _allMods.Select(mod => mod.Model).ToArray(),
                fastMode,
                progress: progress);
            await _packagePatchService.ApplyAsync(SelectedGame, fastMode, progress);
            SetSuccessStatus($"{(fastMode ? "Fast" : "Full")} patch completed for {SelectedGame.DisplayName}");
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not apply mods to the game: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RestoreGameAsync()
    {
        var confirmed = await _dialogs.ConfirmAsync(
            $"Restore {SelectedGame.DisplayName}?",
            "The original HED/PKG files will be restored from BackupImage and the current build output will be removed.",
            "Restore Game");
        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            await _packagePatchService.RestoreAsync(SelectedGame, true, CreateStatusProgress());
            SetSuccessStatus($"{SelectedGame.DisplayName} was restored");
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not restore the game: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ClearBuiltModsAsync()
    {
        try
        {
            IsBusy = true;
            await _packagePatchService.RestoreAsync(SelectedGame, false, CreateStatusProgress());
            SetSuccessStatus($"Built mods were cleared for {SelectedGame.DisplayName}");
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not clear built mods: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task BuildAndPlayAsync()
    {
        try
        {
            IsBusy = true;
            await _buildService.BuildAsync(
                SelectedGame,
                _allMods.Select(mod => mod.Model).ToArray(),
                progress: CreateStatusProgress());
            StatusText = $"Launching {SelectedGame.DisplayName}";
            await _launchService.LaunchAsync(SelectedGame);
            SetSuccessStatus($"{SelectedGame.DisplayName} was launched");
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not build and launch: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PlayAsync()
    {
        try
        {
            IsBusy = true;
            StatusText = $"Launching {SelectedGame.DisplayName}";
            await _launchService.LaunchAsync(SelectedGame);
            SetSuccessStatus($"{SelectedGame.DisplayName} was launched");
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not launch game: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void StopGame()
    {
        _launchService.Stop();
        StatusText = "The running game or emulator was stopped";
    }

    private void LaunchRunningStateChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            OnPropertyChanged(nameof(IsGameRunning));
            StopGameCommand.NotifyCanExecuteChanged();
        });
    }

    private Progress<ModOperationProgress> CreateStatusProgress() => new(value =>
    {
        StatusText = value.Percentage is { } percentage
            ? $"{value.Message} ({percentage:P0})"
            : value.Message;
    });

    private void SetSuccessStatus(string message)
    {
        StatusText = message;
        StatusForeground = "#62D6A7";
    }

    private void SetErrorStatus(string message)
    {
        StatusText = message;
        StatusForeground = "#FF7A7A";
    }

    private async Task UpdateSelectedAsync()
    {
        if (SelectedMod is null)
            return;

        var selectedId = SelectedMod.Id;
        try
        {
            IsBusy = true;
            var progress = new Progress<ModOperationProgress>(value =>
            {
                StatusText = value.Percentage is { } percentage
                    ? $"{value.Message} ({percentage:P0})"
                    : value.Message;
            });
            await _maintenanceService.UpdateAsync(SelectedMod.Model, SelectedGame, progress);
            await RefreshAsync();
            SelectedMod = Mods.FirstOrDefault(mod => mod.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
            SetSuccessStatus($"{SelectedMod?.Name ?? selectedId} was updated successfully");
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not update mod: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveSelectedAsync()
    {
        if (SelectedMod is null)
            return;

        var mod = SelectedMod;
        var confirmed = await _dialogs.ConfirmAsync(
            $"Remove {mod.Name}?",
            $"This will permanently remove '{mod.Id}' from this OpenKH installation.",
            "Remove Mod");
        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            var visibleIndex = Mods.IndexOf(mod);
            await _maintenanceService.RemoveAsync(mod.Model);
            _allMods.Remove(mod);
            Mods.Remove(mod);
            SelectedMod = Mods.Count == 0
                ? null
                : Mods[Math.Min(Math.Max(visibleIndex, 0), Mods.Count - 1)];
            _catalogService.SaveEnabledOrder(SelectedGame, _allMods.Select(item => item.Model));
            NotifySummaryChanged();
            SetSuccessStatus($"{mod.Name} was removed");
        }
        catch (Exception exception)
        {
            SetErrorStatus($"Could not remove mod: {exception.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NotifySummaryChanged()
    {
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(EnabledCount));
        OnPropertyChanged(nameof(LibrarySummary));
        OnPropertyChanged(nameof(EnabledSummary));
        CheckUpdatesCommand.NotifyCanExecuteChanged();
        BuildCommand.NotifyCanExecuteChanged();
        BuildAndPlayCommand.NotifyCanExecuteChanged();
        ApplyToGameCommand.NotifyCanExecuteChanged();
        FastPatchCommand.NotifyCanExecuteChanged();
    }

    public void HandleControllerAction(ControllerAction action)
    {
        switch (action)
        {
            case ControllerAction.PreviousItem:
                SelectRelativeMod(-1);
                break;
            case ControllerAction.NextItem:
                SelectRelativeMod(1);
                break;
            case ControllerAction.Confirm:
                if (SelectedMod is not null)
                    SelectedMod.IsEnabled = !SelectedMod.IsEnabled;
                break;
            case ControllerAction.Cancel:
                SearchText = string.Empty;
                break;
            case ControllerAction.Secondary:
                if (OpenFolderCommand.CanExecute(null))
                    OpenFolderCommand.Execute(null);
                break;
            case ControllerAction.Install:
                if (InstallCommand.CanExecute(null))
                    InstallCommand.Execute(null);
                break;
            case ControllerAction.PreviousGame:
                SelectRelativeGame(-1);
                break;
            case ControllerAction.NextGame:
                SelectRelativeGame(1);
                break;
            case ControllerAction.Refresh:
                if (CheckUpdatesCommand.CanExecute(null))
                    CheckUpdatesCommand.Execute(null);
                break;
            case ControllerAction.MoveUp:
                if (MoveUpCommand.CanExecute(null))
                    MoveUpCommand.Execute(null);
                break;
            case ControllerAction.MoveDown:
                if (MoveDownCommand.CanExecute(null))
                    MoveDownCommand.Execute(null);
                break;
            case ControllerAction.MoveTop:
                if (MoveTopCommand.CanExecute(null))
                    MoveTopCommand.Execute(null);
                break;
        }
    }

    private void SelectRelativeMod(int offset)
    {
        if (Mods.Count == 0)
            return;

        var currentIndex = SelectedMod is null ? 0 : Mods.IndexOf(SelectedMod);
        var nextIndex = Math.Clamp(currentIndex + offset, 0, Mods.Count - 1);
        SelectedMod = Mods[nextIndex];
    }

    private void SelectRelativeGame(int offset)
    {
        var currentIndex = 0;
        for (var index = 0; index < Games.Count; index++)
        {
            if (Games[index] == SelectedGame)
            {
                currentIndex = index;
                break;
            }
        }
        var nextIndex = (currentIndex + offset + Games.Count) % Games.Count;
        SelectedGame = Games[nextIndex];
    }

    private void ControllerConnectionChanged()
    {
        OnPropertyChanged(nameof(ControllerStatusText));
        OnPropertyChanged(nameof(ControllerHelpText));
        OnPropertyChanged(nameof(IsControllerConnected));
        OnPropertyChanged(nameof(ShowControllerHelp));
    }
}
