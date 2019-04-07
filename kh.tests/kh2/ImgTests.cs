using kh.kh2;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace kh.tests.kh2
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
    }
}
