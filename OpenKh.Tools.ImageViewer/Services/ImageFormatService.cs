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

            public GenericImageFormat(
                string name,
                string ext,
                bool isContainer,
                bool isCreationSupported,
                Func<Stream, bool> isValid)
            {
                Name = name;
                Extension = ext;
                IsContainer = isContainer;
                IsCreationSupported = isCreationSupported;
                this.isValid = isValid;
            }

            public string Name { get; }
            public string Extension { get; }
            public bool IsContainer { get; }
            public bool IsCreationSupported { get; }
            public T As<T>() where T : IImageFormat => (T)(object)this;

            public bool IsValid(Stream stream) => isValid(stream);
        }

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
                base(name, ext, isCreationSupported, true, isValid)
            {
                this.read = read;
                this.write = write;
            }

            public IImageContainer Read(Stream stream) => read(stream);
            public void Write(Stream stream, IImageContainer image) => write(stream, image);
        }

        private class ImageContainer : IImageContainer
        {
            private readonly IImageRead[] _images;

            public ImageContainer(IEnumerable<IImageRead> images)
            {
                _images = images.ToArray();
            }

            public int Count => _images.Length;
            public IEnumerable<IImageRead> Images => _images;
            public IImageRead GetImage(int index) => _images[index];
        }

        private static readonly IImageFormat[] imageFormat;

        static ImageFormatService()
        {
            imageFormat = new IImageFormat[]
            {
                GetImageFormat("IMGD", "imd", false, Imgd.IsValid, Imgd.Read, (stream, image) =>
                    new Imgd(image.Size, image.PixelFormat, image.GetData(), image.GetClut(), false)),

                GetImageFormat("IMGZ", "imz", true, Imgz.IsValid, s => Imgz.Read(s), (stream, container) =>
                    throw new NotImplementedException()),

                GetImageFormat("TIM2", "tm2", true, Tm2.IsValid, s => Tm2.Read(s), (stream, container) =>
                    throw new NotImplementedException()),
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

        private static IImageFormat GetImageFormat(
            string name,
            string extension,
            bool isCreationSupported,
            Func<Stream, bool> isValid,
            Func<Stream, IImageRead> read,
            Action<Stream, IImageRead> write)
        {
            return new SingleImageFormat(name, extension, isCreationSupported, isValid, read, write);
        }

        private static IImageFormat GetImageFormat(
            string name,
            string extension,
            bool isCreationSupported,
            Func<Stream, bool> isValid,
            Func<Stream, IEnumerable<IImageRead>> read,
            Action<Stream, IEnumerable<IImageRead>> write)
        {
            return new MultipleImageFormat(name, extension, isCreationSupported, isValid,
                stream => new ImageContainer(read(stream)),
                (stream, container) => write(stream, container.Images));
        }
    }
}
