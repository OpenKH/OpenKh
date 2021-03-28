using System;
using System.Drawing;
using System.Linq;

namespace OpenKh.Imaging
{
    public static class ImageDataHelpers
    {
        public const int RedChannel = 0;
        public const int GreenChannel = 1;
        public const int BlueChannel = 2;
        public const int AlphaChannel = 3;
        public static readonly byte[] BGRA = new byte[] { BlueChannel, GreenChannel, RedChannel, AlphaChannel };
        public static readonly byte[] RGBA = new byte[] { RedChannel, GreenChannel, BlueChannel, AlphaChannel };

        public static byte[] FromIndexed8ToBitmap32(byte[] data, byte[] clut, byte[] channelOrder)
        {
            var bitmap = new byte[data.Length * 4];
            for (int i = 0; i < data.Length; i++)
            {
                var clutIndex = data[i];
                bitmap[i * 4 + 0] = clut[clutIndex * 4 + channelOrder[2]];
                bitmap[i * 4 + 1] = clut[clutIndex * 4 + channelOrder[1]];
                bitmap[i * 4 + 2] = clut[clutIndex * 4 + channelOrder[0]];
                bitmap[i * 4 + 3] = clut[clutIndex * 4 + channelOrder[3]];
            }
            return bitmap;
        }

        public static byte[] FromIndexed4ToBitmap32(byte[] data, byte[] clut, byte[] channelOrder)
        {
            var bitmap = new byte[data.Length * 8];
            for (int i = 0; i < data.Length; i++)
            {
                var subData = data[i];
                var clutIndex1 = subData >> 4;
                var clutIndex2 = subData & 0x0F;
                bitmap[i * 8 + 0] = clut[clutIndex1 * 4 + channelOrder[2]];
                bitmap[i * 8 + 1] = clut[clutIndex1 * 4 + channelOrder[1]];
                bitmap[i * 8 + 2] = clut[clutIndex1 * 4 + channelOrder[0]];
                bitmap[i * 8 + 3] = clut[clutIndex1 * 4 + channelOrder[3]];
                bitmap[i * 8 + 4] = clut[clutIndex2 * 4 + channelOrder[2]];
                bitmap[i * 8 + 5] = clut[clutIndex2 * 4 + channelOrder[1]];
                bitmap[i * 8 + 6] = clut[clutIndex2 * 4 + channelOrder[0]];
                bitmap[i * 8 + 7] = clut[clutIndex2 * 4 + channelOrder[3]];
            }
            return bitmap;
        }

        public static byte[] FromBitmap32(byte[] data, byte[] channelOrder)
        {
            var dstData = new byte[data.Length];

            for (var i = 0; i < data.Length; i += 4)
            {
                dstData[i + 0] = data[i + channelOrder[0]];
                dstData[i + 1] = data[i + channelOrder[1]];
                dstData[i + 2] = data[i + channelOrder[2]];
                dstData[i + 3] = data[i + channelOrder[3]];
            }

            return dstData;
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
