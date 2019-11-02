using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;


namespace OpenKh.Tools.ObjentryEditor.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private static string ApplicationName = Utilities.GetApplicationName();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private string _fileName;

        public string Title => $"{FileName ?? "untitled"} | {ApplicationName}";

        public ObjentryViewModel Objentry { get; private set; }

        private string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public RelayCommand OpenCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand AboutCommand { get; }
    
        public MainViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, new[]
                {
                    ("00objentry.bin", "bin"),
                    ("All files", "*")
                });

                if (fd.ShowDialog() == true)
                {
                    OpenFile(fd.FileName);
                }
            }, x => true);

            SaveCommand = new RelayCommand(x =>
            {
                if (!string.IsNullOrEmpty(FileName))
                {
                    SaveFile(FileName, FileName);
                }
                else
                {
                    SaveAsCommand.Execute(x);
                }
            }, x => true);

            SaveAsCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save);
                if (fd.ShowDialog() == true)
                {
                    SaveFile(FileName, fd.FileName);
                    FileName = fd.FileName;
                }
            }, x => true);

            ExitCommand = new RelayCommand(x =>
            {
                Window.Close();
            }, x => true);

            AboutCommand = new RelayCommand(x =>
            {
                new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);

        }

        public bool OpenFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            //if (!Bar.IsValid(stream))
            //{
            //    MessageBox.Show(Window, $"{Path.GetFileName(fileName)} is not a valid BAR file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return false;
            //}

            //var items = Bar.Read(stream);

            //if (!Is00battle(items))
            //{
            //    MessageBox.Show(Window, $"{Path.GetFileName(fileName)} does not appear to be a valid 00battle.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return false;
            //}

            //LoadBattleItems(items);

            //FileName = fileName;
            Objentry = new ObjentryViewModel(BaseTable<Objentry>.Read(stream));

            return true;
        });

        public void SaveFile(string previousFileName, string fileName)
        {
            //if (File.Exists(previousFileName))
            //{
            //    bool isBar = false;
            //    List<Bar.Entry> entries;

            //    entries = File.OpenRead(previousFileName).Using(stream =>
            //    {
            //        isBar = Bar.IsValid(stream);
            //        return isBar ? Bar.Read(stream) : null;
            //    });

            //    if (isBar)
            //        File.Create(fileName).Using(stream => WriteBar(entries, stream));
            //    else
            //        File.Create(fileName).Using(WriteMsg);
            //}
            //else
            //{
            //    File.Create(fileName).Using(WriteMsg);
            //}
        }
    }
}
