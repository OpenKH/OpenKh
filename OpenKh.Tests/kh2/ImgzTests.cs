using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ImgzTests
    {
        [Fact]
        public void IsValidTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x49);
                stream.WriteByte(0x4d);
                stream.WriteByte(0x47);
                stream.WriteByte(0x5a);
                stream.Position = 0;
                Assert.True(Imgz.IsValid(stream));
            }
        }
    }
}
