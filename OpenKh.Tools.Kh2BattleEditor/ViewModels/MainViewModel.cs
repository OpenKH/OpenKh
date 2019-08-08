using System.Linq;
using System.Windows;
using OpenKh.Tools.Common;
using Xe.Tools;

namespace OpenKh.Tools.Kh2BattleEditor.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private static string ApplicationName = Utilities.GetApplicationName();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private string _fileName;

        public string Title => $"{FileName ?? "untitled"} | {ApplicationName}";

        private string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }
}
