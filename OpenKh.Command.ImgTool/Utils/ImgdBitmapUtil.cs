using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenKh.Common;
using OpenKh.Kh2;
using static System.Drawing.Imaging.PixelFormat;

namespace OpenKh.Kh2.Utils
{
    public class ImgdBitmapUtil
    {
        public static Bitmap ToBitmap(Imgd imgd)
        {
            switch (imgd.PixelFormat)
            {
                case Imaging.PixelFormat.Indexed4:
                    {
                        var bitmap = new Bitmap(imgd.Size.Width, imgd.Size.Height, PixelFormat.Format4bppIndexed);
                        var dest = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        try
                        {
                            var sourceBits = imgd.GetData();
                            var sourceWidth = imgd.Size.Width;
                            var sourceStride = ((sourceWidth + 1) / 2) & (~1);
                            for (int y = 0; y < bitmap.Height; y++)
                            {
                                Marshal.Copy(sourceBits, sourceStride * y, dest.Scan0 + dest.Stride * y, sourceStride);
                            }
                        }
                        finally
                        {
                            bitmap.UnlockBits(dest);
                        }

                        {
                            var clut = imgd.GetClut();
                            var palette = bitmap.Palette;
                            for (int index = 0; index < 16; index++)
                            {
                                palette.Entries[index] = Color.FromArgb(
                                    clut[4 * index + 3],
                                    clut[4 * index + 0],
                                    clut[4 * index + 1],
                                    clut[4 * index + 2]
                                );
                            }
                            bitmap.Palette = palette;
                        }

                        return bitmap;
                    }
                case Imaging.PixelFormat.Indexed8:
                    {
                        var bitmap = new Bitmap(imgd.Size.Width, imgd.Size.Height, PixelFormat.Format8bppIndexed);
                        var dest = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        try
                        {
                            var sourceBits = imgd.GetData();
                            var sourceWidth = imgd.Size.Width;
                            for (int y = 0; y < bitmap.Height; y++)
                            {
                                Marshal.Copy(sourceBits, sourceWidth * y, dest.Scan0 + dest.Stride * y, sourceWidth);
                            }
                        }
                        finally
                        {
                            bitmap.UnlockBits(dest);
                        }

                        {
                            var clut = imgd.GetClut();
                            var palette = bitmap.Palette;
                            for (int index = 0; index < 256; index++)
                            {
                                palette.Entries[index] = Color.FromArgb(
                                    clut[4 * index + 3],
                                    clut[4 * index + 0],
                                    clut[4 * index + 1],
                                    clut[4 * index + 2]
                                );
                            }
                            bitmap.Palette = palette;
                        }

                        return bitmap;
                    }
                case Imaging.PixelFormat.Rgba8888:
                    {
                        var bitmap = new Bitmap(imgd.Size.Width, imgd.Size.Height, PixelFormat.Format32bppPArgb);
                        var dest = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                        try
                        {
                            var sourceBits = imgd.GetData();
                            var sourceWidth = 4 * imgd.Size.Width;
                            for (int y = 0; y < bitmap.Height; y++)
                            {
                                Marshal.Copy(sourceBits, sourceWidth * y, dest.Scan0 + dest.Stride * y, sourceWidth);
                            }
                        }
                        finally
                        {
                            bitmap.UnlockBits(dest);
                        }

                        return bitmap;
                    }
            }
            throw new NotSupportedException($"{imgd.PixelFormat} not recognized!");
        }

        class ReadAs32bppPixels
        {
            public ReadAs32bppPixels(Bitmap bitmap)
            {
                Width = bitmap.Width;
                Height = bitmap.Height;

                var src = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var srcBits = new byte[src.Stride * src.Height];
                try
                {
                    Marshal.Copy(src.Scan0, srcBits, 0, srcBits.Length);
                }
                finally
                {
                    bitmap.UnlockBits(src);
                }

                Pixels = new List<uint>(Width * Height);

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Pixels.Add(BitConverter.ToUInt32(srcBits, src.Stride * y + 4 * x));
                    }
                }

            }

