using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenKh.Tests.Imaging
{
    public class ImageExtensionsTests
    {
        private class TestImager : IImageRead
        {
            public Size Size { get; set; }
            public PixelFormat PixelFormat { get; set; }
            public byte[] ClutPassThru { get; set; }
            public byte[] DataPassThru { get; set; }

            public byte[] GetClut() => ClutPassThru;
            public byte[] GetData() => DataPassThru;
        }

        [Fact]
        public void ToBgra32Indexed4Test()
        {
            var image = new TestImager
            {
                Size = new Size(2, 1),
                PixelFormat = PixelFormat.Indexed4,
                DataPassThru = new byte[] { 0x10 },
                ClutPassThru = new byte[]
                {
                    // R G B A
                    0x88, 0x44, 0x22, 0x11, // 0
                    0x66, 0x55, 0x44, 0x33, // 1
                },
            };
            var pixels = image.ToBgra32();
            Assert.Equal(
                expected: new byte[]
                {
                    // B G R A
                    0x44, 0x55, 0x66, 0x33, // 1
                    0x22, 0x44, 0x88, 0x11, // 0
                },
                actual: pixels
            );
        }

        [Fact]
        public void ToBgra32Indexed8Test()
        {
            var image = new TestImager
            {
                Size = new Size(2, 1),
                PixelFormat = PixelFormat.Indexed8,
                DataPassThru = new byte[] { 1, 0 },
                ClutPassThru = new byte[]
                {
                    // R G B A
                    0x88, 0x44, 0x22, 0x11, // 0
                    0x66, 0x55, 0x44, 0x33, // 1
                },
            };
            var pixels = image.ToBgra32();
            Assert.Equal(
                expected: new byte[]
                {
                    // B G R A
                    0x44, 0x55, 0x66, 0x33, // 1
                    0x22, 0x44, 0x88, 0x11, // 0
                },
                actual: pixels
            );
        }

        [Fact]
        public void ToBgra32Rgba8888Test()
        {
            var image = new TestImager
            {
                Size = new Size(2, 1),
                PixelFormat = PixelFormat.Rgba8888,
                DataPassThru = new byte[] {
                    // B G R A
                    0x88, 0x44, 0x22, 0x11, // 0
                    0x66, 0x55, 0x44, 0x33, // 1
                },
            };
            var pixels = image.ToBgra32();
            Assert.Equal(
                expected: new byte[]
                {
                    // B G R A
                    0x88, 0x44, 0x22, 0x11, // 0
                    0x66, 0x55, 0x44, 0x33, // 1
                },
                actual: pixels
            );
        }
    }
}
