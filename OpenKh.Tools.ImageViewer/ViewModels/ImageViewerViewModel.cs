using OpenKh.Common;
using OpenKh.Tools.Common;
using OpenKh.Tools.ImageViewer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ImageViewer.ViewModels
{
    public class ImageViewerViewModel : BaseNotifyPropertyChanged
    {
        private const int ZoomLevelFit = -1;
        private static readonly IImageFormatService _imageFormatService = new ImageFormatService();
        private static readonly List<FileDialogFilter> OpenFilters = FileDialogFilterComposer
            .Compose()
            .AddExtensions("All supported images", GetAllSupportedExtensions())
            .Concat(_imageFormatService.Formats.Select(x => FileDialogFilter.ByExtensions($"{x.Name} image", x.Extension)))
            .ToList()
            .AddAllFiles();

        private static readonly List<FileDialogFilter> ExportFilters = FileDialogFilterComposer
            .Compose()
            .AddExtensions("All supported images for export", GetAllSupportedExtensions())
            .Concat(_imageFormatService.Formats.Where(x => x.IsCreationSupported).Select(x => FileDialogFilter.ByExtensions($"{x.Name} image", x.Extension)))
            .ToList()
            .AddAllFiles();

        private static string ApplicationName = Utilities.GetApplicationName();
        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

        public string Title
        {
            get
            {
                var fileName = IsTool ? _toolInvokeDesc.Title : (Path.GetFileName(FileName) ?? "untitled");

                return $"{fileName} | {ApplicationName}";
            }
        }

        public bool IsTool => _toolInvokeDesc != null;

        public ImageViewerViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                FileDialog.OnOpen(fileName =>
                {
                    LoadImage(fileName);
                }, OpenFilters);
            }, x => !IsTool);

            SaveCommand = new RelayCommand(x =>
            {
                if (IsTool)
                {
                    Save(_toolInvokeDesc.SelectedEntry.Stream);
                }
                else if (!string.IsNullOrEmpty(FileName))
                {
                    using (var stream = File.Open(FileName, FileMode.Create))
                    {
                        Save(stream);
                    }
                }
                else
                {
                    SaveAsCommand.Execute(x);
                }
            }, x => ImageFormat != null);

            SaveAsCommand = new RelayCommand(x =>
            {
                var filter = new List<FileDialogFilter>().AddExtensions($"{ImageFormat.Name} format", ImageFormat.Extension);
                FileDialog.OnSave(fileName =>
                {
                    using (var stream = File.Open(fileName, FileMode.Create))
                    {
                        Save(stream);
                    }
                }, filter);
            }, x => ImageFormat != null && !IsTool);

            ExitCommand = new RelayCommand(x =>
            {
                Window.Close();
            }, x => true);

            AboutCommand = new RelayCommand(x =>
            {
                new OpenKh.Tools.Common.Dialogs.AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);

            ExportCommand = new RelayCommand(x =>
            {
                FileDialog.OnSave(fileName =>
                {
                    var imageFormat = _imageFormatService.GetFormatByFileName(fileName);
                    if (imageFormat == null)
                    {
                        var extension = Path.GetExtension(fileName);
                        MessageBox.Show($"The format with extension {extension} is not supported for export.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    File.OpenWrite(fileName).Using(stream => Save(stream, imageFormat));
                }, ExportFilters);
            }, x => true);

            ImportCommand = new RelayCommand(x =>
            {
                var defaultFileName = $"{Path.GetFileNameWithoutExtension(FileName)}.png";
                FileDialog.OnOpen(fileName =>
                {
                    using (var fStream = File.OpenRead(fileName))
                    {
                        throw new NotImplementedException();
                    }
                }, new List<FileDialogFilter>().AddExtensions("PNG image", "png"));
            }, x => false);

            ZoomLevel = ZoomLevelFit;
        }

        public ImageViewerViewModel(Stream stream) :
            this()
        {
            LoadImage(stream);
        }

        private string _fileName;
        private IImageFormat _imageFormat;
        private IImageContainer _imageContainer;
        private double _zoomLevel;
        private bool _zoomFit;
        private ImageViewModel _imageViewModel;
        private ImageContainerViewModel _imageContainerItems;
        private ToolInvokeDesc _toolInvokeDesc;

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public RelayCommand OpenCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand ExitCommand { get; set; }
        public RelayCommand AboutCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }

        public IEnumerable<KeyValuePair<string, double>> ZoomLevels { get; } =
            new double[]
            {
                0.25, 0.33, 0.5, 0.75, 1, 1.25, 1.50, 1.75, 2, 2.5, 3, 4, 6, 8, 12, 16, 1
            }
            .Select(x => new KeyValuePair<string, double>($"{x * 100.0}%", x))
            .Concat(new KeyValuePair<string, double>[]
            {
                new KeyValuePair<string, double>("Fit", ZoomLevelFit)
            })
            .ToArray();

        private IImageFormat ImageFormat
        {
            get => _imageFormat;
            set
            {
                _imageFormat = value;
                OnPropertyChanged(nameof(ImageType));
                OnPropertyChanged(nameof(ImageMultiple));
                OnPropertyChanged(nameof(ImageSelectionVisibility));
                OnPropertyChanged(nameof(SaveCommand));
                OnPropertyChanged(nameof(SaveAsCommand));
            }
        }

        private IImageContainer ImageContainer
        {
            get => _imageContainer;
            set
            {
                _imageContainer = value;
                ImageContainerItems = new ImageContainerViewModel(_imageContainer);
            }
        }

        public ImageViewModel Image
        {
            get => _imageViewModel;
            set
            {
                _imageViewModel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImageZoomWidth));
                OnPropertyChanged(nameof(ImageZoomHeight));
            }
        }

        public ImageContainerViewModel ImageContainerItems
        {
            get => _imageContainerItems;
            set
            {
                _imageContainerItems = value;
                OnPropertyChanged();
            }
        }

        public string ImageType => _imageFormat?.Name ?? "Unknown";
        public string ImageMultiple => _imageFormat != null ? _imageFormat.IsContainer ? "Multiple" : "Single" : null;
        public Visibility ImageSelectionVisibility => (_imageFormat?.IsContainer ?? false) ? Visibility.Visible : Visibility.Collapsed;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                _zoomLevel = value;
                ZoomFit = _zoomLevel <= 0;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImageZoomWidth));
                OnPropertyChanged(nameof(ImageZoomHeight));
            }
        }

        public bool ZoomFit
        {
            get => _zoomFit;
            set
            {
                _zoomFit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImageFitVisibility));
                OnPropertyChanged(nameof(ImageCustomZoomVisibility));
            }
        }

        public double ImageZoomWidth => (Image?.Width * ZoomLevel) ?? 0;
        public double ImageZoomHeight => (Image?.Height * ZoomLevel) ?? 0;

        public Visibility ImageFitVisibility => ZoomFit ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ImageCustomZoomVisibility => ZoomFit ? Visibility.Collapsed : Visibility.Visible;

        public void LoadImage(ToolInvokeDesc toolInvokeDesc)
        {
            _toolInvokeDesc = toolInvokeDesc;
            LoadImage(_toolInvokeDesc.SelectedEntry.Stream);
        }

        public void LoadImage(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                LoadImage(stream);
                FileName = fileName;
            }
        }

        private void LoadImage(Stream stream)
        {
            var imageFormat = _imageFormatService.GetFormatByContent(stream);
            if (imageFormat == null)
                throw new Exception("Image format not found for the given stream.");

            ImageFormat = imageFormat;

            if (ImageFormat.IsContainer)
            {
                ImageContainer = _imageFormat.As<IImageMultiple>().Read(stream);
                Image = ImageContainerItems.First();
            }
            else
            {
                Image = new ImageViewModel(_imageFormat.As<IImageSingle>().Read(stream));
            }
        }

        public void Save(Stream stream) => Save(stream, ImageFormat);

        public void Save(Stream stream, IImageFormat imageFormat)
        {
            if (imageFormat.IsContainer)
            {
                imageFormat.As<IImageMultiple>().Write(stream, ImageContainer);
            }
            else
            {
                imageFormat.As<IImageSingle>().Write(stream, Image.Source);
            }
        }

        private static string GetAllSupportedExtensions()
        {
            var extensions = _imageFormatService.Formats.Select(x => $"*.{x.Extension}");
            return string.Join(";", extensions).Substring(2);
        }
    }
}
