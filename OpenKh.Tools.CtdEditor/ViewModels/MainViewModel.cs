using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Tools.Common;
using OpenKh.Tools.CtdEditor.Interfaces;
using OpenKh.Tools.CtdEditor.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private static string ApplicationName = Utilities.GetApplicationName();
        private static readonly IEnumerable<FileDialogFilter> CtdFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("CTD message", "ctd")
            .AddAllFiles();
        private static readonly IEnumerable<FileDialogFilter> FontArcFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("Font archive", "arc")
            .AddAllFiles();

        private readonly CtdDrawHandler _drawHandler = new CtdDrawHandler();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private string _fileName;
        private CtdViewModel _ctdViewModel;
        private FontsArc _fonts;

        public string Title => $"{Path.GetFileName(FileName) ?? "untitled"} | {ApplicationName}";

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

        public RelayCommand OpenFontCommand { get; }
        public RelayCommand OpenFontEditorCommand { get; }

        public CtdViewModel CtdViewModel
        {
            get => _ctdViewModel;
            private set { _ctdViewModel = value; OnPropertyChanged(); }
        }

        public Ctd Ctd
        {
            get => CtdViewModel?.Ctd;
            set => CtdViewModel = new CtdViewModel(_drawHandler, value)
            {
                Fonts = _fonts
            };
        }

        public FontsArc Fonts
        {
            get => _fonts;
            set
            {
                _fonts = value;
                CtdViewModel.Fonts = _fonts;
                OnPropertyChanged(nameof(OpenFontEditorCommand));
            }
        }

        public MainViewModel()
        {
            OpenCommand = new RelayCommand(x =>
                FileDialog.OnOpen(fileName => OpenFile(fileName), CtdFilter), x => true);

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
                FileDialog.OnSave(fileName => SaveFile(FileName, fileName), CtdFilter), x => true);

            ExitCommand = new RelayCommand(x =>
            {
                Window.Close();
            }, x => true);

            OpenFontCommand = new RelayCommand(x =>
                FileDialog.OnOpen(fileName => OpenFontFile(fileName), FontArcFilter),
                x => CtdViewModel != null);

            OpenFontEditorCommand = new RelayCommand(x =>
                new FontWindow()
                {
                    DataContext = new FontEditorViewModel(Fonts)
                }.ShowDialog(),
                x => Fonts != null);

            AboutCommand = new RelayCommand(x =>
            {
                new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);

            CtdViewModel = new CtdViewModel(_drawHandler);
        }

        private bool OpenFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            if (!Ctd.IsValid(stream))
            {
                MessageBox.Show(Window, $"{Path.GetFileName(fileName)} is not a valid CTD file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            Ctd = Ctd.Read(stream);
            FileName = fileName;
            return true;
        });

        private void SaveFile(string previousFileName, string fileName)
        {
            File.Create(fileName).Using(stream =>
            {
                Ctd.Write(stream);
            });
        }

        private void OpenFontFile(string fileName)
        {
            Fonts = File.OpenRead(fileName).Using(stream =>
            {
                if (!Arc.IsValid(stream))
                    throw new Exception("Not a valid ARC file");

                return FontsArc.Read(stream);
            });
        }
    }
}
