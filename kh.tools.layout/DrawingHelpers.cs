using kh.Imaging;
using System;
using Xe.Drawing;

namespace kh.tools.layout
{
    public static partial class DrawingHelpers
    {
        public static ISurface CreateSurface(this IDrawing drawing, IImageRead image) => drawing
            .CreateSurface(image.Size.Width,
                image.Size.Height,
                Xe.Drawing.PixelFormat.Format32bppArgb,
                SurfaceType.Input,
                GetDataResource(image));

        private static DataResource GetDataResource(IImageRead image)
        {
            byte[] data;
            switch (image.PixelFormat)
            {
                case Imaging.PixelFormat.Indexed4:
                    data = GetDataResource4bpp(image);
                    break;
                case Imaging.PixelFormat.Indexed8:
                    data = GetDataResource8bpp(image);
                    break;
                default:
                    throw new ArgumentException($"The pixel format {image.PixelFormat} is not supported.");
            }

            return new DataResource
            {
                Data = data,
                Stride = image.Size.Width * 4
            };
        }

        private unsafe static byte[] GetDataResource4bpp(IImageRead image)
        {
            var size = image.Size;
            var data = image.GetData();
            var clut = image.GetClut();
            var dstData = new byte[size.Width * size.Height * sizeof(uint)];
            var srcIndex = 0;
            var dstIndex = 0;

            for (var y = 0; y < size.Height; y++)
            {
                for (var i = 0; i < size.Width / 2; i++)
                {
                    var ch = data[srcIndex++];
                    var palIndex1 = (ch >> 4);
                    var palIndex2 = (ch & 15);
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 2];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 1];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 0];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 3];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 2];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 1];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 0];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 3];
                }
            }

            return dstData;
        }

        private unsafe static byte[] GetDataResource8bpp(IImageRead image)
        {
            var size = image.Size;
            var data = image.GetData();
            var clut = image.GetClut();
            var dstData = new byte[size.Width * size.Height * sizeof(uint)];
            var srcIndex = 0;
            var dstIndex = 0;

            for (var y = 0; y < size.Height; y++)
            {
                for (var i = 0; i < size.Width; i++)
                {
                    var palIndex = data[srcIndex++];
                    dstData[dstIndex++] = clut[palIndex * 4 + 2];
                    dstData[dstIndex++] = clut[palIndex * 4 + 1];
                    dstData[dstIndex++] = clut[palIndex * 4 + 0];
                    dstData[dstIndex++] = clut[palIndex * 4 + 3];
                }
            }

            return dstData;
        }
    }
}
