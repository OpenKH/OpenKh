using OpenKh.Imaging;
using System.Drawing;

namespace OpenKh.Kh2
{
    public partial class Imgd
    {
        /// <param name="data">
        /// Accept pixel order in the following styles:
        /// - `Indexed4`: The first pixel is high byte. The second pixel is low byte.
        /// - `Indexed8`: No conversion, one byte is one pixel.
        /// - `Rgba8888`: `RR GG BB AA`
        /// </param>
        /// <param name="clut">
        /// - Place color look at table in this order: `RR GG BB AA`, for `Indexed4` or `Indexed8` images.
        /// - `AA` ranges from 0 to 255. This will map to Ps2 alpha range (0 to 128).
        /// </param>
        /// <param name="isSwizzled"></param>
        public Imgd(
            Size size,
            PixelFormat pixelFormat,
            byte[] data,
            byte[] clut,
            bool isSwizzled)
        {
            Size = size;
            _format = GetFormat(pixelFormat);
            _flags = 3 | (isSwizzled ? SwizzledFlag : 0);

            if (isSwizzled)
            {
                switch (_format)
                {
                    case Tm2.GsPSM.GS_PSMT4:
                        Data = Swizzle4bpp(size, data);
                        break;
                    case Tm2.GsPSM.GS_PSMT8:
                        Data = Swizzle8bpp(size, data);
                        break;
                    default:
                        Data = data;
                        break;
                }
            }
            else
            {
                Data = data;
            }

            switch (_format)
            {
                case Tm2.GsPSM.GS_PSMT4:
                    Clut = GetKh2Clut4(clut);
                    break;
                case Tm2.GsPSM.GS_PSMT8:
                    Clut = GetKh2Clut8(clut);
                    break;
            }
        }

        /// <param name="data">
        /// Accept pixel order in the following styles:
        /// - `Indexed4`: The first pixel is high byte. The second pixel is low byte.
        /// - `Indexed8`: No conversion, one byte is one pixel.
        /// - `Rgba8888`: `BB GG RR AA` (same as Format32bppArgb)
        /// </param>
        /// <param name="clut">
        /// - Place color look at table in this order: `RR GG BB AA`, for `Indexed4` or `Indexed8` images.
        /// - `AA` ranges from 0 to 255. This will map to Ps2 alpha range (0 to 128).
        /// </param>
        public static Imgd Create(
            Size size,
            PixelFormat pixelFormat,
            byte[] data,
            byte[] clut,
            bool isSwizzled) =>
            new Imgd(size, pixelFormat, ImageDataHelpers.GetInvertedRedBlueChannels(data, size, pixelFormat), clut, isSwizzled);

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
            var inputColorCount = rawClut.Length / 4;
            var newClut = new byte[16 * 4];
            for (var i = 0; i < inputColorCount; i++)
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
            var inputColorCount = rawClut.Length / 4;
            var newClut = new byte[256 * 4];

            for (var i = 0; i < inputColorCount; i++)
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
