using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OpenKh.Tests.kh2
{
    public class ImgTests
    {
        private readonly ITestOutputHelper output;

        public ImgTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("50worldmap")]
        public void TestDecompression(string fileName)
        {
            using (var fileCompressed = File.OpenRead($"kh2/res/{fileName}.cmp"))
            {
                using (var fileExpected = File.OpenRead($"kh2/res/{fileName}.dec"))
                {
                    var decompressedStream = Img.Decompress(fileCompressed);

                    int matched = 0;
                    for (int i = 0; i < decompressedStream.Length; i++)
                    {
                        int expected = fileExpected.ReadByte();
                        int actual = decompressedStream[i];
                        if (expected == actual)
                            matched++;
                    }

                    Assert.Equal(fileExpected.Length, matched);
                }
            }
        }

        [Theory]
        [InlineData("50worldmap")]
        public void CompressCorrectly(string fileName) =>
            File.OpenRead($"kh2/res/{fileName}.dec").Using(stream =>
            {
                Helpers.AssertStream(stream, inStream =>
                    new MemoryStream(Img.Decompress(Img.Compress(inStream.ReadAllBytes()))));
            });

        [Theory]
        [InlineData("50worldmap")]
        public void CompressAtLEastEquallyOrMore(string fileName) =>
            File.OpenRead($"kh2/res/{fileName}.dec").Using(stream =>
            {
                const int CompressionGrace = 6;
                const int OriginalCompressedSizeLength = 0x167 + CompressionGrace;

                var compress = Img.Compress(stream.ReadAllBytes());
                if (compress.Length > OriginalCompressedSizeLength)
                {
                    throw new XunitException($"Compressed file is {compress.Length} byte, but the official one is {OriginalCompressedSizeLength}.\n" +
                        "The compression algorithm is not performant enough.");
                }
            });

        [Fact]
        public void CompressCorrectlyWhenKeyIsFoundInTheData() =>
            new MemoryStream(Enumerable.Range(0, 0x100).Select(x => (byte)x).ToArray()).Using(stream =>
            {
                Helpers.AssertStream(stream, inStream =>
                    new MemoryStream(Img.Decompress(Img.Compress(inStream.ReadAllBytes()))));
            });

        [Fact]
        public void CompressChunkSimple()
        {
            const int HeaderSize = 5;
            var dec = new byte[0x29];
            var cmp = Img.Compress(dec);

            var expected = new byte[]
            {
                0x25, 0x01, 0x01, 0x00
            };

            Assert.Equal(expected.Length, cmp.Length - HeaderSize);
            Assert.Equal(expected, cmp.Take(cmp.Length - HeaderSize));
        }

        public void AlwaysCompressCorrectly()
        {
            using var imgStream = File.OpenRead("G:\\KH2.IMG");
            new Img(imgStream, File.OpenRead("G:\\KH2.IDX").Using(Idx.Read), true)
                .Entries.ToList()
                .Where(x => x.IsCompressed)
                .Where(x => !x.IsStreamed)
                .Select(x => new
                {
                    Name = IdxName.Lookup(x),
                    Entry = x,
                    Data = imgStream
                        .SetPosition(x.Offset * 0x800)
                        .ReadBytes((x.BlockLength + 1) * 0x800)
                })
                .ToList()
                .AsParallel()
                .WithDegreeOfParallelism(16)
                .ForAll(x =>
                {
                    new MemoryStream(Img.Decompress(x.Data)).Using(stream =>
                    {
                        Helpers.AssertStream(stream, inStream =>
                        {
                            var compressedData = Img.Compress(inStream.ReadAllBytes());
                            var decompressedData = Img.Decompress(compressedData);

                            return new MemoryStream(decompressedData);
                        });
                    });
                });
        }
    }
}
