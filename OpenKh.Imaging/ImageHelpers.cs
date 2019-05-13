using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenKh.Imaging
{
    public static class ImageHelpers
    {
        public static byte[] ToBgra32(this IImageRead imageRead)
        {
            switch (imageRead.PixelFormat)
            {
                case PixelFormat.Indexed4:
                    return FromIndexed4ToBgra(imageRead.GetData(), imageRead.GetClut());
                case PixelFormat.Indexed8:
                    return FromIndexed8ToBgra(imageRead.GetData(), imageRead.GetClut());
                default:
                    throw new NotImplementedException($"The PixelFormat {imageRead.PixelFormat} cannot be converted to a Bgra32.");
            }
        }

        public static byte[] FromIndexed8ToBgra(byte[] data, byte[] clut)
        {
            var bitmap = new byte[data.Length * 4];
            for (int i = 0; i < data.Length; i++)
            {
                var clutIndex = data[i];
                bitmap[i * 4 + 0] = clut[clutIndex * 4 + 2];
                bitmap[i * 4 + 1] = clut[clutIndex * 4 + 1];
                bitmap[i * 4 + 2] = clut[clutIndex * 4 + 0];
                bitmap[i * 4 + 3] = clut[clutIndex * 4 + 3];
            }
            return bitmap;
        }

        public static byte[] FromIndexed4ToBgra(byte[] data, byte[] clut)
        {
            var bitmap = new byte[data.Length * 8];
            for (int i = 0; i < data.Length; i++)
            {
                var subData = data[i];
                var clutIndex1 = subData >> 4;
                var clutIndex2 = subData & 0x0F;
                bitmap[i * 8 + 0] = clut[clutIndex1 * 4 + 2];
                bitmap[i * 8 + 1] = clut[clutIndex1 * 4 + 1];
                bitmap[i * 8 + 2] = clut[clutIndex1 * 4 + 0];
                bitmap[i * 8 + 3] = clut[clutIndex1 * 4 + 3];
                bitmap[i * 8 + 4] = clut[clutIndex2 * 4 + 2];
                bitmap[i * 8 + 5] = clut[clutIndex2 * 4 + 1];
                bitmap[i * 8 + 6] = clut[clutIndex2 * 4 + 0];
                bitmap[i * 8 + 7] = clut[clutIndex2 * 4 + 3];
            }
            return bitmap;
        }

        public static void SaveImage(this IImageRead imageRead, string fileName)
        {
            using (var gdiBitmap = imageRead.CreateBitmap())
            {
                gdiBitmap.Save(fileName);
            }
        }

        private static Bitmap CreateBitmap(this IImageRead imageRead)
        {
            var drawingPixelFormat = imageRead.PixelFormat.GetDrawingPixelFormat();
            Bitmap bitmap = new Bitmap(imageRead.Size.Width, imageRead.Size.Height, drawingPixelFormat);

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, drawingPixelFormat);

            var srcData = imageRead.GetData();
            Marshal.Copy(srcData, 0, bitmapData.Scan0, srcData.Length);

            bitmap.UnlockBits(bitmapData);

            var isIndexed = imageRead.PixelFormat.IsIndexed();
            if (isIndexed)
            {
                var palette = bitmap.Palette;
                var clut = imageRead.GetClut();
                var colorsCount = Math.Min(clut.Length / 4, palette.Entries.Length);

                for (var i = 0; i < colorsCount; i++)
                {
                    palette.Entries[i] = Color.FromArgb(
                        clut[i * 4 + 3],
                        clut[i * 4 + 0],
                        clut[i * 4 + 1],
                        clut[i * 4 + 2]);
                }

                bitmap.Palette = palette;
            }

            return bitmap;
        }

        private static bool IsIndexed(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Indexed4:
                case PixelFormat.Indexed8:
                    return true;
                default:
                    return false;
            }
        }

        private static System.Drawing.Imaging.PixelFormat GetDrawingPixelFormat(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Indexed4:
                    return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
                case PixelFormat.Indexed8:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                case PixelFormat.Rgba8888:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                default:
                    throw new NotImplementedException(
                        $"The reading from pixel format {pixelFormat} is not implemented.");
            }
        }
    }
}
