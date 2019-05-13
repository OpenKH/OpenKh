using OpenKh.Kh2;
using System.IO;
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
            Assert.Equal(32, layout.L1Items.Count);
            Assert.Equal(32, layout.L2Items.Count);
            Assert.Equal(5, layout.SequenceItems.Count);
        });

        [Fact]
        public void ValidateL1() => Common.FileOpenRead(FilePath, stream =>
        {
            var layout = Layout.Read(stream);
            Assert.Equal(0, layout.L1Items[0].TextureIndex);
            Assert.Equal(0, layout.L1Items[0].SequenceIndex);
            Assert.Equal(0, layout.L1Items[0].AnimationGroup);
            Assert.Equal(0, layout.L1Items[0].ShowAtFrame);
            Assert.Equal(0, layout.L1Items[0].PositionX);
            Assert.Equal(0, layout.L1Items[0].PositionY);
            Assert.Equal(0, layout.L1Items[1].TextureIndex);
            Assert.Equal(0, layout.L1Items[1].SequenceIndex);
            Assert.Equal(1, layout.L1Items[1].AnimationGroup);
            Assert.Equal(0, layout.L1Items[1].ShowAtFrame);
            Assert.Equal(0, layout.L1Items[1].PositionX);
            Assert.Equal(0, layout.L1Items[1].PositionY);
            Assert.Equal(1, layout.L1Items[2].TextureIndex);
            Assert.Equal(1, layout.L1Items[2].SequenceIndex);
            Assert.Equal(0, layout.L1Items[2].AnimationGroup);
            Assert.Equal(0, layout.L1Items[2].ShowAtFrame);
            Assert.Equal(0, layout.L1Items[2].PositionX);
            Assert.Equal(0, layout.L1Items[2].PositionY);
        });

        [Fact]
        public void ValidateL2() => Common.FileOpenRead(FilePath, stream =>
        {
            var layout = Layout.Read(stream);
            Assert.Equal(0, layout.L2Items[0].L1Index);
            Assert.Equal(1, layout.L2Items[0].L1Count);
            Assert.Equal(0, layout.L2Items[0].Unknown04);
            Assert.Equal(0, layout.L2Items[0].Unknown08);
            Assert.Equal(0, layout.L2Items[0].Unknown0c);
            Assert.Equal(0, layout.L2Items[0].Unknown10);
            Assert.Equal(1, layout.L2Items[1].L1Index);
            Assert.Equal(1, layout.L2Items[1].L1Count);
            Assert.Equal(0, layout.L2Items[1].Unknown04);
            Assert.Equal(0, layout.L2Items[1].Unknown08);
            Assert.Equal(0, layout.L2Items[1].Unknown0c);
            Assert.Equal(0, layout.L2Items[1].Unknown10);
            Assert.Equal(2, layout.L2Items[2].L1Index);
            Assert.Equal(1, layout.L2Items[2].L1Count);
            Assert.Equal(0, layout.L2Items[2].Unknown04);
            Assert.Equal(0, layout.L2Items[2].Unknown08);
            Assert.Equal(0, layout.L2Items[2].Unknown0c);
            Assert.Equal(0, layout.L2Items[2].Unknown10);
        });

        [Fact]
        public void WriteLayoutTest() => Common.FileOpenRead(FilePath, stream =>
        {
            var expected = new BinaryReader(stream).ReadBytes((int)stream.Length);
            var layout = new MemoryStream(expected).Using(x => Layout.Read(x));
            new MemoryStream().Using(x =>
            {
                layout.Write(x);
                x.Position = 0;
                var actual = new BinaryReader(x).ReadBytes((int)x.Length);
                x.Dump("dump.layd");
                Assert.Equal(expected.Length, actual.Length);
                Assert.Equal(expected, actual);
            });
        });
    }
}
