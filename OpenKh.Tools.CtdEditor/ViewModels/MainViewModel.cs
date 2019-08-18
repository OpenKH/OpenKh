using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Tools.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private static string ApplicationName = Utilities.GetApplicationName();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private string _fileName;
        private CtdViewModel _ctdViewModel;

        public string Title => $"{Path.GetFileName(FileName) ?? "untitled"} | {ApplicationName}";

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

        public CtdViewModel CtdViewModel
        {
            get => _ctdViewModel;
            private set { _ctdViewModel = value; OnPropertyChanged(); }
        }

        public Ctd Ctd
        {
            get => CtdViewModel?.Ctd;
            set => CtdViewModel = new CtdViewModel(value);
        }

        public MainViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, new[]
                {
                    ("CTD text file", "ctd"),
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
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, new[]
                {
                    ("CTD text file", "ctd"),
                    ("All files", "*")
                });
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

            CtdViewModel = new CtdViewModel();
        }

        public bool OpenFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            if (!Ctd.IsValid(stream))
            {
                MessageBox.Show(Window, $"{Path.GetFileName(fileName)} is not a valid CTD file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Ctd = Ctd.Read(stream);
            FileName = fileName;
            return true;
        });

        public void SaveFile(string previousFileName, string fileName)
        {
            File.Create(fileName).Using(stream =>
            {
                Ctd.Write(stream);
            });
        }
    }
}
