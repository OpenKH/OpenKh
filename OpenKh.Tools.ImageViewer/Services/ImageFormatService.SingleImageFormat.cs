using OpenKh.Imaging;
using System;
using System.IO;

namespace OpenKh.Tools.ImageViewer.Services
{
    public partial class ImageFormatService
    {
        private class SingleImageFormat : GenericImageFormat, IImageSingle
        {
            private readonly Func<Stream, IImage> read;
            private readonly Action<Stream, IImage> write;

            public SingleImageFormat(
                string name,
                string ext,
                bool isCreationSupported,
                Func<Stream, bool> isValid,
                Func<Stream, IImage> read,
                Action<Stream, IImage> write) :
                base(name, ext, false, isCreationSupported, isValid)
            {
                this.read = read;
                this.write = write;
            }

            public IImage Read(Stream stream) => read(stream);
            public void Write(Stream stream, IImage image) => write(stream, image);
        }
    }
}
