using OpenKh.Common;
using OpenKh.Imaging;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Imaging
{
    public class Tm2Tests
    {
        [Theory]
        [InlineData(new byte[] { 0x54, 0x49, 0x4d, 0x32 }, 16, true)]
        [InlineData(new byte[] { 0x54, 0x49, 0x4d, 0x31 }, 16, false)]
        [InlineData(new byte[] { 0x54, 0x49, 0x4d, 0x32 }, 15, false)]
        public void IsValidTest(byte[] header, int length, bool expected) => new MemoryStream()
            .Using(stream =>
            {
                stream.Write(header, 0, header.Length);
                stream.SetLength(length);

                Assert.Equal(expected, Tm2.IsValid(stream));
            });
    }
}
