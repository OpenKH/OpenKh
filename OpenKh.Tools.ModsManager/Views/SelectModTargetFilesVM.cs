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
        public ICommand SearchCommand { get; set; }

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

        public IEnumerable<ActionCommand> Actions { get; set; }

        public Action<IEnumerable<SearchHit>> OnSearchHitsSelected { get; set; } = _ => { };

        public SelectModTargetFilesVM()
        {
        }
    }
}
