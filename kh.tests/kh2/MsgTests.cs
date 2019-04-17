using kh.kh2;
using System.IO;
using Xunit;

namespace kh.tests.kh2
{
    public class MsgTests : Common
    {
        private readonly string FilePath = "kh2/res/titl.bin";

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

        [Fact]
        public void IsNotValidTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x02);
                stream.WriteByte(0x00);
                stream.WriteByte(0x00);
                stream.WriteByte(0x00);
                stream.Position = 0;
                Assert.False(Msg.IsValid(stream));
            }
        }

        [Fact]
        public void OpenThrowsAnExceptionIfTheStreamIsNotValid()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x02);
                stream.WriteByte(0x00);
                stream.WriteByte(0x00);
                stream.WriteByte(0x00);
                stream.Position = 0;
                Assert.Throws<InvalidDataException>(() => Msg.Open(stream));
            }
        }

        [Fact]
        public void ReadsRightAmountOfEntries() =>
            FileOpenRead(FilePath, stream =>
            {
                var entries = Msg.Open(stream);
                Assert.Equal(694, entries.Count);
            });

        [Fact]
        public void ReadsIdCorrectly() =>
            FileOpenRead(FilePath, stream =>
            {
                var entries = Msg.Open(stream);
                Assert.Equal(20233, entries[0].Id);
                Assert.Equal(20234, entries[1].Id);
            });

        [Fact]
        public void ReadsTextDataCorrectly() =>
            FileOpenRead(FilePath, stream =>
            {
                var entries = Msg.Open(stream);
                Assert.Equal(19, entries[0].Data.Length);
                Assert.Equal(7, entries[1].Data.Length);
            });

        [Fact]
        public void WriteAndReadCorrecty() =>
            FileOpenRead(FilePath, stream =>
            {
                var expected = new BinaryReader(stream).ReadBytes((int)stream.Length);
                var entries = Msg.Open(new MemoryStream(expected));
                byte[] actual;

                using (var outStream = new MemoryStream())
                {
                    Msg.Write(outStream, entries);
                    outStream.Position = 0;
                    actual = new BinaryReader(outStream).ReadBytes((int)outStream.Length);
                }

                Assert.Equal(expected, actual);
            });

        [Fact]
        public void DecodeTextCorrectly()
        {
            var decodedEntry = MsgParser.Map(new Msg.Entry()
            {
                Data = new byte[]
                {
                    0x35, 0x9E, 0xA5, 0xA5, 0xA8, 0x01, 0xB0, 0xA8, 0xAB, 0xA5, 0x9D, 0x48, 0x00
                }
            });

            Assert.Equal(MsgParser.Command.PrintText, decodedEntry[0].Command);
            Assert.Equal("Hello world!", decodedEntry[0].Text);
            Assert.Equal(MsgParser.Command.End, decodedEntry[1].Command);
        }

        [Fact]
        public void DecodeIconCorrectly()
        {
            var decodedEntry = MsgParser.Map(new Msg.Entry()
            {
                Data = new byte[]
                {
                    0x09, 0x00, 0x09, 0x01, 0x00
                }
            });

            Assert.Equal(MsgParser.Command.PrintIcon, decodedEntry[0].Command);
            Assert.Equal(0, decodedEntry[0].Data[0]);
            Assert.Equal(MsgParser.Command.PrintIcon, decodedEntry[1].Command);
            Assert.Equal(1, decodedEntry[1].Data[0]);
            Assert.Equal(MsgParser.Command.End, decodedEntry[2].Command);
        }

        [Fact]
        public void DecodeColorCommandCorrectly()
        {
            var decodedEntry = MsgParser.Map(new Msg.Entry()
            {
                Data = new byte[]
                {
                    0x07, 0x00, 0xFF, 0x00, 0x00, 0x00
                }
            });

            Assert.Equal(MsgParser.Command.Color, decodedEntry[0].Command);
            Assert.Equal(0, decodedEntry[0].Data[0]);
            Assert.Equal(0xFF, decodedEntry[0].Data[1]);
            Assert.Equal(0, decodedEntry[0].Data[2]);
            Assert.Equal(0, decodedEntry[0].Data[3]);
            Assert.Equal(MsgParser.Command.End, decodedEntry[1].Command);
        }

        [Theory]
        [InlineData(0x02, "0123456789")]
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
        //[InlineData(0x11, "")]
        [InlineData(0x12, "23456789")]
        [InlineData(0x13, "456789")]
        [InlineData(0x14, "23456789")]
        [InlineData(0x15, "23456789")]
        [InlineData(0x16, "123456789")]
        //[InlineData(0x17, "")]
        [InlineData(0x18, "23456789")]
        [InlineData(0x19, "123456789")]
        [InlineData(0x1a, "123456789")]
        [InlineData(0x1b, "123456789")]
        [InlineData(0x1c, "123456789")]
        [InlineData(0x1d, "123456789")]
        [InlineData(0x1e, "123456789")]
        [InlineData(0x1f, "123456789")]
        public void DecodeTheRightAmountOfCharacters(byte commandId, string expectedText)
        {
            var decodedEntry = MsgParser.Map(new Msg.Entry()
            {
                Data = new byte[]
                {
                    commandId, 0x90, 0x91, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99
                }
            });

            Assert.Equal(expectedText, decodedEntry[1].Text);
        }
    }
}
