using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Models.ViewHelper;
using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xe.Tools;

namespace OpenKh.Tools.ModsManager.Views
{
    public class SelectModTargetFilesVM : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;

        #region SearchCommand
        private ICommand _searchCommand = null;
        public ICommand SearchCommand
        {
            get => _searchCommand;
            set
            {
                _searchCommand = value;
                OnPropertyChanged(nameof(SearchCommand));
            }
        }
        #endregion

        #region SearchKeywords
        private string _searchKeywords = "";
        public string SearchKeywords
        {
            get => _searchKeywords;
            set
            {
                _searchKeywords = value;
                OnPropertyChanged(nameof(SearchKeywords));
            }
        }
        #endregion

        #region SearchHits
        private IEnumerable<SearchHit> _searchHits = Enumerable.Empty<SearchHit>();
        public IEnumerable<SearchHit> SearchHits
        {
            get => _searchHits;
            set
            {
                _searchHits = value;
                OnPropertyChanged(nameof(SearchHits));
            }
        }
        #endregion

        #region SearchingTask
        private Task _searchingTask = null;
        public Task SearchingTask
        {
            get => _searchingTask;
            set
            {
                _searchingTask = value;
                OnPropertyChanged(nameof(SearchingTask));
            }
        }
        #endregion

        #region Actions
        private IEnumerable<ActionCommand> _actions = Array.Empty<ActionCommand>();
        public IEnumerable<ActionCommand> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                OnPropertyChanged(nameof(Actions));
            }
        }
        #endregion

        #region SearchHitSelectedList
        private IEnumerable<SearchHit> _searchHitSelectedList = Array.Empty<SearchHit>();
        public IEnumerable<SearchHit> SearchHitSelectedList
        {
            get => _searchHitSelectedList;
            set
            {
                _searchHitSelectedList = value;
                OnPropertyChanged(nameof(SearchHitSelectedList));
            }
        }
        #endregion

    }
}
