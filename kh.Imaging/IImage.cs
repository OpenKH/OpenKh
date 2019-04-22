using System.Drawing;

namespace kh.Imaging
{
    public interface IImage
    {
        Size Size { get; }

        PixelFormat PixelFormat { get; }
    }
}
