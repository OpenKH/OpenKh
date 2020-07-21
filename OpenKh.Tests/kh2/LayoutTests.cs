using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class LayoutTests
    {
        private const string FilePath = "kh2/res/eh_l.layd";

        [Fact]
        public void IsValidTest() => new MemoryStream()
            .Using(stream =>
            {
                stream.WriteByte(0x4C);
                stream.WriteByte(0x41);
                stream.WriteByte(0x59);
                stream.WriteByte(0x44);
                stream.Write(new byte[28]);
                stream.Position = 0;
                Assert.True(Layout.IsValid(stream));
                Assert.Equal(0, stream.Position);
            });

        [Fact]
        public void IsInvalidHeaderTest() => new MemoryStream()
            .Using(stream =>
            {
                stream.WriteByte(0x42);
                stream.WriteByte(0x41);
                stream.WriteByte(0x52);
                stream.WriteByte(0x01);
                stream.Write(new byte[28]);
                stream.Position = 0;
                Assert.False(Layout.IsValid(stream));
                Assert.Equal(0, stream.Position);
            });

        [Fact]
        public void IsInvalidStreamLengthTest() => new MemoryStream()
            .Using(stream =>
            {
                stream.WriteByte(0x4C);
                stream.WriteByte(0x41);
                stream.WriteByte(0x59);
                stream.WriteByte(0x44);
                stream.Position = 0;
                Assert.False(Layout.IsValid(stream));
                Assert.Equal(0, stream.Position);
            });

        [Fact]
        public void ValidateLayoutHeader() => Common.FileOpenRead(FilePath, stream =>
        {
            var layout = Layout.Read(stream);
            Assert.Equal(32, layout.SequenceGroups.SelectMany(x => x.Sequences).Count());
            Assert.Equal(32, layout.SequenceGroups.Count);
            Assert.Equal(5, layout.SequenceItems.Count);
        });

        [Fact]
        public void ValidateL2() => Common.FileOpenRead(FilePath, stream =>
        {
            var layout = Layout.Read(stream);
            Assert.Equal(1, layout.SequenceGroups[0].Sequences.Count);
            Assert.Equal(0, layout.SequenceGroups[0].Unknown04);
            Assert.Equal(0, layout.SequenceGroups[0].Unknown08);
            Assert.Equal(0, layout.SequenceGroups[0].Unknown0c);
            Assert.Equal(0, layout.SequenceGroups[0].Unknown10);
            Assert.Equal(1, layout.SequenceGroups[1].Sequences.Count);
            Assert.Equal(0, layout.SequenceGroups[1].Unknown04);
            Assert.Equal(0, layout.SequenceGroups[1].Unknown08);
            Assert.Equal(0, layout.SequenceGroups[1].Unknown0c);
            Assert.Equal(0, layout.SequenceGroups[1].Unknown10);
            Assert.Equal(1, layout.SequenceGroups[2].Sequences.Count);
            Assert.Equal(0, layout.SequenceGroups[2].Unknown04);
            Assert.Equal(0, layout.SequenceGroups[2].Unknown08);
            Assert.Equal(0, layout.SequenceGroups[2].Unknown0c);
            Assert.Equal(0, layout.SequenceGroups[2].Unknown10);
        });

        [Fact]
        public void WriteLayoutTest() => Common.FileOpenRead(FilePath, stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Layout.Read(inStream).Write(outStream);

                return outStream;
            });
        });
    }
}
