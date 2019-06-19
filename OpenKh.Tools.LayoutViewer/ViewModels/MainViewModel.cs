using kh.tools.common;
using OpenKh.Kh2;
using OpenKh.Tools.LayoutViewer.Interfaces;
using OpenKh.Tools.LayoutViewer.Models;
using OpenKh.Tools.LayoutViewer.Service;
using OpenKh.Tools.LayoutViewer.Views;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private const string DefaultLayoutName = "FAKE";
        private static string ApplicationName = Utilities.GetApplicationName();
        private string layoutName;
        private string fileName;
        private TexturesViewModel texturesViewModel;

        public string Title => $"{LayoutName ?? DefaultLayoutName} | {FileName ?? "untitled"} | {ApplicationName}";
        private string FileName
        {
            get => fileName;
            set
            {
                fileName = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public EditorDebugRenderingService EditorDebugRenderingService { get; }

        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        public RelayCommand OpenCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand ExitCommand { get; set; }
        public RelayCommand AboutCommand { get; set; }

        public LayoutEditorViewModel LayoutEditor { get; set; }

        public string LayoutName
        {
            get => layoutName; private set
            {
                layoutName = value.Length > 4 ? value.Substring(0, 4) : value;
                OnPropertyChanged(nameof(LayoutName));
                OnPropertyChanged(nameof(Title));
            }
        }

        public SequenceEditorViewModel SequenceEditor { get; private set; }

        public MainViewModel()
        {
            EditorDebugRenderingService = new EditorDebugRenderingService();
            LayoutEditor = new LayoutEditorViewModel(EditorDebugRenderingService);
            SequenceEditor = new SequenceEditorViewModel(EditorDebugRenderingService);

            OpenCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, FileDialog.Type.Any);
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
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, FileDialog.Type.Any);
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
        }

        private static IEnumerable<Bar.Entry> ReadBarEntriesFromFileName(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                if (!Bar.IsValid(stream))
                    throw new InvalidDataException("Not a bar file");

                return Bar.Open(stream);
            }
        }

        public void OpenFile(string fileName, bool doNotShowLayoutSelectionDialog = false)
        {
            if (OpenBarContent(ReadBarEntriesFromFileName(fileName), doNotShowLayoutSelectionDialog))
                FileName = fileName;
        }

        public void SaveFile(string previousFileName, string fileName)
        {
            var existingEntries = File.Exists(previousFileName) ? ReadBarEntriesFromFileName(previousFileName).ToList() : new List<Bar.Entry>();

            var layoutBarEntry = existingEntries.FirstOrDefault(x => x.Type == Bar.EntryType.Layout && x.Name == LayoutName);
            if (layoutBarEntry == null)
                existingEntries.Add(layoutBarEntry = new Bar.Entry
                {
                    Index = 0,
                    Name = LayoutName,
                    Type = Bar.EntryType.Layout
                });
            var layoutStream = layoutBarEntry.Stream = new MemoryStream();
            LayoutEditor.Layout.Write(layoutStream);

            var imagesBarEntry = existingEntries.FirstOrDefault(x => x.Type == Bar.EntryType.Layout && x.Name == LayoutName);
            if (imagesBarEntry == null)
                existingEntries.Add(imagesBarEntry = new Bar.Entry
                {
                    Index = 0,
                    Name = LayoutName,
                    Type = Bar.EntryType.Imgz
                });
            var imgzStream = imagesBarEntry.Stream = new MemoryStream();
            Imgz.Save(imgzStream, LayoutEditor.Images);

            using (var stream = File.Create(fileName))
                Bar.Save(stream, existingEntries);
        }

        private bool OpenBarContent(IEnumerable<Bar.Entry> entries, bool doNotShowLayoutSelectionDialog = false)
        {
            LayoutEntryModel layoutEntryModel;

            if (!doNotShowLayoutSelectionDialog && entries.Count(x => x.Type == Bar.EntryType.Layout) > 1)
            {
                var vm = new LayoutSelectionViewModel(entries);
                var dialog = new LayoutSelectionDialog()
                {
                    DataContext = vm
                };

                layoutEntryModel = dialog.ShowDialog() == true ? vm.SelectedLayoutEntry : null;
            }
            else
            {
                var barLayoutEntry = entries.FirstOrDefault(x => x.Type == Bar.EntryType.Layout);
                var layout = barLayoutEntry != null ? Layout.Read(barLayoutEntry.Stream) : null;
                var images = entries.Where(x => x.Type == Bar.EntryType.Imgz).Select(x => Imgz.Open(x.Stream)).First();

                layoutEntryModel = new LayoutEntryModel
                {
                    Name = barLayoutEntry?.Name,
                    Layout = layout,
                    Images = images.ToList()
                };
            }

            if (layoutEntryModel == null)
                return false;

            OpenLayout(layoutEntryModel);
            return true;
        }

        private void OpenLayout(LayoutEntryModel layoutEntryModel)
        {
            LayoutName = layoutEntryModel.Name;

            texturesViewModel = new TexturesViewModel(layoutEntryModel.Images);
            LayoutEditor.SequenceGroups = new SequenceGroupsViewModel(layoutEntryModel.Layout, texturesViewModel, EditorDebugRenderingService);
            LayoutEditor.Layout = layoutEntryModel.Layout;
            LayoutEditor.Images = layoutEntryModel.Images;

            SequenceEditor.SelectedSequence = layoutEntryModel.Layout.SequenceItems.FirstOrDefault();
            SequenceEditor.SelectedImage = layoutEntryModel.Images.FirstOrDefault();
        }
    }
}
