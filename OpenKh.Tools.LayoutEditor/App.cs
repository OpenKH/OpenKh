using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using OpenKh.Common;
using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Tools.Common;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.LayoutEditor.Dialogs;
using OpenKh.Tools.LayoutEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using Xe.Tools.Wpf.Dialogs;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.LayoutEditor
{
    public class App : IEditorSettings, IDisposable
    {
        private static readonly List<FileDialogFilter> Filters = FileDialogFilterComposer
            .Compose()
            .AddExtensions("2LD Layout container file", "2ld")
            .AddExtensions("2DD Sequence container file", "2dd")
            .AddAllFiles();
        private const string DefaultName = "FAKE";

        private readonly MonoGameImGuiBootstrap _bootstrap;
        private bool _exitFlag = false;

        private string _animationName;
        private string _spriteName;
        private string _fileName;
        private ToolInvokeDesc _toolInvokeDesc;
        private IApp _app;

        private bool _linkToPcsx2;
        private ProcessStream _processStream;
        private int _processOffset;

        private const string LinkToPcsx2ActionName = "Open file and link it to PCSX2";
        private const string ResourceSelectionDialogTitle = "Resource selection";
        private bool _isResourceSelectionDialogOpening;
        private bool _isResourceSelectingLayout;
        private ResourceSelectionDialog _resourceSelectionDialog;
        private Dictionary<Keys, Action> _keyMapping = new Dictionary<Keys, Action>();

        public event IEditorSettings.ChangeBackground OnChangeBackground;

        public string Title
        {
            get
            {
                var contentName = $"{AnimationName ?? DefaultName},{TextureName ?? DefaultName}";
                var fileName = IsToolDesc ? _toolInvokeDesc.Title : (FileName ?? "untitled");
                if (_processStream != null)
                    fileName = $"{fileName}@pcsx2:{_processOffset}";

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

        public string TextureName
        {
            get => _spriteName;
            set => _spriteName = value.Length > 4 ? value.Substring(0, 4) : value;
        }

        public bool CheckerboardBackground { get; set; }
        public ColorF EditorBackground
        {
            get => new ColorF(Settings.Default.BgColorR, Settings.Default.BgColorG,
                Settings.Default.BgColorB, 1f);
            set
            {
                Settings.Default.BgColorR = value.R;
                Settings.Default.BgColorG = value.G;
                Settings.Default.BgColorB = value.B;
                Settings.Default.Save();
            }
        }

        public App(MonoGameImGuiBootstrap bootstrap)
        {
            _bootstrap = bootstrap;
            _bootstrap.Title = Title;
            AddKeyMapping(Keys.O, MenuFileOpenWithoutPcsx2);
            AddKeyMapping(Keys.A, MenuFileOpenPcsx2);
            AddKeyMapping(Keys.S, MenuFileSave);
        }

        public bool MainLoop()
        {
            ProcessKeyMapping();

            bool dummy = true;
            if (ImGui.BeginPopupModal(ResourceSelectionDialogTitle, ref dummy,
                ImGuiWindowFlags.Popup | ImGuiWindowFlags.Modal | ImGuiWindowFlags.AlwaysAutoResize))
            {
                _resourceSelectionDialog.Run();
                ImGui.EndPopup();

                if (_resourceSelectionDialog.HasResourceBeenSelected)
                {
                    if (_isResourceSelectingLayout)
                        OpenLayoutEditor(_resourceSelectionDialog.SelectedAnimation,
                            _resourceSelectionDialog.SelectedTexture);
                    else
                        OpenSequenceEditor(_resourceSelectionDialog.SelectedAnimation,
                            _resourceSelectionDialog.SelectedTexture);
                }
            }

            ImGuiEx.MainWindow(() =>
            {
                MainMenu();
                MainWindow();
            });

            if (_isResourceSelectionDialogOpening)
            {
                ImGui.OpenPopup(ResourceSelectionDialogTitle);
                _isResourceSelectionDialogOpening = false;
            }

            return _exitFlag;
        }

        public void Dispose()
        {
            CloseProcessStream();
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
                    ForMenuItem("Open...", "CTRL+O", MenuFileOpenWithoutPcsx2);
                    ForMenuItem($"{LinkToPcsx2ActionName}...", "CTRL+A", MenuFileOpenPcsx2);
                    ForMenuItem("Save", "CTRL+S", MenuFileSave, CurrentEditor != null);
                    ForMenuItem("Save as...", MenuFileSaveAs, CurrentEditor != null);
                    ImGui.Separator();
                    ForMenu("Preferences", () =>
                    {
                        //var checkerboardBackground = CheckerboardBackground;
                        //if (ImGui.Checkbox("Checkerboard background", ref checkerboardBackground))
                        //{
                        //    CheckerboardBackground = false;
                        //    OnChangeBackground?.Invoke(this, this);
                        //}

                        var editorBackground = new Vector3(EditorBackground.R,
                            EditorBackground.G, EditorBackground.B);
                        if (ImGui.ColorEdit3("Background color", ref editorBackground))
                        {
                            EditorBackground = new ColorF(editorBackground.X,
                                editorBackground.Y, editorBackground.Z, 1f);
                            OnChangeBackground?.Invoke(this, this);
                        }
                    });
                    ImGui.Separator();
                    ForMenuItem("Exit", MenuFileExit);
                });
                _app?.Menu();
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

        private void MenuFileOpenWithoutPcsx2()
        {
            _linkToPcsx2 = false;
            CloseProcessStream();
            MenuFileOpen();
        }

        private void MenuFileOpenPcsx2()
        {
            CloseProcessStream();
            var processes = Process.GetProcessesByName("pcsx2");
            if (processes.Length == 0)
            {
                ShowLinkPcsx2ErrorProcessNotFound();
                return;
            }

            _linkToPcsx2 = true;
            MenuFileOpen();
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
            var existingEntries = File.Exists(previousFileName) ?
                ReadBarEntriesFromFileName(previousFileName) : new List<Bar.Entry>();

            var animationEntry = CurrentEditor.SaveAnimation(AnimationName);
            if (_processStream != null)
            {
                animationEntry.Stream.SetPosition(0);
                _processStream.SetPosition(_processOffset);
                animationEntry.Stream.CopyTo(_processStream);
                animationEntry.Stream.SetPosition(0);
            }

            var newEntries = existingEntries
                .AddOrReplace(animationEntry)
                .AddOrReplace(CurrentEditor.SaveTexture(TextureName));
            File.Create(fileName).Using(stream => Bar.Write(stream, newEntries));

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
                return Open2dd(entries);
            if (DoesContainLayoutAnimations(entries))
                return Open2ld(entries, doNotShowLayoutSelectionDialog);

            throw new Exception("The specified file does not contain any sequence or layout content to be played.");
        }

        private bool Open2ld(IEnumerable<Bar.Entry> entries, bool doNotShowLayoutSelectionDialog = false)
        {
            var layoutEntries = entries.Count(x => x.Type == Bar.EntryType.Layout);
            int imagesEntries = entries.Count(x => x.Type == Bar.EntryType.Imgz);

            if (layoutEntries == 0)
                throw new Exception("No layout found.");
            if (imagesEntries == 0)
                throw new Exception("No image container found.");
            if (layoutEntries > 1 || imagesEntries > 1)
            {
                OpenResourceSelectionDialog(entries, Bar.EntryType.Layout, Bar.EntryType.Imgz);
                return true;
            }

            var layoutEntry = entries.First(x => x.Type == Bar.EntryType.Layout);
            var textureContainerEntry = entries.First(x => x.Type == Bar.EntryType.Imgz);
            OpenLayoutEditor(layoutEntry, textureContainerEntry);
            return true;
        }

        private bool Open2dd(IEnumerable<Bar.Entry> entries)
        {
            var sequenceEntries = entries.Count(x => x.Type == Bar.EntryType.Seqd);
            int imagesEntries = entries.Count(x => x.Type == Bar.EntryType.Imgd);

            if (sequenceEntries == 0)
                throw new Exception("No sequence found.");
            if (imagesEntries == 0)
                throw new Exception("No image found.");
            if (sequenceEntries > 1 || imagesEntries > 1)
            {
                OpenResourceSelectionDialog(entries, Bar.EntryType.Seqd, Bar.EntryType.Imgd);
                return true;
            }

            var sequenceEntry = entries.First(x => x.Type == Bar.EntryType.Seqd);
            var textureEntry = entries.First(x => x.Type == Bar.EntryType.Imgd);
            OpenSequenceEditor(sequenceEntry, textureEntry);
            return true;
        }

        private void OpenResourceSelectionDialog(IEnumerable<Bar.Entry> entries,
            Bar.EntryType animationType, Bar.EntryType textureType)
        {
            _resourceSelectionDialog = new ResourceSelectionDialog(
                entries, animationType, textureType);
            _isResourceSelectionDialogOpening = true;
            _isResourceSelectingLayout = animationType == Bar.EntryType.Layout;
        }

        private void OpenSequenceEditor(Bar.Entry sequenceEntry, Bar.Entry textureEntry)
        {
            AnimationName = sequenceEntry.Name;
            TextureName = textureEntry.Name;

            if (_linkToPcsx2)
            {
                _linkToPcsx2 = false;
                if (!LinkSeqdToPcs2(sequenceEntry.Stream))
                    return;
            }

            var app = new AppSequenceEditor(_bootstrap,
                this,
                Sequence.Read(sequenceEntry.Stream.SetPosition(0)),
                Imgd.Read(textureEntry.Stream));
            _app = app;
            CurrentEditor = app;
        }

        private void OpenLayoutEditor(Bar.Entry layoutEntry, Bar.Entry textureContainerEntry)
        {
            AnimationName = layoutEntry.Name;
            TextureName = textureContainerEntry.Name;

            if (_linkToPcsx2)
            {
                _linkToPcsx2 = false;
                if (!LinkLaydToPcs2(layoutEntry.Stream))
                    return;
            }

            var app = new AppLayoutEditor(_bootstrap,
                this,
                Layout.Read(layoutEntry.Stream.SetPosition(0)),
                Imgz.Read(textureContainerEntry.Stream));
            _app = app;
            CurrentEditor = app;
        }

        private void UpdateTitle()
        {
            _bootstrap.Title = Title;
        }

        private void AddKeyMapping(Keys key, Action action)
        {
            _keyMapping[key] = action;
        }

        private void ProcessKeyMapping()
        {
            var k = Keyboard.GetState();
            if (k.IsKeyDown(Keys.LeftControl))
            {
                var keys = k.GetPressedKeys();
                foreach (var key in keys)
                {
                    if (_keyMapping.TryGetValue(key, out var action))
                        action();
                }
            }
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

        private bool LinkLaydToPcs2(Stream stream) =>
            LinkToPcs2(stream, Layout.MagicCodeValidator, 0x1c);

        private bool LinkSeqdToPcs2(Stream stream) =>
            LinkToPcs2(stream, Sequence.MagicCodeValidator, 0x2c);

        private bool LinkToPcs2(Stream stream, uint magicCode, int headerLength)
        {
            var process = Process.GetProcessesByName("pcsx2").FirstOrDefault();
            if (process == null)
            {
                ShowLinkPcsx2ErrorProcessNotFound();
                return false;
            }

            var processStream = new ProcessStream(process, ToolConstants.Pcsx2BaseAddress, ToolConstants.Ps2MemoryLength);
            var bufferedStream = new BufferedStream(processStream, 0x10000);

            var header = stream.SetPosition(sizeof(uint)).ReadBytes(headerLength);
            while (bufferedStream.Position < bufferedStream.Length)
            {
                if (bufferedStream.ReadUInt32() == magicCode)
                {
                    // header matches. Check if the rest of 0x2c SEQD header matches as well...
                    if (header.SequenceEqual(bufferedStream.ReadBytes(headerLength)))
                    {
                        _processStream = processStream;
                        _processOffset = (int)(bufferedStream.Position - headerLength - sizeof(uint));
                        return true;
                    }
                }
            }

            ShowLinkPcsx2ErrorFileNotFound();
            CloseProcessStream();
            return false;
        }

        private void CloseProcessStream()
        {
            _processStream?.Dispose();
            _processStream = null;
        }

        private static bool DoesContainSequenceAnimations(IEnumerable<Bar.Entry> entries) =>
            entries.Any(x => x.Type == Bar.EntryType.Seqd);

        private static bool DoesContainLayoutAnimations(IEnumerable<Bar.Entry> entries) =>
            entries.Any(x => x.Type == Bar.EntryType.Layout);

        private static void ShowLinkPcsx2ErrorProcessNotFound() =>
            ShowError("No PCSX2 process found.\nPlease run PCSX2 with Kingdom Hearts II/Re:CoM first and try again.", LinkToPcsx2ActionName);

        private static void ShowLinkPcsx2ErrorFileNotFound() =>
            ShowError("The file you specified can not be found on the running game.", LinkToPcsx2ActionName);

        public static void ShowError(string message, string title = "Error") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        private void ShowAboutDialog() =>
            MessageBox.Show("OpenKH is amazing.");
    }
}
