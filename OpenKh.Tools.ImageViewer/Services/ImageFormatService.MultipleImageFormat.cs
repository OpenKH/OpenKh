using System;
using System.IO;

namespace OpenKh.Tools.ImageViewer.Services
{
    public partial class ImageFormatService
    {
        private class MultipleImageFormat : GenericImageFormat, IImageMultiple
        {
            private readonly Func<Stream, IImageContainer> read;
            private readonly Action<Stream, IImageContainer> write;

            public MultipleImageFormat(
                string name,
                string ext,
                bool isCreationSupported,
                Func<Stream, bool> isValid,
                Func<Stream, IImageContainer> read,
                Action<Stream, IImageContainer> write) :
                base(name, ext, true, isCreationSupported, isValid)
            {
                this.read = read;
                this.write = write;
            }

            public IImageContainer Read(Stream stream) => read(stream);
            public void Write(Stream stream, IImageContainer image) => write(stream, image);
        }
    }
}
