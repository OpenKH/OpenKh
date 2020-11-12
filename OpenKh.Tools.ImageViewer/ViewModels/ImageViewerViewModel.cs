using OpenKh.Command.ImgTool.Utils;
using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Utils;
using OpenKh.Tools.Common;
using OpenKh.Tools.Common.Imaging;
using OpenKh.Tools.ImageViewer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        private static readonly List<FileDialogFilter> ExportToContainerFilters = FileDialogFilterComposer
            .Compose()
            .AddExtensions("All supported images for export", GetAllSupportedExtensions(x => x.IsCreationSupported && x.IsContainer))
            .Concat(_imageFormatService.Formats.Where(x => x.IsCreationSupported && x.IsContainer).Select(x => FileDialogFilter.ByExtensions($"{x.Name} image", x.Extension)))
            .ToList()
            .AddAllFiles();

        private static readonly List<FileDialogFilter> ExportToSingleImageFilters = FileDialogFilterComposer
            .Compose()
            .AddExtensions("All supported images for export", GetAllSupportedExtensions(x => x.IsCreationSupported))
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
                    // Clear current bar entry content before saving.
                    _toolInvokeDesc.SelectedEntry.Stream.SetLength(0);

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
                new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
            }, x => true);

            ExportCurrentCommand = new RelayCommand(x =>
            {
                var singleImage = Image?.Source;
                if (singleImage != null)
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

                        File.OpenWrite(fileName).Using(
                            stream =>
                            {
                                if (imageFormat.IsContainer)
                                {
                                    imageFormat.As<IImageMultiple>().Write(
                                        stream,
                                        new ImageFormatService.ImageContainer(new IImageRead[] { singleImage })
                                    );
                                }
                                else
                                {
                                    imageFormat.As<IImageSingle>().Write(
                                       stream,
                                       singleImage
                                   );
                                }
                            }
                        );
                    }, ExportToSingleImageFilters);
                }
            }, x => true);

            ExportAllCommand = new RelayCommand(x =>
            {
                var multiImages = GetImagesForExport();
                if (multiImages.Any())
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

                        var imageContainer = new ImageFormatService.ImageContainer(multiImages);

                        File.OpenWrite(fileName).Using(stream => imageFormat.As<IImageMultiple>().Write(stream, imageContainer));
                    }, ExportToContainerFilters);
                }
            }, x => true);

            ImportCommand = new RelayCommand(
                parameter =>
                {
                    AddImage(Enum.Parse<AddPosition>($"{parameter}"));
                },
                x => IsMultipleImageFormat
            );

            CreateNewImgzCommand = new RelayCommand(
                x =>
                {
                    if (IsSingleImageFormat || IsMultipleImageFormat)
                    {
                        if (MessageBoxResult.OK != MessageBox.Show("This will discard all of existing images.", null, MessageBoxButton.OKCancel, MessageBoxImage.Exclamation))
                        {
                            return;
                        }
                    }

                    FileName = null;

                    EditImageList(
                        currentImageList =>
                        {
                            var newImage = CreateDummyImage();
                            return new EditResult
                            {
                                imageList = new IImageRead[] { newImage },
                                selection = newImage,
                            };
                        }
                    );
                }
            );

            ConvertToImgzCommand = new RelayCommand(
                x =>
                {
                    FileName = null;

                    EditImageList(
                        currentImageList =>
                        {
                            return new EditResult
                            {
                                imageList = new IImageRead[] { Image.Source },
                                selection = Image.Source
                            };
                        }
                    );
                },
                x => IsSingleImageFormat
            );

            InsertEmptyImageCommand = new RelayCommand(
                x =>
                {
                    AddEmptyImage(AddPosition.BeforeCurrent);
                },
                x => IsMultipleImageFormat
            );

            RemoveImageCommand = new RelayCommand(
                x =>
                {
                    EditImageList(
                        currentImageList =>
                        {
                            return new EditResult
                            {
                                imageList = currentImageList.Except(new IImageRead[] { Image?.Source })
                            };
                        }
                    );
                },
                x => IsMultipleImageFormat && ImageContainer.Count >= 1
            );

            ConvertBitsPerPixelCommand = new RelayCommand(
                parameter =>
                {
                    try
                    {
                        EditImageList(
                            currentImageList =>
                            {
                                var sourceImage = Image.Source;

                                var bpp = Convert.ToInt32(parameter);

                                var newImage = ImgdBitmapUtil.ToImgd(
                                    sourceImage.CreateBitmap(),
                                    bpp,
                                    QuantizerFactory.MakeFrom(
                                        bpp,
                                        UsePngquant ?? false
                                    )
                                );

                                return new EditResult
                                {
                                    imageList = currentImageList
                                        .Select(it => ReferenceEquals(it, sourceImage) ? newImage : it),

                                    selection = newImage,
                                };
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                },
                x => IsMultipleImageFormat && ImageContainer.Count >= 1
            );

            ZoomLevel = ZoomLevelFit;
        }

        public bool IsMultipleImageFormat => _imageFormat != null ? _imageFormat.IsContainer : false;
        public bool IsSingleImageFormat => _imageFormat != null ? !_imageFormat.IsContainer : false;

        public bool? UsePngquant { get; set; }


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
        public RelayCommand ExportCurrentCommand { get; set; }
        public RelayCommand ExportAllCommand { get; set; }
        public RelayCommand ImportCommand { get; set; }
        public RelayCommand CreateNewImgzCommand { get; }
        public RelayCommand ConvertToImgzCommand { get; private set; }
        public RelayCommand InsertEmptyImageCommand { get; private set; }
        public RelayCommand RemoveImageCommand { get; private set; }
        public RelayCommand ConvertBitsPerPixelCommand { get; private set; }
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

        private IImageRead CreateDummyImage()
            => Imgd.Create(
                new System.Drawing.Size(128, 128),
                PixelFormat.Indexed8,
                new byte[128 * 128],
                Enumerable.Repeat((byte)0x80, 4 * 256).ToArray(), // gray color palette
                false
            );

        class EditResult
        {
            internal IEnumerable<IImageRead> imageList;
            internal IImageRead selection;
        }

        private void EditImageList(Func<List<IImageRead>, EditResult> editor)
        {
            var currentImageList = (ImageContainer?.Images.ToList()) ?? new List<IImageRead>();
            var currentIndex = currentImageList.IndexOf(Image?.Source);

            var editResult = editor(currentImageList);

            ImageContainer = new ImageFormatService.ImageContainer(editResult.imageList);

            if (ImageContainerItems.Any())
            {
                if (editResult.selection != null)
                {
                    Image = ImageContainerItems
                        .FirstOrDefault(it => ReferenceEquals(editResult.selection, it.Source));
                }
                else
                {
                    Image = ImageContainerItems
                        .Skip(currentIndex)
                        .FirstOrDefault() ?? ImageContainerItems.Last();
                }
            }
            else
            {
                Image = null;
            }

            ImageFormat = _imageFormatService.Formats.Single(it => it.Name == "IMGZ");
        }

        enum AddPosition
        {
            BeforeCurrent,
            ReplaceCurrent,
            AfterCurrent,
            Last,
        }

        private void AddEmptyImage(AddPosition addPosition)
        {
            EditImageList(
                imageList =>
                {
                    var incomingImages = new IImageRead[] { CreateDummyImage() };
                    var position = Math.Max(0, imageList.IndexOf(Image?.Source));

                    switch (addPosition)
                    {
                        case AddPosition.BeforeCurrent:
                            imageList.InsertRange(position, incomingImages);
                            break;
                        case AddPosition.AfterCurrent:
                            imageList.InsertRange(position + 1, incomingImages);
                            break;
                        case AddPosition.Last:
                        default:
                            imageList.AddRange(incomingImages);
                            break;
                    }

                    return new EditResult
                    {
                        imageList = imageList,
                        selection = incomingImages.LastOrDefault(),
                    };
                }
            );
        }

        private void AddImage(AddPosition addPosition)
        {
            FileDialog.OnOpen(fileName =>
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var imageFormat = _imageFormatService.GetFormatByContent(stream);
                    if (imageFormat == null)
                        throw new Exception("Image format not found for the given stream.");

                    var incomingImages = imageFormat.IsContainer
                        ? imageFormat.As<IImageMultiple>().Read(stream).Images
                        : new IImageRead[] { imageFormat.As<IImageSingle>().Read(stream) };

                    EditImageList(
                        imageList =>
                        {
                            var position = Math.Max(0, imageList.IndexOf(Image?.Source));

                            var selectedImage = Image?.Source;

                            switch (addPosition)
                            {
                                case AddPosition.BeforeCurrent:
                                case AddPosition.ReplaceCurrent:
                                    imageList.InsertRange(position, incomingImages);
                                    break;
                                case AddPosition.AfterCurrent:
                                    imageList.InsertRange(position + 1, incomingImages);
                                    break;
                                case AddPosition.Last:
                                default:
                                    imageList.AddRange(incomingImages);
                                    break;
                            }

                            if (addPosition == AddPosition.ReplaceCurrent)
                            {
                                imageList.Remove(selectedImage);
                            }

                            return new EditResult
                            {
                                imageList = imageList,
                                selection = incomingImages.LastOrDefault(),
                            };
                        }
                    );
                }
            }, OpenFilters);
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

        private static string GetAllSupportedExtensions(Func<IImageFormat, bool> filter = null)
        {
            filter = filter ?? (it => true);
            var extensions = _imageFormatService.Formats.Where(filter).Select(x => $"*.{x.Extension}");
            return string.Join(";", extensions).Substring(2);
        }

        class RunCmd
        {
            private Process p;

            public string App => p.StartInfo.FileName;
            public int ExitCode => p.ExitCode;

            public RunCmd(string app, string arg)
            {
                var psi = new ProcessStartInfo(app, arg)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                var p = Process.Start(psi);
                p.WaitForExit();
                this.p = p;
            }
        }

        private IEnumerable<IImageRead> GetImagesForExport()
        {
            if (ImageContainer != null)
            {
                // currently container (multiple images) format
                return ImageContainer.Images.ToArray();
            }
            if (Image?.Source != null)
            {
                // currently single image format
                return new IImageRead[] { Image.Source };
            }
            // no image loaded
            return new IImageRead[0];
        }
    }
}
