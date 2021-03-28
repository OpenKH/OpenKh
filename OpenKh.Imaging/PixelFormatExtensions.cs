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
    }
}
