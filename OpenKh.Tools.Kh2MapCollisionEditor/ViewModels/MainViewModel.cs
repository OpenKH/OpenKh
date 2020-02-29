using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.Common;
using OpenKh.Tools.Kh2MapCollisionEditor.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.Kh2MapCollisionEditor.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private static string ApplicationName = Utilities.GetApplicationName();
        private static readonly IEnumerable<FileDialogFilter> CtdFilter = FileDialogFilterComposer.Compose()
            .AddExtensions("COCT file", "coct")
            .AddAllFiles();
        private Window Window => Utilities.GetCurrentWindow();

        private string _fileName;
        private bool _isProcess;
        private ProcessStream _processStream;

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
        public RelayCommand OpenProcessCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand AboutCommand { get; }

        public Coct Coct { get; set; }

        public MainViewModel()
        {
            OpenCommand = new RelayCommand(x =>
                FileDialog.OnOpen(fileName => OpenFile(fileName), CtdFilter), x => true);

            OpenProcessCommand = new RelayCommand(x => OpenProcess(), x => true);

            SaveCommand = new RelayCommand(x =>
            {
                if (_isProcess)
                {
                    SaveStream(_processStream);
                }
                else
                {
                    if (!string.IsNullOrEmpty(FileName))
                        SaveFile(FileName, FileName);
                    else
                        SaveAsCommand.Execute(x);
                }
            }, x => true);

            SaveAsCommand = new RelayCommand(x =>
                FileDialog.OnSave(fileName => SaveFile(FileName, fileName), CtdFilter), x => true);

            ExitCommand = new RelayCommand(x =>
            {
                Window.Close();
            }, x => true);

            AboutCommand = new RelayCommand(x =>
            {
                new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);
        }

        private void OpenFile(string fileName) =>
            File.OpenRead(fileName).Using(stream =>
            {
                if (OpenStream(stream))
                {
                    _isProcess = false;
                    FileName = fileName;
                }
            });

        private bool OpenStream(Stream stream)
        {
            if (!Coct.IsValid(stream))
            {
                Utilities.ShowError("Not a valid COCT file.");
                return false;
            }

            Coct = Coct.Read(stream);
            return true;
        }

        private void SaveFile(string oldFileName, string fileName)
        {
            File.Create(fileName).Using(SaveStream);
            FileName = fileName;
        }

        private void SaveStream(Stream stream) => Coct.Write(stream);

        private void OpenProcess()
        {
            var dialog = new OpenProcessDialog();
            if (dialog.ShowDialog() == true)
            {
                var stream = dialog.SelectedProcessStream;
                if (OpenStream(stream))
                {
                    _isProcess = true;
                    _processStream = stream;
                    FileName = $"PCSX2 ({stream.BaseAddress:X08})";
                }
            }
        }
    }
}
