using OpenKh.Kh2;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class IdxTests
    {
        [Theory]
        [InlineData("test", 0x338BCFAC)]
        [InlineData("hello world!", 0xFD8495B7)]
        [InlineData("00battle.bin", 0x2029C445)]
        public void CalculateHash32(string text, uint hash)
        {
            Assert.Equal(hash, Idx.GetHash32(text));
        }

        [Theory]
        [InlineData("test", 0xB82F)]
        [InlineData("hello world!", 0x0907)]
        public void CalculateHash16(string text, ushort hash)
        {
            Assert.Equal(hash, Idx.GetHash16(text));
        }
    }
}
