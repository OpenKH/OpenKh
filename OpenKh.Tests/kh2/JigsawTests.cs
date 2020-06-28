using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class JigsawTests
    {
        private const string FileName = "kh2/res/15jigsaw.bin";

        [Fact]
        public void HasRightAmountOfEntries() => Common.FileOpenRead(FileName, stream =>
        {
            Assert.Equal(0x9E, Jigsaw.Read(stream).Count);
        });

        [Fact]
        public void WriteBackTheSameFile() => Common.FileOpenRead(FileName, stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Jigsaw.Write(outStream, Jigsaw.Read(inStream));

                return outStream;
            });
        });
    }
}
