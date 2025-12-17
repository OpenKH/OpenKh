using OpenKh.Tools.ModsManager.ExtensionMethods;
using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using Xe.Tools.Wpf.Commands;
using static OpenKh.Tools.ModsManager.Services.DownloadableModsService;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class ModSearchWindow : Window
    {
        private readonly string _currentGameId;
        private readonly ObservableCollection<DownloadableModViewModel> _allMods = new ObservableCollection<DownloadableModViewModel>();
        private readonly Action _reloadModsList;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly DownloadableModsService _downloadableModsService = DownloadableModsService.Default;
        private readonly Subject<DownloadableModViewModel> _onModInstalled = new Subject<DownloadableModViewModel>();
        private readonly ObservableCollection<string> _messages = new ObservableCollection<string>();
        private readonly Subject<string> _emitMessage = new Subject<string>();
        private readonly Subject<DownloadableModModel> _modEmitter = new Subject<DownloadableModModel>();
        private System.Windows.Data.FilterEventHandler _lastFilter = null;

        /// <summary>
        /// Cancelation token for loading operation
        /// </summary>
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public ModSearchVM VM { get; }

        public ModSearchWindow(Action reloadModsList = null)
        {
            InitializeComponent();
            DataContext = VM = new ModSearchVM();
            _reloadModsList = reloadModsList;

            IObservable<PropertyChangedEventArgs> OnVmPropertyChanged(string propertyName)
                => Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                    h => (s, e) => h(e),
                    h => VM.PropertyChanged += h,
                    h => VM.PropertyChanged -= h
                )
                    .Where(it => it.PropertyName == null || it.PropertyName == propertyName)
                    ;

            OnVmPropertyChanged(nameof(VM.SearchQuery))
                .Subscribe(
                    e =>
                    {
                        VM.HasSearchQuery = !string.IsNullOrEmpty(VM.SearchQuery);
                        ApplyNewFilter();
                    }
                )
                .AddTo(_disposables);

            OnVmPropertyChanged(nameof(VM.SelectedMod))
                .Subscribe(
                    e =>
                    {
                        VM.HasSelectedMod = VM.SelectedMod != null;
                    }
                )
                .AddTo(_disposables);

            VM.DownloadableMods.Source = _allMods;

            Observable.CombineLatest(
                Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => (s, e) => h(e),
                    h => VM.DownloadableMods.View.CollectionChanged += h,
                    h => VM.DownloadableMods.View.CollectionChanged -= h
                ),
                OnVmPropertyChanged(nameof(VM.IsLoading)),
                (a, b) => !VM.IsLoading && VM.DownloadableMods.View.IsEmpty
            )
                .Subscribe(it => VM.HasNoMods = it)
                .AddTo(_disposables);

            // Subscribe to status update events
            Observable.FromEvent<StatusUpdateHandler, string>(
                h => arg => h(arg),
                h => _downloadableModsService.OnStatusUpdate += h,
                h => _downloadableModsService.OnStatusUpdate -= h
            )
                .ObserveOn(DispatcherSynchronizationContext.Current)
                .Subscribe(
                    status =>
                    {
                        // Event handler to update the loading status
                        VM.LoadingStatusText = status;

                        _emitMessage.OnNext(status);
                    }
                )
                .AddTo(_disposables);

            // Subscribe to diag log events
            Observable.FromEvent<DiagLogHandler, string>(
                h => arg => h(arg),
                h => _downloadableModsService.OnDiagLog += h,
                h => _downloadableModsService.OnDiagLog -= h
            )
                .ObserveOn(DispatcherSynchronizationContext.Current)
                .Subscribe(
                    status =>
                    {
                        _emitMessage.OnNext(status);
                    }
                )
                .AddTo(_disposables);

            _onModInstalled
                .ObserveOn(DispatcherSynchronizationContext.Current)
                .Subscribe(
                    mod =>
                    {
                        try
                        {
                            // Clear selected mod if it was the one installed
                            if (VM.SelectedMod == mod)
                                VM.SelectedMod = null;

                            // Remove the mod from the list
                            _allMods.Remove(mod);

                            VM.DownloadableMods.View.Refresh();

                            // Select another mod if available
                            if (VM.SelectedMod == null && VM.DownloadableMods.View.MoveCurrentToFirst())
                                VM.SelectedMod = (DownloadableModViewModel)VM.DownloadableMods.View.CurrentItem;

                            // Update the main window's mod list if main view model is available
                            // Refresh the mods list in the main window
                            _reloadModsList?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error updating mod lists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                )
                .AddTo(_disposables);

            Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => (s, e) => h(e),
                h => _messages.CollectionChanged += h,
                h => _messages.CollectionChanged -= h
            )
                .ObserveOn(Scheduler.Immediate)
                .Subscribe(
                    e =>
                    {
                        VM.HasLog = _messages.Any();
                        VM.NumMessages = _messages.Count;
                    }
                )
                .AddTo(_disposables);

            _emitMessage
                .ObserveOn(DispatcherSynchronizationContext.Current)
                .Subscribe(msg => _messages.Add(msg))
                .AddTo(_disposables);

            // Use the current game ID from configuration
            _currentGameId = ConfigurationService.LaunchGame;

            _modEmitter
                .ObserveOn(DispatcherSynchronizationContext.Current)
                .Subscribe(
                    mod =>
                    {
                        var modVm = new DownloadableModViewModel(mod);
                        modVm.ModInstalled += OnModInstalled;
                        AddOrUpdateMod(modVm);
                    }
                )
                .AddTo(_disposables);

            // Initialize commands
            VM.ClearSearchCommand = new RelayCommand(_ => ClearSearch());

            VM.CancelCommand = new RelayCommand(_ => _cts.Cancel());

            VM.ClearLog = new RelayCommand(_ => _messages.Clear());

            VM.ShowLog = new RelayCommand(_ => MessageBox.Show(string.Join("\n", _messages.TakeLast(100))));

            VM.CopyLog = new RelayCommand(
                _ =>
                {
                    try
                    {
                        Clipboard.SetText(string.Join("\n", _messages));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error copying log to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            );

            Loaded += ModSearchWindow_Loaded;

            // Clean up resources on close
            Closed += (s, e) =>
            {
                _cts.Cancel();
                _disposables.Dispose();
            };
        }

        private async void ModSearchWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load mods for the game
            await LoadModsForSelectedGame();
        }

        // Removed game selector event handler as we now use the game selected in the main window

        private async Task LoadModsForSelectedGame()
        {
            if (string.IsNullOrEmpty(_currentGameId))
                return;

            // Cancel any previous operation in progress
            _cts.Cancel();
            // We won't dispose CancellationTokenSource.
            // It may cause ObjectDisposedException in normal flow.
            // Make GC collect it later.
            _cts = new CancellationTokenSource();

            try
            {
                VM.IsLoading = true;
                VM.LoadingStatusText = $"Loading mods for {GetGameName(_currentGameId)}...";

                _allMods.Clear();

                try
                {
                    foreach (var one in await _downloadableModsService.GetDownloadableModsLocallyAsync(_currentGameId))
                    {
                        _modEmitter.OnNext(one);
                    }

                    // Use the new method with cancellation support
                    await _downloadableModsService.GetDownloadableModsForGameAsync(
                        gameId: _currentGameId,
                        emitAsync: async mod => _modEmitter.OnNext(mod),
                        fallbackToLocalCache: false,
                        cancellationToken: _cts.Token
                    );
                }
                catch (OperationCanceledException)
                {
                    // Operation cancelled, return empty list
                    return;
                }
                catch (Exception ex)
                {
                    // Capture other exceptions and display them
                    VM.LoadingStatusText = $"Error: {ex.Message}";
                    _emitMessage.OnNext($"Full exception data: {ex}");
                    return;
                }

                var filtered = VM.DownloadableMods.View
                    .Cast<DownloadableModViewModel>();

                // Select first item
                VM.SelectedMod = filtered
                    .FirstOrDefault();

                // Show the number of mods found
                if (filtered.Any())
                {
                    VM.LoadingStatusText = $"Found {filtered.Count()} available mods for {GetGameName(_currentGameId)}";
                }
                else
                {
                    VM.LoadingStatusText = $"No available mods found for {GetGameName(_currentGameId)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading mods: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                VM.IsLoading = false;
            }
        }

        private void AddOrUpdateMod(DownloadableModViewModel modVm)
        {
            for (int y = 0, cy = _allMods.Count; y < cy; y++)
            {
                if (_allMods[y].Repo == modVm.Repo)
                {
                    // Update existing mod
                    _allMods[y] = modVm;
                    return;
                }
            }

            // Add new mod
            _allMods.Add(modVm);
        }

        /// <summary>
        /// Filters the mods list based on the current search query
        /// </summary>
        private void ApplyNewFilter()
        {
            // Remember selected mod
            var selectedMod = VM.SelectedMod;

            if (_lastFilter != null)
            {
                VM.DownloadableMods.Filter -= _lastFilter;
                _lastFilter = null;
            }

            // If we have a search query, filter by title and description
            if (!string.IsNullOrWhiteSpace(VM.SearchQuery))
            {
                var searchTerms = VM.SearchQuery.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                _lastFilter = (s, e) =>
                {
                    if (e.Item is DownloadableModViewModel mod)
                    {
                        // Check if any search term is found in title, description, or author
                        e.Accepted = searchTerms.All(term =>
                            (mod.Title?.ToLower().Contains(term) == true) ||
                            (mod.Description?.ToLower().Contains(term) == true) ||
                            (mod.Author?.ToLower().Contains(term) == true));
                    }
                    else
                    {
                        e.Accepted = false;
                    }
                };
                VM.DownloadableMods.Filter += _lastFilter;
            }

            var filtered = VM.DownloadableMods.View.Cast<DownloadableModViewModel>();

            // Try to restore selection or select first item
            if (true
                && selectedMod != null
                && filtered
                    .FirstOrDefault(vm => vm.Repo == selectedMod.Repo) is var found
                && found != null
            )
            {
                VM.SelectedMod = found;
            }
            else if (filtered.Any())
            {
                VM.SelectedMod = filtered.First();
            }
            else
            {
                VM.SelectedMod = null;
            }
        }

        /// <summary>
        /// Clears the search box and shows all mods
        /// </summary>
        private void ClearSearch()
        {
            VM.SearchQuery = string.Empty;
        }

        private void Mod_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DownloadableModViewModel mod)
            {
                VM.SelectedMod = mod;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        // Helper method to get readable game name
        private string GetGameName(string gameId)
        {
            switch (gameId?.ToLower())
            {
                case "kh1":
                    return "Kingdom Hearts 1";
                case "kh2":
                    return "Kingdom Hearts 2";
                case "bbs":
                    return "Birth By Sleep";
                case "recom":
                    return "Re:Chain of Memories";
                case "kh3d":
                    return "Dream Drop Distance";
                default:
                    return gameId ?? "Unknown Game";
            }
        }

        /// <summary>
        /// Called when a mod has been successfully installed
        /// </summary>
        /// <param name="mod">The mod that was installed</param>
        private void OnModInstalled(DownloadableModViewModel mod)
        {
            _onModInstalled.OnNext(mod);
        }
    }
}
