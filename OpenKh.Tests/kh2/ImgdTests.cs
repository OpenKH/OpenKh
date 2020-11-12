using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
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
        public void WriteRgba8888PixelOrder()
        {
            var actual = new MemoryStream();
            Imgd
                .Create(
                    new Size(2, 2),
                    PixelFormat.Rgba8888,
                    new byte[] {
                        // Input: R, G, B, A
                        0x27, 0x7F, 0xFF, 0xFF,
                        0x4C, 0xB1, 0x22, 0xFF,
                        0xE8, 0xA2, 0x00, 0xFF,
                        0x00, 0xF2, 0xFF, 0xFF,
                    },
                    null,
                    false
                )
                .Write(actual);

            var expectedData = File.ReadAllBytes($"kh2/res/2x2x32.imd");

            Assert.Equal(expectedData, actual.ToArray());
        }
    }
}
