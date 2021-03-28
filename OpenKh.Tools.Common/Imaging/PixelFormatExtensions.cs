using System;

namespace OpenKh.Imaging
{
    public static class PixelFormatExtensions
    {
        internal static System.Drawing.Imaging.PixelFormat GetDrawingPixelFormat(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Indexed4:
                    return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
                case PixelFormat.Indexed8:
                    return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                case PixelFormat.Rgba1555:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                case PixelFormat.Rgb888:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                case PixelFormat.Rgbx8888:
                    return System.Drawing.Imaging.PixelFormat.Format32bppRgb;
                case PixelFormat.Rgba8888:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                default:
                    throw new NotImplementedException(
                        $"The pixel format {pixelFormat} is not implemented.");
            }
        }

        internal static PixelFormat GetPixelFormat(this System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                    return PixelFormat.Indexed4;
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return PixelFormat.Indexed8;
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormat.Rgb888;
                case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                    return PixelFormat.Rgba1555;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormat.Rgba8888;
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                    return PixelFormat.Rgbx8888;
                default:
                    throw new NotImplementedException(
                        $"The pixel format {pixelFormat} is not implemented.");
            }
        }
    }
}
