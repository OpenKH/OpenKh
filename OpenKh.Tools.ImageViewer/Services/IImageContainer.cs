using OpenKh.Imaging;
using System.Collections.Generic;

namespace OpenKh.Tools.ImageViewer.Services
{
    public interface IImageContainer
    {
        int Count { get; }

        IEnumerable<IImageRead> Images { get; }

        IImageRead GetImage(int index);
    }
}
