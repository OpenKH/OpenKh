using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2.Ard;
using System.Drawing;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Imaging
{
    public class PngTests
    {
        [Theory]
        [InlineData("4")]
        [InlineData("8")]
        [InlineData("24")]
        [InlineData("32")]
        public void ReadingTests(string prefix)
        {
            File.OpenRead($"Imaging/res/png/{prefix}.png").Using(PngImage.Read);
        }

        [Theory]
        [InlineData("icon0_e")]
        public void RarelyEncounteredCaseTests(string prefix)
        {
            File.OpenRead($"Imaging/res/png/{prefix}.png").Using(PngImage.Read);
        }

        [Fact]
        public void Load4x2_4()
        {
            var clut = new byte[] {
                0, 0, 0, 255,
                0, 0, 255, 255,
                255, 0, 0, 255,
                255, 0, 255, 255,
                0, 255, 255, 255,
                255, 255, 0, 255,
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
            };

            var image = File.OpenRead($"Imaging/res/png/4x2_4.png").Using(PngImage.Read);
            Assert.Equal(4, image.Size.Width);
            Assert.Equal(2, image.Size.Height);
            Assert.Equal(PixelFormat.Indexed4, image.PixelFormat);
            Assert.Equal(clut, image.GetClut());
            Assert.Equal(new byte[] { 0x02, 0x56, 0x14, 0x36, }, image.GetData());
        }

        [Fact]
        public void Load4x8_8()
        {
            var clut = new byte[] {
                0, 0, 0, 255,
                136, 0, 21, 255,
                237, 28, 36, 255,
                63, 72, 204, 255,
                163, 73, 164, 255,
                127, 127, 127, 255,
                0, 162, 232, 255,
                185, 122, 87, 255,
                34, 177, 76, 255,
                255, 127, 39, 255,
                255, 174, 201, 255,
                195, 195, 195, 255,
                255, 201, 14, 255,
                181, 230, 29, 255,
                239, 228, 176, 255,
                255, 242, 0, 255,
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

            var pixels = new byte[] {
                0, 5, 1, 2,
                6, 8, 15, 9,
                3, 4, 16, 11,
                14, 12, 10, 7,
                13, 16, 16, 16,
                16, 16, 16, 16,
                16, 16, 16, 16,
                16, 16, 16, 16,
            };

            var image = File.OpenRead($"Imaging/res/png/4x8_8.png").Using(PngImage.Read);
            Assert.Equal(4, image.Size.Width);
            Assert.Equal(8, image.Size.Height);
            Assert.Equal(PixelFormat.Indexed8, image.PixelFormat);
            Assert.Equal(clut, image.GetClut());
            Assert.Equal(pixels, image.GetData());
        }

        [Fact]
        public void Load2x2_24()
        {
            var pixels = new byte[] {
                0, 0, 0,
                0, 0, 255,
                255, 0, 0,
                255, 255, 255,
            };

            var image = File.OpenRead($"Imaging/res/png/2x2_24.png").Using(PngImage.Read);
            Assert.Equal(2, image.Size.Width);
            Assert.Equal(2, image.Size.Height);
            Assert.Equal(PixelFormat.Rgb888, image.PixelFormat);
            Assert.Equal(pixels, image.GetData());
        }

        [Fact]
        public void Load2x2_32()
        {
            var pixels = new byte[] {
                0, 0, 0, 255,
                0, 0, 255, 255,
                255, 0, 0, 255,
                255, 255, 255, 255,
            };

            var image = File.OpenRead($"Imaging/res/png/2x2_32.png").Using(PngImage.Read);
            Assert.Equal(2, image.Size.Width);
            Assert.Equal(2, image.Size.Height);
            Assert.Equal(PixelFormat.Rgba8888, image.PixelFormat);
            Assert.Equal(pixels, image.GetData());
        }

        private class Bitmap1x1_32 : IImageRead
        {
            public Size Size => new Size(1, 1);
            public PixelFormat PixelFormat => PixelFormat.Rgba8888;

            public byte[] GetClut() => null;
            public byte[] GetData() => new byte[] { 0x44, 0x22, 0x11, 0x88 };
        }

        [Fact]
        public void Save1x1_32()
        {
            using var stream = File.Create("1x1_32.png");
            var bitmap = new Bitmap1x1_32();
            PngImage.Write(stream, bitmap);

            stream.FromBegin();
            {
                var it = PngImage.Read(stream);
                Assert.Equal(bitmap.Size, it.Size);
                Assert.Equal(bitmap.PixelFormat, it.PixelFormat);
                Assert.Equal(bitmap.GetData(), it.GetData());
            }
        }

        private class Bitmap1x1_24 : IImageRead
        {
            public Size Size => new Size(1, 1);
            public PixelFormat PixelFormat => PixelFormat.Rgb888;

            public byte[] GetClut() => null;
            public byte[] GetData() => new byte[] { 0x44, 0x22, 0x11 };
        }

        [Fact]
        public void Save1x1_24()
        {
            using var stream = File.Create("1x1_24.png");
            var bitmap = new Bitmap1x1_24();
            PngImage.Write(stream, bitmap);

            stream.FromBegin();
            {
                var it = PngImage.Read(stream);
                Assert.Equal(bitmap.Size, it.Size);
                Assert.Equal(bitmap.PixelFormat, it.PixelFormat);
                Assert.Equal(bitmap.GetData(), it.GetData());
            }
        }

        private class Bitmap4x4_4 : IImageRead
        {
            public Size Size => new Size(4, 4);
            public PixelFormat PixelFormat => PixelFormat.Indexed4;

            public byte[] GetClut() => new byte[] {
                0x00, 0x00, 0x00, 0xFF,
                0x00, 0x00, 0x11, 0xFF,
                0x00, 0x00, 0x22, 0xFF,
                0x00, 0x00, 0x33, 0xFF,
                0x00, 0x00, 0x44, 0xFF,
                0x00, 0x00, 0x55, 0xFF,
                0x00, 0x00, 0x66, 0xFF,
                0x00, 0x00, 0x77, 0xFF,
                0x00, 0x00, 0x88, 0xFF,
                0x00, 0x00, 0x99, 0xFF,
                0x00, 0x00, 0xaa, 0xFF,
                0x00, 0x00, 0xbb, 0xFF,
                0x00, 0x00, 0xcc, 0xFF,
                0x00, 0x00, 0xdd, 0xFF,
                0x00, 0x00, 0xee, 0xFF,
                0x00, 0x00, 0xff, 0xFF,
            };
            public byte[] GetData() => new byte[] {
                0x01, 0x23,
                0x45, 0x67,
                0x89, 0xab,
                0xcd, 0xef,
            };
        }

        [Fact]
        public void Save4x4_4()
        {
            using var stream = File.Create("4x4_4.png");
            var bitmap = new Bitmap4x4_4();
            PngImage.Write(stream, bitmap);

            stream.FromBegin();
            {
                var it = PngImage.Read(stream);
                Assert.Equal(bitmap.Size, it.Size);
                Assert.Equal(bitmap.PixelFormat, it.PixelFormat);
                Assert.Equal(bitmap.GetData(), it.GetData());
            }
        }

        private class Bitmap16x16_8 : IImageRead
        {
            public Size Size => new Size(16, 16);
            public PixelFormat PixelFormat => PixelFormat.Indexed8;

            public byte[] GetClut() => Enumerable.Range(0, 256)
                .SelectMany(index => new byte[] { (byte)index, 0, 0, 255 })
                .ToArray();
            public byte[] GetData() => Enumerable.Range(0, 256)
                .Select(index => (byte)index)
                .ToArray();
        }

        [Fact]
        public void Save16x16_8()
        {
            using var stream = File.Create("16x16_8.png");
            var bitmap = new Bitmap16x16_8();
            PngImage.Write(stream, bitmap);

            stream.FromBegin();
            {
                var it = PngImage.Read(stream);
                Assert.Equal(bitmap.Size, it.Size);
                Assert.Equal(bitmap.PixelFormat, it.PixelFormat);
                Assert.Equal(bitmap.GetData(), it.GetData());
            }
        }
    }
}
