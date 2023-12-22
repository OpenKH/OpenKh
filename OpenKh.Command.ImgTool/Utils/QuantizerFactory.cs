using nQuant.Core.TrueColor;
using OpenKh.Command.ImgTool.Interfaces;
using OpenKh.Imaging;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace OpenKh.Command.ImgTool.Utils
{
    public class QuantizerFactory
    {
        private class CommonQuantizerParam : ICommonQuantizerParam
        {
            public int BitsPerPixel { get; set; }

            public bool PngQuant { get; set; }
        }

        public static Func<IImageRead, IImageRead> MakeFrom(int BitsPerPixel, bool InvokePngquant) =>
            MakeFrom(
                new CommonQuantizerParam
                {
                    BitsPerPixel = BitsPerPixel,
                    PngQuant = InvokePngquant,
                }
            );

        public static Func<IImageRead, IImageRead> MakeFrom(ICommonQuantizerParam param)
        {
            return bitmap =>
            {
                switch (param.BitsPerPixel)
                {
                    case 4:
                    case 8:
                    {
                        var maxColors = 1 << param.BitsPerPixel;

                        if (param.PngQuant)
                        {
                            var psi = new ProcessStartInfo("pngquant.exe", $"{maxColors} -")
                            {
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                RedirectStandardInput = true,
                                RedirectStandardOutput = true,
                            };
                            using var p = Process.Start(psi);
                            using (var temp = new MemoryStream())
                            {
                                PngImage.Write(temp, bitmap);
                                temp.Position = 0;
                                temp.CopyTo(p.StandardInput.BaseStream);
                                p.StandardInput.BaseStream.Close(); // prevent pngquant from blocking.
                            }
                            using (var temp = new MemoryStream())
                            {
                                p.StandardOutput.BaseStream.CopyTo(temp);
                                temp.Position = 0;
                                return PngImage.Read(temp);
                            }
                        }
                        else
                        {
                            var intArray = new int[bitmap.Size.Width * bitmap.Size.Height];
                            Buffer.BlockCopy(bitmap.ToBgra32(), 0, intArray, 0, 4 * intArray.Length);

                            var input = new BitmapInput
                            {
                                Width = bitmap.Size.Width,
                                Height = bitmap.Size.Height,
                                PixelLines = Enumerable.Range(0, bitmap.Size.Height)
                                    .Select(
                                        index => intArray
                                            .Skip(bitmap.Size.Width * index)
                                            .Take(bitmap.Size.Width)
                                            .ToArray()
                                    )
                            };

                            var output = new BitmapQuantizer().QuantizeImage(
                                input,
                                alphaThreshold: -1,
                                alphaFader: 1,
                                maxColors
                            );

                            return ConvertFrom(output);
                        }
                    }
                }
                return bitmap;
            };
        }

        private class LocalImager : IImageRead
        {
            public Size Size { get; set; }
            public PixelFormat PixelFormat { get; set; }

            internal byte[] Clut { get; set; }
            internal byte[] Data { get; set; }

            public byte[] GetClut() => Clut;
            public byte[] GetData() => Data;
        }

        private static IImageRead ConvertFrom(BitmapOutput output)
        {
            var size = new Size(output.Width, output.Height);
            switch (output.BitsPixel)
            {
                case 4:
                    return new LocalImager
                    {
                        Size = size,
                        PixelFormat = PixelFormat.Indexed4,
                        Clut = ConvertClutFrom(output.Palette),
                        Data = ConvertDataFrom(output.PixelLines, (output.Width + 1) / 2, output.Height),
                    };
                case 8:
                    return new LocalImager
                    {
                        Size = size,
                        PixelFormat = PixelFormat.Indexed8,
                        Clut = ConvertClutFrom(output.Palette),
                        Data = ConvertDataFrom(output.PixelLines, output.Width, output.Height),
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        private static byte[] ConvertDataFrom(IEnumerable<byte[]> pixelLines, int stride, int height)
        {
            var pitch = ((stride + 3) & ~3);
            var buf = new byte[pitch * height];
            int y = 0;
            foreach (var line in pixelLines)
            {
                Buffer.BlockCopy(line, 0, buf, pitch * y, stride);
                y++;
            }
            return buf;
        }

        private static byte[] ConvertClutFrom(int[] palette)
        {
            var buf = new byte[4 * palette.Length];
            Buffer.BlockCopy(palette, 0, buf, 0, buf.Length);
            return buf;
        }
    }
}
