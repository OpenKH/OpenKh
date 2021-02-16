using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ModelTextureTests
    {
        private const string FileName1 = "kh2/res/model_texture1.tex";
        private const string FileName2 = "kh2/res/model_texture2.tex";

        [Fact]
        public void IsValidReturnsTrueWhenStreamContainsValidData() => File.OpenRead(FileName1).Using(stream =>
        {
            Assert.True(ModelTexture.IsValid(stream));
        });

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
            var images = ModelTexture.Read(stream).Images;

            AssertPalette(images, 0, 0, 0, 0, 0);
            AssertPalette(images, 0, 4, 10, 10, 10);
            AssertPalette(images, 0, 8, 11, 15, 23);
            AssertPalette(images, 0, 16, 23, 19, 29);
            AssertPalette(images, 0, 32, 23, 35, 56);
            AssertPalette(images, 0, 64, 57, 51, 71);
            AssertPalette(images, 0, 128, 153, 96, 7);

            AssertPalette(images, 1, 3, 22, 18, 27);
        });

        [Theory]
        [InlineData(0, 0, 0, 0x0)]
        [InlineData(1, 0, 0, 0x4)]
        [InlineData(2, 0, 0, 0x8)]
        [InlineData(4, 0, 0, 0x10)]
        [InlineData(7, 0, 0, 0x1c)]
        [InlineData(8, 0, 0, 0x100)]
        [InlineData(16, 0, 0, 0x20)]
        [InlineData(32, 0, 0, 0x200)]
        [InlineData(64, 0, 0, 0x400)]
        [InlineData(0, 4, 0, 0x40)]
        [InlineData(8, 4, 0, 0x140)]
        [InlineData(16, 4, 0, 0x60)]
        [InlineData(32, 4, 0, 0x240)]
        [InlineData(0, 0x08, 0, 0x1000)]
        [InlineData(0, 0x08, 1, 0x1020)]
        [InlineData(0, 0x08, 2, 0x1200)]
        [InlineData(0, 0x08, 3, 0x1220)]
        [InlineData(0, 0x08, 4, 0x1400)]
        [InlineData(0, 0x08, 8, 0x1800)]
        [InlineData(0, 0x0c, 0, 0x1040)]
        [InlineData(0, 0x10, 0, 0x80)]
        [InlineData(0, 0x20, 0, 0x2000)]
        public void PointerTest(int index, int cbp, int csa, int expectedPointer)
        {
            Assert.Equal(expectedPointer / 4, ModelTexture.GetClutPointer(index, cbp, csa));
        }

        private void AssertPalette(List<ModelTexture.Texture> textures, int imageIndex, int colorIndex, byte r, byte g, byte b)
        {
            var texture = textures[imageIndex];
            var clut = texture.GetClut();

            try
            {
                Assert.Equal(r, clut[colorIndex * 4 + 0]);
                Assert.Equal(g, clut[colorIndex * 4 + 1]);
                Assert.Equal(b, clut[colorIndex * 4 + 2]);
            }
            catch
            {
                Console.WriteLine($"Error for texture {imageIndex}");
                throw;
            }
        }

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
        [InlineData("8bit-512-256")]
        [InlineData("8bit-512-512")]
        public void CreateAndRead(string baseName)
        {
            var image = File.OpenRead($"kh2/res/image-{baseName}.imd").Using(s => Imgd.Read(s));

            {
                var outStream = new MemoryStream();
                {
                    var textures = new ModelTexture(new Imgd[] { image });
                    textures.Write(outStream);
                }
                {
                    outStream.Position = 0;
                    var textures = ModelTexture.Read(outStream);
                    Assert.Single(textures.Images);

                    var converted = textures.Images.Single();

                    Assert.Equal(image.PixelFormat, converted.PixelFormat);
                    Assert.Equal(image.Size, converted.Size);
                    Assert.Equal(image.GetClut(), converted.GetClut());
                    Assert.Equal(image.GetData(), converted.GetData());
                }
            }

            {
                var outStream = new MemoryStream();
                {
                    var textures = new ModelTexture(new Imgd[] { image, image });
                    textures.Write(outStream);
                }
                {
                    outStream.Position = 0;
                    var textures = ModelTexture.Read(outStream);
                    Assert.Equal(2, textures.Images.Count);

                    foreach (var converted in textures.Images)
                    {
                        Assert.Equal(image.PixelFormat, converted.PixelFormat);
                        Assert.Equal(image.Size, converted.Size);
                        Assert.Equal(image.GetClut(), converted.GetClut());
                        Assert.Equal(image.GetData(), converted.GetData());
                    }
                }
            }

            {
                var outStream = new MemoryStream();
                {
                    var textures = new ModelTexture(new Imgd[] { image, image, image, image });
                    textures.Write(outStream);
                }
                {
                    outStream.Position = 0;
                    var textures = ModelTexture.Read(outStream);
                    Assert.Equal(4, textures.Images.Count);

                    foreach (var converted in textures.Images)
                    {
                        Assert.Equal(image.PixelFormat, converted.PixelFormat);
                        Assert.Equal(image.Size, converted.Size);
                        Assert.Equal(image.GetClut(), converted.GetClut());
                        Assert.Equal(image.GetData(), converted.GetData());
                    }
                }
            }
        }
    }
}
