using OpenKh.Imaging;
using System;

namespace OpenKh.Kh2.Extensions
{
    public static class ImageReadExtensions
    {
        public static Imgd AsImgd(this IImageRead image, bool isSwizzled = false)
        {
            if (image is Imgd imgd)
            {
                return imgd;
            }
            else
            {
                switch (image.PixelFormat)
                {
                    case PixelFormat.Rgba8888:
                    case PixelFormat.Rgbx8888:
                        return new Imgd(image.Size, image.PixelFormat, GetAsRGBA(image.GetData()), image.GetClut(), isSwizzled);
                    default:
                        return new Imgd(image.Size, image.PixelFormat, image.GetData(), image.GetClut(), isSwizzled);
                }
            }
        }

        private static byte[] GetAsRGBA(byte[] inData)
        {
            var outData = new byte[inData.Length];
            for (var x = 0; x < inData.Length; x += 4)
            {
                outData[x + 0] = inData[x + 2]; // B to R
                outData[x + 1] = inData[x + 1]; // G to G
                outData[x + 2] = inData[x + 0]; // R to B
                outData[x + 3] = inData[x + 3]; // A
            }
            return outData;
        }
    }
}
