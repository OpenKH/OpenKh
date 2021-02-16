using OpenKh.Imaging;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.ImageViewer.Services
{
    public partial class ImageFormatService
    {
        internal class ImageContainer : IImageContainer
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
    }
}
