using OpenKh.Imaging;
using System;
using System.IO;

namespace OpenKh.Tools.ImageViewer.Services
{
    public partial class ImageFormatService
    {
        private class SingleImageFormat : GenericImageFormat, IImageSingle
        {
            private readonly Func<Stream, IImageRead> read;
            private readonly Action<Stream, IImageRead> write;

            public SingleImageFormat(
                string name,
                string ext,
                bool isCreationSupported,
                Func<Stream, bool> isValid,
                Func<Stream, IImageRead> read,
                Action<Stream, IImageRead> write) :
                base(name, ext, false, isCreationSupported, isValid)
            {
                this.read = read;
                this.write = write;
            }

            public IImageRead Read(Stream stream) => read(stream);
            public void Write(Stream stream, IImageRead image) => write(stream, image);
        }
    }
}
