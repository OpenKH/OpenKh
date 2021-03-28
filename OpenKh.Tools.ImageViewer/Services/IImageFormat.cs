using OpenKh.Imaging;
using System.IO;

namespace OpenKh.Tools.ImageViewer.Services
{
    public interface IImageFormat
    {
        string Name { get; }

        string Extension { get; }

        bool IsContainer { get; }

        bool IsCreationSupported { get; }

        bool IsValid(Stream stream);

        T As<T>() where T : IImageFormat;
    }
}
