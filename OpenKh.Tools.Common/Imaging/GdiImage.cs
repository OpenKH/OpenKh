using OpenKh.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenKh.Tools.Common.Imaging
{
    internal class GdiImage : IImageRead
    {
        private readonly Bitmap _bitmap;

        public GdiImage(Stream stream)
        {
            stream.Position = 0;
            _bitmap = new Bitmap(stream);
        }

        public Size Size => _bitmap.Size;

        public OpenKh.Imaging.PixelFormat PixelFormat => _bitmap.PixelFormat.GetPixelFormat();

        public byte[] GetClut()
        {
            var palette = _bitmap.Palette?.Entries ?? new Color[0];
            var clut = new byte[palette.Length * 4];

            for (var i = 0; i < palette.Length; i++)
            {
                var color = palette[i];
                clut[i * 4 + 0] = color.R;
                clut[i * 4 + 1] = color.G;
                clut[i * 4 + 2] = color.B;
                clut[i * 4 + 3] = color.A;
            }

            return clut;
        }

        public byte[] GetData()
        {
            var rect = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);
            var bitmapData = _bitmap.LockBits(rect, ImageLockMode.ReadOnly, _bitmap.PixelFormat);

            var dstData = new byte[bitmapData.Stride * bitmapData.Height];
            Marshal.Copy(bitmapData.Scan0, dstData, 0, dstData.Length);

            _bitmap.UnlockBits(bitmapData);

            return dstData;
        }
    }
}
