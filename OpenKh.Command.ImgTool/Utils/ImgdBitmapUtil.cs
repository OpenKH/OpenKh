using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using static System.Drawing.Imaging.PixelFormat;

namespace OpenKh.Kh2.Utils
{
    public class ImgdBitmapUtil
    {
        class ReadAs32bppPixels
        {
            public ReadAs32bppPixels(IImageRead bitmap)
            {
                Width = bitmap.Size.Width;
                Height = bitmap.Size.Height;

                Pixels = new uint[Width * Height];

                Buffer.BlockCopy(bitmap.ToBgra32(), 0, Pixels, 0, 4 * Pixels.Length);
            }

            public uint[] Pixels { get; }
            public int Width { get; }
            public int Height { get; }
        }

        static class PaletteColorUsageCounter
        {
            public static bool IfMaxColorCountIsOver(uint[] pixels, int maxColors)
            {
                return pixels
                    .GroupBy(pixel => pixel)
                    .Count() > maxColors;
            }
        }

        class PaletteGenerator
        {
            public PaletteGenerator(uint[] pixels, int maxColors)
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

        public static Imgd ToImgd(IImageRead bitmap, int bpp, Func<IImageRead, IImageRead> quantizer, bool swizzle = false)
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

        public static IEnumerable<Imgd> FromFileToImgdList(string anyFile, int bitsPerPixel, Func<IImageRead, IImageRead> quantizer, bool swizzle)
        {
            switch (Path.GetExtension(anyFile).ToLowerInvariant())
            {
                case ".png":
                {
                    yield return ToImgd(PngImage.Read(new MemoryStream(File.ReadAllBytes(anyFile))), bitsPerPixel, quantizer, swizzle);
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
    }
}
