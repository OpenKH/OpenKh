using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class ModSearchWindow : Window, INotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        public ObservableCollection<DownloadableModViewModel> DownloadableMods { get; } = new ObservableCollection<DownloadableModViewModel>();

        private bool _isLoading;
        private string _loadingStatusText = "Initializing...";
        private DownloadableModViewModel _selectedMod;
        private readonly List<string> _gameIds = new List<string>() { "kh2", "kh1", "bbs", "Recom", "kh3d" };
        private string _currentGameId;
        private string _searchQuery = "";
        private List<DownloadableModModel> _allMods = new List<DownloadableModModel>();
        
        // Reference to the main view model to update installed mods
        private ViewModels.MainViewModel _mainViewModel;
        
        // Cancelation token for loading operation
        private CancellationTokenSource _cts;
        
        // Command for clearing the search box
        public ICommand ClearSearchCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }
        
        public string LoadingStatusText
        {
            get => _loadingStatusText;
            private set
            {
                _loadingStatusText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadingStatusText)));
            }
        }

        // Manejador de evento para actualizar el estado de carga
        private void OnModLoadingStatusUpdate(string status)
        {
            Application.Current.Dispatcher.Invoke(() => {
                LoadingStatusText = status;
            });
        }

        public bool HasNoMods => !IsLoading && DownloadableMods.Count == 0;

        public DownloadableModViewModel SelectedMod
        {
            get => _selectedMod;
            set
            {
                _selectedMod = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedMod)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSelectedMod)));
            }
        }

        public bool HasSelectedMod => SelectedMod != null;
        
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchQuery)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSearchQuery)));
                FilterMods();
            }
        }
        
        public bool HasSearchQuery => !string.IsNullOrWhiteSpace(SearchQuery);

        public ModSearchWindow(ViewModels.MainViewModel mainViewModel = null)
        {
            InitializeComponent();
            DataContext = this;
            
            // Store the reference to main view model
            _mainViewModel = mainViewModel;
            
            // Subscribe to status update events
            DownloadableModsService.OnStatusUpdate += OnModLoadingStatusUpdate;
            
            // Use the current game ID from configuration
            _currentGameId = ConfigurationService.LaunchGame;
            
            // Initialize commands
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());
            
            Loaded += ModSearchWindow_Loaded;
            
            // Clean up resources on close
            Closing += (s, e) => {
                _cts?.Cancel();
                _cts?.Dispose();
                
                // Unsubscribe from events
                DownloadableModsService.OnStatusUpdate -= OnModLoadingStatusUpdate;
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
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            
            try
            {
                IsLoading = true;
                LoadingStatusText = $"Loading mods for {GetGameName(_currentGameId)}...";
                
                var mods = await Task.Run(async () => 
                {
                    try {
                        // Use the new method with cancellation support
                        var mods = await DownloadableModsService.GetDownloadableModsForGameAsync(_currentGameId, _cts.Token);
                        // Store all mods for filtering
                        _allMods = mods;
                        return mods;
                    } catch (OperationCanceledException) {
                        // Operation cancelled, return empty list
                        _allMods.Clear();
                        return new List<DownloadableModModel>();
                    } catch (Exception ex) {
                        // Capture other exceptions and display them
                        Application.Current.Dispatcher.Invoke(() => {
                            LoadingStatusText = $"Error: {ex.Message}";
                        });
                        return new List<DownloadableModModel>();
                    }
                });
                
                if (!_cts.Token.IsCancellationRequested)
                {
                    DownloadableMods.Clear();
                    foreach (var mod in mods)
                    {
                        var viewModel = new DownloadableModViewModel(mod);
                        // Subscribe to the ModInstalled event
                        viewModel.ModInstalled += OnModInstalled;
                        DownloadableMods.Add(viewModel);
                    }
                    
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasNoMods)));
                    
                    // Select first item
                    if (DownloadableMods.Count > 0)
                        SelectedMod = DownloadableMods[0];
                    
                    // Apply search filter if there is a search query
                    if (!string.IsNullOrEmpty(SearchQuery))
                    {
                        FilterMods();
                    }
                    
                    // Show the number of mods found
                    if (DownloadableMods.Count > 0)
                        LoadingStatusText = $"Found {DownloadableMods.Count} available mods for {GetGameName(_currentGameId)}";
                    else
                        LoadingStatusText = $"No available mods found for {GetGameName(_currentGameId)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading mods: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Clear status update event
                DownloadableModsService.OnStatusUpdate -= (status) => {};
                
                IsLoading = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasNoMods)));
            }
        }
        
        /// <summary>
        /// Filters the mods list based on the current search query
        /// </summary>
        private void FilterMods()
        {
            if (_allMods == null)
                return;
                
            // Remember selected mod
            var selectedMod = SelectedMod;
                
            // Clear and refill the observable collection with filtered results
            DownloadableMods.Clear();
            
            IEnumerable<DownloadableModModel> filteredMods = _allMods;
            
            // If we have a search query, filter by title and description
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchTerms = SearchQuery.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                filteredMods = _allMods.Where(mod => 
                {
                    // Check if any search term is found in title, description, or author
                    return searchTerms.All(term => 
                        (mod.Title?.ToLower().Contains(term) == true) ||
                        (mod.Description?.ToLower().Contains(term) == true) ||
                        (mod.OriginalAuthor?.ToLower().Contains(term) == true));
                });
            }
            
            // Add filtered mods to the collection
            foreach (var mod in filteredMods)
            {
                var viewModel = new DownloadableModViewModel(mod);
                viewModel.ModInstalled += OnModInstalled;
                DownloadableMods.Add(viewModel);
            }
            
            // Try to restore selection or select first item
            if (selectedMod != null && DownloadableMods.Any(vm => vm.Repo == selectedMod.Repo))
            {
                SelectedMod = DownloadableMods.First(vm => vm.Repo == selectedMod.Repo);
            }
            else if (DownloadableMods.Count > 0)
            {
                SelectedMod = DownloadableMods[0];
            }
            else
            {
                SelectedMod = null;
            }
            
            // Update UI properties
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasNoMods)));
        }
        
        /// <summary>
        /// Clears the search box and shows all mods
        /// </summary>
        private void ClearSearch()
        {
            SearchQuery = string.Empty;
        }

        private void Mod_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DownloadableModViewModel mod)
            {
                SelectedMod = mod;
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
                case "kh1": return "Kingdom Hearts 1";
                case "kh2": return "Kingdom Hearts 2";
                case "bbs": return "Birth By Sleep";
                case "recom": return "Re:Chain of Memories";
                case "kh3d": return "Dream Drop Distance";
                default: return gameId ?? "Unknown Game";
            }
        }
        
        /// <summary>
        /// Called when a mod has been successfully installed
        /// </summary>
        /// <param name="mod">The mod that was installed</param>
        private void OnModInstalled(DownloadableModViewModel mod)
        {
            // Execute on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    // Clear selected mod if it was the one installed
                    if (SelectedMod == mod)
                        SelectedMod = null;
                        
                    // Remove the mod from the list
                    DownloadableMods.Remove(mod);
                    
                    // Update properties
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasNoMods)));
                    
                    // Select another mod if available
                    if (DownloadableMods.Count > 0 && SelectedMod == null)
                        SelectedMod = DownloadableMods[0];
                    
                    // Update the main window's mod list if main view model is available
                    if (_mainViewModel != null)
                    {
                        // Refresh the mods list in the main window
                        _mainViewModel.ReloadModsList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating mod lists: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
    }
}
