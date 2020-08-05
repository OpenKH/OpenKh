using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using NLog;
using OpenKh.Kh2;

namespace OpenKh.Command.MapGen.Utils
{
    class ImageResizer
    {
        private static readonly int[] AllowedTextureWidthList = new int[] { 128, 256, 512 };
        private static readonly int[] AllowedTextureHeightList = new int[] { 64, 128, 256, 512 };
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static Imgd NormalizeImageSize(Imgd imgd)
        {
            if (false
                || !AllowedTextureWidthList.Contains(imgd.Size.Width)
                || !AllowedTextureHeightList.Contains(imgd.Size.Height)
            )
            {
                var newWidth = (imgd.Size.Width <= 128) ? 128
                    : (imgd.Size.Width <= 256) ? 256
                    : 512;

                var newHeight = (imgd.Size.Height <= 64) ? 64
                    : (imgd.Size.Height <= 128) ? 128
                    : (imgd.Size.Height <= 256) ? 256
                    : 512;

                var width = imgd.Size.Width;
                var height = imgd.Size.Height;

                logger.Info($"This image will be resized due to comformance of texture size. Resizing from ({width}, {height}) to ({newWidth}, {newHeight}).");

                if (imgd.PixelFormat == Imaging.PixelFormat.Indexed8)
                {
                    var bits = imgd.GetData();
                    var newBits = new byte[newWidth * newHeight];
                    var dstToSrcX = Enumerable.Range(0, newWidth)
                        .Select(xPos => (int)(xPos / (float)newWidth * width))
                        .ToArray();
                    var dstToSrcY = Enumerable.Range(0, newHeight)
                        .Select(yPos => (int)(yPos / (float)newHeight * height))
                        .ToArray();

                    for (var y = 0; y < newHeight; y++)
                    {
                        var dstOffset = newWidth * y;
                        var srcOffset = width * dstToSrcY[y];

                        for (var x = 0; x < newWidth; x++)
                        {
                            newBits[dstOffset + x] = bits[srcOffset + dstToSrcX[x]];
                        }
                    }

                    return new Imgd(
                        new Size(newWidth, newHeight),
                        Imaging.PixelFormat.Indexed8,
                        newBits,
                        imgd.GetClut(),
                        false
                    );
                }
                else
                {
                    throw new NotSupportedException($"{imgd}");
                }
            }
            else
            {
                return imgd;
            }
        }
    }
}
