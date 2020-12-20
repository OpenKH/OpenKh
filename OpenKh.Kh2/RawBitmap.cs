using System;
using System.Drawing;
using System.IO;
using OpenKh.Imaging;

namespace OpenKh.Kh2
{
    public class RawBitmap : IImageRead
    {
        private const int PaletteCount = 256;
        private const int BitsPerColor = 32;
        private static readonly uint[] FontColors = new uint[]
        {
            0x00000000,
            0x80a0a0a0,
            0xe4cccccc,
            0xfff0f0f0
        };
        private static readonly byte[] FontPalette1 = GeneratePalette1(FontColors);
        private static readonly byte[] FontPalette2 = GeneratePalette2(FontColors);

        private readonly byte[] _data;
        private readonly byte[] _clut;

        private RawBitmap(Stream stream, int width, int height, bool is8bit, bool defaultPaletteSwitch = false)
        {
            Size = new Size(width, height);
            PixelFormat = is8bit ? PixelFormat.Indexed8 : PixelFormat.Indexed4;

            var reader = new BinaryReader(stream);
            var bpp = is8bit ? 8 : 4;
            _data = reader.ReadBytes(width * height * bpp / 8);

            if (bpp == 4)
                ImageDataHelpers.SwapEndianIndexed4(_data);

            // If we did not reached the end of the stream, then it does mean that there is a palette
            if (stream.Position < stream.Length)
            {
                _clut = reader.ReadBytes(PaletteCount * BitsPerColor / 8);
            }
            else
            {
                // Just assign a default palette
                _clut = defaultPaletteSwitch ? FontPalette2 : FontPalette1;
            }
        }

        public Size Size { get; }

        public PixelFormat PixelFormat { get; }

        public byte[] GetClut()
        {
            switch (PixelFormat)
            {
                case PixelFormat.Indexed8: return GetClut8();
                case PixelFormat.Indexed4: return GetClut4();
                default:
                    throw new NotSupportedException($"The format {PixelFormat} is not supported.");
            }
        }

        private byte[] GetClut4()
        {
            var data = new byte[16 * 4];
            for (var i = 0; i < 16; i++)
            {
                data[i * 4 + 0] = _clut[(i & 15) * 4 + 0];
                data[i * 4 + 1] = _clut[(i & 15) * 4 + 1];
                data[i * 4 + 2] = _clut[(i & 15) * 4 + 2];
                data[i * 4 + 3] = Ps2.FromPs2Alpha(_clut[(i & 15) * 4 + 3]);
            }

            return data;
        }

        private byte[] GetClut8()
        {
            var data = new byte[256 * 4];
            for (var i = 0; i < 256; i++)
            {
                var srcIndex = Ps2.Repl(i);
                data[i * 4 + 0] = _clut[srcIndex * 4 + 0];
                data[i * 4 + 1] = _clut[srcIndex * 4 + 1];
                data[i * 4 + 2] = _clut[srcIndex * 4 + 2];
                data[i * 4 + 3] = Ps2.FromPs2Alpha(_clut[srcIndex * 4 + 3]);
            }

            return data;
        }

        public byte[] GetData() => _data;

        public static RawBitmap Read8bit(Stream stream, int width, int height) =>
            new RawBitmap(stream, width, height, true);

        public static RawBitmap Read4bitPalette1(Stream stream, int width, int height) =>
            new RawBitmap(stream, width, height, false, false);

        public static RawBitmap Read4bitPalette2(Stream stream, int width, int height) =>
            new RawBitmap(stream, width, height, false, true);

        private static byte[] GeneratePalette1(uint[] fontColors)
        {
            var palette = new byte[16 * 4];
            for (int i = 0, index = 0; i < 16; i++)
            {
                palette[index++] = (byte)((fontColors[i & 3] >> 0) & 0xFF);
                palette[index++] = (byte)((fontColors[i & 3] >> 8) & 0xFF);
                palette[index++] = (byte)((fontColors[i & 3] >> 16) & 0xFF);
                palette[index++] = (byte)((((fontColors[i & 3] >> 24) & 0xFF) + 1) / 2);
            }

            return palette;
        }

        private static byte[] GeneratePalette2(uint[] fontColors)
        {
            var palette = new byte[16 * 4];
            for (int i = 0, index = 0; i < 16; i++)
            {
                palette[index++] = (byte)((fontColors[i / 4] >> 0) & 0xFF);
                palette[index++] = (byte)((fontColors[i / 4] >> 8) & 0xFF);
                palette[index++] = (byte)((fontColors[i / 4] >> 16) & 0xFF);
                palette[index++] = (byte)((((fontColors[i / 4] >> 24) & 0xFF) + 1) / 2);
            }

            return palette;
        }
    }
}
