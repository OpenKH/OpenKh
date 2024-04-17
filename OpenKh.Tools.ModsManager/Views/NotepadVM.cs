using OpenKh.Tools.ModsManager.Services;
using System.Windows.Input;
using Xe.Tools;

namespace OpenKh.Tools.ModsManager.Views
{
    public class NotepadVM : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;

        #region CopyAllCommand
        private ICommand _copyAllCommand = null;
        public ICommand CopyAllCommand
        {
            get => _copyAllCommand;
            set
            {
                _copyAllCommand = value;
                OnPropertyChanged(nameof(CopyAllCommand));
            }
        }
        #endregion

        #region SaveAsCommand
        private ICommand _saveAsCommand = null;
        public ICommand SaveAsCommand
        {
            get => _saveAsCommand;
            set
            {
                _saveAsCommand = value;
                OnPropertyChanged(nameof(SaveAsCommand));
            }
        }
        #endregion

        #region Text
        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        #endregion
    }
}
