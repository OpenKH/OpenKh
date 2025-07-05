using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xe.Tools;

namespace OpenKh.Tools.ModsManager.Views
{
    public class YamlGeneratorVM : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;

        #region GenerateCommand
        private ICommand _generateCommand = null;
        public ICommand GenerateCommand
        {
            get => _generateCommand;
            set
            {
                _generateCommand = value;
                OnPropertyChanged(nameof(GenerateCommand));
            }
        }
        #endregion

        #region LoadPrefCommand
        private ICommand _loadPrefCommand = null;
        public ICommand LoadPrefCommand
        {
            get => _loadPrefCommand;
            set
            {
                _loadPrefCommand = value;
                OnPropertyChanged(nameof(LoadPrefCommand));
            }
        }
        #endregion

        #region SavePrefCommand
        private ICommand _savePrefCommand = null;
        public ICommand SavePrefCommand
        {
            get => _savePrefCommand;
            set
            {
                _savePrefCommand = value;
                OnPropertyChanged(nameof(SavePrefCommand));
            }
        }
        #endregion

        #region SelectedPref
        private ConfigurationService.YamlGenPref _selectedPref = null;
        public ConfigurationService.YamlGenPref SelectedPref
        {
            get => _selectedPref;
            set
            {
                _selectedPref = value;
                OnPropertyChanged(nameof(SelectedPref));
            }
        }
        #endregion

        #region PrefLabel
        private string _prefLabel = "";
        public string PrefLabel
        {
            get => _prefLabel;
            set
            {
                _prefLabel = value;
                OnPropertyChanged(nameof(PrefLabel));
            }
        }
        #endregion

        #region Prefs
        private IEnumerable<ConfigurationService.YamlGenPref> _prefs = Array.Empty<ConfigurationService.YamlGenPref>();
        public IEnumerable<ConfigurationService.YamlGenPref> Prefs
        {
            get => _prefs;
            set
            {
                _prefs = value;
                OnPropertyChanged(nameof(Prefs));
            }
        }
        #endregion

        #region ModYmlFilePath
        private string _modYmlFilePath = "";
        public string ModYmlFilePath
        {
            get => _modYmlFilePath;
            set
            {
                _modYmlFilePath = value;
                OnPropertyChanged(nameof(ModYmlFilePath));
            }
        }
        #endregion

        #region GeneratingTask
        private Task _generatingTask;
        public Task GeneratingTask
        {
            get => _generatingTask;
            set
            {
                _generatingTask = value;
                OnPropertyChanged(nameof(GeneratingTask));
            }
        }
        #endregion

        #region Tools
        private IEnumerable<GetDiffService> _tools = Enumerable.Empty<GetDiffService>();
        public IEnumerable<GetDiffService> Tools
        {
            get => _tools;
            set
            {
                _tools = value;
                OnPropertyChanged(nameof(Tools));
                SelectedTool = _tools?.FirstOrDefault();
            }
        }
        #endregion

        #region SelectedTool
        private GetDiffService _selectedTool;
        public GetDiffService SelectedTool
        {
            get => _selectedTool;
            set
            {
                _selectedTool = value;
                OnPropertyChanged(nameof(SelectedTool));
            }
        }
        #endregion

        #region GameDataPath
        private string _gameDataPath = "";
        public string GameDataPath
        {
            get => _gameDataPath;
            set
            {
                _gameDataPath = value;
                OnPropertyChanged(nameof(GameDataPath));
            }
        }
        #endregion

        #region AppenderCommand
        private ICommand _appenderCommand = null;
        public ICommand AppenderCommand
        {
            get => _appenderCommand;
            set
            {
                _appenderCommand = value;
                OnPropertyChanged(nameof(AppenderCommand));
            }
        }
        #endregion

        #region AppenderTask
        private Task _appenderTask;
        public Task AppenderTask
        {
            get => _appenderTask;
            set
            {
                _appenderTask = value;
                OnPropertyChanged(nameof(AppenderTask));
            }
        }
        #endregion
    }
}
