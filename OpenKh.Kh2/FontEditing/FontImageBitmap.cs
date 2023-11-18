using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace OpenKh.Kh2.FontEditing
{
    public class FontImageBitmap : IImageRead
    {
        private readonly byte[] _clut;
        private readonly byte[] _data;

        public FontImageBitmap(byte[] body, bool firstImage, byte[] clut = null)
        {
            if ((body.Length & 255) != 0)
            {
                throw new InvalidCastException("fontimage evt/sys width must be 512");
            }

            _clut = clut ?? new byte[]
            {
                0, 0, 0, 255,
                85, 85, 85, 255,
                170, 170, 170, 255,
                255, 255, 255, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
                0, 0, 0, 255,
            };

            int width = 512;
            int height = body.Length / 256;

            var data = new byte[width / 2 * height];

            var rightShift = firstImage ? 0 : 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x += 2)
                {
                    var at = width / 2 * y + x / 2;

                    var one = body[at] >> rightShift;
                    var leftPixel = one & 3;
                    var rightPixel = (one >> 4) & 3;

                    data[at] = (byte)((leftPixel << 4) | rightPixel);
                }
            }

            Size = new Size(width, height);
            PixelFormat = PixelFormat.Indexed4;
            _data = data;
        }

        public Size Size { get; }

        public PixelFormat PixelFormat { get; }

        public byte[] GetClut() => _clut;

        public byte[] GetData() => _data;

        public static byte[] WriteFont(IImageRead image1, IImageRead image2)
        {
            if (false
                || image1 == null
                || image2 == null
                || image1.Size.Width != 512
                || image1.Size != image2.Size
            )
            {
                throw new Exception("Font image width expected 512. And two font images expect to be same size.");
            }

            var source1 = image1.ToBgra32();
            var source2 = image2.ToBgra32();

            var height = image1.Size.Height;
            var pixels = new byte[256 * height];

            for (int y = 0; y < height; y++)
            {
                var readOfs = 512 * 4 * y;
                var writeOfs = 256 * y;

                for (int x = 0; x < 512; x += 2, readOfs += 8, writeOfs += 1)
                {
                    var plane1FirstBlue = source1[readOfs];
                    var plane1FirstGreen = source1[readOfs + 1];
                    var plane1FirstRed = source1[readOfs + 2];
                    var plane1SecondBlue = source1[readOfs + 4];
                    var plane1SecondGreen = source1[readOfs + 5];
                    var plane1SecondRed = source1[readOfs + 6];
                    var plane2FirstBlue = source2[readOfs];
                    var plane2FirstGreen = source2[readOfs + 1];
                    var plane2FirstRed = source2[readOfs + 2];
                    var plane2SecondBlue = source2[readOfs + 4];
                    var plane2SecondGreen = source2[readOfs + 5];
                    var plane2SecondRed = source2[readOfs + 6];
                    // At 4bpp bitmap, lo and hi are swapped. lo first, hi second.
                    pixels[writeOfs] = MakePixel(
                        CutTo2Bit(plane1FirstBlue, plane1FirstGreen, plane1FirstRed) | (CutTo2Bit(plane2FirstBlue, plane2FirstGreen, plane2FirstRed) << 2),
                        CutTo2Bit(plane1SecondBlue, plane1SecondGreen, plane1SecondRed) | (CutTo2Bit(plane2SecondBlue, plane2SecondGreen, plane2SecondRed) << 2)
                    );
                }
            }

            return pixels;
        }

        private static byte MakePixel(int lo, int hi)
        {
            return (byte)((hi << 4) | (lo & 15));
        }

        private static int CutTo2Bit(byte blue, byte green, byte red)
        {
            var intensity = (blue + (int)green + red) / 3;

            if (0xe0 <= intensity)
            {
                return 3;
            }
            else if (0xb0 <= intensity)
            {
                return 2;
            }
            else if (0x90 <= intensity)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
