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
    }
}
