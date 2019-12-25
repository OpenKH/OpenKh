using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.Common;
using System.Collections.Generic;
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
        private static readonly string ApplicationName = Utilities.GetApplicationName();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private string _fileName;
        private static readonly List<FileDialogFilter> Filters = FileDialogFilterComposer.Compose().AddExtensions("00objentry.bin", "bin").AddAllFiles();

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
                FileDialog.OnOpen(fileName =>
                {
                    OpenFile(fileName);
                }, Filters);
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
                FileDialog.OnSave(fileName =>
                {
                    SaveFile(FileName, fileName);
                    FileName = fileName;
                }, Filters);
            }, x => true);

            ExitCommand = new RelayCommand(x =>
            {
                Window.Close();
            }, x => true);

            AboutCommand = new RelayCommand(x =>
            {
                new OpenKh.Tools.Common.Dialogs.AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);
        }

        public bool OpenFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            Objentry = new ObjentryViewModel(Kh2.Objentry.Read(stream));
            OnPropertyChanged("Objentry");
            FileName = fileName;
            return true;
        });

        public void SaveFile(string previousFileName, string fileName)
        {
            var search = Objentry.SearchTerm;
            Objentry.SearchTerm = string.Empty;

            using (var f = File.Create(fileName))
                Kh2.Objentry.Write(f, Objentry.AsObjEntries());

            Objentry.SearchTerm = search;
        }
    }
}
