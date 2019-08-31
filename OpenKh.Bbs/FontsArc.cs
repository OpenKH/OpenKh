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
            private const int Width = 128;
            private readonly byte[] _imageData;
            private readonly byte[] _clutData;

            internal Image(string name, Arc.Entry mtx, Arc.Entry clu)
            {
                Name = name;

                _imageData = new byte[mtx.Data.Length];
                Array.Copy(mtx.Data, _imageData, _imageData.Length);

                _clutData = new byte[clu.Data.Length];
                Array.Copy(clu.Data, _clutData, _clutData.Length);
            }

            public string Name { get; }

            public Size Size => new Size(Width, _imageData.Length * 2 / Width);

            public PixelFormat PixelFormat => PixelFormat.Indexed4;

            public byte[] GetClut() => _clutData;

            public byte[] GetData() => _imageData;
        }

        private readonly IEnumerable<Arc.Entry> _entries;

        private FontsArc(Stream stream)
        {
            _entries = Arc.Read(stream);
        }

        public IEnumerable<IImageRead> Images =>
            _entries
            .Select(x => new
            {
                Name = Path.GetFileNameWithoutExtension(x.Name),
                Extension = Path.GetExtension(x.Name),
                Entry = x
            })
            .GroupBy(x => x.Name)
            .Select(x => new Image(x.Key, x.First(img => img.Extension == ".mtx").Entry, x.First(img => img.Extension == ".clu").Entry));

        public static bool IsValid(Stream stream) => Arc.IsValid(stream);
        public static FontsArc Read(Stream stream) => new FontsArc(stream);
    }
}
