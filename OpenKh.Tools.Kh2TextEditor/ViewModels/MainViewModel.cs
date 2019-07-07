using kh.tools.common;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.Kh2TextEditor.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private const string DefaultName = "FAKE";
        private static string ApplicationName = Utilities.GetApplicationName();
        private string _fileName;
        private string _barEntryName;

        public string Title => $"{_barEntryName ?? DefaultName} | {FileName ?? "untitled"} | {ApplicationName}";

        private string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        public RelayCommand OpenCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand SaveAsCommand { get; private set; }
        public RelayCommand ExitCommand { get; private set; }
        public RelayCommand AboutCommand { get; private set; }

        public TextEditorViewModel TextEditor { get; private set; }

        public MainViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, new (string, string)[]
                {
                    ("BAR file", "bar"),
                    ("MSG file", "msg"),
                    ("All files", "")
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
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, FileDialog.Type.Any);
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

            TextEditor = new TextEditorViewModel();
        }

        public bool OpenFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            _barEntryName = null;
            if (!TryReadMsg(stream) && !TryReadMsgAsBar(stream))
            {
                MessageBox.Show(Window, "Invalid or not existing Message data found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (_barEntryName == null)
            {
                _barEntryName = Path.GetFileNameWithoutExtension(fileName);
                if (_barEntryName.Length > 4)
                    _barEntryName = _barEntryName.Substring(0, 4);

                _barEntryName = _barEntryName.ToLower();
            }

            FileName = fileName;
            return true;
        });

        public void SaveFile(string previousFileName, string fileName)
        {
            if (File.Exists(previousFileName))
            {
                bool isBar = false;
                List<Bar.Entry> entries;

                entries = File.OpenRead(previousFileName).Using(stream =>
                {
                    isBar = Bar.IsValid(stream);
                    return isBar ? Bar.Read(stream) : null;
                });

                if (isBar)
                    File.Create(fileName).Using(stream => WriteBar(entries, stream));
                else
                    File.Create(fileName).Using(WriteMsg);
            }
            else
            {
                File.Create(fileName).Using(WriteMsg);
            }
        }

        private bool TryReadMsg(Stream stream)
        {
            if (!Msg.IsValid(stream))
                return false;

            TextEditor.MessageEntries = Msg.Read(stream);
            return true;
        }

        private bool TryReadMsgAsBar(Stream stream)
        {
            if (!Bar.IsValid(stream))
                return false;

            var msgEntry = Bar.Read(stream)
                .FirstOrDefault(x => x.Type == Bar.EntryType.Binary);

            if (msgEntry == null)
                return false;

            _barEntryName = msgEntry.Name;
            return TryReadMsg(msgEntry.Stream);
        }

        private void WriteMsg(Stream stream) =>
            Msg.Write(stream, TextEditor.MessageEntries);

        private void WriteBar(List<Bar.Entry> entries, Stream stream)
        {
            var newEntries = entries
                .ForEntry(Bar.EntryType.Binary, _barEntryName, 0, entry => WriteMsg(entry.Stream));

            Bar.Write(stream, newEntries);
        }
    }
}
