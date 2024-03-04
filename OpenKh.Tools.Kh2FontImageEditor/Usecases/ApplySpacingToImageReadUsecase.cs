using OpenKh.Engine.Extensions;
using OpenKh.Imaging;
using OpenKh.Tools.Kh2FontImageEditor.Helpers;
using System;
using System.Collections;
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

        public IImageRead ApplyToIndexed4(IImageRead image, Func<int, byte> getSpacing, GlyphCell[] glyphCells)
        {
            if (image.PixelFormat != PixelFormat.Indexed4)
            {
                throw new NotSupportedException($"The image format must be Indexed4");
            }

            var data = _copyArrayUsecase.Copy(image.GetData());
            var clut = new byte[] // RR GG BB AA
            {
                  0,   0,   0, 255, // Unprotected glyph intensity 0 (black)
                 85,   0,   0, 255, // Unprotected glyph intensity 1 (RED) --> glyph overflow
                170,   0,   0, 255, // Unprotected glyph intensity 2 (RED) --> glyph overflow
                255,   0,   0, 255, // Unprotected glyph intensity 3 (RED) --> glyph overflow
                  0,   0, 255, 255, // Protected glyph intensity 0 (blue)
                 85,  85,  85, 255, // Protected glyph intensity 1 (gray)
                170, 170, 170, 255, // Protected glyph intensity 2 (gray)
                255, 255, 255, 255, // Protected glyph intensity 3 (white)
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
                  0,   0,   0, 255,
            };

            var bitmapWidth = image.Size.Width;
            var bitmapHeight = image.Size.Height;

            var stride = bitmapWidth / 2;

            void ProtectRect(int fromX, int fromY, byte width, int height)
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

                ProtectRect(
                    glyph.Cell.X,
                    glyph.Cell.Y,
                    getSpacing(glyph.SpacingIndex),
                    glyph.Cell.Height
                );
            }

            return new SimpleImage(bitmapWidth, bitmapHeight, PixelFormat.Indexed4, data, PixelFormat.Rgba8888, clut);
        }

        public IImageRead ApplyToFullColored(IImageRead image, Func<int, byte> getSpacing, GlyphCell[] glyphCells)
        {
            var data = _copyArrayUsecase.Copy(image.ToBgra32());

            var bitmapWidth = image.Size.Width;
            var bitmapHeight = image.Size.Height;

            var protect = new BitArray(bitmapWidth * bitmapHeight);

            {
                void ProtectRect(int fromX, int fromY, byte width, int height)
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            protect[bitmapWidth * (fromY + y) + fromX + x] = true;
                        }
                    }
                }

                for (int idx = 0, max = glyphCells.Length; idx < max; idx++)
                {
                    var glyph = glyphCells[idx];

                    ProtectRect(
                        glyph.Cell.X,
                        glyph.Cell.Y,
                        getSpacing(glyph.SpacingIndex),
                        glyph.Cell.Height
                    );
                }
            }

            var ofs = 0;

            for (int index = 0; index < protect.Length; index++, ofs += 4)
            {
                if (protect[index])
                {
                    if (data[ofs + 0] == 0 && data[ofs + 1] == 0 && data[ofs + 2] == 0)
                    {
                        // make pixel blue as valid pixel inside cell
                        data[ofs + 0] = 255;
                        data[ofs + 1] = 0;
                        data[ofs + 2] = 0;
                        data[ofs + 3] = 255;
                    }
                }
                else
                {
                    // make pixel red as invalid pixel outside cell

                    var b = data[ofs + 0];
                    var g = data[ofs + 1];
                    var r = data[ofs + 2];

                    var intensity = Math.Max(Math.Max(b, g), r);

                    data[ofs + 0] = 0;
                    data[ofs + 1] = 0;
                    data[ofs + 2] = intensity;
                    data[ofs + 3] = 255;
                }
            }

            return new SimpleImage(bitmapWidth, bitmapHeight, PixelFormat.Rgba8888, data, PixelFormat.Rgba8888);
        }
    }
}
