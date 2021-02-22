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

using ReactiveUI;
using Avalonia.Controls;

namespace OpenKh.Tools.BarTool.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        Bar _currentFile;
        string _fileName;

        public ObservableCollection<EntryModel> Items { get; private set; }
        public EnumModel<Bar.EntryType> Types { get; }

        public string Title { get; private set; }

        public EntryModel CurrentItem { get; set; }

        public IReactiveCommand NewCommand => ReactiveCommand.Create(NewEvent);
        public IReactiveCommand OpenCommand => ReactiveCommand.Create(OpenEvent);
        public IReactiveCommand SaveCommand => ReactiveCommand.Create(SaveEvent);
        public IReactiveCommand SubOpenCommand => ReactiveCommand.Create(SubOpenEvent);
        public IReactiveCommand DeleteCommand => ReactiveCommand.Create(DeleteEvent);
        public IReactiveCommand ImportCommand => ReactiveCommand.Create(ImportEvent);
        public IReactiveCommand ExtractCommand => ReactiveCommand.Create(ExtractEvent);
        public IReactiveCommand AddCommand => ReactiveCommand.Create(AddEvent);
        public IReactiveCommand ExtractAllCommand => ReactiveCommand.Create(ExtractAllEvent);

        public MainWindowViewModel()
        {
            _currentFile = new Bar();
            _fileName = "";

            Items = new ObservableCollection<EntryModel>();
            Types = new EnumModel<Bar.EntryType>();

            Title = "Untitled | BAR - OpenKH";
        }

        private static string GetSuggestedFileName(Bar.Entry item) =>
            $"{item.Name}.{Helpers.GetSuggestedExtension(item.Type)}";

        async Task<bool> SaveCheck()
        {
            switch (MainWindow.Instance.IsSaved)
            {
                case false:
                    var _result = await MessageBox.Show(MainWindow.Instance, "Your latest changes are not saved.\nAre you sure you want to proceed?", "Unsaved Progress", MessageBox.MessageBoxButtons.YesNo);
                    
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
                MainWindow.Instance.IsSaved = true;
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

                var _files = await _dialog.ShowAsync(MainWindow.Instance);

                if (_files.Length == 1)
                {
                    _fileName = Path.GetFileName(_files[0]);

                    using (FileStream _stream = new FileStream(_files[0], FileMode.Open))
                    {
                        _currentFile = Bar.Read(_stream);

                        foreach (var _item in _currentFile)
                        {
                            var _barItem = new EntryModel(_item);
                            Items.Add(_barItem);
                        }

                        Title = string.Format("{0} | BAR - OpenKH", _fileName);
                        this.RaisePropertyChanged(nameof(Title));
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
                InitialFileName = _fileName
            };

            var _file = await _dialog.ShowAsync(MainWindow.Instance);

            if (!string.IsNullOrEmpty(_file))
            {
                using (FileStream _stream = new FileStream(_file, FileMode.OpenOrCreate))
                    Bar.Write(_stream, Items.Select(item => item.Entry));

                MainWindow.Instance.IsSaved = true;
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

            var _files = await _dialog.ShowAsync(MainWindow.Instance);

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

            Items.Add(new EntryModel(_entry));
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

            var _file = await _dialog.ShowAsync(MainWindow.Instance);

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

            var _folder = await _dialog.ShowAsync(MainWindow.Instance);

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
    }
}
