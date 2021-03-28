using OpenKh.Kh2.Messages;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class MsgEncoderTests
    {
        public enum EncoderType
        {
            InternationalSystem,
            JapaneseEvent,
            JapaneseSystem,
            TurkishSystem
        };

        [Fact]
        public void DecodeTextCorrectly()
        {
            var decoded = Encoders.InternationalSystem.Decode(new byte[]
            {
                0x35, 0x9E, 0xA5, 0xA5, 0xA8, 0x01, 0xB0, 0xA8, 0xAB, 0xA5, 0x9D, 0x48, 0x00
            });

            Assert.Equal(MessageCommand.PrintText, decoded[0].Command);
            Assert.Equal("Hello world!", decoded[0].Text);
            Assert.Equal(MessageCommand.End, decoded[1].Command);
        }

        [Fact]
        public void DecodeComplexTextCorrectly()
        {
            var decoded = Encoders.InternationalSystem.Decode(new byte[]
            {
                0x90, 0x7a, 0x91, 0x00
            });

            Assert.Equal(MessageCommand.PrintText, decoded[0].Command);
            Assert.Equal("0", decoded[0].Text);
            Assert.Equal(MessageCommand.PrintComplex, decoded[1].Command);
            Assert.Equal("VII", decoded[1].Text);
            Assert.Equal(MessageCommand.PrintText, decoded[2].Command);
            Assert.Equal("1", decoded[2].Text);
            Assert.Equal(MessageCommand.End, decoded[3].Command);
        }

        [Fact]
        public void DecodeIconCorrectly()
        {
            var decoded = Encoders.InternationalSystem.Decode(new byte[]
            {
                0x09, 0x00, 0x09, 0x01, 0x00
            });

            Assert.Equal(MessageCommand.PrintIcon, decoded[0].Command);
            Assert.Equal(0, decoded[0].Data[0]);
            Assert.Equal(MessageCommand.PrintIcon, decoded[1].Command);
            Assert.Equal(1, decoded[1].Data[0]);
            Assert.Equal(MessageCommand.End, decoded[2].Command);
        }

        [Fact]
        public void DecodeColorCommandCorrectly()
        {
            var decoded = Encoders.InternationalSystem.Decode(new byte[]
            {
                0x07, 0x00, 0xFF, 0x00, 0x00, 0x00
            });

            Assert.Equal(MessageCommand.Color, decoded[0].Command);
            Assert.Equal(0, decoded[0].Data[0]);
            Assert.Equal(0xFF, decoded[0].Data[1]);
            Assert.Equal(0, decoded[0].Data[2]);
            Assert.Equal(0, decoded[0].Data[3]);
            Assert.Equal(MessageCommand.End, decoded[1].Command);
        }

        [Theory]
        [InlineData(0x03, "0123456789")]
        [InlineData(0x04, "123456789")]
        [InlineData(0x05, "6789")]
        [InlineData(0x06, "123456789")]
        [InlineData(0x07, "456789")]
        [InlineData(0x08, "3456789")]
        [InlineData(0x09, "123456789")]
        [InlineData(0x0a, "123456789")]
        [InlineData(0x0b, "123456789")]
        [InlineData(0x0c, "123456789")]
        [InlineData(0x0d, "0123456789")]
        [InlineData(0x0e, "123456789")]
        [InlineData(0x0f, "56789")]
        //[InlineData(0x10, "")]
        [InlineData(0x11, "456789")]
        [InlineData(0x12, "23456789")]
        [InlineData(0x13, "456789")]
        [InlineData(0x14, "23456789")]
        [InlineData(0x15, "23456789")]
        [InlineData(0x16, "123456789")]
        //[InlineData(0x17, "")]
        [InlineData(0x18, "23456789")]
        public void DecodeTheRightAmountOfCharacters(byte commandId, string expectedText)
        {
            var decoded = Encoders.InternationalSystem.Decode(new byte[]
            {
                commandId, 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99
            });

            Assert.Equal(expectedText, decoded[1].Text);
        }

        [Theory]
        [InlineData("I", 0x74)]
        [InlineData("II", 0x75)]
        [InlineData("III", 0x76)]
        [InlineData("IV", 0x77)]
        [InlineData("V", 0x78)]
        [InlineData("VI", 0x79)]
        [InlineData("♪", 0x8e)]
        public void EncodeComplexTextCorrectly(string value, byte expected)
        {
            var actual = Encoders.InternationalSystem.Encode(new List<MessageCommandModel>
            {
                new MessageCommandModel
                {
                    Command = MessageCommand.PrintComplex,
                    Text = value
                }
            })[0];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SimpleReEncodeTest()
        {
            var expected = new byte[]
            {
                0x01, 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96,
                0x97, 0x98, 0x99, 0x9a, 0x9b, 0x9c, 0x9d, 0x9e,
            };
            var decoded = Encoders.InternationalSystem.Decode(expected);
            var encoded = Encoders.InternationalSystem.Encode(decoded);

            Assert.Equal(expected, encoded);
        }

        [Theory]
        [InlineData(0x02)]
        [InlineData(0x03)]
        [InlineData(0x04)]
        [InlineData(0x05)]
        [InlineData(0x06)]
        [InlineData(0x07)]
        [InlineData(0x08)]
        [InlineData(0x09)]
        [InlineData(0x0a)]
        [InlineData(0x0b)]
        [InlineData(0x0c)]
        [InlineData(0x0d)]
        [InlineData(0x0e)]
        [InlineData(0x0f)]
        //[InlineData(0x10)]
        [InlineData(0x11)]
        [InlineData(0x12)]
        [InlineData(0x13)]
        [InlineData(0x14)]
        [InlineData(0x15)]
        [InlineData(0x16)]
        //[InlineData(0x17)]
        [InlineData(0x18)]
        public void AdvancedReEncodeTest(byte commandByte)
        {
            var expected = new byte[]
            {
                commandByte, 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96,
                0x97, 0x98, 0x99, 0x9a, 0x9b, 0x9c, 0x9d, 0x9e,
            };
            var decoded = Encoders.InternationalSystem.Decode(expected);
            var encoded = Encoders.InternationalSystem.Encode(decoded);

            Assert.Equal(expected, encoded);
        }

        [Theory]
        [InlineData(0x1a, 0x00, '珍')]
        [InlineData(0x1b, 0x00, '何')]
        [InlineData(0x1c, 0x00, '係')]
        [InlineData(0x1e, 0x40, '外')]
        [InlineData(0x1f, 0x00, '銅')]
        [InlineData(0x1f, 0xc7, '念')]
        [InlineData(0x1f, 0xc8, '還')]
        [InlineData(0x1f, 0xDF, '夕')]
        public void DecodeJapaneseSystemTextCorrectly(byte command, byte data, char expected)
        {
            var decoded = Encoders.JapaneseSystem.Decode(new byte[] { command, data });

            Assert.NotEmpty(decoded);
            Assert.Equal(expected, decoded.Single().Text.Single());
        }

        [Theory]
        [InlineData("\x19\x0a\x19\x1e\x55", "ロビー")]
        public void DecodeJapaneseSystemTextWordCorrectly(string data, string expected)
        {
            var decoded = Encoders.JapaneseSystem.Decode(
                System.Text.Encoding.GetEncoding("latin1").GetBytes(data)
            );

            Assert.NotEmpty(decoded);
            Assert.Equal(expected, decoded.Single().Text);
        }

        [Theory]
        [InlineData(0x84, 0x00, "III")]
        [InlineData(0x85, 0x00, "VII")]
        [InlineData(0x86, 0x00, "VIII")]
        [InlineData(0x87, 0x00, "X")]
        [InlineData(0x19, 0xb2, "XIII")]
        [InlineData(0x1b, 0x54, "I")]
        [InlineData(0x1b, 0x55, "II")]
        [InlineData(0x1b, 0x56, "IV")]
        [InlineData(0x1b, 0x57, "V")]
        [InlineData(0x1b, 0x58, "VI")]
        [InlineData(0x1b, 0x59, "IX")]
        [InlineData(0x1e, 0x66, "IV")]
        public void DecodeRomanNumbersFromJapaneseSystemTable(byte command, byte data, string expected)
        {
            var decoded = Encoders.JapaneseSystem.Decode(new byte[] { command, data });

            Assert.NotEmpty(decoded);
            Assert.Equal(MessageCommand.PrintComplex, decoded.First().Command);
            Assert.Equal(expected, decoded.First().Text);
        }

        [Theory]
        [InlineData(0x84, 0x00, "III")]
        [InlineData(0x85, 0x00, "VII")]
        [InlineData(0x86, 0x00, "VIII")]
        [InlineData(0x87, 0x00, "X")]
        [InlineData(0x19, 0xb2, "XIII")]
        [InlineData(0x1b, 0x54, "I")]
        [InlineData(0x1b, 0x55, "II")]
        [InlineData(0x1b, 0x56, "IV")]
        [InlineData(0x1b, 0x57, "V")]
        [InlineData(0x1b, 0x58, "VI")]
        [InlineData(0x1b, 0x59, "IX")]
        public void EncodeRomanNumbersForJapaneseSystemTable(byte command, byte data, string textSource)
        {
            var encoded = Encoders.JapaneseSystem.Encode(new List<MessageCommandModel>
            {
                new MessageCommandModel
                {
                    Command = MessageCommand.PrintComplex,
                    Text = textSource
                }
            });

            if (data == 0)
                Assert.Equal(new byte[] { command }, encoded);
            else
                Assert.Equal(new byte[] { command, data }, encoded);
        }

        [Theory]
        [InlineData(EncoderType.InternationalSystem)]
        [InlineData(EncoderType.JapaneseEvent)]
        [InlineData(EncoderType.JapaneseSystem)]
        [InlineData(EncoderType.TurkishSystem)]
        public void EncodeNewLineCharacterCorrectly(EncoderType encoderType)
        {
            var encoder = new IMessageEncode[]
            {
                Encoders.InternationalSystem,
                Encoders.JapaneseEvent,
                Encoders.JapaneseSystem,
                Encoders.TurkishSystem
            }[(int)encoderType];

            var encoded = encoder.Encode(new List<MessageCommandModel>
            {
                new MessageCommandModel
                {
                    Command = MessageCommand.PrintText,
                    Text = "HE\nLLO",
                }
            });
            Assert.Equal(new byte[] { 0x35, 0x32, 0x02, 0x39, 0x39, 0x3C }, encoded);
        }

        [Theory]
        [InlineData(EncoderType.InternationalSystem)]
        [InlineData(EncoderType.JapaneseEvent)]
        [InlineData(EncoderType.JapaneseSystem)]
        [InlineData(EncoderType.TurkishSystem)]
        public void DecodeNewLineCharacterCorrectly(EncoderType encoderType)
        {
            var decoder = new IMessageDecode[]
            {
                Encoders.InternationalSystem,
                Encoders.JapaneseEvent,
                Encoders.JapaneseSystem,
                Encoders.TurkishSystem
            }[(int)encoderType];

            var models = decoder.Decode(new byte[] { 0x35, 0x32, 0x02, 0x39, 0x39, 0x3C });
            Assert.Single(models);
            Assert.Equal("HE\nLLO", models[0].Text);
        }

        [Theory]
        [InlineData(0x1a, 0x02, '納')]
        [InlineData(0x1b, 0x00, '竜')]
        [InlineData(0x1c, 0x00, '操')]
        [InlineData(0x1d, 0x00, '猫')]
        [InlineData(0x1f, 0x00, '漠')]
        public void DecodeJapaneseEventTextCorrectly(byte command, byte data, char expected)
        {
            var decoded = Encoders.JapaneseEvent.Decode(new byte[] { command, data });

            Assert.NotEmpty(decoded);
            Assert.Equal(expected, decoded.Single().Text.Single());
        }

        [Theory]
        [InlineData("\x19\x25\x55", "プー")]
        [InlineData("\x19\x16\xe0\x01\xf8\x55\xf3\x66\x66", "ゼア ノート--")]
        public void DecodeJapaneseEventTextWordCorrectly(string data, string expected)
        {
            var decoded = Encoders.JapaneseEvent.Decode(
                System.Text.Encoding.GetEncoding("latin1").GetBytes(data)
            );

            Assert.NotEmpty(decoded);
            Assert.Equal(expected, decoded.Single().Text);
        }
    }
}
