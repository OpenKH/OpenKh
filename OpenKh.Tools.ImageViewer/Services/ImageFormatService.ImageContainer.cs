using OpenKh.Imaging;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.ImageViewer.Services
{
    public partial class ImageFormatService
    {
        internal class ImageContainer : IImageContainer
        {
            private readonly IImage[] _images;

            public ImageContainer(IEnumerable<IImage> images)
            {
                _images = images.ToArray();
            }

            public int Count => _images.Length;
            public IEnumerable<IImage> Images => _images;
            public IImage GetImage(int index) => _images[index];
        }
    }
}
