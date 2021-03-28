using OpenKh.Command.TexFooter.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenKh.Command.TexFooter.Utils
{
    public class SpriteBitmap : ISpriteImageSource
    {
        public SpriteBitmap(string pngFile)
        {
            using Bitmap bitmap = new Bitmap(pngFile);

            var bitmapData = bitmap.LockBits(
                new Rectangle(Point.Empty, bitmap.Size),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat
            );
            try
            {
                Data = new byte[bitmapData.Stride * bitmapData.Height];
                Marshal.Copy(bitmapData.Scan0, Data, 0, Data.Length);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }

            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    BitsPerPixel = 8;
                    break;

                case PixelFormat.Format4bppIndexed:
                    BitsPerPixel = 4;

                    PerformSwapPixelOrder(Data);
                    break;

                default:
                    throw new NotSupportedException($"PixelFormat: {bitmap.PixelFormat} ≠ 4 or 8");
            }

            Size = bitmap.Size;
        }

        private static void PerformSwapPixelOrder(byte[] dst)
        {
            for (int x = 0; x < dst.Length; x++)
            {
                var b = dst[x];
                dst[x] = (byte)(b << 4 | b >> 4);
            }
        }

        public int BitsPerPixel { get; }

        public Size Size { get; }

        public byte[] Data { get; }
    }
}
