using OpenKh.Tools.Common;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Contextes;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Common.Extensions;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2TextEditor.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;
using System.Diagnostics;

namespace OpenKh.Tools.Kh2TextEditor.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private const string DefaultName = "FAKE";
        private const string GuideUrl = "https://openkh.dev/kh2/tool/Kh2TextEditor/OpenKh.Tools.Kh2TextEditor";
        private static string ApplicationName = Utilities.GetApplicationName();
        private string _fileName;
        private string _barEntryName;
        private FontContext _fontContext = new FontContext();
        private FontType _fontType;

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
        public RelayCommand OpenCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand GuideCommand { get; }
        public RelayCommand AboutCommand { get; }

        public RelayCommand OpenFontImageCommand { get; }
        public RelayCommand SaveFontImageCommand { get; }
        public RelayCommand EditFontImageCommand { get; }
        public RelayCommand OpenFontInfoCommand { get; }
        public RelayCommand SaveFontInfoCommand { get; }
        public RelayCommand EditFontInfoCommand { get; }

        public TextEditorViewModel TextEditor { get; private set; }

        public FontType FontType
        {
            get => _fontType;
            set
            {
                _fontType = value;
                InvalidateFontContext();
            }
        }

        public MainViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, new (string, string)[]
                {
                    ("BAR file", "bar"),
                    ("MSG file", "msg"),
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

            OpenFontImageCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, new (string, string)[]
                {
                    ("fontimage.bar", "bar"),
                    ("All files", "*")
                });

                if (fd.ShowDialog() == true)
                {
                    OpenFontImageFile(fd.FileName);
                }
            }, x => true);

            SaveFontImageCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, new (string, string)[]
                {
                    ("fontimage.bar", "bar"),
                    ("All files", "*")
                });

                if (fd.ShowDialog() == true)
                {
                    SaveFontImageFile(fd.FileName);
                }
            }, x => true);

            OpenFontInfoCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, new (string, string)[]
                {
                    ("fontinfo.bar", "bar"),
                    ("All files", "*")
                });

                if (fd.ShowDialog() == true)
                {
                    OpenFontInfoFile(fd.FileName);
                }
            }, x => true);

            SaveFontInfoCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, new (string, string)[]
                {
                    ("fontinfo.bar", "bar"),
                    ("All files", "*")
                });

                if (fd.ShowDialog() == true)
                {
                    SaveFontInfoFile(fd.FileName);
                }
            }, x => true);

            GuideCommand = new RelayCommand(x =>
            {
                Process.Start(new ProcessStartInfo(GuideUrl));
            }, x => true);

            AboutCommand = new RelayCommand(x =>
            {   
                new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);

            TextEditor = new TextEditorViewModel();
            FontType = FontType.System;
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

        private void OpenFontImageFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            if (Bar.IsValid(stream))
            {
                _fontContext.Read(Bar.Read(stream));
                InvalidateFontContext();
            }
        });

        private void SaveFontImageFile(string fileName)
        {
            throw new NotImplementedException();
        }

        private void OpenFontInfoFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            if (Bar.IsValid(stream))
            {
                _fontContext.Read(Bar.Read(stream));
                InvalidateFontContext();
            }
        });

        private void SaveFontInfoFile(string fileName)
        {
            throw new NotImplementedException();
        }

        private void InvalidateFontContext()
        {
            TextEditor.TextContext = FontType == FontType.System ?
                _fontContext.ToKh2EuSystemTextContext() :
                _fontContext.ToKh2EuEventTextContext();
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
