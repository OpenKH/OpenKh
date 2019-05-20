using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        private static System.Windows.Media.PixelFormat GetPixelFormat(Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case Imaging.PixelFormat.Indexed4:
                    return PixelFormats.Indexed4;
                case Imaging.PixelFormat.Indexed8:
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

        private static int GetBpp(Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case Imaging.PixelFormat.Indexed4: return 4;
                case Imaging.PixelFormat.Indexed8: return 8;
                default:
                    throw new ArgumentException($"Pixel format {pixelFormat} not supported", nameof(pixelFormat));
            }
        }
    }
}
