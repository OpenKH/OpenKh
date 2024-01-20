using OpenKh.Imaging;
using System.IO;

namespace OpenKh.Tools.ImageViewer.Services
{
    public interface IImageSingle : IImageFormat
    {
        IImage Read(Stream stream);

        void Write(Stream stream, IImage image);
    }
}
