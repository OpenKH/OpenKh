using OpenKh.Imaging;
using System.IO;

namespace OpenKh.Tools.ImageViewer.Services
{
    public interface IImageFormat
    {
        string Name { get; }

        string Extension { get; }

        bool IsValid(Stream stream);

        IImageRead Read(Stream stream);

        void Write(Stream stream, IImageRead image);
    }
}
