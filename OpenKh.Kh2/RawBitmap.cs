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

        private readonly byte[] _data;
        private readonly byte[] _clut;

        private RawBitmap(Stream stream, int width, int height)
        {
            Size = new Size(width, height);

            var reader = new BinaryReader(stream);
            _data = reader.ReadBytes(width * height);
            _clut = reader.ReadBytes(PaletteCount * BitsPerColor / 8);
        }

        public Size Size { get; }

        public PixelFormat PixelFormat => PixelFormat.Indexed8;

        public byte[] GetClut()
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

        public static RawBitmap Read(Stream stream, int width, int height) =>
            new RawBitmap(stream, width, height);
    }
}
