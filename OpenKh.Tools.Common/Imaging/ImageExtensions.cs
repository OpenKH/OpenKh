using OpenKh.Imaging;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace OpenKh.Tools.Common.Imaging
{
    public static class ImageExtensions
    {
        public static void SaveImage(this IImageRead imageRead, string fileName)
        {
            using (var gdiBitmap = imageRead.CreateBitmap())
            {
                gdiBitmap.Save(fileName);
            }
        }

        public static Bitmap CreateBitmap(this IImageRead imageRead)
        {
            var drawingPixelFormat = imageRead.PixelFormat.GetDrawingPixelFormat();
            Bitmap bitmap = new Bitmap(imageRead.Size.Width, imageRead.Size.Height, drawingPixelFormat);

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, drawingPixelFormat);

            var srcData = imageRead.GetData();
            var dstLength = Math.Min(srcData.Length, bitmapData.Stride * bitmapData.Height);
            Marshal.Copy(srcData, 0, bitmapData.Scan0, dstLength);

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
    }
}
