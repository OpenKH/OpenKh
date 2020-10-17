using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        public void SampleRgba8888AsImgd()
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

        /// <summary>
        /// AsImgd() shouldn't double alpha channel data.
        /// </summary>
        [Fact]
        public void Rgba8888ImgdAsImgdx3()
        {
            var imgd = File.OpenRead($"kh2/res/2x2x32.imd").Using(Imgd.Read)
                .AsImgd()
                .AsImgd()
                .AsImgd();
            Assert.Equal(new Size(2, 2), imgd.Size);
            Assert.Equal(PixelFormat.Rgba8888, imgd.PixelFormat);
            Assert.Equal(
                new byte[] {
                    // GetData(): B, G, R, A
                    0xFF, 0x7F, 0x27, 0xFF,
                    0x22, 0xB1, 0x4C, 0xFF,
                    0x00, 0xA2, 0xE8, 0xFF,
                    0xFF, 0xF2, 0x00, 0xFF,
                },
                imgd.GetData()
            );
            Assert.Equal(
                new byte[] {
                    // Data: R, G, B, A
                    0x27, 0x7F, 0xFF, 0xFF,
                    0x4C, 0xB1, 0x22, 0xFF,
                    0xE8, 0xA2, 0x00, 0xFF,
                    0x00, 0xF2, 0xFF, 0xFF,
                },
                imgd.Data
            );
        }
    }
}
