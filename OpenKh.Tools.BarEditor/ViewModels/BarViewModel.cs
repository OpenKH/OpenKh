using OpenKh.Kh2;
using OpenKh.Tools.BarEditor.Interfaces;
using OpenKh.Tools.BarEditor.Models;
using OpenKh.Tools.BarEditor.Services;
using OpenKh.Tools.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools.Models;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.BarEditor.ViewModels
{
    public class BarViewModel : GenericListModel<BarEntryModel>, IViewSettings
    {
        private static string ApplicationName = Utilities.GetApplicationName();
        private string _fileName;
        private static readonly List<FileDialogFilter> Filters = FileDialogFilterComposer.Compose().AddAllFiles();
        private readonly ToolInvokeDesc _toolInvokeDesc;
        private Bar.MotionsetType _motionsetType;
        private bool _showSlotNumber;
        private bool _showMovesetName;

        public static EnumModel<Bar.MotionsetType> MotionsetTypes { get; } = new EnumModel<Bar.MotionsetType>();

        public string Title => $"{FileName ?? "untitled"} | {ApplicationName}";

        public BarViewModel(ToolInvokeDesc desc) : this()
        {
            _toolInvokeDesc = desc;
            OpenStream(desc.SelectedEntry.Stream);

            NewCommand = new RelayCommand(x => { }, x => false);
            OpenCommand = new RelayCommand(x => { }, x => false);
            SaveCommand = new RelayCommand(x =>
            {
                var memoryStream = new MemoryStream();

                Bar.Write(memoryStream, Items.Select(item => item.Entry), MotionsetType);

                var stream = _toolInvokeDesc.SelectedEntry.Stream;

                stream.Position = 0;
                stream.SetLength(0);
                memoryStream.Position = 0;
                memoryStream.CopyTo(stream);
            });
            SaveAsCommand = new RelayCommand(x => { }, x => false);
        }

        public BarViewModel() : base(new BarEntryModel[0])
        {
            Types = new EnumModel<Bar.EntryType>();

            NewCommand = new RelayCommand(x =>
            {
                FileName = "untitled.bar";
                Items.Clear();
                MotionsetType = 0;
            }, x => true);

            OpenCommand = new RelayCommand(x =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    OpenFileName(fileName);
                    FileName = fileName;
                }, Filters);
            }, x => true);

            SaveCommand = new RelayCommand(x =>
            {
                if (!string.IsNullOrEmpty(FileName))
                {
                    SaveToFile(FileName);
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
                    SaveToFile(fileName);
                }, Filters, Path.GetFileName(FileName));
            }, x => true);

            ExitCommand = new RelayCommand(x =>
            {
                Window.Close();
            }, x => true);

            AboutCommand = new RelayCommand(x =>
            {
                new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);

            OpenItemCommand = new RelayCommand(x =>
            {
                try
                {
                    var tempFileName = SaveToTempraryFile();
                    switch (ToolsLoaderService.OpenTool(FileName, tempFileName, SelectedItem.Entry))
                    {
                        case Common.ToolInvokeDesc.ContentChangeInfo.File:
                            ReloadFromTemporaryFile(tempFileName);
                            break;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                OnPropertyChanged(nameof(SelectedItem));
            }, x => true);

            ExportCommand = new RelayCommand(x =>
            {
                var defaultFileName = GetSuggestedFileName(SelectedItem.Entry);

                FileDialog.OnSave(fileName =>
                {
                    using var fStream = File.OpenWrite(fileName);
                    SelectedItem.Entry.Stream.Position = 0;
                    SelectedItem.Entry.Stream.CopyTo(fStream);
                }, Filters, defaultFileName);
            }, x => IsItemSelected);

            ExportAllCommand = new RelayCommand(x =>
            {
                FileDialog.OnFolder(folder =>
                {
                    foreach (var item in Items.Select(item => item.Entry))
                    {
                        var fileName = GetSuggestedFileName(item);
                        using var fStream = File.OpenWrite(Path.Combine(folder, fileName));
                        item.Stream.Position = 0;
                        item.Stream.CopyTo(fStream);
                    }
                });
            }, x => true);

            ImportCommand = new RelayCommand(x =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    var fStream = File.OpenRead(fileName);
                    var memStream = new MemoryStream((int)fStream.Length);
                    fStream.CopyTo(memStream);
                    SelectedItem.Entry.Stream = memStream;

                    OnPropertyChanged(nameof(SelectedItem));
                }, Filters);
            }, x => IsItemSelected);
        }

        public void OpenFileName(string fileName)
        {
            using var stream = File.Open(fileName, FileMode.Open);
            OpenStream(stream);
        }

        private void OpenStream(Stream stream)
        {
            Items.Clear();

            var binarc = Bar.Read(stream);
            ShowMovesetName = AutodetectMsetFile(binarc);
            ShowSlotNumber = ShowMovesetName;

            MotionsetType = binarc.Motionset;

            foreach (var item in binarc)
            {
                Items.Add(new BarEntryModel(item, this));
            }
        }

        private void SaveToFile(string fileName)
        {
            var memoryStream = new MemoryStream();
            Bar.Write(memoryStream, Items.Select(item => item.Entry), MotionsetType);

            using var stream = File.Open(fileName, FileMode.Create);
            memoryStream.Position = 0;
            memoryStream.CopyTo(stream);
        }

        private string GetTemporaryFileName(string actualFileName)
        {
            return Path.GetTempFileName();
        }

        private string SaveToTempraryFile()
        {
            var tempFileName = GetTemporaryFileName(FileName);
            SaveToFile(tempFileName);

            return tempFileName;
        }

        private void ReloadFromTemporaryFile(string tempFileName)
        {
            OpenFileName(tempFileName);
        }

        private static string GetSuggestedFileName(Bar.Entry item) =>
            $"{item.Name}_{item.Index}.{Helpers.GetSuggestedExtension(item.Type)}";

        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public Bar.MotionsetType MotionsetType
        {
            get => _motionsetType;
            set
            {
                _motionsetType = value;
                InvalidateBarEntryTags();
                OnPropertyChanged(nameof(MotionsetType));
                OnPropertyChanged(nameof(IsMsetInfoVisible));
            }
        }

        public Visibility IsMsetInfoVisible =>
            MotionsetType == Bar.MotionsetType.Player &&
            ShowMovesetName == true ? Visibility.Visible : Visibility.Collapsed;

        public RelayCommand NewCommand { get; set; }
        public RelayCommand OpenCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand ExitCommand { get; set; }
        public RelayCommand AboutCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }
        public RelayCommand ExportAllCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }
        public RelayCommand OpenItemCommand { get; set; }

        public EnumModel<Bar.EntryType> Types { get; set; }

        public string ExportFileName => IsItemSelected ?
            GetSuggestedFileName(SelectedItem.Entry) : string.Empty;

        public bool IsPlayer => MotionsetType != Bar.MotionsetType.Default;

        public bool ShowSlotNumber
        {
            get => _showSlotNumber;
            set
            {
                _showSlotNumber = value;
                InvalidateBarEntryTags();
                OnPropertyChanged();
            }
        }

        public bool ShowMovesetName
        {
            get => _showMovesetName;
            set
            {
                _showMovesetName = value;
                InvalidateBarEntryTags();
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMsetInfoVisible));
            }
        }

        public int GetSlotIndex(BarEntryModel item) => Items.IndexOf(item);

        private void InvalidateBarEntryTags()
        {
            foreach (var item in Items)
                item.InvalidateTag();
        }

        protected override BarEntryModel OnNewItem()
        {
            return new BarEntryModel(new Bar.Entry()
            {
                Stream = new MemoryStream()
            }, this);
        }

        protected override void OnSelectedItem(BarEntryModel item)
        {
            base.OnSelectedItem(item);

            ExportCommand.CanExecute(SelectedItem);
            ImportCommand.CanExecute(SelectedItem);
            OpenItemCommand.CanExecute(SelectedItem);

            OnPropertyChanged(nameof(ExportFileName));
        }

        private static bool AutodetectMsetFile(Bar binarc)
        {
            // When the motionset type is not default, it's easy to assume it's a MSET.
            if (binarc.Motionset != Bar.MotionsetType.Default)
                return true;

            // When not, we need to make an assumption. For example a MSET usually
            // contains a bunch of ANB. We can say that if we find mroe tha N ANB
            // entries then it's a MSET.
            const int AnbCountAssumption = 4;
            return binarc.Count(x => x.Type == Bar.EntryType.Anb) >= AnbCountAssumption;
        }
    }
}
