using System;

namespace OpenKh.Imaging
{
    public static class ImageExtensions
    {
        public static byte[] ToBgra32(this IImageRead imageRead)
        {
            switch (imageRead.PixelFormat)
            {
                case PixelFormat.Indexed4:
                    return ImageDataHelpers.FromIndexed4ToBgra(imageRead.GetData(), imageRead.GetClut());
                case PixelFormat.Indexed8:
                    return ImageDataHelpers.FromIndexed8ToBgra(imageRead.GetData(), imageRead.GetClut());
                case PixelFormat.Rgba8888:
                    return imageRead.GetData();
                default:
                    throw new NotImplementedException($"The PixelFormat {imageRead.PixelFormat} cannot be converted to a Bgra32.");
            }
        }
    }
}
