using OpenKh.Imaging;
using OpenKh.Kh2.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ImageReadExtensionsTests
    {
        class SampleImager : IImageRead
        {
            public Size Size { get; set; }

            public PixelFormat PixelFormat { get; set; }

            public byte[] ForGetClut { get; set; }
            public byte[] ForGetData { get; set; }

            public byte[] GetClut() => ForGetClut;

            public byte[] GetData() => ForGetData;
        }

        [Fact]
        public void Rgba8888AsImgd()
        {
            var it = ImageReadExtensions.AsImgd(
                new SampleImager
                {
                    PixelFormat = PixelFormat.Rgba8888,
                    Size = new Size(1, 1),
                    ForGetData = new byte[]
                    {
                        // GetData(): B, G, R, A
                        0x11, 0x22, 0x33, 0x44,
                    },
                }
            );
            Assert.Equal(
                new byte[]
                {
                    // GetData(): B, G, R, A
                    0x11, 0x22, 0x33, 0x44 * 2,
                },
                it.GetData()
            );
            Assert.Equal(
                new byte[]
                {
                    // Data: R, G, B, A
                    0x33, 0x22, 0x11, 0x44,
                },
                it.Data
            );
        }
    }
}
