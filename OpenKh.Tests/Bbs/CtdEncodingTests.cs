using OpenKh.Bbs.Messages;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class CtdEncodingTests
    {
        private readonly ICtdMessageDecode euDec = CtdEncoders.International;
        private readonly ICtdMessageEncode euEnc = CtdEncoders.International;

        [Theory]
        [InlineData(' ', 0x20)]
        [InlineData('A', 0x41)]
        public void InternationalEncodingTest(char ch, byte expected, byte? expected2 = null)
        {
            var actual = euEnc.Encode($"{ch}");
            Assert.Equal(expected, actual[0]);
            if (expected2.HasValue)
                Assert.Equal(expected2, actual[1]);
        }

        [Theory]
        [InlineData(' ', 0x20)]
        [InlineData('A', 0x41)]
        public void InternationalDecodingTest(char expected, byte ch, byte? ch2 = null)
        {
            string actual;
            if (ch2.HasValue)
                actual = euDec.Decode(new byte[] { ch, ch2.Value });
            else
                actual = euDec.Decode(new byte[] { ch });

            Assert.Equal(expected, actual[0]);
        }
    }
}
