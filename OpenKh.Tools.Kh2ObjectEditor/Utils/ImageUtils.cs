using OpenKh.Command.TexFooter.Models;
using OpenKh.Command.TexFooter.Utils;
using OpenKh.Kh2;
using OpenKh.Kh2.TextureFooter;
using OpenKh.Kh2.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Kh2ObjectEditor.Utils
{
    public class ImageUtils
    {
        public static Imgd loadPngFileAsImgd()
        {
            Imgd returnImgd = null;

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "PNG file | *.png";
            bool? success = openFileDialog.ShowDialog();

            if (success == true)
            {
                if (Directory.Exists(openFileDialog.FileName))
                {
                    return null;
                }
                else if (File.Exists(openFileDialog.FileName))
                {
                    returnImgd = ImageUtils.pngToImgd(openFileDialog.FileName);
                }
            }

            return returnImgd;
        }

        public static TextureAnimation loadPngFileAsTextureAnimation()
        {
            TextureAnimation returnTextureAnimation = null;

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "PNG file | *.png";
            bool? success = openFileDialog.ShowDialog();

            if (success == true)
            {
                if (Directory.Exists(openFileDialog.FileName))
                {
                    return null;
                }
                else if (File.Exists(openFileDialog.FileName))
                {
                    returnTextureAnimation = CreateTextureAnimation(openFileDialog.FileName);
                }
            }

            return returnTextureAnimation;
        }

        /*private static readonly IImageFormatService _imageFormatService = new ImageFormatService();

        private IImageFormat _imageFormat;
        private IImageContainer _imageContainer;
        private ImageContainerViewModel _imageContainerItems;
        private ImageViewModel _imageViewModel;
        private IImageFormat ImageFormat
        {
            get => _imageFormat;
            set
            {
                _imageFormat = value;
                //OnPropertyChanged(nameof(ImageType));
                //OnPropertyChanged(nameof(ImageMultiple));
                //OnPropertyChanged(nameof(ImageSelectionVisibility));
                //OnPropertyChanged(nameof(SaveCommand));
                //OnPropertyChanged(nameof(SaveAsCommand));
            }
        }

        public void LoadImage(Stream stream)
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
        private IImageContainer ImageContainer
        {
            get => _imageContainer;
            set
            {
                _imageContainer = value;
                ImageContainerItems = new ImageContainerViewModel(_imageContainer);
            }
        }
        public ImageContainerViewModel ImageContainerItems
        {
            get => _imageContainerItems;
            set
            {
                _imageContainerItems = value;
                //OnPropertyChanged();
            }
        }
        public ImageViewModel Image
        {
            get => _imageViewModel;
            set
            {
                _imageViewModel = value;
                //OnPropertyChanged();
                //OnPropertyChanged(nameof(ImageZoomWidth));
                //OnPropertyChanged(nameof(ImageZoomHeight));
            }
        }




        // NOTE: No TextFooter
        public ModelTexture buildModelTexture(List<string> imagePaths)
        {
            List<Imgd> imagesAsImgd = new List<Imgd>();
            ModelTexture modelTexture = new ModelTexture(imagesAsImgd);

            return modelTexture;
        }
        public ModelTexture(IEnumerable<Imgd> images)
            : this(new Build { images = images.ToArray(), })
        {

        }*/

        // This function extracts the bitmaps form the textureFooterData
        public static List<Bitmap> footerToImages(ModelTexture modelTexture)
        {
            TextureFooterDataIMEx textureFooterData = new TextureFooterDataIMEx(modelTexture.TextureFooterData);

            List<Bitmap> bitmaps = new List<Bitmap>();
            textureFooterData.TextureAnimationList?
                    .Select((it, index) => (it, index))
                    .ToList()
                    .ForEach(
                        pair =>
                        {
                            TextureAnimation src = pair.it.Source;
                            Bitmap bitmap = ToBitmap(
                                src.BitsPerPixel,
                                src.SpriteWidth,
                                src.SpriteHeight,
                                src.NumSpritesInImageData,
                                src.SpriteStride,
                                src.SpriteImage
                            );

                            bitmaps.Add( bitmap );
                        }
                    );

            return bitmaps;
        }
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        // VVV This should work? VVV
        public TextureFooterDataIMEx generateIMEx(TextureFooterData src)
        {
            return new TextureFooterDataIMEx(src);
        }
        public TextureAnimationIMEx generateIMEx(TextureAnimation src)
        {
            return new TextureAnimationIMEx(src);
        }
        public TextureAnimation ReplaceAnimationTexture(TextureAnimationIMEx textureAnimIMEx, string pngPath)
        {
            var spriteImage = new SpriteBitmap(pngPath);
            var bitsPerPixel = spriteImage.BitsPerPixel;

            var back = new TextureAnimation
            {
                Unk1 = textureAnimIMEx.Unk1,
                TextureIndex = textureAnimIMEx.TextureIndex,
                FrameStride = textureAnimIMEx.FrameStride,
                BitsPerPixel = Convert.ToUInt16(bitsPerPixel),
                BaseSlotIndex = textureAnimIMEx.BaseSlotIndex,
                MaximumSlotIndex = Convert.ToUInt16(textureAnimIMEx.BaseSlotIndex + textureAnimIMEx.SlotTable.Length - 1),

                NumAnimations = Convert.ToUInt16(textureAnimIMEx.FrameGroupList.Length),
                NumSpritesInImageData = textureAnimIMEx.NumSpritesInImageData,
                UOffsetInBaseImage = textureAnimIMEx.UOffsetInBaseImage,
                VOffsetInBaseImage = textureAnimIMEx.VOffsetInBaseImage,
                SpriteWidth = Convert.ToUInt16(spriteImage.Size.Width),
                SpriteHeight = Convert.ToUInt16(spriteImage.Size.Height / textureAnimIMEx.NumSpritesInImageData),

                OffsetSlotTable = 0,

                OffsetAnimationTable = 0,

                OffsetSpriteImage = 0,
                DefaultAnimationIndex = textureAnimIMEx.DefaultAnimationIndex,

                SlotTable = textureAnimIMEx.SlotTable,
                FrameGroupList = textureAnimIMEx.FrameGroupList,
                SpriteImage = spriteImage.Data,
            };

            return back;
        }

        /*public void insertImagesToFooter(ModelTexture modelTexture, TextureFooterDataIMEx model, List<string> imagePaths)
        {
            model.ConvertBackTo(
                    pngFile =>
                    {
                        return new SpriteBitmap(Path.Combine(outDir, pngFile));
                    },
                    modelTexture.TextureFooterData
                );
        }

        public TextureFooterData ConvertBackTo(Func<string, ISpriteImageSource> imageLoader, TextureFooterData back)
        {
            back.UvscList.Clear();
            back.UvscList.AddRange(UvscList);
            back.TextureAnimationList.Clear();
            back.TextureAnimationList.AddRange(
                TextureAnimationList
                    .Select(texa => texa.ConvertBack(imageLoader))
            );
            back.UnkFooter = UnkFooter;
            back.ShouldEmitDMYAtFirst = ShouldEmitDMYAtFirst;
            back.ShouldEmitKN5 = ShouldEmitKN5;
            return back;
        }*/

        // From TexFooter.SpriteImageUtil
        public static Bitmap ToBitmap(
            int BitsPerPixel,
            int SpriteWidth,
            int SpriteHeight,
            int NumSpritesInImageData,
            int SpriteStride,
            byte[] SpriteImage
        )
        {
            if (BitsPerPixel != 4 && BitsPerPixel != 8)
            {
                throw new NotSupportedException($"BitsPerPixel: {BitsPerPixel} ≠ 4 or 8");
            }

            var pixFmt = (BitsPerPixel == 8) ? PixelFormat.Format8bppIndexed : PixelFormat.Format4bppIndexed;

            var bitmap = new Bitmap(SpriteWidth, SpriteHeight * NumSpritesInImageData, pixFmt);
            var bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, pixFmt);
            try
            {
                if (SpriteStride != bitmapData.Stride)
                {
                    throw new NotSupportedException($"Stride: {SpriteStride} ≠ {bitmapData.Stride}");
                }

                if (BitsPerPixel == 8)
                {
                    Marshal.Copy(SpriteImage, 0, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height);
                }
                else
                {
                    Marshal.Copy(SwapPixelOrder(SpriteImage), 0, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height);
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
            var palette = bitmap.Palette;
            if (BitsPerPixel == 8)
            {
                Enumerable.Range(0, 256).ToList().ForEach(
                    index => palette.Entries[index] = Color.FromArgb(index, index, index)
                );
            }
            else
            {
                Enumerable.Range(0, 16).ToList().ForEach(
                    index =>
                    {
                        var light = Math.Min(255, 16 * index);
                        palette.Entries[index] = Color.FromArgb(light, light, light);
                    }
                );
            }
            bitmap.Palette = palette;

            return bitmap;
        }
        private static byte[] SwapPixelOrder(byte[] src)
        {
            var dst = new byte[src.Length];
            for (int x = 0; x < dst.Length; x++)
            {
                var b = src[x];
                dst[x] = (byte)(b << 4 | b >> 4);
            }
            return dst;
        }

        // Reading Footer
        // TextureFooterDataIMEx textureFooterData = new TextureFooterDataIMEx(footerData);

        /*textureFooterData.TextureAnimationList?
                    .Select((it, index) => (it, index))
                    .ToList()
                    .ForEach(
                        pair =>
                        {
            var src = pair.it._source;
            var bitmap = SpriteImageUtil.ToBitmap(
                src.BitsPerPixel,
                src.SpriteWidth,
                src.SpriteHeight,
                src.NumSpritesInImageData,
                src.SpriteStride,
                src.SpriteImage
            );
            var pngFile = Path.Combine(outDir, $"{baseName}.footer-{key}-{pair.index}.png");
            bitmap.Save(pngFile, ImageFormat.Png);

            pair.it.SpriteImageFile = "./" + Path.GetFileName(pngFile);
        }
                    );*/










        public static Imgd pngToImgd(string filePath)
        {
            try
            {
                var inputFile = filePath;
                Imgd imgd;

                // Alpha enabled png → always 32 bpp
                using (var bitmap = new Bitmap(inputFile))
                {
                    imgd = ImgdBitmapUtil.ToImgd(bitmap, 8, null);

                    var buffer = new MemoryStream();
                    imgd.Write(buffer);
                }

                return imgd;
            }
            catch (Exception excep)
            {
                throw new Exception("Error loading texture: " + filePath);
            }
        }

        public static ModelTexture.Texture imgdToTexture(Imgd imgdFile)
        {
            Imgd[] imgdArray = new Imgd[1];
            imgdArray[0] = imgdFile;
            ModelTexture modTex = new ModelTexture(imgdArray);
            // The only method for loading the textures is reading the whole binary again
            Stream buffer = new MemoryStream();
            modTex.Write(buffer);
            modTex = ModelTexture.Read(buffer);

            return modTex.Images[0];
        }

        public static ModelTexture.Texture pngToTexture(string filePath)
        {
            return imgdToTexture(pngToImgd(filePath));
        }
        public static BitmapSource getBitmapSource(TextureAnimation texAnim, byte[] clutPalette)
        {
            return GetBimapSource(ToBgra32(texAnim.SpriteImage, clutPalette), texAnim.SpriteWidth, texAnim.SpriteHeight * texAnim.NumSpritesInImageData);
        }

        // Makes the Indexed8 Image a BGRA32 Image
        public static byte[] ToBgra32(byte[] image, byte[] clutPalette)
        {
            return OpenKh.Imaging.ImageDataHelpers.FromIndexed8ToBitmap32(image, clutPalette, OpenKh.Imaging.ImageDataHelpers.RGBA);
        }
        // Gets the bitmapSource from a BGRA32 Image
        // NOTE: Can be done using pixelformat Indexed8 to create a BitmapSource but it needs a palette instead of null
        public static BitmapSource GetBimapSource(byte[] imageBGRA32, int width, int height)
        {
            const double dpi = 96.0;

            return BitmapSource.Create(width, height, dpi, dpi, System.Windows.Media.PixelFormats.Bgra32, null, imageBGRA32, width * 4);
        }



        public static TextureAnimation CreateTextureAnimation(string pngPath)
        {
            SpriteBitmap spriteBM = new SpriteBitmap(pngPath);
            var spriteImage = spriteBM;
            var bitsPerPixel = spriteImage.BitsPerPixel;

            var back = new TextureAnimation
            {
                //Unk1 = Unk1,
                TextureIndex = 0,
                FrameStride = 0,
                BitsPerPixel = Convert.ToUInt16(bitsPerPixel),
                BaseSlotIndex = 0,
                MaximumSlotIndex = 0,

                NumAnimations = 0,
                NumSpritesInImageData = 1,
                UOffsetInBaseImage = 0,
                VOffsetInBaseImage = 0,
                SpriteWidth = Convert.ToUInt16(spriteImage.Size.Width),
                SpriteHeight = Convert.ToUInt16(spriteImage.Size.Height),

                OffsetSlotTable = 0,

                OffsetAnimationTable = 0,

                OffsetSpriteImage = 0,
                DefaultAnimationIndex = 0,

                SlotTable = new short[0],
            FrameGroupList = new TextureFrameGroup[0],
                SpriteImage = spriteImage.Data,
            };

            return back;
        }
    }
}
