using kh.kh2;
using System.IO;
using Xunit;

namespace kh.tests.kh2
{
    public class LayoutTests
    {
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
    }
}
