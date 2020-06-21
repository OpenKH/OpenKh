using ImGuiNET;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.Common;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.LayoutEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Xe.Tools.Wpf.Dialogs;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.LayoutEditor
{
    public class App : IDisposable
    {
        private static readonly List<FileDialogFilter> Filters = FileDialogFilterComposer.Compose().AddAllFiles();
        private const string DefaultName = "FAKE";

        private readonly MonoGameImGuiBootstrap _bootstrap;
        private bool _exitFlag = false;

        private string _animationName;
        private string _spriteName;
        private string _fileName;
        private ToolInvokeDesc _toolInvokeDesc;
        private IApp _app;

        public string Title
        {
            get
            {
                var contentName = $"{AnimationName ?? DefaultName},{SpriteName ?? DefaultName}";
                var fileName = IsToolDesc ? _toolInvokeDesc.Title : (FileName ?? "untitled");

                return $"{contentName} | {fileName} | {MonoGameImGuiBootstrap.ApplicationName}";
            }
        }

        private string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                UpdateTitle();
            }
        }

        public bool IsToolDesc => _toolInvokeDesc != null;
        public ISaveBar CurrentEditor { get; private set; }

        public string AnimationName
        {
            get => _animationName;
            set => _animationName = value.Length > 4 ? value.Substring(0, 4) : value;
        }

        public string SpriteName
        {
            get => _spriteName;
            set => _spriteName = value.Length > 4 ? value.Substring(0, 4) : value;
        }

        public App(MonoGameImGuiBootstrap bootstrap)
        {
            _bootstrap = bootstrap;
            _bootstrap.Title = Title;
        }

        public bool MainLoop()
        {
            ImGuiEx.MainWindow(() =>
            {
                MainMenu();
                MainWindow();
            });

            return _exitFlag;
        }

        public void Dispose()
        {
        }

        private void MainWindow()
        {
            if (_app != null)
            {
                _app.Run();
            }
            else
            {
                ImGui.Text("No files loaded at the moment");
            }
        }

        void MainMenu()
        {
            ForMenuBar(() =>
            {
                ForMenu("File", () =>
                {
                    ForMenuItem("Open...", MenuFileOpen);
                    ForMenuItem("Save", MenuFileSave);
                    ForMenuItem("Save as...", MenuFileSaveAs);
                    ImGui.Separator();
                    ForMenuItem("Exit", MenuFileExit);
                });
                ForMenu("Help", () =>
                {
                    ForMenuItem("About", ShowAboutDialog);
                });
            });
        }

        private void MenuFileOpen()
        {
            FileDialog.OnOpen(fileName =>
            {
                OpenFile(fileName);
            }, Filters);
        }

        private void MenuFileSave()
        {
            if (!string.IsNullOrEmpty(FileName))
                SaveFile(FileName, FileName);
            else
                MenuFileSaveAs();
        }

        private void MenuFileSaveAs()
        {
            FileDialog.OnSave(fileName =>
            {
                SaveFile(FileName, fileName);
                FileName = fileName;
            }, Filters);
        }

        private void MenuFileExit() => _exitFlag = true;

        public void OpenToolDesc(ToolInvokeDesc toolInvokeDesc)
        {
            _toolInvokeDesc = toolInvokeDesc;
            OpenFile(_toolInvokeDesc.ActualFileName);
        }

        public void OpenFile(string fileName, bool doNotShowLayoutSelectionDialog = false)
        {
            if (OpenBarContent(ReadBarEntriesFromFileName(fileName), doNotShowLayoutSelectionDialog))
                FileName = fileName;
        }

        public void SaveFile(string previousFileName, string fileName)
        {
            var existingEntries = File.Exists(previousFileName) ? ReadBarEntriesFromFileName(previousFileName) : new List<Bar.Entry>();

            using (var stream = File.Create(fileName))
                Bar.Write(stream, CurrentEditor.Save(existingEntries));

            if (IsToolDesc)
                _toolInvokeDesc.ContentChange = ToolInvokeDesc.ContentChangeInfo.File;
        }

        private static IEnumerable<Bar.Entry> ReadBarEntriesFromFileName(string fileName) =>
            File.OpenRead(fileName).Using(stream =>
            {
                if (!Bar.IsValid(stream))
                    throw new InvalidDataException("Not a bar file");

                return Bar.Read(stream);
            });

        private bool OpenBarContent(IEnumerable<Bar.Entry> entries, bool doNotShowLayoutSelectionDialog = false)
        {
            var layoutEntries = entries.Count(x => x.Type == Bar.EntryType.Layout);
            var sequenceEntries = entries.Count(x => x.Type == Bar.EntryType.Seqd);

            if (DoesContainSequenceAnimations(entries))
                return Open2dd(entries, doNotShowLayoutSelectionDialog);
            if (DoesContainLayoutAnimations(entries))
                return Open2ld(entries, doNotShowLayoutSelectionDialog);

            throw new Exception("The specified file does not contain any sequence or layout content to be played.");
        }

        private bool Open2ld(IEnumerable<Bar.Entry> entries, bool doNotShowLayoutSelectionDialog = false)
        {
            //LayoutEntryModel layoutEntryModel;
            var layoutEntries = entries.Count(x => x.Type == Bar.EntryType.Layout);
            int imagesEntries = entries.Count(x => x.Type == Bar.EntryType.Imgz);

            if (!doNotShowLayoutSelectionDialog && (layoutEntries > 1 || imagesEntries > 1))
            {
                //var dialog = new AppElementSelection(entries);
                //layoutEntryModel = dialog.Run() == true ? dialog.SelectedLayoutEntry : null;
            }
            else if (layoutEntries > 0 && imagesEntries > 0)
            {
                var layoutName = entries.FirstOrDefault(x => x.Type == Bar.EntryType.Layout);
                var imagesName = entries.FirstOrDefault(x => x.Type == Bar.EntryType.Imgz);
                var layout = layoutName != null ? Layout.Read(layoutName.Stream) : null;
                var images = entries.Where(x => x.Type == Bar.EntryType.Imgz).Select(x => Imgz.Read(x.Stream)).First();

                //layoutEntryModel = new LayoutEntryModel
                //{
                //    Layout = new LayoutEntryPropertyModel<Layout>
                //    {
                //        Name = layoutName.Name,
                //        Value = layout
                //    },
                //    Images = new LayoutEntryPropertyModel<List<Imgd>>
                //    {
                //        Name = imagesName.Name,
                //        Value = images.ToList()
                //    },
                //};
            }
            else
            {
                if (layoutEntries == 0)
                    ShowError("No Layout data found.");
                else if (imagesEntries == 0)
                    ShowError("No IMGZ data found.");
                else
                    ShowError("Unspecified error. Please report this.");
                return false;
            }

            //if (layoutEntryModel == null)
            //    return false;

            //OpenLayout(layoutEntryModel);
            return true;
        }

        private bool Open2dd(IEnumerable<Bar.Entry> entries, bool doNotShowLayoutSelectionDialog = false)
        {
            var sequenceEntries = entries.Count(x => x.Type == Bar.EntryType.Seqd);
            int imagesEntries = entries.Count(x => x.Type == Bar.EntryType.Imgd);

            if (sequenceEntries == 0)
                throw new Exception("No sequence found.");
            if (sequenceEntries > 1)
                throw new Exception("Did not expected multiple sequences.");
            var sequenceEntry = entries.First(x => x.Type == Bar.EntryType.Seqd);
            AnimationName = sequenceEntry.Name;

            if (imagesEntries == 0)
                throw new Exception("No image found.");
            if (imagesEntries > 1)
                throw new Exception("Did not expected multiple images for a single sequence.");
            var imageEntry = entries.First(x => x.Type == Bar.EntryType.Imgd);
            SpriteName = imageEntry.Name;

            _app = new AppSequenceEditor(
                Sequence.Read(sequenceEntry.Stream),
                Imgd.Read(imageEntry.Stream));
            UpdateTitle();

            return true;
        }

        private void UpdateTitle()
        {
            _bootstrap.Title = Title;
        }

        //private void OpenLayout(LayoutEntryModel layoutEntryModel)
        //{
        //    AnimationName = layoutEntryModel.Layout.Name;
        //    SpriteName = layoutEntryModel.Images.Name;

        //    var texturesViewModel = new TexturesViewModel(layoutEntryModel.Images.Value);

        //    var layoutEditorViewModel = new LayoutEditorViewModel(this, this, EditorDebugRenderingService)
        //    {
        //        SequenceGroups = new SequenceGroupsViewModel(layoutEntryModel.Layout.Value, texturesViewModel, EditorDebugRenderingService),
        //        Layout = layoutEntryModel.Layout.Value,
        //        Images = layoutEntryModel.Images.Value
        //    };

        //    CurrentEditor = layoutEditorViewModel;
        //    OnControlChanged?.Invoke(new LayoutEditorView()
        //    {
        //        DataContext = layoutEditorViewModel
        //    });
        //}


        private static bool DoesContainSequenceAnimations(IEnumerable<Bar.Entry> entries) =>
            entries.Any(x => x.Type == Bar.EntryType.Seqd);

        private static bool DoesContainLayoutAnimations(IEnumerable<Bar.Entry> entries) =>
            entries.Any(x => x.Type == Bar.EntryType.Layout);

        public static void ShowError(string message, string title = "Error") =>
            MessageBox.Show(null, message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        private void ShowAboutDialog() =>
            MessageBox.Show("OpenKH is amazing.");
    }
}
