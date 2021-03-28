using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2.Ard;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Imaging
{
    public class PngTests
    {
        [Theory]
        [InlineData("4")]
        [InlineData("8")]
        [InlineData("24")]
        [InlineData("32")]
        public void ReadingTests(string prefix)
        {
            File.OpenRead($"Imaging/res/png/{prefix}.png").Using(PngImage.Read);
        }
    }
}
