using kh.kh2;
using System.IO;
using Xunit;

namespace kh.tests.kh2
{
    public class ImgdTests
    {
        [Fact]
        public void IsValidTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x49);
                stream.WriteByte(0x4d);
                stream.WriteByte(0x47);
                stream.WriteByte(0x44);
                stream.Position = 0;
                Assert.True(Imgd.IsValid(stream));
                Assert.Equal(0, stream.Position);
            }
        }
    }
}
