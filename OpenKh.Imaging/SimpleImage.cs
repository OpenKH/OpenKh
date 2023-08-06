using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace OpenKh.Imaging
{
    public class SimpleImage : IImageRead
    {
        public Size Size { get; private set; }
        public int Width { get => Size.Width; }
        public int Height { get => Size.Height; }
        public PixelFormat PixelFormat { get; private set; }
        public PixelFormat ClutFormat { get; private set; }
        public byte[] Data { get; private set; }
        public byte[] Clut { get; private set; }    // TODO: NULLABLE

        public SimpleImage(int width, int height, PixelFormat format, byte[] data, PixelFormat clutFormat, byte[] clut = null)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero");
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero");
            if (format == PixelFormat.Undefined)
                throw new ArgumentException("Format must not be undefined", nameof(format));
            if (format.IsIndexed() && clut == null)
                throw new ArgumentException("Clut is required when format is indexed", nameof(clut));
            if (clut != null && clutFormat == PixelFormat.Undefined)
                throw new ArgumentException("Clut format must not be undefined when a clut is used", nameof(clutFormat));
            if (clut != null && clutFormat.IsIndexed())
                throw new ArgumentException("Clut format must not be indexed", nameof(clutFormat));

            Size = new Size(width, height);
            PixelFormat = format;
            ClutFormat = clutFormat;
            Data = data.ToArray();
            if (clut != null)
                Clut = clut.ToArray();
        }

        public byte[] GetClut() => Clut;

        public byte[] GetData() => Data;
    }
}
