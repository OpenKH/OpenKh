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

        public class Font
        {
            public string Name { get; }
            public IImageRead Image1 { get; }
            public IImageRead Image2 { get; }
            public FontInfo Info { get; }
            public FontCharacterInfo[] CharactersInfo { get; }

            public Font(IEnumerable<Arc.Entry> entries, string name)
            {
                Name = name;

                var mtx = RequireFileEntry(entries, $"{name}.mtx");
                var clu = RequireFileEntry(entries, $"{name}.clu");
                Info = new MemoryStream(RequireFileEntry(entries, $"{name}.inf").Data)
                    .Using(stream => FontInfo.Read(stream));

                var clut = new byte[0x40];
                Array.Copy(clu.Data, 0, clut, 0, clut.Length);
                Image1 = new Image(name, mtx, clut, Info.ImageWidth, Info.MaxImageHeight, PixelFormat.Indexed4);

                Array.Copy(clu.Data, 0x40, clut, 0, clut.Length);
                Image2 = new Image(name, mtx, clut, Info.ImageWidth, Info.MaxImageHeight, PixelFormat.Indexed4);

                CharactersInfo = new MemoryStream(RequireFileEntry(entries, $"{name}.cod").Data).Using(stream =>
                    FontCharacterInfo.Read(stream));
            }
        }

        private readonly IEnumerable<Arc.Entry> _entries;

        private FontsArc(Stream stream)
        {
            _entries = Arc.Read(stream);

            FontIcon = CreateFontIconImage(_entries, "FontIcon");
            FontCmd = new Font(_entries, "cmdfont");
            FontHelp = new Font(_entries, "helpfont");
            FontMenu = new Font(_entries, "menufont");
            FontMes = new Font(_entries, "mesfont");
            FontNumeral = new Font(_entries, "numeral");
        }

        public IImageRead FontIcon { get; }
        public Font FontCmd { get; }
        public Font FontHelp { get; }
        public Font FontMenu { get; }
        public Font FontMes { get; }
        public Font FontNumeral { get; }

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
