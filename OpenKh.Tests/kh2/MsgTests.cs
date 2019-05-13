using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
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
    }
}
