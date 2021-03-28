using OpenKh.Imaging;
using System;

namespace OpenKh.Engine.Extensions
{
    public static class ImageReadExtensions
    {
        private static readonly byte[] Bgra = new byte[] { 2, 1, 0, 3 };
        private static readonly byte[] Rgba = new byte[] { 0, 1, 2, 3 };

        public static byte[] AsBgra8888(this IImageRead image)
        {
            switch (image.PixelFormat)
            {
                case PixelFormat.Indexed4:
                    return ImageDataHelpers.FromIndexed4ToBitmap32(
                        image.GetData(), image.GetClut(), Bgra);
                case PixelFormat.Indexed8:
                    return ImageDataHelpers.FromIndexed8ToBitmap32(
                        image.GetData(), image.GetClut(), Bgra);
                case PixelFormat.Rgba8888:
                    return ImageDataHelpers.FromBitmap32(image.GetData(), Rgba);
                default:
                    throw new ArgumentException($"The pixel format {image.PixelFormat} is not supported.");
            }
        }

        public static byte[] AsRgba8888(this IImageRead image)
        {
            switch (image.PixelFormat)
            {
                case PixelFormat.Indexed4:
                    return ImageDataHelpers.FromIndexed4ToBitmap32(
                        image.GetData(), image.GetClut(), Rgba);
                case PixelFormat.Indexed8:
                    return ImageDataHelpers.FromIndexed8ToBitmap32(
                        image.GetData(), image.GetClut(), Rgba);
                case PixelFormat.Rgba8888:
                    return ImageDataHelpers.FromBitmap32(image.GetData(), Bgra);
                default:
                    throw new ArgumentException($"The pixel format {image.PixelFormat} is not supported.");
            }
        }
    }
}
