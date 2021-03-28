using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.ImageViewer.Services
{
    public interface IImageFormatService
    {
        IEnumerable<IImageFormat> Formats { get; }

        IImageFormat GetFormatByFileName(string fileName);

        IImageFormat GetFormatByContent(Stream stream);
    }
}
