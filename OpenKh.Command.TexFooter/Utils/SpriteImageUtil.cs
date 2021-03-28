using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenKh.Command.TexFooter.Utils
{
    static class SpriteImageUtil
    {
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
    }
}
