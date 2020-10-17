using OpenKh.Imaging;
using System;

namespace OpenKh.Kh2.Extensions
{
    public static class ImageReadExtensions
    {
        public static Imgd AsImgd(this IImageRead image, bool isSwizzled = false)
        {
            switch (image.PixelFormat)
            {
                case PixelFormat.Rgba8888:
                case PixelFormat.Rgbx8888:
                    return new Imgd(image.Size, image.PixelFormat, SwapRAndB(image.GetData()), image.GetClut(), isSwizzled);
                default:
                    return new Imgd(image.Size, image.PixelFormat, image.GetData(), image.GetClut(), isSwizzled);
            }
        }

        private static byte[] SwapRAndB(byte[] inData)
        {
            var outData = new byte[inData.Length];
            for (var x = 0; x < inData.Length; x += 4)
            {
                outData[x + 0] = inData[x + 2];
                outData[x + 1] = inData[x + 1];
                outData[x + 2] = inData[x + 0];
                outData[x + 3] = inData[x + 3];
            }
            return outData;
        }
    }
}
