using OpenKh.Tools.Common;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Contextes;
using OpenKh.Kh2.Extensions;
using OpenKh.Tools.Kh2TextEditor.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;
using OpenKh.Tools.Kh2TextEditor.Services;
using System.Text;
using OpenKh.Engine.Renders;
using OpenKh.Engine.Extensions;

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
        private EncodingType _encodingType;

        private static readonly List<FileDialogFilter> MessageFilters = FileDialogFilterComposer
            .Compose()
            .AddExtensions("Message files", "bar", "msg", "bin")
            .AddAllFiles();

        private static readonly List<FileDialogFilter> FontImageFilters = FileDialogFilterComposer
            .Compose()
            .AddExtensions("fontimage.bar", "bar")
            .AddAllFiles();

        private static readonly List<FileDialogFilter> FontInfoFilters = FileDialogFilterComposer
            .Compose()
            .AddExtensions("fontinfo.bar", "bar")
            .AddAllFiles();

        private static readonly List<FileDialogFilter> ExportFilters = FileDialogFilterComposer
            .Compose()
            .Concat(TextExporters.GetAll().Select(x => FileDialogFilter.ByExtensions(x.Filter().Item1, x.Filter().Item2)))
            .ToList();

        private static readonly List<FileDialogFilter> ImportFilters = FileDialogFilterComposer
            .Compose()
            .Concat(TextImporters.GetAll().Select(x => FileDialogFilter.ByExtensions(x.Filter().Item1, x.Filter().Item2)))
            .ToList();

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
        public RelayCommand ExportMessageAsCommand { get; }
        public RelayCommand ImportMessageFromCommand { get; }

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

        public bool OptimizeOnSave { get; set; }

        public FontType FontType
        {
            get => _fontType;
            set
            {
                _fontType = value;
                InvalidateFontContext();
            }
        }

        public EncodingType EncodingType
        {
            get => _encodingType;
            set
            {
                _encodingType = value;
                InvalidateFontContext();
            }
        }

        public MainViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    OpenFile(fileName);
                }, MessageFilters);
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
                }, MessageFilters);
            }, x => true);

            ExportMessageAsCommand = new RelayCommand(x =>
            {
                FileDialog.OnSave(fileName =>
                {
                    var selectedExtension = $"{Path.GetExtension(fileName).TrimStart('.')}";

                    ExportMessageAsFile(
                        fileName: fileName,
                        textExporter: TextExporters.FindFromFile(fileName)
                    );
                }, ExportFilters);
            }, x => true);

            ImportMessageFromCommand = new RelayCommand(x =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    var selectedExtension = $"{Path.GetExtension(fileName).TrimStart('.')}";

                    var textImporter = TextImporters.FindFromFile(fileName);
                    if (textImporter != null)
                    {
                        ImportMessageFromFile(
                            fileName: fileName,
                            textImporter
                        );
                    }
                    else
                    {
                        MessageBox.Show($"Failed to match text decoder for your file:\n{fileName}");
                    }
                }, ImportFilters);
            }, x => true);

            ExitCommand = new RelayCommand(x =>
            {
                Window.Close();
            }, x => true);

            OpenFontImageCommand = new RelayCommand(x =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    OpenFontImageFile(fileName);
                }, FontImageFilters);
            }, x => true);

            SaveFontImageCommand = new RelayCommand(x =>
            {
                FileDialog.OnSave(fileName =>
                {
                    SaveFontImageFile(fileName);
                }, FontImageFilters);
            }, x => true);

            OpenFontInfoCommand = new RelayCommand(x =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    OpenFontInfoFile(fileName);
                }, FontInfoFilters);
            }, x => true);

            SaveFontInfoCommand = new RelayCommand(x =>
            {
                FileDialog.OnSave(fileName =>
                {
                    SaveFontInfoFile(fileName);
                }, FontInfoFilters);
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

        public void ExportMessageAsFile(string fileName, ITextExporter textExporter)
        {
            new StreamWriter(fileName, false, Encoding.UTF8).Using(
                writer => textExporter.Export(
                    TextEditor.Messages
                        .Select(
                            source => new ExchangeableMessage
                            {
                                Id = source.Id,
                                Text = source.Text,
                            }
                        ),
                    writer
                )
            );
        }

        public void ImportMessageFromFile(string fileName, ITextImporter textImporter)
        {
            var importedMessages = new StreamReader(fileName, Encoding.UTF8).Using(
                reader => textImporter.Import(reader)
                    .ToArray() // make sure to import all messages from file before closing StreamReader!
            );

            foreach (var importMessage in importedMessages)
            {
                var found = TextEditor.Messages.SingleOrDefault(it => it.Id == importMessage.Id);
                if (found != null)
                {
                    found.Text = importMessage.Text;
                }
            }
        }

        public void OpenFontImageFile(string fileName) => File.OpenRead(fileName).Using(stream =>
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

        public void OpenFontInfoFile(string fileName) => File.OpenRead(fileName).Using(stream =>
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
            RenderingMessageContext context;

            switch (EncodingType)
            {
                case EncodingType.European:
                    switch (FontType)
                    {
                        case FontType.System:
                            context = _fontContext.ToKh2EuSystemTextContext();
                            break;
                        case FontType.Event:
                            context = _fontContext.ToKh2EuEventTextContext();
                            break;
                        default:
                            context = null;
                            break;
                    }
                    break;
                case EncodingType.Japanese:
                    switch (FontType)
                    {
                        case FontType.System:
                            context = _fontContext.ToKh2JpSystemTextContext();
                            break;
                        case FontType.Event:
                            context = _fontContext.ToKh2JpEventTextContext();
                            break;
                        default:
                            context = null;
                            break;
                    }
                    break;
                case EncodingType.Turkish:
                    switch (FontType)
                    {
                        case FontType.System:
                            context = _fontContext.ToKh2TRSystemTextContext();
                            break;
                        case FontType.Event:
                            context = _fontContext.ToKh2TREventTextContext();
                            break;
                        default:
                            context = null;
                            break;
                    }
                    break;
                default:
                    context = null;
                    break;
            }

            TextEditor.TextContext = context;
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
                .FirstOrDefault(x => x.Type == Bar.EntryType.List);

            if (msgEntry == null)
                return false;

            _barEntryName = msgEntry.Name;
            return TryReadMsg(msgEntry.Stream);
        }

        private void WriteMsg(Stream stream)
        {
            if (OptimizeOnSave)
                Msg.WriteOptimized(stream, TextEditor.MessageEntries);
            else
                Msg.Write(stream, TextEditor.MessageEntries);
            stream.SetLength(stream.Position);
        }

        private void WriteBar(List<Bar.Entry> entries, Stream stream)
        {
            var newEntries = entries
                .ForEntry(Bar.EntryType.List, _barEntryName, 0, entry => WriteMsg(entry.Stream));

            Bar.Write(stream, newEntries);
        }
    }
}
