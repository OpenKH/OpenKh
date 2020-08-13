using OpenKh.Common;
using OpenKh.Bbs;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class ArcTests
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
        public void IsNotValidWhenHeaderDoesNotMatchTest()
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
        public void IsNotValidWhenStreamIsNotLongEnoughTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(1);
                stream.WriteByte(2);
                stream.WriteByte(3);
                Assert.False(Arc.IsValid(stream));
            }
        }

        [Fact]
        public void ReadCorrectAmountOfEntries() => File.OpenRead(FileName).Using(stream =>
        {
            var entries = Arc.Read(stream);
            Assert.Equal(3, entries.Count());
        });

        [Fact]
        public void ReadEntryNamesCorrectly() => File.OpenRead(FileName).Using(stream =>
        {
            var entries = Arc.Read(stream).ToArray();
            Assert.Equal("TBoxDtTe.itb", entries[0].Name);
            Assert.Equal("ColeDtTe.itc", entries[1].Name);
            Assert.Equal("FileNameTest", entries[2].Name);
        });

        [Fact]
        public void ReadEntryFullPathCorrectly() => File.OpenRead(FileName).Using(stream =>
        {
            var entries = Arc.Read(stream).ToArray();
            Assert.Equal("TBoxDtTe.itb", entries[0].Path);
            Assert.Equal("ColeDtTe.itc", entries[1].Path);
            Assert.Equal("arc/effect/FileNameTest", entries[2].Path);
        });

        [Fact]
        public void ReadEntryLengthCorrectly() => File.OpenRead(FileName).Using(stream =>
        {
            var entries = Arc.Read(stream).ToArray();
            Assert.Equal(0x3ec, entries[0].Data.Length);
            Assert.Equal(0xbc, entries[1].Data.Length);
        });

        [Fact]
        public void ReadEntryContentCorrectly() => File.OpenRead(FileName).Using(stream =>
        {
            var entries = Arc.Read(stream).ToArray();
            Assert.Equal(0x42, entries[0].Data[2]);
            Assert.Equal(0x43, entries[1].Data[2]);
        });

        [Fact]
        public void IsPointerFieldShouldBeCorrectlyPopulated() => File.OpenRead(FileName).Using(stream =>
        {
            var entries = Arc.Read(stream).ToArray();
            Assert.False(entries[0].IsLink);
            Assert.False(entries[1].IsLink);
            Assert.True(entries[2].IsLink);
        });

        [Fact]
        public void PointersShouldHaveNullData() => File.OpenRead(FileName).Using(stream =>
        {
            var entries = Arc.Read(stream).ToArray();
            Assert.Null(entries[2].Data);
        });

        [Fact]
        public void WritesBackCorrectly() => File.OpenRead(FileName).Using(stream =>
            Helpers.AssertStream(stream, x =>
            {
                var entries = Arc.Read(stream);

                var outStream = new MemoryStream();
                entries.Write(outStream);

                return outStream;
            }));
    }
}
