using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using OpenKh.Kh2;
using OpenKh.Tools.BarTool.Views;
using OpenKh.Tools.BarTool.Models;

using Avalonia.Controls;
using ReactiveUI;

namespace OpenKh.Tools.BarTool.ViewModels
{
    public class MainWindowViewModel
    {
        Bar _currentFile;

        public ObservableCollection<EntryModel> Items { get; private set; }
        public EnumModel<Bar.EntryType> Types { get; }

        public IReactiveCommand NewCommand { get; }
        public IReactiveCommand OpenCommand { get; }
        public IReactiveCommand SaveCommand { get; }


        public MainWindowViewModel()
        {
            _currentFile = new Bar();

            Items = new ObservableCollection<EntryModel>();
            Types = new EnumModel<Bar.EntryType>();

            NewCommand = ReactiveCommand.Create(NewEvent);
            OpenCommand = ReactiveCommand.Create(OpenEvent);
            SaveCommand = ReactiveCommand.Create(SaveEvent);
        }

        void NewEvent()
        {
            Items.Clear();
        }

        async void OpenEvent()
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

            var _files =  await _dialog.ShowAsync(MainWindow.Instance);

            if (_files.Length == 1)
            {
                using (FileStream _stream = new FileStream(_files[0], FileMode.Open))
                {
                    _currentFile = Bar.Read(_stream);

                    foreach (var _item in _currentFile)
                    {
                        var _barItem = new EntryModel(_item);
                        Items.Add(_barItem);
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
                }
            };

            var _file = await _dialog.ShowAsync(MainWindow.Instance);

            using (FileStream _stream = new FileStream(_file, FileMode.OpenOrCreate))
                Bar.Write(_stream, Items.Select(item => item.Entry));
        }
    }
}
