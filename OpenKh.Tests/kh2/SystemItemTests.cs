using OpenKh.Kh2.System;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class SystemItemTests
    {
        [Fact]
        public void CheckForLength() => Common.FileOpenRead("kh2/res/item.bin", stream =>
        {
            var entries = Item.Read(stream);
            Assert.Equal(535, entries.Items1.Count);
            Assert.Equal(151, entries.Items2.Count);
        });

        [Fact]
        public void ShouldWriteTheExactSameFile() => Common.FileOpenRead("kh2/res/item.bin", stream =>
        {
            Helpers.AssertStream(stream, x =>
            {
                var outStream = new MemoryStream();

                Item.Read(x).Write(outStream);

                return outStream;
            });
        });
    }
}
