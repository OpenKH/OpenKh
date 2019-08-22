using OpenKh.Imaging;
using OpenKh.Tools.Common;
using OpenKh.Tools.ImageViewer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ImageViewer.ViewModels
{
    public class ImageViewerViewModel : BaseNotifyPropertyChanged
    {
        private static readonly IImageFormatService _imageFormatService = new ImageFormatService();
        private static readonly (string, string)[] _filter = _imageFormatService.Formats
            .Select(x => ($"{x.Name} image", x.Extension))
            .Concat(new[] { ("All files", "*") })
            .ToArray();


        public ImageViewerViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, _filter);
                if (fd.ShowDialog() == true)
                {
                    using (var stream = File.OpenRead(fd.FileName))
                    {
                        LoadImage(stream);
                        FileName = fd.FileName;
                    }
                }
            }, x => true);

            SaveCommand = new RelayCommand(x =>
            {
                if (!string.IsNullOrEmpty(FileName))
                {
                    using (var stream = File.Open(FileName, FileMode.Create))
                    {
                        _imageFormat.Write(stream, ImageRead);
                    }
                }
                else
                {
                    SaveAsCommand.Execute(x);
                }
            }, x => ImageRead != null);

            SaveAsCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, _filter);
                fd.DefaultFileName = FileName;

                if (fd.ShowDialog() == true)
                {
                    using (var stream = File.Open(fd.FileName, FileMode.Create))
                    {
                        _imageFormat.Write(stream, ImageRead);
                    }
                }
            }, x => ImageRead != null);

            ExitCommand = new RelayCommand(x =>
            {
                Window.Close();
            }, x => true);

            AboutCommand = new RelayCommand(x =>
            {
                new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);

            ExportCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, FileDialog.Type.ImagePng);
                fd.DefaultFileName = $"{Path.GetFileNameWithoutExtension(FileName)}.png";

                if (fd.ShowDialog() == true)
                {
                    using (var fStream = File.OpenWrite(fd.FileName))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(Image));
                        encoder.Save(fStream);
                    }
                }
            }, x => true);

            ImportCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, FileDialog.Type.ImagePng);
                fd.DefaultFileName = $"{Path.GetFileNameWithoutExtension(FileName)}.png";

                if (fd.ShowDialog() == true)
                {
                    using (var fStream = File.OpenRead(fd.FileName))
                    {
                        throw new NotImplementedException();
                    }
                }
            }, x => false);
        }

        public ImageViewerViewModel(Stream stream) :
            this()
        {
            LoadImage(stream);
        }

        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        private string _fileName;
        private IImageRead _imageRead;
        private IImageFormat _imageFormat;
        private BitmapSource _bitmapSource;

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(SaveCommand));
                OnPropertyChanged(nameof(SaveAsCommand));
            }
        }

        public RelayCommand OpenCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand ExitCommand { get; set; }
        public RelayCommand AboutCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }

        private IImageRead ImageRead
        {
            get => _imageRead;
            set
            {
                _imageRead = value;
                Image = _imageRead.GetBimapSource();
            }
        }

        public BitmapSource Image
        {
            get => _bitmapSource;
            set
            {
                _bitmapSource = value;
                OnPropertyChanged();
            }
        }

        private void LoadImage(Stream stream)
        {
            _imageFormat = _imageFormatService.GetFormatByContent(stream);
            if (_imageFormat == null)
                throw new Exception("Image format not found for the given stream.");

            ImageRead = _imageFormat.Read(stream);
        }
    }
}
