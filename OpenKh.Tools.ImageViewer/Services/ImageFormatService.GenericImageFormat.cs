using System;
using System.IO;

namespace OpenKh.Tools.ImageViewer.Services
{
    public partial class ImageFormatService
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
    }
}
