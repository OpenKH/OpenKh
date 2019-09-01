using OpenKh.Bbs;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class ArcTests : Common
    {
        private static readonly string FileName = "Bbs/res/arctest.arc";

        [Fact]
        public void IsValidTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x41);
                stream.WriteByte(0x52);
                stream.WriteByte(0x43);
                stream.WriteByte(0x00);
                Assert.True(Arc.IsValid(stream));
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
                Assert.False(Arc.IsValid(stream));
            }
        }

        [Fact]
        public void ReadCorrectAmountOfEntries() => FileOpenRead(FileName, stream =>
        {
            var entries = Arc.Read(stream);
            Assert.Equal(2, entries.Count());
        });

        [Fact]
        public void ReadEntryNamesCorrectly() => FileOpenRead(FileName, stream =>
        {
            var entries = Arc.Read(stream).ToArray();
            Assert.Equal("TBoxDtTe.itb", entries[0].Name);
            Assert.Equal("ColeDtTe.itc", entries[1].Name);
        });

        [Fact]
        public void ReadEntryLengthCorrectly() => FileOpenRead(FileName, stream =>
        {
            var entries = Arc.Read(stream).ToArray();
            Assert.Equal(0x3ec, entries[0].Data.Length);
            Assert.Equal(0xbc, entries[1].Data.Length);
        });

        [Fact]
        public void ReadEntryContentCorrectly() => FileOpenRead(FileName, stream =>
        {
            var entries = Arc.Read(stream).ToArray();
            Assert.Equal(0x42, entries[0].Data[2]);
            Assert.Equal(0x43, entries[1].Data[2]);
        });

        [Fact]
        public void WritesBackCorrectly() => FileOpenRead(FileName, stream =>
            Helpers.AssertStream(stream, x =>
            {
                var entries = Arc.Read(stream);

                var outStream = new MemoryStream();
                entries.Write(outStream);

                return outStream;
            }));
    }
}
