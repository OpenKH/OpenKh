using System.Drawing;

namespace OpenKh.Imaging
{
    public interface IImage
    {
        Size Size { get; }

        PixelFormat PixelFormat { get; }
    }
}
