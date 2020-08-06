using nQuant;
using OpenKh.Command.ImgTool.Interfaces;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace OpenKh.Command.ImgTool.Utils
{
    static class QuantizerFactory
    {
        public static Func<Bitmap, Bitmap> MakeFrom(ICommonQuantizerParam param)
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
                                    bitmap.Save(temp, ImageFormat.Png);
                                    temp.Position = 0;
                                    temp.CopyTo(p.StandardInput.BaseStream);
                                    p.StandardInput.BaseStream.Close(); // prevent pngquant from blocking.
                                }
                                var newBitmap = new Bitmap(p.StandardOutput.BaseStream);
                                return newBitmap;
                            }
                            else
                            {
                                return (Bitmap)new WuQuantizer().QuantizeImage(
                                    new Bitmap(bitmap), // make sure it is 32 bpp, not 24 bpp
                                    alphaThreshold: 10, // default
                                    alphaFader: 70, // default
                                    maxColors
                                );
                                // I believe that nQuant never throws exception.
                            }
                        }
                }
                return bitmap;
            };
        }
    }
}
