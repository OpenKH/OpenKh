using OpenKh.Tools.ModsManager.Models.ViewHelper;
using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xe.Tools;

namespace OpenKh.Tools.ModsManager.Views
{
    public class CopySourceFilesVM : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;

        public ICommand ProceedCommand { get; set; }

        #region ProceedTask
        private Task _proceedTask = null;
        public Task ProceedTask
        {
            get => _proceedTask;
            set
            {
                _proceedTask = value;
                OnPropertyChanged(nameof(ProceedTask));
            }
        }
        #endregion

        #region CopySourceList
        private IEnumerable<CopySourceFile> _copySourceList = Array.Empty<CopySourceFile>();
        public IEnumerable<CopySourceFile> CopySourceList
        {
            get => _copySourceList;
            set
            {
                _copySourceList = value;
                OnPropertyChanged(nameof(CopySourceList));
            }
        }
        #endregion

        #region SelectedPrimarySource
        private PrimarySource _selectedPrimarySource = null;
        public PrimarySource SelectedPrimarySource
        {
            get => _selectedPrimarySource;
            set
            {
                _selectedPrimarySource = value;
                OnPropertyChanged(nameof(SelectedPrimarySource));
            }
        }
        #endregion

        #region PrimarySourceList
        private IEnumerable<PrimarySource> _primarySourceList = Array.Empty<PrimarySource>();
        public IEnumerable<PrimarySource> PrimarySourceList
        {
            get => _primarySourceList;
            set
            {
                _primarySourceList = value;
                OnPropertyChanged(nameof(PrimarySourceList));
                SelectedPrimarySource = value?.FirstOrDefault();
            }
        }
        #endregion

        public ICommand CheckAllCommand { get; set; }
        public ICommand UncheckAllCommand { get; set; }
    }
}
