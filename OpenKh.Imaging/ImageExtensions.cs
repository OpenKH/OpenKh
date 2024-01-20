using System;

namespace OpenKh.Imaging
{
    public static class ImageExtensions
    {
        public static byte[] ToBgra32(this IImage imageRead)
        {
            switch (imageRead.PixelFormat)
            {
                case PixelFormat.Indexed4:
                    return ImageDataHelpers.FromIndexed4ToBitmap32(imageRead.GetData(), imageRead.GetClut(),
                        ImageDataHelpers.RGBA);
                case PixelFormat.Indexed8:
                    return ImageDataHelpers.FromIndexed8ToBitmap32(imageRead.GetData(), imageRead.GetClut(),
                        ImageDataHelpers.RGBA);
                case PixelFormat.Rgb888:
                    return ImageDataHelpers.FromRgb888ToBgra32(imageRead.GetData());
                case PixelFormat.Rgba8888:
                    return imageRead.GetData();
                default:
                    throw new NotImplementedException($"The PixelFormat {imageRead.PixelFormat} cannot be converted to a Bgra32.");
            }
        }
    }
}
