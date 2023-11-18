using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Contextes;
using OpenKh.Tools.Common.Imaging;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.Kh2FontImageEditor.Helpers;
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.Kh2FontImageEditor.Views
{
    public class MainWindowVM : BaseNotifyPropertyChanged
    {
        public MainWindowVM(
            ShowErrorMessageUsecase showErrorMessageUsecase,
            ExitAppUsecase exitAppUsecase,
            ConvertFontImageUsecase convertFontImageUsecase,
            CombineBarUsecase combineBarUsecase,
            ConvertFontDataUsecase convertFontDataUsecase,
            ReplacePaletteAlphaUsecase replacePaletteAlphaUsecase,
            Func<SpacingWindow> newSpacingWindow,
            CopyArrayUsecase copyArrayUsecase,
            ApplySpacingToImageReadUsecase applySpacingToImageReadUsecase,
            CreateGlyphCellsUsecase createGlyphCellsUsecase
        )
        {
            _createGlyphCellsUsecase = createGlyphCellsUsecase;
            _applySpacingToImageReadUsecase = applySpacingToImageReadUsecase;
            _copyArrayUsecase = copyArrayUsecase;
            _replacePaletteAlphaUsecase = replacePaletteAlphaUsecase;
            _convertFontDataUsecase = convertFontDataUsecase;
            _combineBarUsecase = combineBarUsecase;
            _convertFontImageUsecase = convertFontImageUsecase;

            void ApplyFontMetrics(
                int sysSpriteWidth, int sysSpriteHeight,
                int evtSpriteWidth, int evtSpriteHeight,
                int sysGlyphWidth, int sysGlyphHeight,
                int evtGlyphWidth, int evtGlyphHeight)
            {
                var iconSpriteWidth = 256;
                var iconSpriteHeight = 160;

                {
                    var cells = _createGlyphCellsUsecase.CreateFontGlyphCells(
                        sysSpriteWidth,
                        sysSpriteHeight,
                        256,
                        true,
                        sysGlyphWidth,
                        sysGlyphHeight
                    );

                    System1.State = System1.State with
                    {
                        GlyphSize = new System.Drawing.Size(
                            sysGlyphWidth,
                            sysGlyphHeight
                        ),
                        GlyphCells = cells,
                    };
                }

                {
                    var cells = _createGlyphCellsUsecase.CreateFontGlyphCells(
                        sysSpriteWidth,
                        sysSpriteHeight,
                        256,
                        false,
                        sysGlyphWidth,
                        sysGlyphHeight
                    );

                    System2.State = System2.State with
                    {
                        GlyphSize = new System.Drawing.Size(
                            sysGlyphWidth,
                            sysGlyphHeight
                        ),
                        GlyphCells = cells,
                    };
                }

                {
                    var cells = _createGlyphCellsUsecase.CreateFontGlyphCells(
                        evtSpriteWidth,
                        evtSpriteHeight,
                        512,
                        true,
                        evtGlyphWidth,
                        evtGlyphHeight
                    );

                    Event1.State = Event1.State with
                    {
                        GlyphSize = new System.Drawing.Size(
                            evtGlyphWidth,
                            evtGlyphHeight
                        ),
                        GlyphCells = cells,
                    };
                }

                {
                    var cells = _createGlyphCellsUsecase.CreateFontGlyphCells(
                        evtSpriteWidth,
                        evtSpriteHeight,
                        512,
                        false,
                        evtGlyphWidth,
                        evtGlyphHeight
                    );

                    Event2.State = Event2.State with
                    {
                        GlyphSize = new System.Drawing.Size(
                            evtGlyphWidth,
                            evtGlyphHeight
                        ),
                        GlyphCells = cells,
                    };
                }

                {
                    var cells = _createGlyphCellsUsecase.CreateSimpleGlyphCells(
                        iconSpriteWidth,
                        iconSpriteHeight,
                        Constants.FontIconWidth,
                        Constants.FontIconHeight
                    );

                    Icon1.State = Icon1.State with
                    {
                        GlyphSize = new System.Drawing.Size(
                            Constants.FontIconWidth,
                            Constants.FontIconHeight
                        ),
                        GlyphCells = cells,
                    };
                }
            }

            OpenFontImageCommand = new RelayCommand(
                _ =>
                {
                    try
                    {
                        FileDialog.OnOpen(fontImageFile =>
                        {
                            _fontImageBar = File.OpenRead(fontImageFile).Using(stream => Bar.Read(stream));

                            _fontImageData = _convertFontImageUsecase.Decode(_fontImageBar);

                            System1!.State = System1.State with { Image = _fontImageData.ImageSystem?.GetBimapSource() };
                            System2!.State = System2.State with { Image = _fontImageData.ImageSystem2?.GetBimapSource() };
                            Event1!.State = Event1.State with { Image = _fontImageData.ImageEvent?.GetBimapSource() };
                            Event2!.State = Event2.State with { Image = _fontImageData.ImageEvent2?.GetBimapSource() };
                            Icon1!.State = Icon1.State with { Image = _fontImageData.ImageIcon?.GetBimapSource() };

                            _lastOpenedFontImage = fontImageFile;

                            switch (MessageBox.Show("FontEuropean?", "", MessageBoxButton.YesNoCancel))
                            {
                                case MessageBoxResult.Yes:
                                    ApplyFontMetrics(
                                        512, 512,
                                        512, 512,
                                        Constants.FontEuropeanSystemWidth,
                                        Constants.FontEuropeanSystemHeight,
                                        Constants.FontEuropeanEventWidth,
                                        Constants.FontEuropeanEventHeight
                                    );
                                    break;

                                case MessageBoxResult.No:
                                    ApplyFontMetrics(
                                        512, 512,
                                        512, 1024,
                                        Constants.FontJapaneseSystemWidth,
                                        Constants.FontJapaneseSystemHeight,
                                        Constants.FontJapaneseEventWidth,
                                        Constants.FontJapaneseEventHeight
                                    );

                                    break;
                            }
                        }, OpenFontImageFilters);
                    }
                    catch (Exception ex)
                    {
                        showErrorMessageUsecase.Show(ex);
                    }
                }
            );
            OpenFontInfoCommand = new RelayCommand(
                _ =>
                {
                    try
                    {
                        FileDialog.OnOpen(fontInfoFile =>
                        {
                            _fontInfoBar = File.OpenRead(fontInfoFile).Using(stream => Bar.Read(stream));

                            _fontInfoData = _convertFontDataUsecase.Decode(_fontInfoBar);

                            _lastOpenedFontInfo = fontInfoFile;
                        }, OpenFontInfoFilters);
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

            System1 = new ImagerModel(
                caption: "System1",
                isFontImage: true,
                getImage: it => it.ImageSystem,
                setImage: (it, one) => it with { ImageSystem = one },
                getSpacing: it => it.System,
                setSpacing: (it, one) => it with { System = one },
                state: new ImagerState()
            );
            System2 = new ImagerModel(
                caption: "System2",
                isFontImage: true,
                getImage: it => it.ImageSystem2,
                setImage: (it, one) => it with { ImageSystem2 = one },
                getSpacing: it => it.System,
                setSpacing: (it, one) => it with { System = one },
                state: new ImagerState()
            );
            Event1 = new ImagerModel(
                caption: "Event1",
                isFontImage: true,
                getImage: it => it.ImageEvent,
                setImage: (it, one) => it with { ImageEvent = one },
                getSpacing: it => it.Event,
                setSpacing: (it, one) => it with { Event = one },
                state: new ImagerState()
            );
            Event2 = new ImagerModel(
                caption: "Event2",
                isFontImage: true,
                getImage: it => it.ImageEvent2,
                setImage: (it, one) => it with { ImageEvent2 = one },
                getSpacing: it => it.Event,
                setSpacing: (it, one) => it with { Event = one },
                state: new ImagerState()
            );
            Icon1 = new ImagerModel(
                caption: "Icon",
                isFontImage: false,
                getImage: it => it.ImageIcon,
                setImage: (it, one) => it with { ImageIcon = one },
                getSpacing: it => it.Icon,
                setSpacing: (it, one) => it with { Icon = one },
                state: new ImagerState()
            );

            ImagerImportCommand = new RelayCommand(
                any =>
                {
                    try
                    {
                        var imager = any as ImagerModel ?? throw new NullReferenceException();

                        FileDialog.OnOpen(
                            fileName =>
                            {
                                var pngImage = File.OpenRead(fileName).Using(it => PngImage.Read(it));
                                imager.State = imager.State with { Image = pngImage.GetBimapSource() };
                                _fontImageData = imager.SetImage(_fontImageData, pngImage);
                            },
                            OpenPngFilters,
                            defaultFileName: $"{imager.Caption}.png"
                        );
                    }
                    catch (Exception ex)
                    {
                        showErrorMessageUsecase.Show(ex);
                    }
                }
            );
            ImagerExportCommand = new RelayCommand(
                any =>
                {
                    try
                    {
                        var imager = any as ImagerModel ?? throw new NullReferenceException();

                        var image = imager.GetImage(_fontImageData);
                        if (image != null)
                        {
                            FileDialog.OnSave(
                                fileName =>
                                {
                                    var temp = new MemoryStream();
                                    Png.Write(
                                        temp,
                                        imager.IsFontImage
                                            ? _replacePaletteAlphaUsecase.ReplacePaletteAlphaWith(image, 255)
                                            : image
                                    );
                                    File.WriteAllBytes(fileName, temp.ToArray());
                                },
                                OpenPngFilters,
                                defaultFileName: $"{imager.Caption}.png"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        showErrorMessageUsecase.Show(ex);
                    }
                }
            );
            ImagerEditSpacingCommand = new RelayCommand(
                any =>
                {
                    try
                    {
                        var imager = any as ImagerModel ?? throw new NullReferenceException();

                        var image = imager.GetImage(_fontImageData);
                        if (true
                            && image != null
                            && imager.State.GlyphSize is System.Drawing.Size glyphSize
                            && imager.State.GlyphCells is GlyphCell[] glyphCells
                        )
                        {
                            var spacingSource = imager.GetSpacing(_fontInfoData);
                            if (spacingSource != null)
                            {
                                var spacing = _copyArrayUsecase.Copy(spacingSource);

                                ImageSource CreateImage()
                                {
                                    if (imager.IsFontImage)
                                    {
                                        return _applySpacingToImageReadUsecase.ApplyToIndexed4(
                                            image,
                                            index => (index < spacing.Length) ? spacing[index] : (byte)0,
                                            glyphCells
                                        )
                                            .GetBimapSource();
                                    }
                                    else
                                    {
                                        return _applySpacingToImageReadUsecase.ApplyToFullColored(
                                            image,
                                            index => (index < spacing.Length) ? spacing[index] : (byte)0,
                                            glyphCells
                                        )
                                            .GetBimapSource();
                                    }
                                }

                                Action refreshImage = () => { };

                                var window = newSpacingWindow();
                                window.Show();
                                var vm = window.DataContext as SpacingWindowVM ?? throw new NullReferenceException();
                                var state = new SpacingWindowVM.StateModel(
                                    CreateImage(),
                                    image.Size.Width,
                                    image.Size.Height,
                                    new RelayCommand(
                                        _ =>
                                        {
                                            _fontInfoData = imager.SetSpacing(_fontInfoData, spacing);
                                            window.Close();
                                        }
                                    ),
                                    (x, y, delta) =>
                                    {
                                        var hit = glyphCells.FirstOrDefault(it => it.Cell.Contains(x, y));
                                        if (hit != null && hit.SpacingIndex < spacing.Length)
                                        {
                                            spacing[hit.SpacingIndex] =
                                                (byte)Math.Max(
                                                    0,
                                                    Math.Min(
                                                        glyphSize.Width,
                                                        spacing[hit.SpacingIndex] + delta
                                                    )
                                                );
                                            refreshImage();
                                        }
                                    }
                                );
                                vm.State = state;

                                refreshImage = () =>
                                {
                                    vm.State = state with { Image = CreateImage() };
                                };
                            }
                        }
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
            var work = new MemoryStream();
            Bar.Write(
                work,
                _fontImageBar = _combineBarUsecase.Combine(
                    _fontImageBar,
                    _convertFontImageUsecase.Encode(_fontImageData)
                )
            );
            File.WriteAllBytes(fileName, work.ToArray());
        }

        private string? _lastOpenedFontImage = null;
        private string? _lastOpenedFontInfo = null;
        private IEnumerable<Bar.Entry> _fontImageBar = Array.Empty<Bar.Entry>();
        private FontImageData _fontImageData = new FontImageData(null, null, null, null, null);
        private IEnumerable<Bar.Entry> _fontInfoBar = Array.Empty<Bar.Entry>();
        private FontInfoData _fontInfoData = new FontInfoData(null, null, null);

        public ICommand OpenFontImageCommand { get; }
        public ICommand OpenFontInfoCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand AboutCommand { get; }

        private readonly CreateGlyphCellsUsecase _createGlyphCellsUsecase;
        private readonly ApplySpacingToImageReadUsecase _applySpacingToImageReadUsecase;
        private readonly CopyArrayUsecase _copyArrayUsecase;
        private readonly ReplacePaletteAlphaUsecase _replacePaletteAlphaUsecase;
        private readonly ConvertFontDataUsecase _convertFontDataUsecase;
        private readonly CombineBarUsecase _combineBarUsecase;
        private readonly ConvertFontImageUsecase _convertFontImageUsecase;

        public ICommand ImagerImportCommand { get; }
        public ICommand ImagerExportCommand { get; }
        public ICommand ImagerEditSpacingCommand { get; }

        public ImagerModel System1 { get; }
        public ImagerModel System2 { get; }
        public ImagerModel Event1 { get; }
        public ImagerModel Event2 { get; }
        public ImagerModel Icon1 { get; }

        public record ImagerState(
            ImageSource? Image = null,
            System.Drawing.Size? GlyphSize = null,
            GlyphCell[]? GlyphCells = null
        );

        public class ImagerModel : BaseNotifyPropertyChanged
        {
            public ImagerModel(
                string caption,
                bool isFontImage,
                Func<FontImageData, IImageRead?> getImage,
                Func<FontImageData, IImageRead, FontImageData> setImage,
                Func<FontInfoData, byte[]?> getSpacing,
                Func<FontInfoData, byte[]?, FontInfoData> setSpacing,
                ImagerState state
            )
            {
                Caption = caption;
                IsFontImage = isFontImage;
                GetImage = getImage;
                SetImage = setImage;
                GetSpacing = getSpacing;
                SetSpacing = setSpacing;
                _state = state;
            }

            public string Caption { get; }
            public bool IsFontImage { get; }
            public Func<FontImageData, IImageRead?> GetImage { get; }
            public Func<FontImageData, IImageRead, FontImageData> SetImage { get; }
            public Func<FontInfoData, byte[]?> GetSpacing { get; }
            public Func<FontInfoData, byte[]?, FontInfoData> SetSpacing { get; }

            #region State property
            private ImagerState _state;
            public ImagerState State
            {
                get => _state;
                set
                {
                    if (_state != value)
                    {
                        _state = value;
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
