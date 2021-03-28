using OpenKh.Common;
using OpenKh.Kh1;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Kh1
{
    public class KingdomArchiveTests
    {
        [Fact]
        public void ReadCorrectly()
        {
            var files = File.OpenRead("Kh1/res/kh1archive.bin").Using(KingdomArchive.Read);
            Assert.Equal(2, files.Count);
            Assert.Equal(0x80, files[0].Length);
            Assert.Equal(0x80, files[1].Length);

            Assert.Equal(0x11, files[0][0]);
            Assert.Equal(0x22, files[0][1]);

            Assert.Equal((byte)'H', files[1][0]);
            Assert.Equal((byte)'e', files[1][1]);
        }

        [Fact]
        public void WriteUsingByteList() => File.OpenRead("Kh1/res/kh1archive.bin").Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                KingdomArchive.Write(outStream, KingdomArchive.Read(inStream));

                return outStream;
            });
        });

        [Fact]
        public void WriteUsingStreamist() => File.OpenRead("Kh1/res/kh1archive.bin").Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                KingdomArchive.Write(outStream,
                    KingdomArchive.Read(inStream).Select(x => new MemoryStream(x)).ToList());

                return outStream;
            });
        });
    }
}
