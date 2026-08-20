using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace OpenKh.Tools.ModsManager.Views
{
    // Avalonia counterpart of the WPF ModSearchVM: WPF's CollectionViewSource
    // (DownloadableMods.View) is replaced with a plain FilteredMods collection
    // that the window rebuilds when the search query changes.
    public class ModSearchVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ColorThemeService ColorTheme => ColorThemeService.Instance;

        public ObservableCollection<DownloadableModViewModel> FilteredMods { get; } = new ObservableCollection<DownloadableModViewModel>();

        public ICommand ClearSearchCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand ClearLog { get; set; }

        public ICommand ShowLog { get; set; }

        public ICommand CopyLog { get; set; }

        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }

        private string _loadingStatusText = "Initializing...";

        public string LoadingStatusText
        {
            get => _loadingStatusText;
            set
            {
                _loadingStatusText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadingStatusText)));
            }
        }

        private string _searchQuery = "";

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchQuery)));
            }
        }

        private bool _hasSearchQuery;

        public bool HasSearchQuery
        {
            get => _hasSearchQuery;
            set
            {
                _hasSearchQuery = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSearchQuery)));
            }
        }

        private bool _hasNoMods;

        public bool HasNoMods
        {
            get => _hasNoMods;
            set
            {
                _hasNoMods = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasNoMods)));
            }
        }

        private bool _hasSelectedMod;

        public bool HasSelectedMod
        {
            get => _hasSelectedMod;
            set
            {
                _hasSelectedMod = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSelectedMod)));
            }
        }

        private DownloadableModViewModel _selectedMod;

        public DownloadableModViewModel SelectedMod
        {
            get => _selectedMod;
            set
            {
                _selectedMod = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedMod)));
            }
        }

        private bool _hasLog;

        public bool HasLog
        {
            get => _hasLog;
            set
            {
                _hasLog = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasLog)));
            }
        }

        private int _numMessages;

        public int NumMessages
        {
            get => _numMessages;
            set
            {
                _numMessages = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumMessages)));
            }
        }
    }
}
