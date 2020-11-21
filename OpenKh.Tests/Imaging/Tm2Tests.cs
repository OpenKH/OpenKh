using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Imaging
{
    public class Tm2Tests
    {
        [Theory]
        [InlineData(new byte[] { 0x54, 0x49, 0x4d, 0x32 }, 16, true)]
        [InlineData(new byte[] { 0x54, 0x49, 0x4d, 0x31 }, 16, false)]
        [InlineData(new byte[] { 0x54, 0x49, 0x4d, 0x32 }, 15, false)]
        public void IsValidTest(byte[] header, int length, bool expected) => new MemoryStream()
            .Using(stream =>
            {
                stream.Write(header, 0, header.Length);
                stream.SetLength(length);

                Assert.Equal(expected, Tm2.IsValid(stream));
            });

        [Theory]
        [InlineData("image-8bit-128-128", 128, 128, PixelFormat.Indexed8)]
        [InlineData("image-8bit-512-272", 512, 272, PixelFormat.Indexed8)]
        [InlineData("image-32bit-480-279", 480, 279, PixelFormat.Rgba8888)]
        [InlineData("tt02.radar", 512, 512, PixelFormat.Indexed4)]
        public void ReadImagePropertiesTest(
            string fileName,
            int width,
            int height,
            PixelFormat pixelFormat) => File.OpenRead($"Imaging/res/{fileName}.tm2").Using(stream =>
            {
                var image = Tm2.Read(stream).Single();

                Assert.Equal(width, image.Size.Width);
                Assert.Equal(height, image.Size.Height);
                Assert.Equal(pixelFormat, image.PixelFormat);
            });

        [Theory]
        [InlineData("image-8bit-128-128")]
        [InlineData("image-8bit-512-272")]
        [InlineData("image-32bit-480-279")]
        public void IsWritingBackCorrectly(string fileName) => File.OpenRead($"Imaging/res/{fileName}.tm2").Using(x =>
            Helpers.AssertStream(x, stream =>
            {
                var images = Tm2.Read(stream);

                var newStream = new MemoryStream();
                Tm2.Write(newStream, images);

                return newStream;
            }));

        /// <summary>
        /// Scan and test all radar images.
        /// </summary>
        /// <remarks>
        /// Define your [InlineData] if you want to test this case against every asset map file.
        /// </remarks>
        [Theory]
        [InlineData("Imaging/res")]
        //[InlineData(@"H:\KH2fm.OpenKH\map\jp")]
        public void ValidateAllKH2MapRadarImages(string mapFilesDir)
        {
            Directory.GetFiles(mapFilesDir, "*.map").ToList().ForEach(
                file =>
                {
                    File.OpenRead(file).Using(
                        stream =>
                        {
                            Bar.Read(stream)
                                // rada may be multiple occurrence like al06.map
                                .Where(
                                    entry => true
                                        && entry.Name == "rada"
                                        && entry.Type == Bar.EntryType.Tim2
                                )
                                .ToList()
                                .ForEach(
                                    entry =>
                                    {
                                        Assert.True(Tm2.IsValid(entry.Stream), "Should be TM2");

                                        var imageSet = Tm2.Read(entry.Stream);

                                        imageSet.ToList().ForEach(
                                            texture =>
                                            {
                                                // All radar images are 4-bpp
                                                Assert.NotEmpty(texture.GetData());
                                                Assert.Equal(4 * 16, texture.GetClut().Length);
                                            }
                                        );
                                    }
                                );
                        }
                    );
                }
            );
        }
    }
}
