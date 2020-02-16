using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Tools.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.BbsEventTableEditor.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private static string ApplicationName = Utilities.GetApplicationName();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private string _fileName;
        private EventsViewModel _eventsViewModel;

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
        public RelayCommand ExportEventsListCommand { get; }
        public RelayCommand ExportUsedEventsCommand { get; }
        public RelayCommand ExportUsedMapsCommand { get; }
        public RelayCommand AboutCommand { get; }

        public EventsViewModel EventsViewModel
        {
            get => _eventsViewModel;
            private set { _eventsViewModel = value; OnPropertyChanged(); }
        }

        public IEnumerable<Event> Events
        {
            get => EventsViewModel?.Items?.Select(x => x.Event) ?? new Event[0];
            set => EventsViewModel = new EventsViewModel(value);
        }

        public MainViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, new[]
                {
                    ("Event table (EVENT_TE, EVENT_VE, EVENT_AQ)", "*"),
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

            ExportEventsListCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save);
                fd.DefaultFileName = CreateExportFilePath("events_list.txt");
                if (fd.ShowDialog() == true)
                {
                    File.CreateText(fd.FileName).Using(stream =>
                    {
                        foreach (var item in Events)
                        {
                            stream.WriteLine($"ID {item.Id:X03} MAP {Constants.Worlds[item.World]}_{item.Room:D02} EVENT {item.EventIndex:D03}");
                        }
                    });
                }
            }, x => true);

            ExportUsedEventsCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save);
                fd.DefaultFileName = CreateExportFilePath("events_used.txt");
                if (fd.ShowDialog() == true)
                {
                    File.CreateText(fd.FileName).Using(stream =>
                    {
                        foreach (var item in Events)
                        {
                            stream.WriteLine($"event/{Constants.Worlds[item.World]}/{Constants.Worlds[item.World]}_{item.EventIndex:D03}.exa");
                        }
                    });
                }
            }, x => true);

            ExportUsedMapsCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save);
                fd.DefaultFileName = CreateExportFilePath("maps_used.txt");
                if (fd.ShowDialog() == true)
                {
                    File.CreateText(fd.FileName).Using(stream =>
                    {
                        foreach (var item in Events)
                        {
                            stream.WriteLine($"arc/map/{Constants.Worlds[item.World]}{item.Room:D02}.arc");
                        }
                    });
                }
            }, x => true);

            AboutCommand = new RelayCommand(x =>
            {
                new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);

            EventsViewModel = new EventsViewModel();
        }

        public bool OpenFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            if (!Event.IsValid(stream))
            {
                MessageBox.Show(Window, $"{Path.GetFileName(fileName)} is not a valid event file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Events = Event.Read(stream);
            FileName = fileName;
            return true;
        });

        public void SaveFile(string previousFileName, string fileName)
        {
            File.Create(fileName).Using(stream =>
            {
                Event.Write(stream, Events);
            });
        }

        private string CreateExportFilePath(string newFileName)
        {
            var dirName = Path.GetDirectoryName(FileName);
            var fileName = Path.GetFileNameWithoutExtension(FileName);

            return Path.Combine(dirName, $"{fileName}_{newFileName}");
        }
    }
}
