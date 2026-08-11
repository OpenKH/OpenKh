using OpenKh.Tools.ModsManager.Core;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace OpenKh.Tools.ModsManager.Avalonia.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly ModCatalogService _catalogService;
    private readonly LocalModInstaller _modInstaller;
    private readonly IModPackagePicker _modPackagePicker;
    private readonly List<ModListItemViewModel> _allMods = [];
    private GameInfo _selectedGame;
    private ModListItemViewModel? _selectedMod;
    private string _searchText = string.Empty;
    private string _statusText = "Loading your mods";
    private bool _isBusy;

    public MainWindowViewModel(
        ModCatalogService catalogService,
        LocalModInstaller modInstaller,
        IModPackagePicker modPackagePicker)
    {
        _catalogService = catalogService;
        _modInstaller = modInstaller;
        _modPackagePicker = modPackagePicker;
        _selectedGame = catalogService.DefaultGame;
        Games = GameInfo.SupportedGames;
        Mods = [];

        RefreshCommand = new RelayCommand(() => _ = RefreshAsync(), () => !IsBusy);
        MoveUpCommand = new RelayCommand(() => MoveSelected(-1), () => CanMoveSelected(-1));
        MoveDownCommand = new RelayCommand(() => MoveSelected(1), () => CanMoveSelected(1));
        OpenFolderCommand = new RelayCommand(OpenSelectedFolder, () => SelectedMod is not null);
        InstallCommand = new AsyncRelayCommand(InstallPackageAsync, () => !IsBusy);

        _ = RefreshAsync();
    }

    public IReadOnlyList<GameInfo> Games { get; }
    public ObservableCollection<ModListItemViewModel> Mods { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand MoveUpCommand { get; }
    public RelayCommand MoveDownCommand { get; }
    public RelayCommand OpenFolderCommand { get; }
    public AsyncRelayCommand InstallCommand { get; }
    public string InstallationDirectory => _catalogService.InstallationDirectory;

    public GameInfo SelectedGame
    {
        get => _selectedGame;
        set
        {
            if (!SetProperty(ref _selectedGame, value) || value is null)
                return;

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
            OpenFolderCommand.NotifyCanExecuteChanged();
        }
    }

    public bool HasSelection => SelectedMod is not null;

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
        private set => SetProperty(ref _statusText, value);
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
            SelectedMod = Mods.FirstOrDefault();
            StatusText = TotalCount == 0
                ? "No mods were found for this game"
                : $"{LibrarySummary}, {EnabledSummary}";
            NotifySummaryChanged();
        }
        catch (Exception exception)
        {
            StatusText = $"Could not load mods: {exception.Message}";
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
        return index >= 0 && index + offset >= 0 && index + offset < _allMods.Count;
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
        ApplyFilter();
        SelectedMod = Mods.First(mod => mod.Id == selectedId);
        SaveEnabledOrder();
        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
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

    private async Task InstallPackageAsync()
    {
        var packagePath = await _modPackagePicker.PickPackageAsync();
        if (string.IsNullOrEmpty(packagePath))
            return;

        try
        {
            IsBusy = true;
            StatusText = $"Installing {Path.GetFileName(packagePath)}";
            var result = await _modInstaller.InstallAsync(packagePath, SelectedGame);
            await RefreshAsync();
            SelectedMod = Mods.FirstOrDefault(mod =>
                mod.Id.Equals(result.Id, StringComparison.OrdinalIgnoreCase));
            StatusText = $"{result.DisplayName} was installed successfully";
        }
        catch (Exception exception)
        {
            StatusText = $"Could not install mod: {exception.Message}";
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
    }
}
