using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Contextes;
using OpenKh.Tools.Common.Imaging;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.Kh2FontImageEditor.Usecases;
using OpenKh.Tools.Kh2FontImageEditor.UserControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.Kh2FontImageEditor.ViewModels
{
    public class MainWindowVM : BaseNotifyPropertyChanged
    {
        public MainWindowVM(
            ShowErrorMessageUsecase showErrorMessageUsecase,
            ExitAppUsecase exitAppUsecase,
            ReplacePaletteAlphaUsecase replacePaletteAlphaUsecase
        )
        {
            _replacePaletteAlphaUsecase = replacePaletteAlphaUsecase;

            OpenCommand = new RelayCommand(
                _ =>
                {
                    try
                    {
                        FileDialog.OnOpen(fontImageFile =>
                        {
                            LoadFontImage(fontImageFile);
                            _lastOpenedFontImage = fontImageFile;
                            _lastOpenedFontInfo = null;

                            FileDialog.OnOpen(fontInfoFile =>
                            {
                                LoadFontInfo(fontInfoFile);
                                _lastOpenedFontInfo = fontInfoFile;
                            }, OpenFontInfoFilters);

                        }, OpenFontImageFilters);
                    }
                    catch (Exception ex)
                    {
                        showErrorMessageUsecase.Show(ex);
                    }
                }
            );
            SaveCommand = new RelayCommand(
                _ =>
                {
                    try
                    {
                        FileDialog.OnSave(fileName =>
                        {
                            SaveFontImage(fileName);
                            _lastOpenedFontImage = fileName;
                        }, OpenFontImageFilters, defaultFileName: _lastOpenedFontImage);
                    }
                    catch (Exception ex)
                    {
                        showErrorMessageUsecase.Show(ex);
                    }
                }
            );
            SaveAsCommand = new RelayCommand(
                _ =>
                {
                    try
                    {
                        FileDialog.OnSave(fileName =>
                        {
                            SaveFontImage(fileName);
                            _lastOpenedFontImage = fileName;
                        }, OpenFontImageFilters, defaultFileName: _lastOpenedFontImage);
                    }
                    catch (Exception ex)
                    {
                        showErrorMessageUsecase.Show(ex);
                    }
                }
            );
            ExitCommand = new RelayCommand(
                _ =>
                {
                    exitAppUsecase.ExitApp();
                }
            );

            // us icon 24x24 event 24x32 sys 18x24
            // ja icon 24x24 event 24x24 sys 18x18
            System1 = new ImagerModel("System1", true, it => it.ImageSystem, (it, one) => it.ImageSystem = one);
            System2 = new ImagerModel("System2", true, it => it.ImageSystem2, (it, one) => it.ImageSystem2 = one);
            Event1 = new ImagerModel("Event1", true, it => it.ImageEvent, (it, one) => it.ImageEvent = one);
            Event2 = new ImagerModel("Event2", true, it => it.ImageEvent2, (it, one) => it.ImageEvent2 = one);
            Icon = new ImagerModel("Icon", false, it => it.ImageIcon, (it, one) => it.ImageIcon = one);

            ImagerImportCommand = new RelayCommand(
                imager =>
                {
                    try
                    {
                        DoImport((ImagerModel)imager!);
                    }
                    catch (Exception ex)
                    {
                        showErrorMessageUsecase.Show(ex);
                    }
                }
            );
            ImagerExportCommand = new RelayCommand(
                imager =>
                {
                    try
                    {
                        DoExport((ImagerModel)imager!);
                    }
                    catch (Exception ex)
                    {
                        showErrorMessageUsecase.Show(ex);
                    }
                }
            );
            AboutCommand = new RelayCommand(
                _ =>
                {
                    new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
                }
            );
        }

        private void SaveFontImage(string fileName)
        {
            if (_fontContext == null)
            {
                return;
            }

            var work = new MemoryStream();
            Bar.Write(work, _fontContext.WriteFontImage());
            File.WriteAllBytes(fileName, work.ToArray());
        }

        private void DoExport(ImagerModel imager)
        {
            if (_fontContext == null)
            {
                return;
            }

            FileDialog.OnSave(fileName =>
            {
                var temp = new MemoryStream();
                var image = imager.GetImage(_fontContext!);
                Png.Write(
                    temp,
                    imager.IsFontImage
                        ? _replacePaletteAlphaUsecase.ReplacePaletteAlphaWith(image, 255)
                        : image
                );
                File.WriteAllBytes(fileName, temp.ToArray());
            }, OpenPngFilters, defaultFileName: $"{imager.Caption}.png");
        }

        private void DoImport(ImagerModel imager)
        {
            if (_fontContext == null)
            {
                return;
            }

            FileDialog.OnOpen(fileName =>
            {
                DoImport(imager, File.OpenRead(fileName).Using(it => PngImage.Read(it)));
            }, OpenPngFilters, defaultFileName: $"{imager.Caption}.png");
        }

        private void DoImport(ImagerModel imager, PngImage pngImage)
        {
            imager.Image = pngImage.GetBimapSource();
            imager.SetImage(_fontContext!, pngImage);
        }

        private void LoadFontInfo(string fileName)
        {
            _fontContext!.Read(File.OpenRead(fileName).Using(stream => Bar.Read(stream)));
        }

        private void LoadFontImage(string fileName)
        {
            _fontContext = new FontContext();
            _fontContext.Read(File.OpenRead(fileName).Using(stream => Bar.Read(stream)));

            System1.Image = _fontContext.ImageSystem.GetBimapSource();
            System2.Image = _fontContext.ImageSystem2.GetBimapSource();
            Event1.Image = _fontContext.ImageEvent.GetBimapSource();
            Event2.Image = _fontContext.ImageEvent2.GetBimapSource();
            Icon.Image = _fontContext.ImageIcon.GetBimapSource();
        }

        private FontContext? _fontContext = new FontContext();
        private string? _lastOpenedFontImage = null;
        private string? _lastOpenedFontInfo = null;

        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand AboutCommand { get; }

        private readonly ReplacePaletteAlphaUsecase _replacePaletteAlphaUsecase;

        public ICommand ImagerImportCommand { get; }
        public ICommand ImagerExportCommand { get; }

        public ImagerModel System1 { get; }
        public ImagerModel System2 { get; }
        public ImagerModel Event1 { get; }
        public ImagerModel Event2 { get; }
        public ImagerModel Icon { get; }

        public class ImagerModel : BaseNotifyPropertyChanged
        {
            public ImagerModel(
                string caption,
                bool isFontImage,
                Func<FontContext, IImageRead> getImage,
                Action<FontContext, IImageRead> setImage
            )
            {
                Caption = caption;
                IsFontImage = isFontImage;
                GetImage = getImage;
                SetImage = setImage;
            }

            public string Caption { get; }
            public bool IsFontImage { get; }
            public Func<FontContext, IImageRead> GetImage { get; }
            public Action<FontContext, IImageRead> SetImage { get; }

            #region Image property
            private ImageSource? _image;
            public ImageSource? Image
            {
                get => _image;
                set
                {
                    if (_image != value)
                    {
                        _image = value;
                        OnPropertyChanged();
                    }
                }
            }
            #endregion

        }

        private static readonly List<FileDialogFilter> OpenFontImageFilters = FileDialogFilterComposer
            .Compose()
            .AddPatterns("fontimage.bar", new string[] { "fontimage.bar" }.AsEnumerable())
            .ToList()
            .AddAllFiles();

        private static readonly List<FileDialogFilter> OpenFontInfoFilters = FileDialogFilterComposer
            .Compose()
            .AddPatterns("fontinfo.bar", new string[] { "fontinfo.bar" }.AsEnumerable())
            .ToList()
            .AddAllFiles();

        private static readonly List<FileDialogFilter> OpenPngFilters = FileDialogFilterComposer
            .Compose()
            .AddExtensions("png", "png")
            .ToList()
            .AddAllFiles();
    }
}
