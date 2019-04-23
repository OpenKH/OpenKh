using kh.kh2;
using System.IO;
using Xunit;

namespace kh.tests.kh2
{
    public class SequenceTests
    {
        [Fact]
        public void IsValidTest() => new MemoryStream()
            .Using(stream =>
            {
                stream.WriteByte(0x53);
                stream.WriteByte(0x45);
                stream.WriteByte(0x51);
                stream.WriteByte(0x44);
                stream.Write(new byte[44]);
                stream.Position = 0;
                Assert.True(Sequence.IsValid(stream));
                Assert.Equal(0, stream.Position);
            });

        [Fact]
        public void IsInvalidHeaderTest() => new MemoryStream()
            .Using(stream =>
            {
                stream.WriteByte(0xCC);
                stream.WriteByte(0xCC);
                stream.WriteByte(0xCC);
                stream.WriteByte(0xCC);
                stream.Write(new byte[44]);
                stream.Position = 0;
                Assert.False(Sequence.IsValid(stream));
                Assert.Equal(0, stream.Position);
            });

        [Fact]
        public void IsInvalidStreamLengthTest() => new MemoryStream()
            .Using(stream =>
            {
                stream.WriteByte(0x53);
                stream.WriteByte(0x45);
                stream.WriteByte(0x51);
                stream.WriteByte(0x44);
                stream.Position = 0;
                Assert.False(Sequence.IsValid(stream));
                Assert.Equal(0, stream.Position);
            });
    }
}
