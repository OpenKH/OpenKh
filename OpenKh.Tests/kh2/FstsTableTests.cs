using OpenKh.Kh2.System;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class FstsTableTests
    {
        [Fact]
        public void CheckForLength() => Common.FileOpenRead("kh2/res/ftst.bin", stream =>
        {
            var entries = Ftst.Read(stream);
            Assert.Equal(8, entries.Count);
        });

        [Fact]
        public void ShouldWriteTheExactSameFile() => Common.FileOpenRead("kh2/res/ftst.bin", stream =>
        {
            Helpers.AssertStream(stream, x =>
            {
                var outStream = new MemoryStream();

                Ftst.Write(outStream, Ftst.Read(x));

                return outStream;
            });
        });
    }
}