            public List<uint> Pixels { get; }
            public int Width { get; }
            public int Height { get; }
        }

        static class PaletteColorUsageCounter
        {
            public static bool IfMaxColorCountIsOver(List<uint> pixels, int maxColors)
            {
                return pixels
                    .GroupBy(pixel => pixel)
                    .Count() > maxColors;
            }
        }

        class PaletteGenerator
        {
            public PaletteGenerator(List<uint> pixels, int maxColors)
            {
                MostUsedPixels = pixels
                    .GroupBy(pixel => pixel)
                    .OrderByDescending(group => group.Count())
                    .Select(group => group.Key)
                    .Take(maxColors)
                    .ToArray();

                MaxUsedColors = pixels
                    .Distinct()
                    .Count();
            }

            public uint[] MostUsedPixels { get; }
            public int MaxUsedColors { get; }

            public int FindNearest(uint pixel)
            {
                int found = Array.IndexOf(MostUsedPixels, pixel);
                if (found == -1)
                {
                    var a = (byte)(pixel >> 0);
                    var b = (byte)(pixel >> 8);
                    var c = (byte)(pixel >> 16);
                    var d = (byte)(pixel >> 24);

                    int minDistance = int.MaxValue;
                    for (int index = 0; index < MostUsedPixels.Length; index++)
                    {
                        var target = MostUsedPixels[index];
                        var A = (byte)(target >> 0);
                        var B = (byte)(target >> 8);
                        var C = (byte)(target >> 16);
                        var D = (byte)(target >> 24);
                        var distance = Math.Abs((int)a - A) + Math.Abs((int)b - B) + Math.Abs((int)c - C) + Math.Abs((int)d - D);
                        if (distance < minDistance)
                        {
                            found = index;
                            minDistance = distance;
                        }
                    }
                }
                return found;
            }
        }

        public static Imgd ToImgd(Bitmap bitmap, int bpp, Func<Bitmap, Bitmap> quantizer, bool swizzle = false)
        {
            if (quantizer != null)
            {
                var firstSrc = new ReadAs32bppPixels(bitmap);

                if (PaletteColorUsageCounter.IfMaxColorCountIsOver(firstSrc.Pixels, 1 << bpp))
                {
                    bitmap = quantizer(bitmap);
                }
            }

            switch (bpp)
            {
                case 4:
                    {
                        const int maxColors = 16;

                        var src = new ReadAs32bppPixels(bitmap);

                        var newPalette = new PaletteGenerator(src.Pixels, maxColors);

                        if (newPalette.MaxUsedColors > newPalette.MostUsedPixels.Length)
                        {
                            Console.WriteLine(
                                $"Trimming color palette entry count from {newPalette.MaxUsedColors} to ${newPalette.MostUsedPixels.Length}"
                            );
                        }

                        var destBits = new byte[(src.Width * src.Height + 1) / 2];
                        var clut = new byte[4 * maxColors];

                        for (int index = 0; index < newPalette.MostUsedPixels.Length; index++)
                        {
                            var pixel = newPalette.MostUsedPixels[index];

                            clut[4 * index + 0] = (byte)(pixel >> 16);
                            clut[4 * index + 1] = (byte)(pixel >> 8);
                            clut[4 * index + 2] = (byte)(pixel >> 0);
                            clut[4 * index + 3] = (byte)(pixel >> 24);
                        }

                        var srcPointer = 0;
                        var destPointer = 0;

                        for (int y = 0; y < src.Height; y++)
                        {
                            for (int x = 0; x < src.Width; x++)
                            {
                                var newPixel = newPalette.FindNearest(src.Pixels[srcPointer++]) & 15;
                                if (0 == (x & 1))
                                {
                                    // first pixel: hi byte
                                    destBits[destPointer] = (byte)(newPixel << 4);
                                }
                                else
                                {
                                    // second pixel: lo byte
                                    destBits[destPointer++] |= (byte)(newPixel);
                                }
                            }
                        }

                        return Imgd.Create(bitmap.Size, Imaging.PixelFormat.Indexed4, destBits, clut, swizzle);
                    }
                case 8:
                    {
                        const int maxColors = 256;

                        var src = new ReadAs32bppPixels(bitmap);

                        var newPalette = new PaletteGenerator(src.Pixels, maxColors);

                        if (newPalette.MaxUsedColors > newPalette.MostUsedPixels.Length)
                        {
                            Console.WriteLine(
                                $"Trimming color palette entry count from {newPalette.MaxUsedColors} to ${newPalette.MostUsedPixels.Length}"
                            );
                        }

                        var destBits = new byte[src.Width * src.Height];
                        var clut = new byte[4 * maxColors];

                        for (int index = 0; index < newPalette.MostUsedPixels.Length; index++)
                        {
                            var pixel = newPalette.MostUsedPixels[index];

                            clut[4 * index + 0] = (byte)(pixel >> 16);
                            clut[4 * index + 1] = (byte)(pixel >> 8);
                            clut[4 * index + 2] = (byte)(pixel >> 0);
                            clut[4 * index + 3] = (byte)(pixel >> 24);
                        }

                        var srcPointer = 0;
                        var destPointer = 0;

                        for (int y = 0; y < src.Height; y++)
                        {
                            for (int x = 0; x < src.Width; x++)
                            {
                                destBits[destPointer++] = (byte)newPalette.FindNearest(src.Pixels[srcPointer++]);
                            }
                        }

                        return Imgd.Create(bitmap.Size, Imaging.PixelFormat.Indexed8, destBits, clut, swizzle);
                    }
                case 32:
                    {
                        var src = new ReadAs32bppPixels(bitmap);

                        var destBits = new byte[4 * src.Width * src.Height];

                        var srcPointer = 0;
                        var destPointer = 0;

                        for (int y = 0; y < src.Height; y++)
                        {
                            for (int x = 0; x < src.Width; x++)
                            {
                                var pixel = src.Pixels[srcPointer++];

                                destBits[destPointer + 0] = (byte)(pixel >> 16);
                                destBits[destPointer + 1] = (byte)(pixel >> 8);
                                destBits[destPointer + 2] = (byte)(pixel >> 0);
                                destBits[destPointer + 3] = (byte)(pixel >> 24);

                                destPointer += 4;
                            }
                        }

                        return Imgd.Create(bitmap.Size, Imaging.PixelFormat.Rgba8888, destBits, new byte[0], swizzle);
                    }
            }
            throw new NotSupportedException($"BitsPerPixel {bpp} not recognized!");
        }

        public static IEnumerable<Imgd> FromFileToImgdList(string anyFile, int bitsPerPixel, Func<Bitmap, Bitmap> quantizer, bool swizzle)
        {
            switch (Path.GetExtension(anyFile).ToLowerInvariant())
            {
                case ".png":
                    {
                        yield return new Bitmap(anyFile).Using(bitmap => ToImgd(bitmap, bitsPerPixel, quantizer, swizzle));
                        break;
                    }
                case ".imd":
                    {
                        yield return File.OpenRead(anyFile).Using(stream => Imgd.Read(stream));
                        break;
                    }
                case ".imz":
                    {
                        foreach (var imgd in File.OpenRead(anyFile).Using(stream => Imgz.Read(stream)))
                        {
                            yield return imgd;
                        }
                        break;
                    }
            }
        }

        public static Imgd ToImgd(Bitmap bitmap)
        {
            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format4bppIndexed:
                    {
                        var destHeight = bitmap.Height;
                        var destStride = (bitmap.Width + 1) & (~1);
                        var destBits = new byte[destStride * destHeight];
                        var src = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                        try
                        {
                            for (int y = 0; y < bitmap.Height; y++)
                            {
                                Marshal.Copy(src.Scan0 + src.Stride * y, destBits, destStride * y, destStride);
                            }
                        }
                        finally
                        {
                            bitmap.UnlockBits(src);
                        }

                        var clut = new byte[4 * 16];
                        {
                            var palette = bitmap.Palette;
                            for (int index = 0; index < 16; index++)
                            {
                                var color = palette.Entries[index];

                                clut[4 * index + 0] = color.R;
                                clut[4 * index + 1] = color.G;
                                clut[4 * index + 2] = color.B;
                                clut[4 * index + 3] = color.A;
                            }
                            bitmap.Palette = palette;
                        }

                        return Imgd.Create(bitmap.Size, Imaging.PixelFormat.Indexed4, destBits, clut, false);
                    }
                case PixelFormat.Format8bppIndexed:
                    {
                        var destHeight = bitmap.Height;
                        var destStride = bitmap.Width;
                        var destBits = new byte[destStride * destHeight];
                        var src = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                        try
                        {
                            for (int y = 0; y < bitmap.Height; y++)
                            {
                                Marshal.Copy(src.Scan0 + src.Stride * y, destBits, destStride * y, destStride);
                            }
                        }
                        finally
                        {
                            bitmap.UnlockBits(src);
                        }

                        var clut = new byte[4 * 256];
                        {
                            var palette = bitmap.Palette;
                            for (int index = 0; index < 256; index++)
                            {
                                var color = palette.Entries[index];

                                clut[4 * index + 0] = color.R;
                                clut[4 * index + 1] = color.G;
                                clut[4 * index + 2] = color.B;
                                clut[4 * index + 3] = color.A;
                            }
                            bitmap.Palette = palette;
                        }

                        return Imgd.Create(bitmap.Size, Imaging.PixelFormat.Indexed4, destBits, clut, false);
                    }
            }
            throw new NotSupportedException($"{bitmap.PixelFormat} not recognized!");
        }
    }
}
