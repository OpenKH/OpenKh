using OpenKh.Imaging;
using System;
using System.Drawing;

namespace OpenKh.Kh2
{
    public partial class Imgd
    {
        public Imgd(
            Size size,
            PixelFormat pixelFormat,
            byte[] data,
            byte[] clut,
            bool isSwizzled)
        {
            Size = size;
            format = GetFormat(pixelFormat);
            swizzled = isSwizzled ? 7 : 3;

            if (isSwizzled)
            {
                switch (format)
                {
                    case Format4bpp:
                        Data = Swizzle4bpp(size, data);
                        break;
                    case Format8bpp:
                        Data = Swizzle8bpp(size, data);
                        break;
                }
            }
            else
            {
                Data = data;
            }

            switch (format)
            {
                case Format4bpp:
                    Clut = GetKh2Clut4(clut);
                    break;
                case Format8bpp:
                    Clut = GetKh2Clut8(clut);
                    break;
            }
        }

        public static Imgd Create(
            Size size,
            PixelFormat pixelFormat,
            byte[] data,
            byte[] clut,
            bool isSwizzled) =>
            new Imgd(size, pixelFormat, data, clut, isSwizzled);

        private static byte[] Swizzle4bpp(Size size, byte[] data)
        {
            data = Ps2.Encode4(data, size.Width / 128, size.Height / 128);
            return Ps2.Decode32(data, size.Width / 128, size.Height / 128);
        }

        private static byte[] Swizzle8bpp(Size size, byte[] data)
        {
            data = Ps2.Encode8(data, size.Width / 128, size.Height / 64);
            return Ps2.Decode32(data, size.Width / 128, size.Height / 64);
        }

        private static byte[] GetKh2Clut4(byte[] rawClut)
        {
            var newClut = new byte[16 * 4];
            if (rawClut.Length < newClut.Length)
                throw new ArgumentException(
                    $"The clut must be at least {newClut.Length} bytes long.");

            for (var i = 0; i < 16; i++)
            {
                newClut[i * 4 + 0] = rawClut[i * 4 + 0];
                newClut[i * 4 + 1] = rawClut[i * 4 + 1];
                newClut[i * 4 + 2] = rawClut[i * 4 + 2];
                newClut[i * 4 + 3] = ToPs2Alpha(rawClut[i * 4 + 3]);
            }

            return newClut;
        }

        private static byte[] GetKh2Clut8(byte[] rawClut)
        {
            var newClut = new byte[256 * 4];
            if (rawClut.Length < newClut.Length)
                throw new ArgumentException(
                    $"The clut must be at least {newClut.Length} bytes long.");

            for (var i = 0; i < 256; i++)
            {
                var dstIndex = Ps2.Repl(i);
                newClut[dstIndex * 4 + 0] = rawClut[i * 4 + 0];
                newClut[dstIndex * 4 + 1] = rawClut[i * 4 + 1];
                newClut[dstIndex * 4 + 2] = rawClut[i * 4 + 2];
                newClut[dstIndex * 4 + 3] = ToPs2Alpha(rawClut[i * 4 + 3]);
            }

            return newClut;
        }

        private byte[] GetSwappedPixelData(byte[] data)
        {
            var newData = new byte[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var pixel = data[i];
                newData[i] = (byte)((pixel >> 4) | (pixel << 4));
            }
            return newData;
        }

        private static byte ToPs2Alpha(byte data) => (byte)((data + 1) / 2);
    }
}
