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
            ShowErrorMessageUsecase showErrorMessageUsecase
        )
        {
            OpenCommand = new RelayCommand(
                _ =>
                {
                    try
                    {
                        FileDialog.OnOpen(fileName =>
                        {
                            LoadFontImage(fileName);
                            _lastOpenedFile = fileName;
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
                            _lastOpenedFile = fileName;
                        }, OpenFontImageFilters, defaultFileName: _lastOpenedFile);
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
                            _lastOpenedFile = fileName;
                        }, OpenFontImageFilters, defaultFileName: _lastOpenedFile);
                    }
                    catch (Exception ex)
                    {
                        showErrorMessageUsecase.Show(ex);
                    }
                }
            );

            System1 = new ImagerModel("System1", it => it.ImageSystem, (it, one) => it.ImageSystem = one);
            System2 = new ImagerModel("System2", it => it.ImageSystem2, (it, one) => it.ImageSystem2 = one);
            Event1 = new ImagerModel("Event1", it => it.ImageEvent, (it, one) => it.ImageEvent = one);
            Event2 = new ImagerModel("Event2", it => it.ImageEvent2, (it, one) => it.ImageEvent2 = one);
            Icon = new ImagerModel("Icon", it => it.ImageIcon, (it, one) => it.ImageIcon = one);

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
                Png.Write(temp, imager.GetImage(_fontContext!)); //TODO use PngImage
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
        private string? _lastOpenedFile = null;

        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand AboutCommand { get; }

        public ICommand ImagerImportCommand { get; }
        public ICommand ImagerExportCommand { get; }

        public ImagerModel System1 { get; }
        public ImagerModel System2 { get; }
        public ImagerModel Event1 { get; }
        public ImagerModel Event2 { get; }
        public ImagerModel Icon { get; }

        public class ImagerModel : BaseNotifyPropertyChanged
        {
            public ImagerModel(string caption, Func<FontContext, IImageRead> getImage, Action<FontContext, IImageRead> setImage)
            {
                Caption = caption;
                GetImage = getImage;
                SetImage = setImage;
            }

            public string Caption { get; }
            internal Func<FontContext, IImageRead> GetImage { get; }
            internal Action<FontContext, IImageRead> SetImage { get; }

            #region Image
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

        private static readonly List<FileDialogFilter> OpenPngFilters = FileDialogFilterComposer
            .Compose()
            .AddExtensions("png", "png")
            .ToList()
            .AddAllFiles();
    }
}
