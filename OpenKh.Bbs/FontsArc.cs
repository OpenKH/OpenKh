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

            internal Image(string name, Arc.Entry mtx, Arc.Entry clu, int width, int maxHeight, PixelFormat pixelFormat)
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

                _imageData = new byte[width * maxHeight * 8 / bpp];
                Array.Copy(mtx.Data, _imageData, Math.Min(_imageData.Length, mtx.Data.Length));
                Unswizzle(_imageData, width);

                _clutData = new byte[clu.Data.Length];
                Array.Copy(clu.Data, _clutData, _clutData.Length);
            }

            public string Name { get; }

            public Size Size { get; }

            public PixelFormat PixelFormat { get; }

            public byte[] GetClut() => _clutData;

            public byte[] GetData() => _imageData;
        }

        private readonly IEnumerable<Arc.Entry> _entries;

        private FontsArc(Stream stream)
        {
            _entries = Arc.Read(stream);

            FontCmd = CreateFontImage(_entries, "cmdfont");
            FontIcon = CreateFontIconImage(_entries, "FontIcon");
            FontHelp = CreateFontImage(_entries, "helpfont");
            FontMenu = CreateFontImage(_entries, "menufont");
            FontMes = CreateFontImage(_entries, "mesfont");
            FontNumeral = CreateFontImage(_entries, "numeral");
        }

        public IImageRead FontCmd { get; }
        public IImageRead FontIcon { get; }
        public IImageRead FontHelp { get; }
        public IImageRead FontMenu { get; }
        public IImageRead FontMes { get; }
        public IImageRead FontNumeral { get; }

        private Image CreateFontImage(IEnumerable<Arc.Entry> entries, string name)
        {
            var mtx = RequireFileEntry(entries, $"{name}.mtx");
            var clu = RequireFileEntry(entries, $"{name}.clu");
            var inf = new MemoryStream(RequireFileEntry(entries, $"{name}.inf").Data)
                .Using(stream => FontInfo.Read(stream));
            var cod = RequireFileEntry(entries, $"{name}.cod");

            return new Image(name, mtx, clu, inf.ImageWidth, inf.MaxImageHeight, PixelFormat.Indexed4);
        }

        private Image CreateFontIconImage(IEnumerable<Arc.Entry> entries, string name)
        {
            var mtx = RequireFileEntry(entries, $"{name}.mtx");
            var clu = RequireFileEntry(entries, $"{name}.clu");
            var inf = new MemoryStream(RequireFileEntry(entries, $"{name}.inf").Data)
                .Using(stream => FontIconInfo.Read(stream));

            return new Image(name, mtx, clu, 256, 64, PixelFormat.Indexed8);
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
            return data;
        }

        public static bool IsValid(Stream stream) => Arc.IsValid(stream);
        public static FontsArc Read(Stream stream) => new FontsArc(stream);
    }
}
