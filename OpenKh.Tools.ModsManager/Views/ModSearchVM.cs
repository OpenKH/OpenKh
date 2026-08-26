using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace OpenKh.Tools.ModsManager.Views
{
    public class ModSearchVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ColorThemeService ColorTheme => ColorThemeService.Instance;

        public CollectionViewSource DownloadableMods { get; } = new CollectionViewSource();

        /// <summary>
        /// Command for clearing the search box
        /// </summary>
        public ICommand ClearSearchCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand ClearLog { get; set; }

        public ICommand ShowLog { get; set; }

        public ICommand CopyLog { get; set; }

        #region IsLoading
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
        #endregion

        #region LoadingStatusText
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
        #endregion

        #region SearchQuery
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
        #endregion

        #region HasSearchQuery
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
        #endregion

        #region HasNoMods
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
        #endregion

        #region HasSelectedMod
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
        #endregion

        #region SelectedMod
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
        #endregion

        #region HasLog
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
        #endregion

        #region NumMessages
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
        #endregion

    }
}
