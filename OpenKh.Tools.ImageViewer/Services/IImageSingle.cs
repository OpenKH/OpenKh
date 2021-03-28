using OpenKh.Imaging;
using System.IO;

namespace OpenKh.Tools.ImageViewer.Services
{
    public interface IImageSingle : IImageFormat
    {
        IImageRead Read(Stream stream);

        void Write(Stream stream, IImageRead image);
    }
}
