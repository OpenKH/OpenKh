using OpenKh.Imaging;
using System.Drawing.Imaging;
using System.IO;

namespace OpenKh.Tools.Common.Imaging
{
    public static class Png
    {
        public static bool IsValid(Stream stream)
        {
            stream.Position = 0;
            return stream.ReadByte() == 0x89 &&
                stream.ReadByte() == 0x50 &&
                stream.ReadByte() == 0x4e &&
                stream.ReadByte() == 0x47 &&
                stream.ReadByte() == 0x0d &&
                stream.ReadByte() == 0x0a &&
                stream.ReadByte() == 0x1a &&
                stream.ReadByte() == 0x0a;
        }

        public static IImageRead Read(Stream stream)
        {
            stream.Position = 0; // IsValid advances 8 bytes
            return new PngImage(stream);
        }

        public static void Write(Stream stream, IImageRead image)
        {
            using (var bitmap = image.CreateBitmap())
            {
                stream.Position = 0;
                bitmap.Save(stream, ImageFormat.Png);
            }
        }
    }

    public static class Bmp
    {
        public static bool IsValid(Stream stream)
        {
            stream.Position = 0;
            return stream.ReadByte() == 0x42 &&
                stream.ReadByte() == 0x4d;
        }

        public static IImageRead Read(Stream stream) => new GdiImage(stream);

        public static void Write(Stream stream, IImageRead image)
        {
            using (var bitmap = image.CreateBitmap())
            {
                stream.Position = 0;
                bitmap.Save(stream, ImageFormat.Bmp);
            }
        }
    }

    public static class Tiff
    {
        public static bool IsValid(Stream stream)
        {
            stream.Position = 0;
            if (stream.ReadByte() == 0x49 && stream.ReadByte() == 0x49)
                return true;

            stream.Position = 0;
            if (stream.ReadByte() == 0x4d && stream.ReadByte() == 0x4d)
                return true;

            return false;
        }

        public static IImageRead Read(Stream stream) => new GdiImage(stream);

        public static void Write(Stream stream, IImageRead image)
        {
            using (var bitmap = image.CreateBitmap())
            {
                stream.Position = 0;
                bitmap.Save(stream, ImageFormat.Tiff);
            }
        }
    }
}
