using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.DpdViewer.ViewModels
{
    public class DpdViewModel : BaseNotifyPropertyChanged
    {
        private Dpd dpd;

        private static readonly List<FileDialogFilter> Filters = FileDialogFilterComposer.Compose().AddExtensions("DPD effect", "dpd");

        public DpdViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    using (var stream = File.OpenRead(fileName))
                    {
                        FileName = fileName;
                        Dpd = new Dpd(stream);

                        OnPropertyChanged(nameof(SaveCommand));
                        OnPropertyChanged(nameof(SaveAsCommand));
                    }
                }, Filters);
            }, x => true);

            SaveCommand = new RelayCommand(x =>
            {
                if (!string.IsNullOrEmpty(FileName))
                {
                    using (var stream = File.Open(FileName, FileMode.Create))
                    {
                        throw new NotImplementedException();
                    }
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
                    using (var stream = File.Open(fileName, FileMode.Create))
                    {
                        throw new NotImplementedException();
                    }
                }, Filters);
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

        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        public string FileName { get; set; }

        public RelayCommand OpenCommand { get; set; }

        public RelayCommand SaveCommand { get; set; }

        public RelayCommand SaveAsCommand { get; set; }

        public RelayCommand ExitCommand { get; set; }

        public RelayCommand AboutCommand { get; set; }

        public Dpd Dpd
        {
            get => dpd;
            set
            {
                dpd = value;
                Textures = new TexturesViewModel(dpd.Textures);
                OnPropertyChanged(nameof(Textures));
            }
        }

        public TexturesViewModel Textures { get; private set; }
    }
}
