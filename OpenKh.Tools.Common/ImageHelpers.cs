using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Drawing;

namespace OpenKh.Tools.Common
{
    public static class ImageHelpers
    {
        public static BitmapSource GetWindowsMediaImage(this IImageRead image)
        {
            var size = image.Size;
            return BitmapSource.Create(
                size.Width, size.Height,
                96.0, 96.0,
                GetPixelFormat(image.PixelFormat),
                GetPalette(image),
                image.GetData(),
                size.Width * GetBpp(image.PixelFormat) / 8);
        }

        public static ISurface CreateSurface(this IDrawing drawing, IImageRead image) => drawing
            .CreateSurface(image.Size.Width,
                image.Size.Height,
                Xe.Drawing.PixelFormat.Format32bppArgb,
                SurfaceType.Input,
                GetDataResource(image));

        private static DataResource GetDataResource(IImageRead image)
        {
            byte[] data;
            switch (image.PixelFormat)
            {
                case OpenKh.Imaging.PixelFormat.Indexed4:
                    data = GetDataResource4bpp(image);
                    break;
                case OpenKh.Imaging.PixelFormat.Indexed8:
                    data = GetDataResource8bpp(image);
                    break;
                default:
                    throw new ArgumentException($"The pixel format {image.PixelFormat} is not supported.");
            }

            return new DataResource
            {
                Data = data,
                Stride = image.Size.Width * 4
            };
        }

        private static byte[] GetDataResource4bpp(IImageRead image)
        {
            var size = image.Size;
            var data = image.GetData();
            var clut = image.GetClut();
            var dstData = new byte[size.Width * size.Height * sizeof(uint)];
            var srcIndex = 0;
            var dstIndex = 0;

            for (var y = 0; y < size.Height; y++)
            {
                for (var i = 0; i < size.Width / 2; i++)
                {
                    var ch = data[srcIndex++];
                    var palIndex1 = (ch >> 4);
                    var palIndex2 = (ch & 15);
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 2];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 1];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 0];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 3];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 2];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 1];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 0];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 3];
                }
            }

            return dstData;
        }

        private static byte[] GetDataResource8bpp(IImageRead image)
        {
            var size = image.Size;
            var data = image.GetData();
            var clut = image.GetClut();
            var dstData = new byte[size.Width * size.Height * sizeof(uint)];
            var srcIndex = 0;
            var dstIndex = 0;

            for (var y = 0; y < size.Height; y++)
            {
                for (var i = 0; i < size.Width; i++)
                {
                    var palIndex = data[srcIndex++];
                    dstData[dstIndex++] = clut[palIndex * 4 + 2];
                    dstData[dstIndex++] = clut[palIndex * 4 + 1];
                    dstData[dstIndex++] = clut[palIndex * 4 + 0];
                    dstData[dstIndex++] = clut[palIndex * 4 + 3];
                }
            }

            return dstData;
        }

        private static System.Windows.Media.PixelFormat GetPixelFormat(OpenKh.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case OpenKh.Imaging.PixelFormat.Indexed4:
                    return PixelFormats.Indexed4;
                case OpenKh.Imaging.PixelFormat.Indexed8:
                    return PixelFormats.Indexed8;
                default:
                    throw new ArgumentException($"Pixel format {pixelFormat} not supported", nameof(pixelFormat));
            }
        }

        private static BitmapPalette GetPalette(IImageRead image)
        {
            var clut = image.GetClut();
            int colorsCount = clut.Length / 4;
            if (colorsCount == 0)
                return null;

            var colors = new List<Color>(colorsCount);

            for (var i = 0; i < colorsCount; i++)
                colors.Add(GetColor(clut, i * 4));

            return new BitmapPalette(colors);
        }

        private static Color GetColor(byte[] clut, int index) => new Color()
        {
            R = clut[index + 0],
            G = clut[index + 1],
            B = clut[index + 2],
            A = clut[index + 3],
        };

        private static int GetBpp(OpenKh.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case OpenKh.Imaging.PixelFormat.Indexed4: return 4;
                case OpenKh.Imaging.PixelFormat.Indexed8: return 8;
                default:
                    throw new ArgumentException($"Pixel format {pixelFormat} not supported", nameof(pixelFormat));
            }
        }
    }
}
