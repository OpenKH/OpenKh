using OpenKh.Bbs;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class CtdTests : Common
    {
        private static readonly string FileName = "Bbs/res/ctdtest.ctd";

        [Fact]
        public void IsValidTest()
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x40);
                stream.WriteByte(0x43);
                stream.WriteByte(0x54);
                stream.WriteByte(0x44);
                stream.Position = 0;
                Assert.True(Ctd.IsValid(stream));
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
                Assert.False(Ctd.IsValid(stream));
            }
        }

        [Fact]
        public void ReadCorrectAmountOfEntry1() => FileOpenRead(FileName, stream =>
        {
            var ctd = Ctd.Read(stream);
            Assert.Equal(41, ctd.Entries1.Count);
        });

        [Fact]
        public void ReadCorrectAmountOfEntry2() => FileOpenRead(FileName, stream =>
        {
            var ctd = Ctd.Read(stream);
            Assert.Equal(14, ctd.Entries2.Count);
        });
    }
}
