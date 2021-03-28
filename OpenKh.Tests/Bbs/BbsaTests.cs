using OpenKh.Bbs;
using System.IO;
using OpenKh.Common;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class BbsaTests
    {
        [Theory]
        [InlineData("COMMON", 0x1514B7D7)]
        [InlineData("FONT", 0xE601E566)]
        [InlineData("DI02EX", 1334999096)]
        [InlineData("ARC/PRESET", 1037329268)]
        [InlineData("MOVIE", 3926536415)]
        [InlineData("OPD.PMF", 2028670912)]
        public void ShouldCalculateTheCorrectHash(string text, uint expected)
        {
            Assert.Equal(expected, Bbsa.GetHash(text));
        }

        [Fact]
        public void TestLba()
        {
            var a = 859218828;
            var offset = a >> 12;
            var size = a & 0xFFF;

            Assert.Equal(0x333CE - 100, offset);
            Assert.Equal(0x38C, size);
        }

        [Theory]
        [InlineData(0x0050414D, "arc/map")]
        [InlineData(0x4E455645, "arc/event")]
        [InlineData(0x30004350, "arc/pc_terra")]
        [InlineData(0x00000000, "arc_")]
        [InlineData(0x80000000, "sound/bgm")]
        [InlineData(0xc0000000, "lua")]
        [InlineData(0x90000000, "sound/se/common")]
        [InlineData(0x91000000, "sound/se/event/ex")]
        [InlineData(0x91010000, "sound/se/event/dp")]
        [InlineData(0x920a0000, "sound/se/footstep/di")]
        [InlineData(0xd0000000, "message/jp/system")]
        [InlineData(0xd1200000, "message/en/map")]
        [InlineData(0xa1570000, "sound/voice/fr/event/jf")]
        public void CalculateDirectoryFromHash(uint hash, string directory)
        {
            var actual = Bbsa.GetDirectoryName(hash);

            Assert.Equal(directory, actual);
        }

        [Theory]
        [InlineData(0x0050414D, "arc/map")]
        [InlineData(0x4E455645, "arc/event")]
        [InlineData(0x30004350, "arc/pc_terra")]
        [InlineData(0x00000000, "arc_")]
        [InlineData(0x80000000, "sound/bgm")]
        [InlineData(0xc0000000, "lua")]
        [InlineData(0x90000000, "sound/se/common")]
        [InlineData(0x91000000, "sound/se/event/ex")]
        [InlineData(0x91010000, "sound/se/event/dp")]
        [InlineData(0x920a0000, "sound/se/footstep/di")]
        [InlineData(0xd0000000, "message/jp/system")]
        [InlineData(0xd1200000, "message/en/map")]
        [InlineData(0xa1570000, "sound/voice/fr/event/jf")]
        public void CalculateHashFromDirectory(uint hash, string directory)
        {
            var actual = Bbsa.GetDirectoryHash(directory);

            Assert.Equal(hash, actual);
        }
    }
}
