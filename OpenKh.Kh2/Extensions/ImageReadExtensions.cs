using OpenKh.Imaging;

namespace OpenKh.Kh2.Extensions
{
    public static class ImageReadExtensions
    {
        public static Imgd AsImgd(this IImageRead image, bool isSwizzled = false) =>
            new Imgd(image.Size, image.PixelFormat, image.GetData(), image.GetClut(), isSwizzled);
    }
}
