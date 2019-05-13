using OpenKh.Imaging;
using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ImgdTests
    {
        private const string FilePath = "kh2/res/image-8bit-128-128.imd";

        [Fact]
        public void IsValidTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x49);
                stream.WriteByte(0x4d);
                stream.WriteByte(0x47);
                stream.WriteByte(0x44);
                stream.Position = 0;
                Assert.True(Imgd.IsValid(stream));
                Assert.Equal(0, stream.Position);
            }
        }

        [Fact]
        public void ReadHeaderTest() => Common.FileOpenRead(FilePath, stream =>
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
            Common.FileOpenRead($"kh2/res/image-{baseName}.imd", stream =>
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
            Common.FileOpenRead($"kh2/res/image-{baseName}.imd", stream =>
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
    }
}
