using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using OpenKh.Kh2;
using OpenKh.Tools.BarTool.Views;
using OpenKh.Tools.BarTool.Models;
using OpenKh.Tools.BarTool.Dialogs;
using OpenKh.Tools.BarTool.Interfaces;

using ReactiveUI;
using Avalonia.Controls;
using System.Diagnostics;

namespace OpenKh.Tools.BarTool.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IViewSettings
    {
        MainWindow Instance;

        string _title;
        string _fileName;
        bool _slotBool;
        bool _msetBool;

        public ObservableCollection<EntryModel> Items { get; private set; }

        public static EnumModel<Bar.MotionsetType> MotionsetTypes { get; } = new EnumModel<Bar.MotionsetType>();
        public static EnumModel<Bar.EntryType> Types { get; } = new EnumModel<Bar.EntryType>();

        public string FileName
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value, nameof(FileName));
        }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value, nameof(Title));
        }

        public EntryModel CurrentItem { get; set; }
        public Bar.MotionsetType MotionsetType { get; set; }

        public IReactiveCommand NewCommand => ReactiveCommand.Create(NewEvent);
        public IReactiveCommand OpenCommand => ReactiveCommand.Create(OpenEvent);
        public IReactiveCommand SaveCommand => ReactiveCommand.Create(SaveEvent);
        public IReactiveCommand SubOpenCommand => ReactiveCommand.Create(SubOpenEvent);
        public IReactiveCommand DeleteCommand => ReactiveCommand.Create(DeleteEvent);
        public IReactiveCommand ImportCommand => ReactiveCommand.Create(ImportEvent);
        public IReactiveCommand ExtractCommand => ReactiveCommand.Create(ExtractEvent);
        public IReactiveCommand AddCommand => ReactiveCommand.Create(AddEvent);
        public IReactiveCommand ExtractAllCommand => ReactiveCommand.Create(ExtractAllEvent);
        public IReactiveCommand AboutCommand => ReactiveCommand.Create(AboutEvent);
        public IReactiveCommand GuideCommand => ReactiveCommand.Create(GuideEvent);

        public bool IsPlayer => MotionsetType == Bar.MotionsetType.Player;
        public int GetSlotIndex(EntryModel item) => Items.IndexOf(item);

        public bool ShowSlotNumber
        {
            get => _slotBool;
            set
            {
                this.RaiseAndSetIfChanged(ref _slotBool, value, nameof(ShowSlotNumber));
                InvalidationEvent();
            }
        }

        public bool ShowMovesetName
        {
            get => _msetBool;
            set
            {
                this.RaiseAndSetIfChanged(ref _msetBool, value, nameof(ShowMovesetName));
                InvalidationEvent();
            }
        }

        public MainWindowViewModel()
        {
            Instance = MainWindow.Instance;
            Items = new ObservableCollection<EntryModel>();

            CurrentItem = new EntryModel(new Bar.Entry(), this);

            _fileName = "Untitled.BAR";
            _title = "Untitled.bar | BAR - OpenKH";

            MotionsetType = Bar.MotionsetType.Default;
            this.RaisePropertyChanged();
        }

        private static string GetSuggestedFileName(Bar.Entry item) =>
            $"{item.Name}.{Helpers.GetSuggestedExtension(item.Type)}";

        public void OpenDrop(string Input)
        {
            Items.Clear();
            FileName = Path.GetFileName(Input);

            using (FileStream _stream = new FileStream(Input, FileMode.Open))
            {
                Instance.CurrentFile = Bar.Read(_stream);
                MotionsetType = Instance.CurrentFile.Motionset;
                this.RaisePropertyChanged(nameof(MotionsetType));

                if (AutoDetectMSET(Instance.CurrentFile))
                    ShowMovesetName = true;

                foreach (var _item in Instance.CurrentFile)
                {
                    var _barItem = new EntryModel(_item, this);
                    Items.Add(_barItem);
                }

                Title = string.Format("{0} | BAR - OpenKH", FileName);
            }
        }

        async Task<bool> SaveCheck()
        {
            switch (Instance.IsSaved)
            {
                case false:
                    var _result = await MessageBox.Show(Instance, "Your latest changes are not saved.\nAre you sure you want to proceed?", "Unsaved Progress", MessageBox.MessageBoxButtons.YesNo);
                    
                    switch (_result)
                    {
                        default:
                            return false;
                        case MessageBox.MessageBoxResult.Yes:
                            return true;
                    }
                case true:
                    return true;
            }
        }

        async void NewEvent()
        {
            if (await SaveCheck())
            {
                Items.Clear();
                Instance.IsSaved = true;

                FileName = "Untitled.bar";
                Title = string.Format("{0} | BAR - OpenKH", FileName);
            }
        }

        async void OpenEvent()
        {
            if (await SaveCheck())
            {
                var _dialog = new OpenFileDialog()
                {
                    Title = "Open an Archive...",
                    Filters = new List<FileDialogFilter>
                {
                     new FileDialogFilter() { Name = "Binary Archive", Extensions = new List<string>() { "bar" } },
                     new FileDialogFilter() { Name = "All Files", Extensions = new List<string>() { "*" } },
                }
                };

                var _files = await _dialog.ShowAsync(Instance);

                if (_files != null && _files.Length == 1)
                {
                    Items.Clear();
                    FileName = Path.GetFileName(_files[0]);

                    using (FileStream _stream = new FileStream(_files[0], FileMode.Open))
                    {
                        Instance.CurrentFile = Bar.Read(_stream);
                        MotionsetType = Instance.CurrentFile.Motionset;
                        this.RaisePropertyChanged(nameof(MotionsetType));

                        if (AutoDetectMSET(Instance.CurrentFile))
                            ShowMovesetName = true;

                        foreach (var _item in Instance.CurrentFile)
                        {
                            var _barItem = new EntryModel(_item, this);
                            Items.Add(_barItem);
                        }

                        Title = string.Format("{0} | BAR - OpenKH", FileName);
                    }
                }
            }
        }

        async void SaveEvent()
        {
            var _dialog = new SaveFileDialog()
            {
                Title = "Save this Archive...",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter() { Name = "Binary Archive", Extensions = new List<string>() { "bar" } },
                    new FileDialogFilter() { Name = "All Files", Extensions = new List<string>() { "*" } },
                },
                InitialFileName = FileName
            };

            var _file = await _dialog.ShowAsync(Instance);

            if (!string.IsNullOrEmpty(_file))
            {
                using (FileStream _stream = new FileStream(_file, FileMode.OpenOrCreate))
                    Bar.Write(_stream, Items.Select(item => item.Entry));

                Instance.IsSaved = true;
            }
        }

        void SubOpenEvent() { throw new NotImplementedException(); }

        void DeleteEvent()
        {
            Items.Remove(CurrentItem);
            this.RaisePropertyChanged(nameof(Items));
        }

        async void ImportEvent()
        {
            var _dialog = new OpenFileDialog()
            {
                Title = "Open a file to import...",
                Filters = new List<FileDialogFilter>
                {
                     new FileDialogFilter() { Name = "All Files", Extensions = new List<string>() { "*" } },
                }
            };

            var _files = await _dialog.ShowAsync(Instance);

            if (_files.Length == 1)
            {
                using (FileStream _stream = new FileStream(_files[0], FileMode.Open))
                {
                    byte[] _bArray = new byte[_stream.Length];
                    _stream.Read(_bArray, 0, (int)_stream.Length);

                    CurrentItem.Entry.Stream = new MemoryStream(_bArray);
                    CurrentItem.Entry.Stream.Position = 0;
                }

                this.RaisePropertyChanged(nameof(CurrentItem));
            }
        }

        void AddEvent()
        {
            var _entry = new Bar.Entry();
            _entry.Name = "newf";

            Items.Add(new EntryModel(_entry, this));
            this.RaisePropertyChanged(nameof(Items));
        }

        async void ExtractEvent()
        {
            var _defName = GetSuggestedFileName(CurrentItem.Entry);

            var _dialog = new SaveFileDialog()
            {
                Title = "Save this file...",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter() { Name = "All Files", Extensions = new List<string>() { "*" } },
                },
                InitialFileName = _defName
            };

            var _file = await _dialog.ShowAsync(Instance);

            if (!string.IsNullOrEmpty(_file))
            {
                using (FileStream _stream = new FileStream(_file, FileMode.OpenOrCreate))
                    CurrentItem.Entry.Stream.CopyTo(_stream);
            }
        }

        async void ExtractAllEvent()
        {
            var _dialog = new OpenFolderDialog()
            {
                Title = "Choose a path for export...",
            };

            var _folder = await _dialog.ShowAsync(Instance);

            if (!string.IsNullOrEmpty(_folder))
            {
                foreach (var _item in Items)
                {
                    var _defName = GetSuggestedFileName(_item.Entry);

                    using (FileStream _stream = new FileStream(Path.Combine(_folder, _defName), FileMode.OpenOrCreate))
                        _item.Entry.Stream.CopyTo(_stream);
                }
            }
        }

        async void AboutEvent() => await MessageBox.Show(Instance, "BAR Editor - OpenKH\n\nVersion X.XX\nCompiled on XX.XX.XXXX", "About this tool", MessageBox.MessageBoxButtons.None);

        void GuideEvent()
        {
            var _process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = "https://openkh.dev/kh2/file/type/bar.html"
                }
            };

            _process.Start();
        }

        void InvalidationEvent()
        {
            foreach (var _item in Items)
                _item.Invalidate();
        }

        private static bool AutoDetectMSET(Bar Input)
        {
            if (Input.Motionset != Bar.MotionsetType.Default)
                return true;

            return Input.Count(x => x.Type == Bar.EntryType.AnimationBinary) >= 4;
        }
    }
}
