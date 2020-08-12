using OpenKh.Common;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Sdk;

namespace OpenKh.Tests.kh2
{
    public class MsgTests
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
                Assert.Throws<InvalidDataException>(() => Msg.Read(stream));
            }
        }

        [Fact]
        public void ReadsRightAmountOfEntries() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var entries = Msg.Read(stream);
                Assert.Equal(694, entries.Count);
            });

        [Fact]
        public void ReadsIdCorrectly() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var entries = Msg.Read(stream);
                Assert.Equal(20233, entries[0].Id);
                Assert.Equal(20234, entries[1].Id);
            });

        [Fact]
        public void ReadsTextDataCorrectly() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var entries = Msg.Read(stream);
                Assert.Equal(19, entries[0].Data.Length);
                Assert.Equal(7, entries[1].Data.Length);
            });

        [Fact]
        public void WriteAndReadCorrecty() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var expected = new BinaryReader(stream).ReadBytes((int)stream.Length);
                var entries = Msg.Read(new MemoryStream(expected));
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
        public void WriteOptimized()
        {
            var stream = new MemoryStream();
            var msgSource = new List<Msg.Entry>()
            {
                new Msg.Entry
                {
                    Id = 100,
                    Data = new byte[] { 0x22, 0x23, 0x24, 0x00 } // this will be optimized with ID 101
                },
                new Msg.Entry
                {
                    Id = 101,
                    Data = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x00 }
                },
                new Msg.Entry
                {
                    Id = 102,
                    Data = new byte[] { 0x23, 0x24, 0x25, 0x00 }
                },
            };
            Msg.WriteOptimized(stream, msgSource);

            SkipHeader(stream);
            AssertMsgLbaEntry(stream, 100, 34);
            AssertMsgLbaEntry(stream, 101, 32);
            AssertMsgLbaEntry(stream, 102, 38);

            stream.Position = 0;
            var msg = Msg.Read(stream);
            Assert.Equal(msgSource[0].Data, msg[0].Data);
            Assert.Equal(msgSource[1].Data, msg[1].Data);
            Assert.Equal(msgSource[2].Data, msg[2].Data);
        }

        [Fact]
        public void WriteOptimizedSecondScenario()
        {
            var stream = new MemoryStream();
            var msgSource = new List<Msg.Entry>()
            {
                new Msg.Entry
                {
                    Id = 100,
                    Data = new byte[] { 0x22, 0x23, 0x00 }
                },
                new Msg.Entry
                {
                    Id = 101,
                    Data = new byte[] { 0x22, 0x23, 0x00 }
                },
                new Msg.Entry
                {
                    Id = 102,
                    Data = new byte[] { 0x22, 0x23, 0x00 }
                },
            };
            Msg.WriteOptimized(stream, msgSource);

            SkipHeader(stream);
            AssertMsgLbaEntry(stream, 100, 32);
            AssertMsgLbaEntry(stream, 101, 32);
            AssertMsgLbaEntry(stream, 102, 32);

            stream.Position = 0;
            var msg = Msg.Read(stream);
            Assert.Equal(msgSource[0].Data, msg[0].Data);
            Assert.Equal(msgSource[1].Data, msg[1].Data);
            Assert.Equal(msgSource[2].Data, msg[2].Data);
        }

        private void SkipHeader(Stream stream)
        {
            stream.Position = 8;
        }

        private void AssertMsgLbaEntry(Stream stream, int expectedId, int expectedPosition)
        {
            var actualId = stream.ReadInt32();
            var actualOffset = stream.ReadInt32();

            if (expectedId != actualId)
                throw new EqualException(expectedId, actualId);

            if (expectedPosition != actualOffset)
                throw new EqualException(expectedPosition, actualOffset);
        }
    }
}
