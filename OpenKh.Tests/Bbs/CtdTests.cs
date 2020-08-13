using OpenKh.Bbs;
using OpenKh.Common;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class CtdTests
    {
        private static readonly string FileName = "Bbs/res/ctdtest.ctd";

        [Fact]
        public void IsValidTest()
        {
            using var stream = new MemoryStream();
            stream.WriteByte(0x40);
            stream.WriteByte(0x43);
            stream.WriteByte(0x54);
            stream.WriteByte(0x44);
            stream.Position = 0;
            Assert.True(Ctd.IsValid(stream));
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
                Assert.False(Ctd.IsValid(stream));
            }
        }

        [Fact]
        public void ReadCorrectAmountOfEntry1() => File.OpenRead(FileName).Using(stream =>
        {
            var ctd = Ctd.Read(stream);
            Assert.Equal(41, ctd.Messages.Count);
        });

        [Fact]
        public void ReadCorrectAmountOfEntry2() => File.OpenRead(FileName).Using(stream =>
        {
            var ctd = Ctd.Read(stream);
            Assert.Equal(14, ctd.Layouts.Count);
        });

        [Theory]
        [InlineData(0, "Command Deck")]
        [InlineData(1, "Action Commands")]
        [InlineData(12345678, null)]
        public void ReadStringCorrectly(int id, string expected) => File.OpenRead(FileName).Using(stream =>
        {
            var ctd = Ctd.Read(stream);
            var str = ctd.GetString(id);

            Assert.Equal(expected, str);
        });

        [Fact]
        public void WritesBackCorrectly() => File.OpenRead(FileName).Using(stream =>
            Helpers.AssertStream(stream, x =>
            {
                var ctd = Ctd.Read(stream);

                var outStream = new MemoryStream();
                ctd.Write(outStream);

                return outStream;
            }));

        [Fact]
        public void CreateEmptyCtdWithoutNullValues()
        {
            var ctd = new Ctd();
            Assert.NotNull(ctd.Messages);
            Assert.NotNull(ctd.Layouts);
            Assert.Empty(ctd.Messages);
            Assert.Empty(ctd.Layouts);
            Assert.Equal(0, ctd.Unknown);
        }
    }
}
