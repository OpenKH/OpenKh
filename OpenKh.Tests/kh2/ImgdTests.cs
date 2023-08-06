using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ImgdTests
    {
        private const string FilePath = "kh2/res/image-8bit-128-128.imd";
        private const string FacFilePath = "kh2/res/image.fac";

        private const bool GenerateTestData = false;

        [Fact]
        public void IsValidTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x49);
                stream.WriteByte(0x4d);
                stream.WriteByte(0x47);
                stream.WriteByte(0x44);
                stream.SetLength(0x40);
                stream.Position = 0;
                Assert.True(Imgd.IsValid(stream));
            }
        }

        [Fact]
        public void ReadHeaderTest() => File.OpenRead(FilePath).Using(stream =>
        {
            var image = Imgd.Read(stream);
            Assert.Equal(128, image.Size.Width);
            Assert.Equal(128, image.Size.Height);
            Assert.Equal(PixelFormat.Indexed8, image.PixelFormat);
        });

        [Theory]
        [InlineData("4bit-128-128")]
        [InlineData("4bit-256-128")]
        [InlineData("4bit-256-512")]
        [InlineData("4bit-512-128")]
        [InlineData("4bit-512-512")]
        [InlineData("8bit-128-128")]
        [InlineData("8bit-128-64")]
        [InlineData("8bit-256-128")]
        [InlineData("8bit-256-256")]
        [InlineData("8bit-32-32")]
        [InlineData("8bit-48-48")]
        [InlineData("8bit-512-256")]
        [InlineData("8bit-512-512")]
        [InlineData("8bit-64-64")]
        [InlineData("32bit-512-512")]
        public void IsWritingBackCorrectly(string baseName) =>
            File.OpenRead($"kh2/res/image-{baseName}.imd").Using(stream =>
        {
            var expectedData = new byte[stream.Length];
            stream.Read(expectedData, 0, expectedData.Length);

            var image = Imgd.Read(new MemoryStream(expectedData));
            using (var dstStream = new MemoryStream(expectedData.Length))
            {
                image.Write(dstStream);
                dstStream.Position = 0;
                var actualData = new byte[dstStream.Length];
                dstStream.Read(actualData, 0, actualData.Length);

                Assert.Equal(expectedData.Length, actualData.Length);
                Assert.Equal(expectedData, actualData);
            }
        });

        [Theory]
        [InlineData("4bit-128-128")]
        [InlineData("4bit-256-128")]
        [InlineData("4bit-256-512")]
        [InlineData("4bit-512-128")]
        [InlineData("4bit-512-512")]
        [InlineData("8bit-128-128")]
        [InlineData("8bit-128-64")]
        [InlineData("8bit-256-128")]
        [InlineData("8bit-256-256")]
        [InlineData("8bit-32-32")]
        [InlineData("8bit-48-48")]
        [InlineData("8bit-512-256")]
        [InlineData("8bit-512-512")]
        [InlineData("8bit-64-64")]
        public void IsCreatingCorrectlyTest(string baseName) =>
            File.OpenRead($"kh2/res/image-{baseName}.imd").Using(stream =>
        {
            var expectedData = new byte[stream.Length];
            stream.Read(expectedData, 0, expectedData.Length);

            var image = Imgd.Read(new MemoryStream(expectedData));
            using (var dstStream = new MemoryStream(expectedData.Length))
            {
                var newImage = Imgd.Create(
                    image.Size,
                    image.PixelFormat,
                    image.GetData(),
                    image.GetClut(),
                    image.IsSwizzled);

                Assert.Equal(image.GetClut(), newImage.GetClut());
                Assert.Equal(image.GetData(), newImage.GetData());

                newImage.Write(dstStream);
                dstStream.Position = 0;
                var actualData = new byte[dstStream.Length];
                dstStream.Read(actualData, 0, actualData.Length);

                Assert.Equal(expectedData.Length, actualData.Length);
                Assert.Equal(expectedData, actualData);
            }
        });

        [Theory]
        [InlineData(FilePath, false)]
        [InlineData(FacFilePath, true)]
        public void IsFac(string fileName, bool expected) =>
            Assert.Equal(expected, File.OpenRead(fileName).Using(stream => Imgd.IsFac(stream)));

        [Fact]
        public void ReadAndWriteFac() =>
            File.OpenRead(FacFilePath)
            .Using(x => Helpers.AssertStream(x, inStream =>
            {
                var images = Imgd.ReadAsFac(inStream).ToList();
                Assert.Equal(3, images.Count);

                var outStream = new MemoryStream();
                Imgd.WriteAsFac(outStream, images);

                return outStream;
            }));

        void CreateImgdAndComparePixelData(
            byte[] pixelData,
            Func<byte[], Imgd> imgdFactory
        )
        {
            var imgd = imgdFactory(pixelData);

            var temp = new MemoryStream();

            imgd.Write(temp);

            temp.Position = 0;

            var loaded = Imgd.Read(temp);

            Assert.Equal(
                pixelData,
                loaded.GetData()
                );
        }

        [Fact]
        public void TestPixelOrder4() => CreateImgdAndComparePixelData(
            new byte[] { 0x12, 0x34, },
            pixelData => Imgd.Create(
                new System.Drawing.Size(2, 2),
                PixelFormat.Indexed4,
                pixelData,
                new byte[4 * 16],
                false
            )
        );

        [Fact]
        public void TestPixelOrder4Swizzled() => CreateImgdAndComparePixelData(
            new byte[] { 0x12, 0x34, }
                .Concat(new byte[128 * 128 / 2 - 2])
                .ToArray(),
            pixelData => Imgd.Create(
                new System.Drawing.Size(128, 128),
                PixelFormat.Indexed4,
                pixelData,
                new byte[4 * 16],
                true
            )
        );

        [Fact]
        public void TestPixelOrder8() => CreateImgdAndComparePixelData(
            new byte[] { 0x12, 0x34, 0x56, 0x78, },
            pixelData => Imgd.Create(
                new System.Drawing.Size(2, 2),
                PixelFormat.Indexed8,
                pixelData,
                new byte[4 * 256],
                false
            )
        );

        [Fact]
        public void TestPixelOrder8Swizzled() => CreateImgdAndComparePixelData(
            new byte[] { 0x12, 0x34, 0x56, 0x78, }
                .Concat(new byte[128 * 64 - 4])
                .ToArray(),
            pixelData => Imgd.Create(
                new System.Drawing.Size(128, 64),
                PixelFormat.Indexed8,
                pixelData,
                new byte[4 * 256],
                true
            )
        );

        [Fact]
        public void ReadRgba8888PixelOrder()
        {
            var imgd = File.OpenRead($"kh2/res/2x2x32.imd").Using(Imgd.Read);
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

        [Fact]
        public void CreateSampleImdFiles()
        {
            // A stateless swizzle process will always consume 8192 bytes.
            // 128 x 128 x 4 bpp → 8192 bytes
            // 128 x  64 x 8 bpp → 8192 bytes

            foreach (var (isSwizzled, swizzledMark) in new (bool, string)[] { (false, "Unswizzled"), (true, "Swizzled") })
            {
                {
                    var imgd = new Imgd(
                        new Size(128, 128),
                        PixelFormat.Indexed4,
                        Enumerable.Range(0, 64 * 128)
                            .Select(it => (byte)((it & 15) | ((it & 15) << 4)))
                            .ToArray(),
                        // Clut
                        // RR GG BB AA
                        // 60 70 222 255
                        Enumerable.Repeat(new byte[] { 60, 70, 222, 255 }, 16)
                            .SelectMany(it => it)
                            .ToArray(),
                        isSwizzled
                    );

                    if (!isSwizzled)
                    {
                        Assert.Equal(
                            expected: Enumerable.Range(0, 64 * 128)
                                .Select(it => (byte)((it & 15) | ((it & 15) << 4)))
                                .ToArray(),
                            actual: imgd.Data
                        );
                    }
                    Assert.Equal(
                        expected: Enumerable.Repeat(new byte[] { 60, 70, 222, 128 }, 16)
                            .SelectMany(it => it)
                            .ToArray(),
                        actual: imgd.Clut
                    );

                    Deal($"R60G70B222A255.Indexed4.newImgd.{swizzledMark}.imd", imgd);
                }

                {
                    var imgd = Imgd.Create(
                        new Size(128, 128),
                        PixelFormat.Indexed4,
                        Enumerable.Range(0, 64 * 128)
                            .Select(it => (byte)((it & 15) | ((it & 15) << 4)))
                            .ToArray(),
                        // Clut
                        // RR GG BB AA
                        // 60 70 222 255
                        Enumerable.Repeat(new byte[] { 60, 70, 222, 255 }, 16)
                            .SelectMany(it => it)
                            .ToArray(),
                        isSwizzled
                    );

                    if (!isSwizzled)
                    {
                        Assert.Equal(
                            expected: Enumerable.Range(0, 64 * 128)
                                .Select(it => (byte)((it & 15) | ((it & 15) << 4)))
                                .ToArray(),
                            actual: imgd.Data
                        );
                    }
                    Assert.Equal(
                        expected: Enumerable.Repeat(new byte[] { 60, 70, 222, 128 }, 16)
                            .SelectMany(it => it)
                            .ToArray(),
                        actual: imgd.Clut
                    );

                    Deal($"R60G70B222A255.Indexed4.ImgdCreate.{swizzledMark}.imd", imgd);
                }

                {
                    var imgd = new Imgd(
                        new Size(128, 64),
                        PixelFormat.Indexed8,
                        Enumerable.Range(0, 128 * 64)
                            .Select(it => (byte)it)
                            .ToArray(),
                        // Clut
                        // RR GG BB AA
                        // 60 70 222 255
                        Enumerable.Repeat(new byte[] { 60, 70, 222, 255 }, 256)
                            .SelectMany(it => it)
                            .ToArray(),
                        isSwizzled
                    );

                    if (!isSwizzled)
                    {
                        Assert.Equal(
                            expected: Enumerable.Range(0, 128 * 64)
                                .Select(it => (byte)it)
                                .ToArray(),
                            actual: imgd.Data
                        );
                    }
                    Assert.Equal(
                        expected: Enumerable.Repeat(new byte[] { 60, 70, 222, 128 }, 256)
                            .SelectMany(it => it)
                            .ToArray(),
                        actual: imgd.Clut
                    );

                    Deal($"R60G70B222A255.Indexed8.newImgd.{swizzledMark}.imd", imgd);
                }
                {
                    var imgd = Imgd.Create(
                        new Size(128, 64),
                        PixelFormat.Indexed8,
                        Enumerable.Range(0, 128 * 64)
                            .Select(it => (byte)it)
                            .ToArray(),
                        // Clut
                        // RR GG BB AA
                        // 60 70 222 255
                        Enumerable.Repeat(new byte[] { 60, 70, 222, 255 }, 256)
                            .SelectMany(it => it)
                            .ToArray(),
                        isSwizzled
                    );

                    if (!isSwizzled)
                    {
                        Assert.Equal(
                            expected: Enumerable.Range(0, 128 * 64)
                                .Select(it => (byte)it)
                                .ToArray(),
                            actual: imgd.Data
                        );
                    }
                    Assert.Equal(
                        expected: Enumerable.Repeat(new byte[] { 60, 70, 222, 128 }, 256)
                            .SelectMany(it => it)
                            .ToArray(),
                        actual: imgd.Clut
                    );

                    Deal($"R60G70B222A255.Indexed8.ImgdCreate.{swizzledMark}.imd", imgd);
                }
                {
                    // Raw pixels are placed in this order, at the unswizzled file.
                    // RR GG BB AA
                    // 60 70 222 255

                    var imgd = new Imgd(
                        new Size(64, 32),
                        PixelFormat.Rgba8888,
                        Enumerable.Repeat(new byte[] { 60, 70, 222, 255 }, 64 * 32) // RR GG BB AA
                            .SelectMany(it => it)
                            .ToArray(),
                        new byte[0],
                        isSwizzled
                    );

                    Assert.Equal(
                        expected: Enumerable.Repeat(new byte[] { 60, 70, 222, 255 }, 64 * 32) // RR GG BB AA
                            .SelectMany(it => it)
                            .ToArray(),
                        actual: imgd.Data // RR GG BB AA
                    );
                    Assert.Equal(
                        expected: Enumerable.Repeat(new byte[] { 222, 70, 60, 255 }, 64 * 32) // BB GG RR AA
                            .SelectMany(it => it)
                            .ToArray(),
                        actual: imgd.GetData() // BB GG RR AA
                    );
                    Assert.Null(imgd.Clut);

                    Deal($"R60G70B222A255.Rgba8888.newImgd.{swizzledMark}.imd", imgd);
                }
                {
                    // Raw pixels are placed in this order, at the unswizzled file.
                    // RR GG BB AA
                    // 60 70 222 255

                    var imgd = Imgd.Create(
                        new Size(64, 32),
                        PixelFormat.Rgba8888,
                        Enumerable.Repeat(new byte[] { 222, 70, 60, 255 }, 64 * 32) // BB GG RR AA
                            .SelectMany(it => it)
                            .ToArray(),
                        new byte[0],
                        isSwizzled
                    );

                    Assert.Equal(
                        expected: Enumerable.Repeat(new byte[] { 60, 70, 222, 255 }, 64 * 32) // RR GG BB AA
                            .SelectMany(it => it)
                            .ToArray(),
                        actual: imgd.Data // RR GG BB AA
                    );
                    Assert.Equal(
                        expected: Enumerable.Repeat(new byte[] { 222, 70, 60, 255 }, 64 * 32) // BB GG RR AA
                            .SelectMany(it => it)
                            .ToArray(),
                        actual: imgd.GetData() // BB GG RR AA
                    );
                    Assert.Null(imgd.Clut);

                    Deal($"R60G70B222A255.Rgba8888.ImgdCreate.{swizzledMark}.imd", imgd);
                }
            }
        }

        private void Deal(string fileName, Imgd imgd)
        {
            var filePath = $"kh2/res/{fileName}";

            if (GenerateTestData)
            {
#pragma warning disable CS0162 // Unreachable code detected
                using var stream = File.Create(filePath);
#pragma warning restore CS0162 // Unreachable code detected
                imgd.Write(stream);
            }
            else
            {
                using var stream = new MemoryStream();
                imgd.Write(stream);

                Assert.Equal(
                    expected: File.ReadAllBytes(filePath),
                    actual: stream.ToArray()
                );
            }
        }
    }
}
