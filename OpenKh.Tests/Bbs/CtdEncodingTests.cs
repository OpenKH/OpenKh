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
        [InlineData('$', 0x81, 0x90)]
        [InlineData('%', 0x81, 0x93)]
        [InlineData('#', 0x81, 0x94)]
        [InlineData('&', 0x81, 0x95)]
        [InlineData('*', 0x81, 0x96)]
        [InlineData('@', 0x81, 0x97)]
        public void InternationalDecodingExtTest(char expected, byte ch, byte ch2)
        {
            string actual = euDec.Decode(new byte[] { ch, ch2 });
            Assert.Equal(expected, actual[0]);
        }

        [Theory]
        [InlineData(0xf1, 0xae, "{:icon button-triangle}")]
        [InlineData(0xf1, 0xb4, "{:icon button-l}")]
        [InlineData(0xf1, 0xc5, "{:icon button-dpad-v}")]
        [InlineData(0xf9, 0x41, "{:color default}")]
        [InlineData(0xf9, 0x58, "{:color white}")]
        [InlineData(0xf9, 0x59, "{:color yellow}")]
        public void InternationalDecodingCommandTest(byte command, byte parameter, string expected)
        {
            string actual = euDec.Decode(new byte[] { command, parameter });
            Assert.Equal(expected, actual);
        }
    }
}
