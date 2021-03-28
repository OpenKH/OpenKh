using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Tools.Common;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;
using OpenKh.Engine;
using OpenKh.Tools.Kh2SystemEditor.Utils;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Kh2SystemEditor.Models.Export;
using OpenKh.Tools.Kh2SystemEditor.Models;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class SystemEditorViewModel : BaseNotifyPropertyChanged, IObjectProvider
    {
        public enum EncodingType
        {
            European,
            Japanese
        }

        private static string ApplicationName = Utilities.GetApplicationName();
        private static readonly List<FileDialogFilter> SystemFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("03system", "bin", "bar").AddAllFiles();
        private static readonly List<FileDialogFilter> IdxFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("KH2.IDX", "idx").AddAllFiles();
        private static readonly List<FileDialogFilter> MsgFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("sys.bar", "bar", "msg", "bin").AddAllFiles();
        private static readonly List<FileDialogFilter> TableExportFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("yml", "yml")
            .AddExtensions("xlsx", "xlsx")
            .AddAllFiles();


        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private string _fileName;
        private EncodingType _encoding;
        private IEnumerable<Bar.Entry> _barItems;
        private Kh2MessageProvider _messageProvider;
        private ItemViewModel _item;
        private TrsrViewModel _trsr;
        private MemtViewModel _memt;
        private FtstViewModel _ftst;

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

        public RelayCommand OpenCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand ExportItemListCommand { get; }
        public RelayCommand ExportTrsrListCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand AboutCommand { get; }
        public RelayCommand LoadSupportIdxCommand { get; }
        public RelayCommand LoadSupportMsgCommand { get; }

        public ItemViewModel Item
        {
            get => _item;
            private set { _item = value; OnPropertyChanged(); }
        }

        public TrsrViewModel Trsr
        {
            get => _trsr;
            private set { _trsr = value; OnPropertyChanged(); }
        }

        public MemtViewModel Memt
        {
            get => _memt;
            private set { _memt = value; OnPropertyChanged(); }
        }

        public FtstViewModel Ftst
        {
            get => _ftst;
            private set { _ftst = value; OnPropertyChanged(); }
        }

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

                Item.RefreshAllMessages();
                Trsr.RefreshAllMessages();
            }
        }

        public IList<ObjectModel> Objects { get; } = new List<ObjectModel>();
        private Dictionary<short, string> _objectsDictionary;

        public SystemEditorViewModel()
        {
            OpenCommand = new RelayCommand(_ => FileDialog.OnOpen(fileName => OpenFile(fileName), SystemFilter, parent: Window));

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

            SaveAsCommand = new RelayCommand(_ => FileDialog.OnSave(fileName =>
            {
                SaveFile(FileName, fileName);
                FileName = fileName;
            }, SystemFilter, defaultFileName: FileName, parent: Window));

            ExportItemListCommand = new RelayCommand(
                _ => FileDialog.OnSave(
                    fileName =>
                    {
                        ExportTable(
                            fileName,
                            Item.Select(viewModel => new ItemExport(_messageProvider, viewModel.Item))
                        );
                    },
                    TableExportFilter,
                    defaultFileName: "ItemList.yml",
                    parent: Window
                )
            );

            ExportTrsrListCommand = new RelayCommand(
                _ => FileDialog.OnSave(
                    fileName =>
                    {
                        ExportTable(
                            fileName,
                            Trsr.Select(viewModel => new TrsrExport(Item, viewModel.Treasure))
                        );
                    },
                    TableExportFilter,
                    defaultFileName: "TrsrList.yml",
                    parent: Window
                )
            );

            ExitCommand = new RelayCommand(x => Window.Close());

            AboutCommand = new RelayCommand(x => new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog());

            LoadSupportIdxCommand = new RelayCommand(_ => Utilities.Catch(() =>
            {
                Kh2Utilities.OpenMsgFromIdxDialog(s =>
                {
                    LoadMessages(s);
                    InvalidateViews();
                });
            }));
            LoadSupportMsgCommand = new RelayCommand(_ => Utilities.Catch(() =>
            {
                Kh2Utilities.OpenMsgFromBarDialog(s =>
                {
                    LoadMessages(s);
                    InvalidateViews();
                });
            }));

            _messageProvider = new Kh2MessageProvider();
            CreateSystem();
        }

        private void ExportTable<T>(string fileName, IEnumerable<T> list)
        {
            try
            {
                DictListWriteUtil.Write(
                    fileName,
                    DictionalizeUtil.ToDictList(list)
                );
            }
            catch (NotSupportedException)
            {
                MessageBox.Show("Export in this file format is not supported yet.");
            }
        }

        public void LoadSupportFiles(string basePath)
        {
            // Try to load MSG, giving priority to the English language
            foreach (var region in Constants.Regions.OrderBy(x => x == "us" ? 0 : 1))
            {
                var tryPath = Path.Combine(basePath, $"msg/{region}/sys.bar");
                if (File.Exists(tryPath))
                {
                    LoadMessages(File.OpenRead(tryPath).Using(Kh2Utilities.ReadMsgFromBar));
                    break;
                }
            }

            var objEntryPath = Path.Combine(basePath, "00objentry.bin");
            if (File.Exists(objEntryPath))
                File.OpenRead(objEntryPath).Using(LoadObjEntry);
        }

        private void LoadMessages(List<Msg.Entry> msgs)
        {
            _messageProvider.Load(msgs);
            switch (IsMsgJapanese(_messageProvider))
            {
                case true:
                    Encoding = EncodingType.Japanese;
                    break;
                case false:
                    Encoding = EncodingType.European;
                    break;
            }
        }

        private void LoadObjEntry(Stream stream)
        {
            var objEntry = Objentry.Read(stream);
            Objects.Clear();
            Objects.Add(new ObjectModel(-1, "Disabled"));
            Objects.Add(new ObjectModel(0, "Ignore"));
            foreach (var obj in objEntry.OrderBy(x =>
            {
                // Order the OBJ Entry list to improve performance
                if (x.ModelName.StartsWith("P_"))
                    return 0;
                if (x.ModelName == "WM_CURSOR")
                    return 1;
                if (x.ModelName.StartsWith("N_"))
                    return 1;
                return 3;
            }))
                Objects.Add(new ObjectModel((int)obj.ObjectId, obj.ModelName));
            _objectsDictionary = Objects.ToDictionary(x => x.Value, x => x.Name);
        }

        private void InvalidateViews()
        {
            SaveSystem();
            LoadSystem(_barItems);
        }

        public bool OpenFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            if (!Bar.IsValid(stream))
            {
                MessageBox.Show(Window, $"{Path.GetFileName(fileName)} is not a valid BAR file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var items = Bar.Read(stream);

            if (!Is03System(items))
            {
                MessageBox.Show(Window, $"{Path.GetFileName(fileName)} does not appear to be a valid 03system.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            LoadSupportFiles(Path.GetDirectoryName(fileName));
            LoadSystem(items);

            FileName = fileName;
            return true;
        });

        public void SaveFile(string previousFileName, string fileName)
        {
            File.Create(fileName).Using(stream =>
            {
                SaveSystem();
                Bar.Write(stream, _barItems);
            });
        }

        private bool Is03System(List<Bar.Entry> items) => items.Any(x => new[]
        {
            "item",
            "trsr",
            "ftst",
            "memt",
        }.Contains(x.Name));

        private void CreateSystem()
        {
            _barItems = new Bar.Entry[0];
            Item = new ItemViewModel(_messageProvider);
            Trsr = new TrsrViewModel(Item);
            Memt = new MemtViewModel(this);
            Ftst = new FtstViewModel();
        }

        private void LoadSystem(IEnumerable<Bar.Entry> entries)
        {
            _barItems = entries;
            Item = new ItemViewModel(_messageProvider, _barItems);
            Trsr = new TrsrViewModel(Item, _barItems);
            Memt = new MemtViewModel(this, _barItems);
            Ftst = new FtstViewModel(_barItems);
        }

        private void SaveSystem()
        {
            _barItems = SaveSystemEntry(_barItems, Item);
            _barItems = SaveSystemEntry(_barItems, Trsr);
            _barItems = SaveSystemEntry(_barItems, Memt);
            _barItems = SaveSystemEntry(_barItems, Ftst);
        }

        private IEnumerable<Bar.Entry> SaveSystemEntry(IEnumerable<Bar.Entry> entries, ISystemGetChanges battleGetChanges) =>
            entries.ForEntry(Bar.EntryType.List, battleGetChanges.EntryName, 0, entry => entry.Stream = battleGetChanges.CreateStream());

        private T GetDefaultViewModelInstance<T>()
            where T : ISystemGetChanges => Activator.CreateInstance<T>();

        private static bool? IsMsgJapanese(Kh2MessageProvider messageProvider)
        {
            const ushort FakeTextId = 0x0ADC;
            if (messageProvider == null)
                return null;

            var data = messageProvider.GetMessage(FakeTextId);
            if (data == null || data.Length == 0)
                return null;

            return data.Length != 5;
        }

        public string GetObjectName(int id)
        {
            if (_objectsDictionary == null)
                return null;
            if (!_objectsDictionary.TryGetValue((short)id, out var value))
                return null;
            return value;
        }
    }
}
