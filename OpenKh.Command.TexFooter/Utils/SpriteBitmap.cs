using OpenKh.Command.TexFooter.Interfaces;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenKh.Command.TexFooter.Utils
{
    public class SpriteBitmap : ISpriteImageSource
    {
        public SpriteBitmap(string pngFile)
        {
            var bitmap = PngImage.Read(new MemoryStream(File.ReadAllBytes(pngFile)));

            Data = bitmap.GetData();

            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Indexed8:
                    BitsPerPixel = 8;
                    break;

                case PixelFormat.Indexed4:
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
