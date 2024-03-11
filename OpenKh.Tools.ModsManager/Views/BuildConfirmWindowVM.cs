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
    public class BuildConfirmWindowVM : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;

        public bool CleanupModDir
        {
            get => ConfigurationService.CleanupModDir;
            set
            {
                ConfigurationService.CleanupModDir = value;
                OnPropertyChanged(nameof(CleanupModDir));
            }
        }

        public ICommand GoCommand { get; set; }
    }
}
