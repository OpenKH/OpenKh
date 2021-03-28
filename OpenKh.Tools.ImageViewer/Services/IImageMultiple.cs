using System.IO;

namespace OpenKh.Tools.ImageViewer.Services
{
    public interface IImageMultiple : IImageFormat
    {
        IImageContainer Read(Stream stream);

        void Write(Stream stream, IImageContainer image);
    }
}
