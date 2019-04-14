using kh.kh2;
using System.IO;
using Xunit;

namespace kh.tests.kh2
{
    public class MsgTests : Common
    {
        private readonly string FileName = "kh2/res/titl.bin";

        [Fact]
        public void IsValidTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x01);
                stream.WriteByte(0x00);
                stream.WriteByte(0x00);
                stream.WriteByte(0x00);
                stream.Position = 0;
                Assert.True(Msg.IsValid(stream));
            }
        }
    }
}
