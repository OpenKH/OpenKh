using OpenKh.Kh1;
using Xunit;

namespace OpenKh.Tests.Kh1
{
    public class Idx1Tests
    {
        [Theory]
        [InlineData("dc01.ard", 0x0007C3B4)]
        [InlineData("title/copyright.tm2", 0x57D148F1)]
        [InlineData("OpenKH is awesome!", 0xF2CBF18B)]
        public void CalculateHash(string text, uint hash)
        {
            Assert.Equal(hash, Idx1.GetHash(text));
        }
    }
}
