using OpenKh.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Common.Wpf
{
    public static class ImageReadExtensions
    {
        public static BitmapSource GetBimapSource(this IImage imageRead)
        {
            const double dpi = 96.0;

            var size = imageRead.Size;
            var data = imageRead.ToBgra32();
            return BitmapSource.Create(size.Width, size.Height, dpi, dpi, PixelFormats.Bgra32, null, data, size.Width * 4);
        }
    }
}
