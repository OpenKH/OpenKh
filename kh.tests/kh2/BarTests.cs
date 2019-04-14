using kh.kh2;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace kh.tests.kh2
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
            FileOpenRead(FilePath, stream =>
            {
                var entries = Bar.Open(stream);
                Assert.Equal(7, entries.Count());
            });

        [Fact]
        public void IsReadingEntryNamesCorrectly() =>
            FileOpenRead(FilePath, stream =>
            {
                var entries = Bar.Open(stream)
                    .ToArray();
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
            FileOpenRead(FilePath, stream =>
            {
                var entries = Bar.Open(stream)
                    .ToArray();
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
            FileOpenRead(FilePath, stream =>
            {
                var entries = Bar.Open(stream)
                    .ToArray();
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
            FileOpenRead(FilePath, stream =>
            {
                var entries = Bar.Open(stream)
                    .ToArray();
                Assert.Equal(0, entries[0].Index);
                Assert.Equal(0, entries[1].Index);
                Assert.Equal(0, entries[2].Index);
                Assert.Equal(0, entries[3].Index);
                Assert.Equal(0, entries[4].Index);
                Assert.Equal(0, entries[5].Index);
                Assert.Equal(0, entries[6].Index);
            });

        [Fact]
        public void IsReadingEntryDataCorrectly() =>
            FileOpenRead(FilePath, stream =>
            {
                var entries = Bar.Open(stream)
                    .ToArray();
                Assert.Equal(86, entries[0].Stream.ReadByte());
                Assert.Equal(86, entries[1].Stream.ReadByte());
                Assert.Equal(86, entries[2].Stream.ReadByte());
                Assert.Equal(86, entries[3].Stream.ReadByte());
                Assert.Equal(86, entries[4].Stream.ReadByte());
                Assert.Equal(86, entries[5].Stream.ReadByte());
                Assert.Equal(86, entries[6].Stream.ReadByte());
            });

        [Fact]
        public void IsWritingBackCorrectly() =>
            FileOpenRead(FilePath, stream =>
            {
                var expectedData = new byte[stream.Length];
                stream.Read(expectedData, 0, expectedData.Length);

                var entries = Bar.Open(new MemoryStream(expectedData));
                using (var dstStream = new MemoryStream(expectedData.Length))
                {
                    Bar.Save(dstStream, entries);
                    dstStream.Position = 0;
                    var actualData = new byte[dstStream.Length];
                    dstStream.Read(actualData, 0, actualData.Length);

                    Assert.Equal(expectedData.Length, actualData.Length);
                    Assert.Equal(expectedData, actualData);
                }
            });

        private void FileOpenRead(string path, Action<Stream> action)
        {
            using (var stream = File.OpenRead(path))
                action(stream);
        }
    }
}
