using kh.Imaging;
using System;
using System.Drawing;

namespace kh.kh2
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
                        break;
                    case Format8bpp:
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
            byte[] clut) =>
            new Imgd(size, pixelFormat, data, clut, false);

        private byte[] GetKh2Clut4(byte[] rawClut)
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

        private byte[] GetKh2Clut8(byte[] rawClut)
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

        private byte ToPs2Alpha(byte data) => (byte)((data + 1) / 2);
    }
}
