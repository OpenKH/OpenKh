using OpenKh.Bbs.Messages;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class CtdEncodingTests
    {
        private readonly ICtdMessageDecode uniDec = CtdEncoders.Unified;
        private readonly ICtdMessageEncode uniEnc = CtdEncoders.Unified;
        private readonly ICtdMessageEncoder encoder = CtdEncoders.Unified;

        [Theory]
        [InlineData(' ', 0x20)]
        [InlineData('A', 0x41)]
        [InlineData('~', 0x7e)]
        public void InternationalEncodingSimpleTest(char ch, byte expected)
        {
            var actual = uniEnc.FromText($"{ch}");
            Assert.Equal(expected, actual[0]);
        }

        [Theory]
        [InlineData('Ò', 0x99, 0x8f)]
        [InlineData('ç', 0x99, 0x9f)]
        [InlineData('ú', 0x99, 0xaf)]
        public void InternationalEncodingExtTest(char ch, byte expected, byte expected2)
        {
            var actual = uniEnc.FromText($"{ch}");
            Assert.Equal(expected, actual[0]);
            Assert.Equal(expected2, actual[1]);
        }

        [Theory]
        [InlineData(' ', 0x20)]
        [InlineData('A', 0x41)]
        [InlineData('~', 0x7e)]
        public void InternationalDecodingSimpleTest(char expected, byte ch)
        {
            string actual = uniDec.ToText(new byte[] { ch });
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
        [InlineData('■', 0x81, 0xa1)]
        public void InternationalDecodingExtTest(char expected, byte ch, byte ch2)
        {
            string actual = uniDec.ToText(new byte[] { ch, ch2 });
            Assert.Equal(expected, actual[0]);
        }

        [Theory]
        [InlineData(0x8143, ',')]
        [InlineData(0x8162, '|')]
        [InlineData(0x8180, '÷')]
        [InlineData(0x8190, '$')]
        [InlineData(0x81a0, '□')]
        [InlineData(0x81ab, '↓')]
        [InlineData(0x81f4, '♪')]
        public void UcsToTextTest(ushort ucs, char expected)
        {
            var byteSequence = encoder.FromUcs(new ushort[] { ucs }).ToArray();
            var text = encoder.ToText(byteSequence);

            Assert.NotEmpty(text);
            Assert.Equal(expected, text[0]);
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
            string actual = uniDec.ToText(new byte[] { command, parameter });
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0xf1, 0xae, "{:icon button-triangle}")]
        [InlineData(0xf1, 0xb4, "{:icon button-l}")]
        [InlineData(0xf1, 0xc5, "{:icon button-dpad-v}")]
        [InlineData(0xf9, 0x41, "{:color default}")]
        [InlineData(0xf9, 0x58, "{:color white}")]
        [InlineData(0xf9, 0x59, "{:color yellow}")]
        public void InternationalEncodingCommandTest(byte command, byte parameter, string input)
        {
            byte[] actual = uniEnc.FromText(input);
            Assert.Equal(new byte[] { command, parameter }, actual);
        }

        [Theory]
        [InlineData(0x8f, "{:unk 8f}")]
        [InlineData(0xfe, "{:unk fe}")]
        public void InternationalDecodingUnknownTest(byte command, string expected)
        {
            string actual = uniDec.ToText(new byte[] { command });
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0x8f, "{:unk 8f}")]
        [InlineData(0xfe, "{:unk fe}")]
        public void InternationalEncodingUnknownTest(byte command, string input)
        {
            byte[] actual = uniEnc.FromText(input);
            Assert.Equal(new byte[] { command }, actual);
        }

        [Theory]
        [InlineData('　', 0x8140)]
        [InlineData('、', 0x8141)]
        [InlineData('¶', 0x81F7)]
        [InlineData('０', 0x824F)]
        [InlineData('ん', 0x82F1)]
        [InlineData('北', 0x966B)]
        [InlineData('犾', 0xEE40)]
        [InlineData('鸙', 0xEEEB)]
        public void JapaneseEncodingDecodingTest(char ch, ushort expectedCode)
        {
            var sjisBytes = uniEnc.FromText(
                ch.ToString(),
                new CtdFromTextOptions { EncodeAsShiftJIS = true, }
            );
            Assert.Equal(
                new byte[] { (byte)(expectedCode >> 8), (byte)expectedCode },
                sjisBytes
            );
            var recoveredText = uniDec.ToText(
                sjisBytes,
                new CtdToTextOptions { DecodeAsShiftJIS = true, }
            );
            Assert.Equal(ch.ToString(), recoveredText);
        }
    }
}
