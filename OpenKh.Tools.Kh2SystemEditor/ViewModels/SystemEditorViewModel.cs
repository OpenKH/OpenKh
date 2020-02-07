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
using OpenKh.Tools.Kh2SystemEditor.Services;
using OpenKh.Common.Exceptions;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class SystemEditorViewModel : BaseNotifyPropertyChanged
    {
        private static string ApplicationName = Utilities.GetApplicationName();
        private static readonly List<FileDialogFilter> SystemFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("03system", "bin", "bar").AddAllFiles();
        private static readonly List<FileDialogFilter> IdxFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("KH2.IDX", "idx").AddAllFiles();
        private static readonly List<FileDialogFilter> MsgFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("sys.bar", "bar", "msg", "bin").AddAllFiles();

        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private string _fileName;
        private IEnumerable<Bar.Entry> _barItems;
        private BucketService _bucketService;
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

            ExitCommand = new RelayCommand(x => Window.Close());

            AboutCommand = new RelayCommand(x => new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog());

            LoadSupportIdxCommand = new RelayCommand(_ => FileDialog.OnOpen(fileName => OpenIdx(fileName), IdxFilter, parent: Window));
            LoadSupportMsgCommand = new RelayCommand(_ => FileDialog.OnOpen(fileName => OpenMsg(fileName), MsgFilter, parent: Window));

            _bucketService = new BucketService();
            CreateSystem();
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
            Item = new ItemViewModel(_bucketService);
            Trsr = new TrsrViewModel(Item);
            Ftst = new FtstViewModel();
        }

        private void LoadSystem(IEnumerable<Bar.Entry> entries)
        {
            _barItems = entries;
            Item = new ItemViewModel(_bucketService, _barItems);
            Trsr = new TrsrViewModel(Item, _barItems);
            Ftst = new FtstViewModel(_barItems);
        }

        private void SaveSystem()
        {
            _barItems = SaveSystemEntry(_barItems, Item);
            _barItems = SaveSystemEntry(_barItems, Trsr);
            _barItems = SaveSystemEntry(_barItems, Ftst);
        }

        private void OpenMsg(string fileName) =>
            File.OpenRead(fileName).Using(stream => LoadMessage(stream));

        private void OpenIdx(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            if (!Idx.IsValid(stream))
                throw new InvalidFileException<Idx>();

            var imgFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.img";
            var imgFilePath = Path.Combine(Path.GetDirectoryName(fileName), imgFileName);
            File.OpenRead(imgFilePath).Using(imgStream =>
            {
                var img = new Img(imgStream, Idx.Read(stream), false);
                foreach (var language in Constants.Languages)
                {
                    if (img.FileOpen($"msg/{language}/sys.bar", LoadMessage))
                        break;
                }
            });
        });

        public void LoadMessage(string fileName) =>
            File.OpenRead(fileName).Using(stream => LoadMessage(stream));

        public void LoadMessage(Stream stream)
        {
            if (!_bucketService.LoadMessages(stream))
                return;

            SaveSystem();
            LoadSystem(_barItems);
        }

        private IEnumerable<Bar.Entry> SaveSystemEntry(IEnumerable<Bar.Entry> entries, ISystemGetChanges battleGetChanges) =>
            entries.ForEntry(Bar.EntryType.Binary, battleGetChanges.EntryName, 0, entry => entry.Stream = battleGetChanges.CreateStream());


        private T GetDefaultViewModelInstance<T>()
            where T : ISystemGetChanges => Activator.CreateInstance<T>();
    }
}
