using OpenKh.Tools.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Tools.LayoutViewer.Interfaces;
using OpenKh.Tools.LayoutViewer.Models;
using OpenKh.Tools.LayoutViewer.Service;
using OpenKh.Tools.LayoutViewer.Views;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;
using System;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged, IElementNames, IEditorSettings
    {
        private const string DefaultName = "FAKE";
        private static string ApplicationName = Utilities.GetApplicationName();
        private string _layoutName;
        private string _imagesName;
        private string _fileName;
        private TexturesViewModel _texturesViewModel;
        private LayoutEditorViewModel _layoutEditor;
        private ToolInvokeDesc _toolInvokeDesc;

        private static readonly List<FileDialogFilter> Filters = FileDialogFilterComposer.Compose().AddAllFiles();

        public string Title
        {
            get
            {
                var layoutContentName = $"{LayoutName ?? DefaultName},{ImagesName ?? DefaultName}";
                var fileName = IsToolDesc ? _toolInvokeDesc.Title : (FileName ?? "untitled");

                return $"{layoutContentName} | {fileName} | {ApplicationName}";
            }
        }
        private string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
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

        public bool IsToolDesc => _toolInvokeDesc != null;

        public LayoutEditorViewModel LayoutEditor
        {
            get => _layoutEditor;
            set
            {
                _layoutEditor = value;
                OnPropertyChanged();
            }
        }

        public string LayoutName
        {
            get => _layoutName;
            set
            {
                _layoutName = value.Length > 4 ? value.Substring(0, 4) : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public string ImagesName
        {
            get => _imagesName;
            set
            {
                _imagesName = value.Length > 4 ? value.Substring(0, 4) : value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public SequenceEditorViewModel SequenceEditor { get; private set; }

        public Color EditorBackground
        {
            get
            {
                var background = Settings.Default.BackgroundColor;
                return Color.FromArgb(background.A, background.R, background.G, background.B);
            }
            set
            {
                Settings.Default.BackgroundColor = System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            EditorDebugRenderingService = new EditorDebugRenderingService();
            LayoutEditor = new LayoutEditorViewModel(this, this, EditorDebugRenderingService);
            SequenceEditor = new SequenceEditorViewModel(EditorDebugRenderingService);

            OpenCommand = new RelayCommand(x => Utilities.Catch(() =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    OpenFile(fileName);
                }, Filters);
            }), x => !IsToolDesc);

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
                FileDialog.OnSave(fileName =>
                {
                    SaveFile(FileName, fileName);
                    FileName = fileName;
                }, Filters);
            }, x => !IsToolDesc);

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

                return Bar.Read(stream);
            }
        }

        public void OpenToolDesc(ToolInvokeDesc toolInvokeDesc)
        {
            _toolInvokeDesc = toolInvokeDesc;
            OpenFile(_toolInvokeDesc.ActualFileName);
        }

        public void ToolSaveContent()
        {
            if (IsToolDesc)
                _toolInvokeDesc.ContentChange = ToolInvokeDesc.ContentChangeInfo.File;
        }

        public void OpenFile(string fileName, bool doNotShowLayoutSelectionDialog = false)
        {
            if (OpenBarContent(ReadBarEntriesFromFileName(fileName), doNotShowLayoutSelectionDialog))
                FileName = fileName;
        }

        public void SaveFile(string previousFileName, string fileName)
        {
            var existingEntries = File.Exists(previousFileName) ? ReadBarEntriesFromFileName(previousFileName) : new List<Bar.Entry>();

            existingEntries = existingEntries.ForEntry(Bar.EntryType.Layout, LayoutName, 0, entry => LayoutEditor.Layout.Write(entry.Stream));
            existingEntries = existingEntries.ForEntry(Bar.EntryType.Imgz, ImagesName, 0, entry => Imgz.Write(entry.Stream, LayoutEditor.Images));

            using (var stream = File.Create(fileName))
                Bar.Write(stream, existingEntries);

            ToolSaveContent();
        }

        private bool OpenBarContent(IEnumerable<Bar.Entry> entries, bool doNotShowLayoutSelectionDialog = false)
        {
            var layoutEntries = entries.Count(x => x.Type == Bar.EntryType.Layout);
            var sequenceEntries = entries.Count(x => x.Type == Bar.EntryType.Seqd);

            if (sequenceEntries > 0)
                return Open2dd(entries, doNotShowLayoutSelectionDialog);
            if (layoutEntries > 0)
                return Open2ld(entries, doNotShowLayoutSelectionDialog);

            throw new Exception("The specified file does not contain any sequence or layout content to be played.");
        }

        private bool Open2ld(IEnumerable<Bar.Entry> entries, bool doNotShowLayoutSelectionDialog = false)
        {
            LayoutEntryModel layoutEntryModel;
            var layoutEntries = entries.Count(x => x.Type == Bar.EntryType.Layout);
            int imagesEntries = entries.Count(x => x.Type == Bar.EntryType.Imgz);

            if (!doNotShowLayoutSelectionDialog && (layoutEntries > 1 || imagesEntries > 1))
            {
                var vm = new LayoutSelectionViewModel(entries);
                var dialog = new LayoutSelectionDialog()
                {
                    DataContext = vm
                };

                layoutEntryModel = dialog.ShowDialog() == true ? vm.SelectedLayoutEntry : null;
            }
            else if (layoutEntries > 0 && imagesEntries > 0)
            {
                var layoutName = entries.FirstOrDefault(x => x.Type == Bar.EntryType.Layout);
                var imagesName = entries.FirstOrDefault(x => x.Type == Bar.EntryType.Imgz);
                var layout = layoutName != null ? Layout.Read(layoutName.Stream) : null;
                var images = entries.Where(x => x.Type == Bar.EntryType.Imgz).Select(x => Imgz.Read(x.Stream)).First();

                layoutEntryModel = new LayoutEntryModel
                {
                    Layout = new LayoutEntryPropertyModel<Layout>
                    {
                        Name = layoutName.Name,
                        Value = layout
                    },
                    Images = new LayoutEntryPropertyModel<List<Imgd>>
                    {
                        Name = imagesName.Name,
                        Value = images.ToList()
                    },
                };
            }
            else
            {
                if (layoutEntries == 0)
                    MessageBox.Show(Window, "No Layout data found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (imagesEntries == 0)
                    MessageBox.Show(Window, "No IMGZ data found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show(Window, "Unspecified error. Please report this.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (layoutEntryModel == null)
                return false;

            OpenLayout(layoutEntryModel);
            return true;
        }

        private bool Open2dd(IEnumerable<Bar.Entry> entries, bool doNotShowLayoutSelectionDialog = false)
        {
            var sequenceEntries = entries.Count(x => x.Type == Bar.EntryType.Seqd);
            int imagesEntries = entries.Count(x => x.Type == Bar.EntryType.Imgd);

            //if (sequenceEntries > 1)
            //    throw new Exception("Did not expected multiple sequences.");
            var sequence = Sequence.Read(entries.First(x => x.Type == Bar.EntryType.Seqd).Stream);
            //if (imagesEntries > 1)
            //    throw new Exception("Did not expected multiple images for a single sequence.");
            var image = Imgd.Read(entries.First(x => x.Type == Bar.EntryType.Imgd).Stream);

            OpenSequence(sequence, image);
            return true;
        }

        private void OpenLayout(LayoutEntryModel layoutEntryModel)
        {
            LayoutName = layoutEntryModel.Layout.Name;
            ImagesName = layoutEntryModel.Images.Name;

            SequenceEditor.SelectedSequence = layoutEntryModel.Layout.Value.SequenceItems.FirstOrDefault();
            SequenceEditor.SelectedImage = layoutEntryModel.Images.Value.FirstOrDefault();

            _texturesViewModel = new TexturesViewModel(layoutEntryModel.Images.Value);
            LayoutEditor = new LayoutEditorViewModel(this, this, EditorDebugRenderingService)
            {
                SequenceGroups = new SequenceGroupsViewModel(layoutEntryModel.Layout.Value, _texturesViewModel, EditorDebugRenderingService),
                Layout = layoutEntryModel.Layout.Value,
                Images = layoutEntryModel.Images.Value
            };
        }

        private void OpenSequence(Sequence sequence, Imgd image)
        {
            MessageBox.Show("Sequence");
        }
    }
}
