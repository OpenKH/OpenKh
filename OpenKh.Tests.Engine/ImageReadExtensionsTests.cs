using OpenKh.Engine.Extensions;
using OpenKh.Imaging;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenKh.Tests.Engine
{
    public class ImageReadExtensionsTests
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
        public void AsBgra8888Indexed4Test()
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
            var pixels = image.AsBgra8888();
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
        public void AsBgra8888Indexed8Test()
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
            var pixels = image.AsBgra8888();
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
        public void AsBgra8888Rgba8888Test()
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
            var pixels = image.AsBgra8888();
            Assert.Equal(
                expected: new byte[]
                {
                    0x88, 0x44, 0x22, 0x11, // 0
                    0x66, 0x55, 0x44, 0x33, // 1
                },
                actual: pixels
            );
        }

        [Fact]
        public void AsRgba8888Indexed4Test()
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
            var pixels = image.AsRgba8888();
            Assert.Equal(
                expected: new byte[]
                {
                    // R G B A
                    0x66, 0x55, 0x44, 0x33, // 1
                    0x88, 0x44, 0x22, 0x11, // 0
                },
                actual: pixels
            );
        }

        [Fact]
        public void AsRgba8888Indexed8Test()
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
            var pixels = image.AsRgba8888();
            Assert.Equal(
                expected: new byte[]
                {
                    // R G B A
                    0x66, 0x55, 0x44, 0x33, // 1
                    0x88, 0x44, 0x22, 0x11, // 0
                },
                actual: pixels
            );
        }

        [Fact]
        public void AsRgba8888Rgba8888Test()
        {
            var image = new TestImager
            {
                Size = new Size(2, 1),
                PixelFormat = PixelFormat.Rgba8888,
                DataPassThru = new byte[] {
                    // B G R A
                    0x22, 0x44, 0x88, 0x11, // 0
                    0x44, 0x55, 0x66, 0x33, // 1
                },
            };
            var pixels = image.AsRgba8888();
            Assert.Equal(
                expected: new byte[]
                {
                    // R G B A
                    0x88, 0x44, 0x22, 0x11, // 0
                    0x66, 0x55, 0x44, 0x33, // 1
                },
                actual: pixels
            );
        }

    }
}
