using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ModelTextureTests
    {
        private const string FileName1 = "kh2/res/model_texture1.tex";
        private const string FileName2 = "kh2/res/model_texture2.tex";

        [Theory]
        [InlineData(FileName1, 1)]
        [InlineData(FileName2, 3)]
        public void ReadCorrectAmountOfTextures(string fileName, int expectedCount) => File.OpenRead(fileName).Using(stream =>
        {
            var modelTexture = ModelTexture.Read(stream);

            Assert.Equal(expectedCount, modelTexture.Images.Count);
        });

        [Fact]
        public void CreateImagesWithTheCorrectInformation() => File.OpenRead(FileName2).Using(stream =>
        {
            var modelTexture = ModelTexture.Read(stream);

            var image0 = modelTexture.Images[0];
            Assert.Equal(256, image0.Size.Width);
            Assert.Equal(256, image0.Size.Height);
            Assert.Equal(PixelFormat.Indexed8, image0.PixelFormat);
            Assert.Equal(256 * 256, image0.GetData().Length);

            var image1 = modelTexture.Images[1];
            Assert.Equal(256, image1.Size.Width);
            Assert.Equal(256, image1.Size.Height);
            Assert.Equal(PixelFormat.Indexed8, image1.PixelFormat);
            Assert.Equal(256 * 256, image1.GetData().Length);

            var image2 = modelTexture.Images[2];
            Assert.Equal(128, image2.Size.Width);
            Assert.Equal(64, image2.Size.Height);
            Assert.Equal(PixelFormat.Indexed8, image2.PixelFormat);
            Assert.Equal(128 * 64, image2.GetData().Length);
        });

        [Theory]
        [InlineData(FileName1)]
        [InlineData(FileName2)]
        public void WriteBackTheSameFile(string fileName) => File.OpenRead(fileName).Using(stream => Helpers.AssertStream(stream, inStream =>
        {
            var outStream = new MemoryStream();
            ModelTexture.Read(inStream).Write(outStream);
            return outStream;
        }));

        [Fact]
        public void Read4bitPaletteCorrectly() => File.OpenRead(FileName1).Using(stream =>
        {
            var clut = ModelTexture.Read(stream).Images.First().GetClut();

            Assert.Equal(87, clut[0]);
            Assert.Equal(98, clut[1]);
            Assert.Equal(106, clut[2]);
            Assert.Equal(255, clut[3]);

            Assert.Equal(95, clut[4]);
            Assert.Equal(105, clut[5]);
            Assert.Equal(114, clut[6]);
            Assert.Equal(255, clut[7]);

            Assert.Equal(108, clut[16]);
            Assert.Equal(118, clut[17]);
            Assert.Equal(128, clut[18]);
            Assert.Equal(255, clut[19]);

            Assert.Equal(134, clut[32]);
            Assert.Equal(147, clut[33]);
            Assert.Equal(158, clut[34]);
            Assert.Equal(255, clut[35]);
        });

        [Fact]
        public void Read8bitPaletteCorrectly() => File.OpenRead(FileName2).Using(stream =>
        {
            var clut = ModelTexture.Read(stream).Images.First().GetClut();

            Assert.Equal(0, clut[0 * 4 + 0]);
            Assert.Equal(0, clut[0 * 4 + 1]);
            Assert.Equal(0, clut[0 * 4 + 2]);
            Assert.Equal(0, clut[0 * 4 + 3]);

            Assert.Equal(10, clut[4 * 4 + 0]);
            Assert.Equal(10, clut[4 * 4 + 1]);
            Assert.Equal(10, clut[4 * 4 + 2]);
            Assert.Equal(255, clut[4 * 4 + 3]);

            Assert.Equal(11, clut[8 * 4 + 0]);
            Assert.Equal(15, clut[8 * 4 + 1]);
            Assert.Equal(23, clut[8 * 4 + 2]);
            Assert.Equal(255, clut[8 * 4 + 3]);

            Assert.Equal(23, clut[16 * 4 + 0]);
            Assert.Equal(19, clut[16 * 4 + 1]);
            Assert.Equal(29, clut[16 * 4 + 2]);
            Assert.Equal(255, clut[16 * 4 + 3]);

            Assert.Equal(23, clut[32 * 4 + 0]);
            Assert.Equal(35, clut[32 * 4 + 1]);
            Assert.Equal(56, clut[32 * 4 + 2]);
            Assert.Equal(255, clut[32 * 4 + 3]);

            Assert.Equal(57, clut[64 * 4 + 0]);
            Assert.Equal(51, clut[64 * 4 + 1]);
            Assert.Equal(71, clut[64 * 4 + 2]);
            Assert.Equal(255, clut[64 * 4 + 3]);

            Assert.Equal(153, clut[128 * 4 + 0]);
            Assert.Equal(96, clut[128 * 4 + 1]);
            Assert.Equal(7, clut[128 * 4 + 2]);
            Assert.Equal(255, clut[128 * 4 + 3]);

        });
    }
}
