using OpenKh.Common;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class BarTests
    {
        private const string FilePath = "kh2/res/bar_test.bar";

        [Fact]
        public void IsValidTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x42);
                stream.WriteByte(0x41);
                stream.WriteByte(0x52);
                stream.WriteByte(0x01);
                stream.Position = 0;
                Assert.True(Bar.IsValid(stream));
            }
        }

        [Fact]
        public void IsNotValidTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(1);
                stream.WriteByte(2);
                stream.WriteByte(3);
                stream.WriteByte(4);
                stream.Position = 0;
                Assert.False(Bar.IsValid(stream));
            }
        }

        [Fact]
        public void IsReadingEntryCountCorrect() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var entries = Bar.Read(stream);
                Assert.Equal(7, entries.Count());
            });

        [Fact]
        public void IsReadingEntryNamesCorrectly() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var entries = Bar.Read(stream);
                Assert.Equal("s_da", entries[0].Name);
                Assert.Equal("m_da", entries[1].Name);
                Assert.Equal("l_da", entries[2].Name);
                Assert.Equal("thru", entries[3].Name);
                Assert.Equal("tack", entries[4].Name);
                Assert.Equal("blow", entries[5].Name);
                Assert.Equal("game", entries[6].Name);
            });

        [Fact]
        public void IsReadingEntrySizesCorrectly() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var entries = Bar.Read(stream);
                Assert.Equal(40, entries[0].Stream.Length);
                Assert.Equal(40, entries[1].Stream.Length);
                Assert.Equal(48, entries[2].Stream.Length);
                Assert.Equal(40, entries[3].Stream.Length);
                Assert.Equal(56, entries[4].Stream.Length);
                Assert.Equal(64, entries[5].Stream.Length);
                Assert.Equal(56, entries[6].Stream.Length);
            });

        [Fact]
        public void IsReadingEntryTypesCorrectly() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var entries = Bar.Read(stream);
                Assert.Equal(Bar.EntryType.Vibration, entries[0].Type);
                Assert.Equal(Bar.EntryType.Vibration, entries[1].Type);
                Assert.Equal(Bar.EntryType.Vibration, entries[2].Type);
                Assert.Equal(Bar.EntryType.Vibration, entries[3].Type);
                Assert.Equal(Bar.EntryType.Vibration, entries[4].Type);
                Assert.Equal(Bar.EntryType.Vibration, entries[5].Type);
                Assert.Equal(Bar.EntryType.Vibration, entries[6].Type);
            });

        [Fact]
        public void IsReadingEntryIndexesCorrectly() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var entries = Bar.Read(stream);
                Assert.False(entries[0].Duplicate);
                Assert.False(entries[1].Duplicate);
                Assert.False(entries[2].Duplicate);
                Assert.False(entries[3].Duplicate);
                Assert.False(entries[4].Duplicate);
                Assert.False(entries[5].Duplicate);
                Assert.False(entries[6].Duplicate);
            });

        [Fact]
        public void IsReadingEntryDataCorrectly() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var entries = Bar.Read(stream);
                Assert.Equal(86, entries[0].Stream.ReadByte());
                Assert.Equal(86, entries[1].Stream.ReadByte());
                Assert.Equal(86, entries[2].Stream.ReadByte());
                Assert.Equal(86, entries[3].Stream.ReadByte());
                Assert.Equal(86, entries[4].Stream.ReadByte());
                Assert.Equal(86, entries[5].Stream.ReadByte());
                Assert.Equal(86, entries[6].Stream.ReadByte());
            });

        [Fact]
        public void IsReadingFromPcsx2Correctly()
        {
            const int MemoryAddress = 0x1c000;
            const int DataLength = 0x10;
            const int TagName = 0x30303030;
            var expectedData = Enumerable.Range(0, DataLength).Select(x => (byte)x).ToArray();

            using var writer = new BinaryWriter(new MemoryStream(0x20000));
            writer.BaseStream.Position = MemoryAddress;
            writer.Write(0x01524142U); // header
            writer.Write(1); // entry count
            writer.Write(MemoryAddress); // where it is physically located in-memory
            writer.Write(0); // default
            writer.Write(123); // entry type
            writer.Write(TagName); // entry name
            writer.Write(MemoryAddress + 0x20); // entry address
            writer.Write(DataLength); // entry size
            writer.Write(expectedData);

            // Attempt to read BAR file from the given portion of memory
            writer.BaseStream.Position = MemoryAddress;
            Assert.True(Bar.IsValid(writer.BaseStream));
            var binarc = Bar.Read(writer.BaseStream);
            Assert.Equal("0000", binarc[0].Name);
            Assert.Equal(expectedData, binarc[0].Stream.ReadBytes());
        }

        [Fact]
        public void IsWritingBackCorrectly() =>
            File.OpenRead(FilePath).Using(stream =>
            {
                var expectedData = new byte[stream.Length];
                stream.Read(expectedData, 0, expectedData.Length);

                var entries = Bar.Read(new MemoryStream(expectedData));
                using (var dstStream = new MemoryStream(expectedData.Length))
                {
                    Bar.Write(dstStream, entries);
                    dstStream.Position = 0;
                    var actualData = new byte[dstStream.Length];
                    dstStream.Read(actualData, 0, actualData.Length);

                    Assert.Equal(expectedData.Length, actualData.Length);
                    Assert.Equal(expectedData, actualData);
                }
            });

        [Theory]
        [InlineData("TEST", "TEST")]
        [InlineData("333", "333")]
        [InlineData("22", "22")]
        [InlineData("1", "1")]
        [InlineData("666666", "6666")]
        public void AlwaysReadCorrectName(string name, string expectedName)
        {
            var entries = new List<Bar.Entry>
            {
                new Bar.Entry
                {
                    Type = Bar.EntryType.DUMMY,
                    Duplicate = false,
                    Name = name,
                    Stream = new MemoryStream()
                }
            };

            using (var tmpStream = new MemoryStream())
            {
                Bar.Write(tmpStream, entries);
                tmpStream.Position = 0;
                entries = Bar.Read(tmpStream);
            }

            Assert.Equal(expectedName, entries[0].Name);
        }
    }
}
