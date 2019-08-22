using System;

namespace OpenKh.Imaging
{
    public static class PixelFormatExtensions
    {
        public static bool IsIndexed(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Indexed4:
                case PixelFormat.Indexed8:
                    return true;
                default:
                    return false;
            }
        }

        internal static System.Drawing.Imaging.PixelFormat GetDrawingPixelFormat(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Indexed4:
                    return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
                case PixelFormat.Indexed8:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                case PixelFormat.Rgba8888:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                default:
                    throw new NotImplementedException(
                        $"The reading from pixel format {pixelFormat} is not implemented.");
            }
        }
    }
}
