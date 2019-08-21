using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Imaging;
using OpenKh.Kh2;

namespace OpenKh.Tools.ImageViewer.Services
{
    public class ImageFormatService : IImageFormatService
    {
        private class GenericImageFormat : IImageFormat
        {
            private readonly Func<Stream, bool> isValid;
            private readonly Func<Stream, IImageRead> read;
            private readonly Action<Stream, IImageRead> write;

            public GenericImageFormat(
                string name,
                string ext,
                Func<Stream, bool> isValid,
                Func<Stream, IImageRead> read,
                Action<Stream, IImageRead> write)
            {
                Name = name;
                Extension = ext;
                this.isValid = isValid;
                this.read = read;
                this.write = write;
            }

            public string Name { get; }
            public string Extension { get; }
            public bool IsValid(Stream stream) => isValid(stream);
            public IImageRead Read(Stream stream) => read(stream);
            public void Write(Stream stream, IImageRead image) => write(stream, image);
        }

        private static readonly IImageFormat[] imageFormat;

        static ImageFormatService()
        {
            imageFormat = new IImageFormat[]
            {
                new GenericImageFormat("IMGD", "imgd", Imgd.IsValid, Imgd.Read, (stream, image) =>
                    new Imgd(image.Size, image.PixelFormat, image.GetData(), image.GetClut(), false)),
            };
        }

        public IEnumerable<IImageFormat> Formats => imageFormat;

        public IImageFormat GetFormatByContent(Stream stream) =>
            imageFormat.FirstOrDefault(x => x.IsValid(stream));

        public IImageFormat GetFormatByFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var dotIndex = extension.IndexOf('.');
            if (dotIndex >= 0)
                extension = extension.Substring(dotIndex);

            return imageFormat.FirstOrDefault(x => string.Compare(x.Extension, extension, System.StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
