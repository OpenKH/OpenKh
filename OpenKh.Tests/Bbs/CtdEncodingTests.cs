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
        [InlineData('~', 0x7e)]
        public void InternationalEncodingSimpleTest(char ch, byte expected)
        {
            var actual = euEnc.Encode($"{ch}");
            Assert.Equal(expected, actual[0]);
        }

        [Theory]
        [InlineData('Ò', 0x99, 0x8f)]
        [InlineData('ç', 0x99, 0x9f)]
        [InlineData('ú', 0x99, 0xaf)]
        public void InternationalEncodingExtTest(char ch, byte expected, byte expected2)
        {
            var actual = euEnc.Encode($"{ch}");
            Assert.Equal(expected, actual[0]);
            Assert.Equal(expected2, actual[1]);
        }

        [Theory]
        [InlineData(' ', 0x20)]
        [InlineData('A', 0x41)]
        [InlineData('~', 0x7e)]
        public void InternationalDecodingSimpleTest(char expected, byte ch)
        {
            string actual = euDec.Decode(new byte[] { ch });
            Assert.Equal(expected, actual[0]);
        }

        [Theory]
        [InlineData('Ò', 0x99, 0x8f)]
        [InlineData('ç', 0x99, 0x9f)]
        [InlineData('ú', 0x99, 0xaf)]
        public void InternationalDecodingExtTest(char expected, byte ch, byte ch2)
        {
            string actual = euDec.Decode(new byte[] { ch, ch2 });
            Assert.Equal(expected, actual[0]);
        }
    }
}
