using System;
using System.Drawing;

namespace OpenKh.Imaging
{
    public static class ImageDataHelpers
    {
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

        public static void InvertRedBlueChannels(byte[] data, Size size, PixelFormat pixelFormat)
        {
            var length = size.Width * size.Height;
            switch (pixelFormat)
            {
                case PixelFormat.Rgb888:
                    for (var i = 0; i < length; i++)
                    {
                        byte tmp = data[i * 3 + 0];
                        data[i * 3 + 0] = data[i * 3 + 2];
                        data[i * 3 + 2] = tmp;
                    }
                    break;
                case PixelFormat.Rgba8888:
                    for (int i = 0; i < length; i++)
                    {
                        byte tmp = data[i * 4 + 0];
                        data[i * 4 + 0] = data[i * 4 + 2];
                        data[i * 4 + 2] = tmp;
                    }
                    break;
                case PixelFormat.Indexed8:
                    break;
                case PixelFormat.Indexed4:
                    // no pixel swapping is required
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"The format {pixelFormat} is invalid or not supported.");
            }
        }

        public static byte[] GetInvertedRedBlueChannels(byte[] data, Size size, PixelFormat pixelFormat)
        {
            var length = size.Width * size.Height;
            var dst = new byte[data.Length];

            switch (pixelFormat)
            {
                case PixelFormat.Rgb888:
                    for (var i = 0; i < length; i++)
                    {
                        dst[i * 3 + 0] = data[i * 3 + 2];
                        dst[i * 3 + 1] = data[i * 3 + 1];
                        dst[i * 3 + 2] = data[i * 3 + 0];
                    }
                    break;
                case PixelFormat.Rgba8888:
                    for (var i = 0; i < length; i++)
                    {
                        dst[i * 4 + 0] = data[i * 4 + 2];
                        dst[i * 4 + 1] = data[i * 4 + 1];
                        dst[i * 4 + 2] = data[i * 4 + 0];
                        dst[i * 4 + 3] = data[i * 4 + 3];
                    }
                    break;
                case PixelFormat.Indexed8:
                    return data;
                case PixelFormat.Indexed4:
                    // no pixel swapping is required
                    return data;
                default:
                    throw new ArgumentOutOfRangeException($"The format {pixelFormat} is invalid or not supported.");
            }

            return dst;
        }

        public static void SwapEndianIndexed4(byte[] data)
        {
            for (var i = 0; i < data.Length; i++)
                data[i] = (byte)(((data[i] & 0x0F) << 4) | (data[i] >> 4));
        }
    }
}
