using OpenKh.Common;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace OpenKh.Bbs
{
    public class FontsArc
    {
        private class Image : IImageRead
        {
            private readonly byte[] _imageData;
            private readonly byte[] _clutData;

            internal Image(string name, Arc.Entry mtx, byte[] clut, int width, int maxHeight, PixelFormat pixelFormat)
            {
                Name = name;
                Size = new Size(width, maxHeight);
                PixelFormat = pixelFormat;

                var bpp = 0;
                switch (pixelFormat)
                {
                    case PixelFormat.Indexed4:
                        bpp = 4;
                        break;
                    case PixelFormat.Indexed8:
                        bpp = 8;
                        break;
                }

                _imageData = new byte[width * maxHeight * bpp / 8];
                Array.Copy(mtx.Data, _imageData, Math.Min(_imageData.Length, mtx.Data.Length));
                _imageData = Unswizzle(_imageData, width * bpp / 8);
                if (pixelFormat == PixelFormat.Indexed4)
                    InvertEndianess(_imageData);

                _clutData = new byte[clut.Length];
                Array.Copy(clut, _clutData, _clutData.Length);
            }

            public string Name { get; }

            public Size Size { get; }

            public PixelFormat PixelFormat { get; }

            public byte[] GetClut() => _clutData;

            public byte[] GetData() => _imageData;
        }

        private readonly IEnumerable<Arc.Entry> _entries;
        private readonly Image _fontCmd1;
        private readonly Image _fontCmd2;
        private readonly Image _fontHelp1;
        private readonly Image _fontHelp2;
        private readonly Image _fontMenu1;
        private readonly Image _fontMenu2;
        private readonly Image _fontMes1;
        private readonly Image _fontMes2;
        private readonly Image _fontNumeral1;
        private readonly Image _fontNumeral2;

        private FontsArc(Stream stream)
        {
            _entries = Arc.Read(stream);

            FontIcon = CreateFontIconImage(_entries, "FontIcon");
            CreateFontImage(_entries, "cmdfont", out _fontCmd1, out _fontCmd2);
            CreateFontImage(_entries, "helpfont", out _fontHelp1, out _fontHelp2);
            CreateFontImage(_entries, "menufont", out _fontMenu1, out _fontMenu2);
            CreateFontImage(_entries, "mesfont", out _fontMes1, out _fontMes2);
            CreateFontImage(_entries, "numeral", out _fontNumeral1, out _fontNumeral2);
        }

        public IImageRead FontIcon { get; }
        public IImageRead FontCmd => _fontCmd1;
        public IImageRead FontCmd2 => _fontCmd2;
        public IImageRead FontHelp => _fontHelp1;
        public IImageRead FontHelp2 => _fontHelp2;
        public IImageRead FontMenu => _fontMenu1;
        public IImageRead FontMenu2 => _fontMenu2;
        public IImageRead FontMes => _fontMes1;
        public IImageRead FontMes2 => _fontMes2;
        public IImageRead FontNumeral => _fontNumeral1;
        public IImageRead FontNumeral2 => _fontNumeral2;

        private void CreateFontImage(IEnumerable<Arc.Entry> entries, string name, out Image image1, out Image image2)
        {
            var mtx = RequireFileEntry(entries, $"{name}.mtx");
            var clu = RequireFileEntry(entries, $"{name}.clu");
            var inf = new MemoryStream(RequireFileEntry(entries, $"{name}.inf").Data)
                .Using(stream => FontInfo.Read(stream));
            var cod = RequireFileEntry(entries, $"{name}.cod");

            var clut = new byte[0x40];
            Array.Copy(clu.Data, 0, clut, 0, clut.Length);
            image1 = new Image(name, mtx, clut, inf.ImageWidth, inf.MaxImageHeight, PixelFormat.Indexed4);

            Array.Copy(clu.Data, 0x40, clut, 0, clut.Length);
            image2 = new Image(name, mtx, clut, inf.ImageWidth, inf.MaxImageHeight, PixelFormat.Indexed4);
        }

        private Image CreateFontImage(IEnumerable<Arc.Entry> entries, string name)
        {
            var mtx = RequireFileEntry(entries, $"{name}.mtx");
            var clu = RequireFileEntry(entries, $"{name}.clu");
            var inf = new MemoryStream(RequireFileEntry(entries, $"{name}.inf").Data)
                .Using(stream => FontInfo.Read(stream));
            var cod = RequireFileEntry(entries, $"{name}.cod");

            return new Image(name, mtx, clu.Data, inf.ImageWidth, inf.MaxImageHeight, PixelFormat.Indexed4);
        }

        private Image CreateFontIconImage(IEnumerable<Arc.Entry> entries, string name)
        {
            var mtx = RequireFileEntry(entries, $"{name}.mtx");
            var clu = RequireFileEntry(entries, $"{name}.clu");
            var inf = new MemoryStream(RequireFileEntry(entries, $"{name}.inf").Data)
                .Using(stream => FontIconInfo.Read(stream));

            return new Image(name, mtx, clu.Data, 256, 64, PixelFormat.Indexed8);
        }

        private static Arc.Entry RequireFileEntry(IEnumerable<Arc.Entry> entries, string name)
        {
            var entry = entries.FirstOrDefault(x => x.Name == name);
            if (entry == null)
                throw new FileNotFoundException($"ARC does not contain the required file {name}.", name);

            return entry;
        }

        private static byte[] Unswizzle(byte[] data, int width)
        {
            var dst = new byte[data.Length];

            for (var i = 0; i < data.Length; i += 16)
            {
                var srcIndex = i;
                var dstIndex = srcIndex % 0x10;
                dstIndex += srcIndex / 0x10 % 8 * width;
                dstIndex += srcIndex / 0x80 % (width / 16) * 16;
                dstIndex += srcIndex / (width * 8) * width * 8;

                Array.Copy(data, srcIndex, dst, dstIndex, 16);
            }

            return dst;
        }

        private static void InvertEndianess(byte[] data)
        {
            for (var i = 0; i < data.Length; i++)
                data[i] = (byte)(((data[i] & 15) << 4) | (data[i] >> 4));
        }

        public static bool IsValid(Stream stream) => Arc.IsValid(stream);
        public static FontsArc Read(Stream stream) => new FontsArc(stream);
    }
}
