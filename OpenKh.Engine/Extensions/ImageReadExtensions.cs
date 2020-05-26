using OpenKh.Imaging;
using System;

namespace OpenKh.Engine.Extensions
{
    public static class ImageReadExtensions
    {
        private static readonly byte[] Bgra = new byte[] { 2, 1, 0, 3 };
        private static readonly byte[] Rgba = new byte[] { 0, 1, 2, 3 };

        public static byte[] AsBgra8888(this IImageRead image)
        {
            switch (image.PixelFormat)
            {
                case PixelFormat.Indexed4: return image.From4bpp(Bgra);
                case PixelFormat.Indexed8: return image.From8bpp(Bgra);
                default:
                    throw new ArgumentException($"The pixel format {image.PixelFormat} is not supported.");
            }
        }

        public static byte[] AsRgba8888(this IImageRead image)
        {
            switch (image.PixelFormat)
            {
                case PixelFormat.Indexed4: return image.From4bpp(Rgba);
                case PixelFormat.Indexed8: return image.From8bpp(Rgba);
                default:
                    throw new ArgumentException($"The pixel format {image.PixelFormat} is not supported.");
            }
        }

        private static byte[] From4bpp(this IImageRead image, byte[] channelOrder)
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
                    dstData[dstIndex++] = clut[palIndex1 * 4 + channelOrder[0]];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + channelOrder[1]];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + channelOrder[2]];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + channelOrder[3]];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + channelOrder[0]];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + channelOrder[1]];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + channelOrder[2]];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + channelOrder[3]];
                }
            }

            return dstData;
        }

        private static byte[] From8bpp(this IImageRead image, byte[] channelOrder)
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
                    dstData[dstIndex++] = clut[palIndex * 4 + channelOrder[0]];
                    dstData[dstIndex++] = clut[palIndex * 4 + channelOrder[1]];
                    dstData[dstIndex++] = clut[palIndex * 4 + channelOrder[2]];
                    dstData[dstIndex++] = clut[palIndex * 4 + channelOrder[3]];
                }
            }

            return dstData;
        }
    }
}
