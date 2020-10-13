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
using OpenKh.Kh2.System;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Kh2SystemEditor.Models.Export;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class SystemEditorViewModel : BaseNotifyPropertyChanged
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
            .AddExtensions("csv", "csv")
            .AddAllFiles();


        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private string _fileName;
        private EncodingType _encoding;
        private IEnumerable<Bar.Entry> _barItems;
        private Kh2MessageProvider _messageProvider;
        private ItemViewModel _item;
        private TrsrViewModel _trsr;
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
                Kh2Utilities.OpenMsgFromIdxDialog(LoadMessages);
            }));
            LoadSupportMsgCommand = new RelayCommand(_ => Utilities.Catch(() =>
            {
                Kh2Utilities.OpenMsgFromBarDialog(LoadMessages);
            }));

            _messageProvider = new Kh2MessageProvider();
            CreateSystem();
        }

        private void ExportTable<T>(string fileName, IEnumerable<T> list)
        {
            DictListWriteUtil.Write(
                fileName,
                DictionalizeUtil.ToDictList(list)
            );
        }

        private void LoadMessages(List<Msg.Entry> msgs)
        {
            _messageProvider.Load(msgs);
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
        }.Contains(x.Name));

        private void CreateSystem()
        {
            _barItems = new Bar.Entry[0];
            Item = new ItemViewModel(_messageProvider);
            Trsr = new TrsrViewModel(Item);
            Ftst = new FtstViewModel();
        }

        private void LoadSystem(IEnumerable<Bar.Entry> entries)
        {
            _barItems = entries;
            Item = new ItemViewModel(_messageProvider, _barItems);
            Trsr = new TrsrViewModel(Item, _barItems);
            Ftst = new FtstViewModel(_barItems);
        }

        private void SaveSystem()
        {
            _barItems = SaveSystemEntry(_barItems, Item);
            _barItems = SaveSystemEntry(_barItems, Trsr);
            _barItems = SaveSystemEntry(_barItems, Ftst);
        }

        private IEnumerable<Bar.Entry> SaveSystemEntry(IEnumerable<Bar.Entry> entries, ISystemGetChanges battleGetChanges) =>
            entries.ForEntry(Bar.EntryType.List, battleGetChanges.EntryName, 0, entry => entry.Stream = battleGetChanges.CreateStream());


        private T GetDefaultViewModelInstance<T>()
            where T : ISystemGetChanges => Activator.CreateInstance<T>();
    }
}
