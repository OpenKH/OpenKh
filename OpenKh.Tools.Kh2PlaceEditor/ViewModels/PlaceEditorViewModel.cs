using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.Kh2PlaceEditor.ViewModels
{
    public class PlaceEditorViewModel : BaseNotifyPropertyChanged
    {
        public enum EncodingType
        {
            European,
            Japanese
        }

        private static readonly List<FileDialogFilter> Filters = FileDialogFilterComposer.Compose().AddExtensions("00place.bin or place.bin", "bin").AddAllFiles();
        private static readonly List<FileDialogFilter> CsvFilter = FileDialogFilterComposer.Compose().AddExtensions("Map names as CSV", "csv");
        private static readonly string ApplicationName = Utilities.GetApplicationName();

        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private readonly Kh2MessageProvider _messageProvider;
        private string _fileName;
        private EncodingType _encoding;

        public string Title => $"{FileName ?? "untitled"} | {ApplicationName}";

        public PlacesViewModel Places { get; private set; }

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
        public RelayCommand ExportAsCsvCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand AboutCommand { get; }
        public RelayCommand LoadSupportIdxCommand { get; }
        public RelayCommand LoadSupportMsgCommand { get; }

        public EncodingType Encoding
        {
            get => _encoding;
            set
            {
                switch (_encoding = value)
                {
                    case EncodingType.European:
                        _messageProvider.Encoder = Encoders.InternationalSystem;
                        break;
                    case EncodingType.Japanese:
                        _messageProvider.Encoder = Encoders.JapaneseSystem;
                        break;
                }

                Places.RefreshAllMessages();
            }
        }

        public PlaceEditorViewModel()
        {
            OpenCommand = new RelayCommand(x => Utilities.Catch(() =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    OpenFile(fileName);
                }, Filters);
            }), x => true);

            SaveCommand = new RelayCommand(x => Utilities.Catch(() =>
            {
                if (!string.IsNullOrEmpty(FileName))
                    SaveFile(FileName, FileName);
                else
                    SaveAsCommand.Execute(x);
            }), x => true);

            SaveAsCommand = new RelayCommand(x => Utilities.Catch(() =>
            {
                FileDialog.OnSave(fileName =>
                {
                    SaveFile(FileName, fileName);
                    FileName = fileName;
                }, Filters);
            }), x => true);

            ExitCommand = new RelayCommand(x =>
            {
                Window.Close();
            }, x => true);

            AboutCommand = new RelayCommand(x =>
            {
                new OpenKh.Tools.Common.Dialogs.AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);

            ExportAsCsvCommand = new RelayCommand(_ =>
            {
                FileDialog.OnSave(fileName => new StreamWriter(fileName, false, new UTF8Encoding(true)).Using(writer =>
                {
                    foreach (var place in Places?.Items?.Cast<PlaceViewModel>() ?? new PlaceViewModel[0])
                    {
                        var row = new string[]
                        {
                            place.Map,
                            place.MessageId.ToString(),
                            place.Name,
                            place.Message
                        }
                        .Select(x => $"\"{x}\"");

                        writer.WriteLine(string.Join(",", row));
                    }
                })
                , CsvFilter);
            });

            LoadSupportIdxCommand = new RelayCommand(_ => Utilities.Catch(() =>
            {
                Kh2Utilities.OpenMsgFromIdxDialog(LoadMessages);
            }));
            LoadSupportMsgCommand = new RelayCommand(_ => Utilities.Catch(() =>
            {
                Kh2Utilities.OpenMsgFromBarDialog(LoadMessages);
            }));

            _messageProvider = new Kh2MessageProvider();
        }

        public bool OpenFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            Places = new PlacesViewModel(_messageProvider, Place.Read(stream));
            FileName = fileName;

            OnPropertyChanged(nameof(Places));
            return true;
        });

        public void SaveFile(string previousFileName, string fileName)
        {
            using (var f = File.Create(fileName))
                Kh2.Place.Write(f, Places.Places);
        }

        private void LoadMessages(List<Msg.Entry> msgs)
        {
            _messageProvider.Load(msgs);
            Places.RefreshAllMessages();
        }
    }
}
