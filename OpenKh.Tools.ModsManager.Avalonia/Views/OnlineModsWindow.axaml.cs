using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Input;
using Avalonia.VisualTree;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class OnlineModsWindow : EmbeddedDialogControl
{
    private readonly OnlineModCatalogService? _catalog;
    private readonly RepositoryModInstaller? _installer;
    private readonly GameInfo? _game;
    private readonly IReadOnlyCollection<string> _installedIds = [];
    private readonly Func<string, Task>? _onModInstalled;
    private readonly List<OnlineModItem> _allMods = [];
    private readonly ObservableCollection<OnlineModItem> _visibleMods = [];
    private readonly HashSet<string> _installedRepositories = new(StringComparer.OrdinalIgnoreCase);
    private readonly CancellationTokenSource _loadCancellation = new();
    private bool _installedAny;
    private bool _isCatalogLoading;
    private bool _isInstalling;
    private bool _isClosed;

    public OnlineModsWindow()
    {
        InitializeComponent();
        ModsList.ItemsSource = _visibleMods;
    }

    public OnlineModsWindow(
        OnlineModCatalogService catalog,
        RepositoryModInstaller installer,
        GameInfo game,
        IReadOnlyCollection<string> installedIds,
        Func<string, Task> onModInstalled) : this()
    {
        _catalog = catalog;
        _installer = installer;
        _game = game;
        _installedIds = installedIds;
        _onModInstalled = onModInstalled;
        GameHelpText.Text = $"Available community mods for {game.DisplayName}";
        Opened += async (_, _) => await LoadAsync();
        Closed += (_, _) =>
        {
            _isClosed = true;
            _loadCancellation.Cancel();
            _loadCancellation.Dispose();
            DisposeImages();
        };
    }

    public bool InstalledAny => _installedAny;

    private async Task LoadAsync()
    {
        if (_catalog is null || _game is null)
            return;

        SetCatalogLoading(true);
        try
        {
            var progress = new Progress<ModOperationProgress>(value =>
            {
                if (_isClosed)
                    return;
                StatusText.Text = value.Message;
                if (value.Percentage is { } percentage)
                {
                    LoadingProgressBar.IsIndeterminate = false;
                    LoadingProgressBar.Value = percentage;
                }
            });
            var itemProgress = new Progress<OnlineModInfo>(mod =>
            {
                if (_isClosed)
                    return;
                TryAddCatalogMod(mod);
                ModsList.SelectedItem ??= _visibleMods.FirstOrDefault();
            });
            var mods = await _catalog.LoadAsync(
                _game,
                _installedIds,
                progress,
                itemProgress,
                _loadCancellation.Token);
            if (_isClosed)
                return;
            foreach (var mod in mods)
                TryAddCatalogMod(mod);
            ModsList.SelectedItem ??= _visibleMods.FirstOrDefault();
            StatusText.Text = _allMods.Count == 1 ? "1 mod is available" : $"{_allMods.Count} mods are available";
        }
        catch (OperationCanceledException) when (_isClosed)
        {
        }
        catch (Exception exception)
        {
            StatusText.Text = $"Could not load online mods: {exception.Message}";
        }
        finally
        {
            if (!_isClosed)
                SetCatalogLoading(false);
        }
    }

    private void Search_OnTextChanged(object? sender, TextChangedEventArgs eventArgs) => ApplyFilter();

    private void ApplyFilter()
    {
        var selected = ModsList.SelectedItem as OnlineModItem;
        _visibleMods.Clear();
        foreach (var mod in _allMods.Where(MatchesCurrentFilter))
            _visibleMods.Add(mod);
        ModsList.SelectedItem = selected is not null && _visibleMods.Contains(selected)
            ? selected
            : _visibleMods.FirstOrDefault();
    }

    private void TryAddCatalogMod(OnlineModInfo mod)
    {
        if (_installedRepositories.Contains(mod.Repository) ||
            _allMods.Any(item => item.Repository.Equals(mod.Repository, StringComparison.OrdinalIgnoreCase)))
            return;

        var item = new OnlineModItem(mod);
        _allMods.Add(item);
        if (MatchesCurrentFilter(item))
            _visibleMods.Add(item);
    }

    private bool MatchesCurrentFilter(OnlineModItem mod)
    {
        var query = SearchTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query))
            return true;

        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return terms.All(mod.Contains);
    }

    private void ModsList_OnSelectionChanged(object? sender, SelectionChangedEventArgs eventArgs)
    {
        var selected = ModsList.SelectedItem as OnlineModItem;
        SelectedTitleText.Text = selected?.Title ?? "Select a mod";
        SelectedAuthorText.Text = selected?.Author ?? "";
        SelectedDescriptionText.Text = selected?.Description ?? "Select an entry to view its description.";
        SelectedRepositoryText.Text = selected?.Repository ?? "";
        SelectedPreviewImage.Source = selected?.PreviewImage;
        UpdateControlState();
    }

    private async void InstallSelected_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_isInstalling || _installer is null || _game is null ||
            ModsList.SelectedItem is not OnlineModItem selected)
            return;

        SetInstalling(true);
        InstallSelectedButton.Content = $"Installing {selected.Title}...";
        StatusText.Text = $"Preparing to install {selected.Title}";
        LoadingProgressBar.Value = 0;
        LoadingProgressBar.IsIndeterminate = true;
        try
        {
            var progress = new Progress<ModOperationProgress>(value =>
            {
                if (_isClosed)
                    return;
                StatusText.Text = value.Message;
                if (value.Percentage is { } percentage)
                {
                    LoadingProgressBar.IsIndeterminate = false;
                    LoadingProgressBar.Value = percentage;
                }
                else
                {
                    LoadingProgressBar.IsIndeterminate = true;
                }
            });
            var result = await _installer.InstallAsync(selected.Repository, _game, progress: progress);
            _installedAny = true;
            _installedRepositories.Add(selected.Repository);
            if (_onModInstalled is not null)
                await _onModInstalled(result.Id);
            var currentSelection = ModsList.SelectedItem as OnlineModItem;
            var installedIndex = _visibleMods.IndexOf(selected);
            _allMods.Remove(selected);
            _visibleMods.Remove(selected);
            ModsList.SelectedItem = currentSelection is not null &&
                !ReferenceEquals(currentSelection, selected) &&
                _visibleMods.Contains(currentSelection)
                    ? currentSelection
                    : _visibleMods.Count == 0
                        ? null
                        : _visibleMods[Math.Clamp(installedIndex, 0, _visibleMods.Count - 1)];
            StatusText.Text = $"{selected.Title} was installed";
            selected.Dispose();
        }
        catch (Exception exception)
        {
            StatusText.Text = $"Could not install {selected.Title}: {exception.Message}";
        }
        finally
        {
            InstallSelectedButton.Content = "Install selected mod";
            SetInstalling(false);
        }
    }

    private void SetCatalogLoading(bool loading)
    {
        _isCatalogLoading = loading;
        LoadingProgressBar.IsVisible = loading || _isInstalling;
        if (loading)
        {
            LoadingProgressBar.Value = 0;
            LoadingProgressBar.IsIndeterminate = true;
        }
        UpdateControlState();
    }

    private void SetInstalling(bool installing)
    {
        _isInstalling = installing;
        LoadingProgressBar.IsVisible = installing || _isCatalogLoading;
        if (!installing && !_isCatalogLoading)
            LoadingProgressBar.IsIndeterminate = false;
        UpdateControlState();
    }

    private void UpdateControlState()
    {
        SearchTextBox.IsEnabled = true;
        ModsList.IsEnabled = true;
        InstallSelectedButton.IsEnabled = !_isInstalling && ModsList.SelectedItem is not null;
        CloseButton.IsEnabled = !_isInstalling;
    }

    private void Close_OnClick(object? sender, RoutedEventArgs eventArgs) => Close(_installedAny);

    public void HandleControllerAction(ControllerAction action)
    {
        if (ControllerWindowNavigator.TryMoveFocus(this, action))
            return;
        else if (action == ControllerAction.Cancel && CloseButton.IsEnabled)
            Close(_installedAny);
        else if (action is ControllerAction.Confirm or ControllerAction.Install)
            ActivateFocusedControl();
    }

    private void ModsList_OnGotFocus(object? sender, FocusChangedEventArgs eventArgs)
    {
        var focused = eventArgs.Source as Control;
        var item = focused as ListBoxItem ??
            focused?.GetVisualAncestors().OfType<ListBoxItem>().FirstOrDefault();
        if (item?.DataContext is { } mod)
            ModsList.SelectedItem = mod;
    }

    private void ActivateFocusedControl()
    {
        var focused = FocusManager?.GetFocusedElement() as Control;
        if (focused == CloseButton)
            Close(_installedAny);
        else if (focused == InstallSelectedButton || focused == ModsList ||
                 focused is ListBoxItem ||
                 focused?.GetVisualAncestors().OfType<ListBoxItem>().Any() == true)
            InstallSelected_OnClick(InstallSelectedButton, new RoutedEventArgs());
    }

    private void DisposeImages()
    {
        foreach (var mod in _allMods)
            mod.Dispose();
    }

    private sealed class OnlineModItem : IDisposable
    {
        public OnlineModItem(OnlineModInfo model)
        {
            Repository = model.Repository;
            Title = model.Title;
            Author = model.Author;
            Description = model.Description;
            IconImage = LoadBitmap(model.IconPath, 96);
            PreviewImage = LoadBitmap(model.PreviewPath, 720);
        }

        public string Repository { get; }
        public string Title { get; }
        public string Author { get; }
        public string Description { get; }
        public Bitmap? IconImage { get; }
        public Bitmap? PreviewImage { get; }
        public string Initial => string.IsNullOrWhiteSpace(Title) ? "?" : Title[..1].ToUpperInvariant();

        public bool Contains(string term) =>
            Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            Author.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            Repository.Contains(term, StringComparison.OrdinalIgnoreCase);

        public void Dispose()
        {
            IconImage?.Dispose();
            PreviewImage?.Dispose();
        }

        private static Bitmap? LoadBitmap(string? path, int decodeWidth)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;

            try
            {
                using var stream = File.OpenRead(path);
                return Bitmap.DecodeToWidth(stream, decodeWidth);
            }
            catch
            {
                return null;
            }
        }
    }
}
