using OpenKh.Imaging;
using OpenKh.Tools.Kh2FontImageEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Usecases
{
    public class ApplySpacingToImageReadUsecase
    {
        private readonly CopyArrayUsecase _copyArrayUsecase;

        public ApplySpacingToImageReadUsecase(
            CopyArrayUsecase copyArrayUsecase)
        {
            _copyArrayUsecase = copyArrayUsecase;
        }

        public IImageRead Apply(IImageRead image, Func<int, byte> getSpacing, GlyphCell[] glyphCells)
        {
            if (image.PixelFormat != PixelFormat.Indexed4)
            {
                throw new NotSupportedException($"The image format must be Indexed4");
            }

            var data = _copyArrayUsecase.Copy(image.GetData());
            var clut = new byte[]
            {
                  0,   0,   0, 255,
                 85,   0,   0, 255,
                170,   0,   0, 255,
                255,   0,   0, 255,
                  0,   0, 255, 255,
                 85,  85,  85, 255,
                170, 170, 170, 255,
                255, 255, 255, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
            };

            var width = image.Size.Width;
            var height = image.Size.Height;

            var stride = width / 2;

            void ApplyBitOrRect(int fromX, int fromY, byte width, int height)
            {
                for (int y = 0; y < height; y++)
                {
                    var vertOfs = stride * (fromY + y);
                    int dstX = fromX;

                    for (int x = 0; x < width; x++, dstX++)
                    {
                        int ofs = vertOfs + dstX / 2;
                        var one = data[ofs];
                        if ((dstX & 1) == 0)
                        {
                            one |= 0x40;
                        }
                        else
                        {
                            one |= 0x04;
                        }
                        data[ofs] = one;
                    }
                }
            }

            for (int idx = 0, max = glyphCells.Length; idx < max; idx++)
            {
                var glyph = glyphCells[idx];

                ApplyBitOrRect(
                    glyph.Cell.X,
                    glyph.Cell.Y,
                    getSpacing(glyph.SpacingIndex),
                    glyph.Cell.Height
                );
            }

            return new SimpleImage(width, height, PixelFormat.Indexed4, data, PixelFormat.Rgba8888, clut);
        }

    }
}
