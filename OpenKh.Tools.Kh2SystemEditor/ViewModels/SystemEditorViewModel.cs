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

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class SystemEditorViewModel : BaseNotifyPropertyChanged
    {
        private static string ApplicationName = Utilities.GetApplicationName();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private string _fileName;
        private IEnumerable<Bar.Entry> _barItems;
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
            OpenCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, new[]
                {
                    ("03system.bin", "bin"),
                    ("BAR file", "bar"),
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
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save);
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

            CreateBattleItems();
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

            LoadBattleItems(items);

            FileName = fileName;
            return true;
        });

        public void SaveFile(string previousFileName, string fileName)
        {
            File.Create(fileName).Using(stream =>
            {
                SaveBattleItems();
                Bar.Write(stream, _barItems);
            });
        }

        private bool Is03System(List<Bar.Entry> items) => items.Any(x => new[]
        {
            "item",
            "trsr",
            "ftst",
        }.Contains(x.Name));

        private void CreateBattleItems()
        {
            _barItems = new Bar.Entry[0];
            Item = GetDefaultViewModelInstance<ItemViewModel>();
            Trsr = GetDefaultViewModelInstance<TrsrViewModel>();
            Ftst = GetDefaultViewModelInstance<FtstViewModel>();
        }

        private void LoadBattleItems(IEnumerable<Bar.Entry> entries)
        {
            _barItems = entries;
            Item = GetViewModelInstance<ItemViewModel>(_barItems);
            //Trsr = GetViewModelInstance<TrsrViewModel>(_barItems);
            //Ftst = GetViewModelInstance<FtstViewModel>(_barItems);
        }

        private void SaveBattleItems()
        {
            _barItems = SaveBattleItem(_barItems, Item);
            _barItems = SaveBattleItem(_barItems, Trsr);
            _barItems = SaveBattleItem(_barItems, Ftst);
        }

        private IEnumerable<Bar.Entry> SaveBattleItem(IEnumerable<Bar.Entry> entries, ISystemGetChanges battleGetChanges) =>
            entries.ForEntry(Bar.EntryType.Binary, battleGetChanges.EntryName, 0, entry => entry.Stream = battleGetChanges.CreateStream());

        private T GetViewModelInstance<T>(IEnumerable<Bar.Entry> entries)
            where T : ISystemGetChanges => (T)Activator.CreateInstance(typeof(T), entries);

        private T GetDefaultViewModelInstance<T>()
            where T : ISystemGetChanges => Activator.CreateInstance<T>();
    }
}
