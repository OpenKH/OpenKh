using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class OnlineModsWindow : EmbeddedDialogControl
{
    private readonly OnlineModCatalogService? _catalog;
    private readonly RepositoryModInstaller? _installer;
    private readonly GameInfo? _game;
    private readonly IReadOnlyCollection<string> _installedIds = [];
    private readonly Func<Task>? _onModInstalled;
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
        Func<Task> onModInstalled) : this()
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
                    LoadingProgressBar.Value = percentage;
            });
            var itemProgress = new Progress<OnlineModInfo>(mod =>
            {
                if (_isClosed)
                    return;
                if (_installedRepositories.Contains(mod.Repository) ||
                    _allMods.Any(item => item.Repository.Equals(mod.Repository, StringComparison.OrdinalIgnoreCase)))
                    return;
                _allMods.Add(new OnlineModItem(mod));
                ApplyFilter();
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
            {
                if (_installedRepositories.Contains(mod.Repository) ||
                    _allMods.Any(item => item.Repository.Equals(mod.Repository, StringComparison.OrdinalIgnoreCase)))
                    continue;
                _allMods.Add(new OnlineModItem(mod));
            }
            ApplyFilter();
            ModsList.SelectedItem = _visibleMods.FirstOrDefault();
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
        var query = SearchTextBox.Text?.Trim();
        var terms = string.IsNullOrWhiteSpace(query)
            ? []
            : query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        _visibleMods.Clear();
        foreach (var mod in _allMods.Where(mod => terms.All(mod.Contains)))
            _visibleMods.Add(mod);
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
        if (_installer is null || _game is null || ModsList.SelectedItem is not OnlineModItem selected)
            return;

        SetInstalling(true);
        try
        {
            var progress = new Progress<ModOperationProgress>(value =>
            {
                StatusText.Text = value.Message;
                if (value.Percentage is { } percentage)
                    LoadingProgressBar.Value = percentage;
            });
            await _installer.InstallAsync(selected.Repository, _game, progress: progress);
            _installedAny = true;
            _installedRepositories.Add(selected.Repository);
            if (_onModInstalled is not null)
                await _onModInstalled();
            _allMods.Remove(selected);
            ApplyFilter();
            ModsList.SelectedItem = _visibleMods.FirstOrDefault();
            StatusText.Text = $"{selected.Title} was installed";
            selected.Dispose();
        }
        catch (Exception exception)
        {
            StatusText.Text = $"Could not install {selected.Title}: {exception.Message}";
        }
        finally
        {
            SetInstalling(false);
        }
    }

    private void SetCatalogLoading(bool loading)
    {
        _isCatalogLoading = loading;
        LoadingProgressBar.IsVisible = loading || _isInstalling;
        if (loading)
            LoadingProgressBar.Value = 0;
        UpdateControlState();
    }

    private void SetInstalling(bool installing)
    {
        _isInstalling = installing;
        LoadingProgressBar.IsVisible = installing || _isCatalogLoading;
        UpdateControlState();
    }

    private void UpdateControlState()
    {
        SearchTextBox.IsEnabled = !_isInstalling;
        ModsList.IsEnabled = !_isInstalling;
        InstallSelectedButton.IsEnabled = !_isInstalling && ModsList.SelectedItem is not null;
        CloseButton.IsEnabled = !_isInstalling;
    }

    private void Close_OnClick(object? sender, RoutedEventArgs eventArgs) => Close(_installedAny);

    public void HandleControllerAction(ControllerAction action)
    {
        if (action is ControllerAction.PreviousControl)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action is ControllerAction.PreviousItem)
            MoveSelection(-1);
        else if (action is ControllerAction.NextItem)
            MoveSelection(1);
        else if (action == ControllerAction.Cancel && CloseButton.IsEnabled)
            Close(_installedAny);
        else if (action is ControllerAction.Confirm or ControllerAction.Install)
            InstallSelected_OnClick(InstallSelectedButton, new RoutedEventArgs());
    }

    private void MoveSelection(int offset)
    {
        if (_visibleMods.Count == 0)
            return;
        var index = ModsList.SelectedIndex < 0 ? 0 : ModsList.SelectedIndex;
        ModsList.SelectedIndex = Math.Clamp(index + offset, 0, _visibleMods.Count - 1);
        if (ModsList.SelectedItem is { } selected)
            ModsList.ScrollIntoView(selected);
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
