using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenKh.Command.TexFooter.Utils
{
    internal static class SpriteImageUtil
    {
        public static IImageRead ToBitmap(
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

            var pixFmt = (BitsPerPixel == 8) ? PixelFormat.Indexed8 : PixelFormat.Indexed4;

            byte[] bitmapData;
            if (BitsPerPixel == 8)
            {
                bitmapData = SpriteImage;
            }
            else
            {
                bitmapData = SwapPixelOrder(SpriteImage);
            }

            byte[] palette;
            if (BitsPerPixel == 8)
            {
                palette = Enumerable.Range(0, 256)
                    .SelectMany(
                        index => new byte[] { (byte)index, (byte)index, (byte)index, 255, }
                    )
                    .ToArray();
            }
            else
            {
                palette = Enumerable.Range(0, 16)
                    .Select(index => (byte)(index | (index << 4)))
                    .SelectMany(
                        light =>
                        {
                            return new byte[] { light, light, light, 255, };
                        }
                    )
                    .ToArray();
            }

            return new LocalBitmap
            {
                Size = new Size(SpriteWidth, SpriteHeight * NumSpritesInImageData),
                PixelFormat = pixFmt,
                Clut = palette,
                Data = bitmapData,
            };
        }

        private class LocalBitmap : IImageRead
        {
            public Size Size { get; internal set; }
            public PixelFormat PixelFormat { get; internal set; }

            internal byte[] Clut { get; set; }
            internal byte[] Data { get; set; }

            public byte[] GetClut() => Clut;
            public byte[] GetData() => Data;
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
