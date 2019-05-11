using kh.kh2;
using System.IO;
using System.Linq;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace kh.tools.lvup.ViewModels
{
    public class LvupViewModel : BaseNotifyPropertyChanged
    {
        public CharactersViewModel Characters { get; set; }

        public RelayCommand OpenCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand ExitCommand { get; }

        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private bool IsFileLoaded;

        public LvupViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(null, FileDialog.Behavior.Open, ("00battle.bin", "bin"));
                if (fd.ShowDialog() == true)
                {
                    Open(fd.FileName);
                }
            });
            SaveCommand = new RelayCommand(x =>
            {

            }, x => IsFileLoaded && Lvup.CanSave());
            ExitCommand = new RelayCommand(x => Window.Close());
        }

        public LvupViewModel(Stream stream)
        {
            Characters = new CharactersViewModel(Lvup.Open(stream));
            OnPropertyChanged(nameof(Characters));
        }

        private void Open(string fileName)
        {
            using (var file = File.Open(fileName, FileMode.Open))
            {
                var ent = Bar.Open(file,
                    (str, type) => str == "lvup" && type == Bar.EntryType.Msg)
                    .First();

                if (ent != null)
                {
                    Characters = new CharactersViewModel(Lvup.Open(ent.Stream));
                    OnPropertyChanged(nameof(Characters));

                    IsFileLoaded = true;
                    OnPropertyChanged(nameof(IsFileLoaded));
                }
            }
        }
    }
}
