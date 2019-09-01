using OpenKh.Bbs;
using OpenKh.Common;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class FontTests
    {
        private static string InfFileName = "Bbs/res/font-test.inf";
        private static string Inf2FileName = "Bbs/res/font-test2.inf";

        [Fact]
        public void ReadFontInfo() => File.OpenRead(InfFileName).Using(stream =>
        {
            var fontInfo = FontInfo.Read(stream);

            Assert.Equal(364, fontInfo.CharacterCount);
            Assert.Equal(512, fontInfo.ImageWidth);
            Assert.Equal(64, fontInfo.MaxImageHeight);
            Assert.Equal(12, fontInfo.CharacterWidth);
            Assert.Equal(12, fontInfo.CharacterHeight);
        });

        [Fact]
        public void WriteBackFontInfo() => Helpers.AssertStream(File.OpenRead(InfFileName), stream =>
        {
            var outStream = new MemoryStream();
            FontInfo.Read(stream).Write(outStream);
            return outStream;
        });

        [Fact]
        public void ReadFontIconInfo() => File.OpenRead(Inf2FileName).Using(stream =>
        {
            var iconsInfo = FontIconInfo.Read(stream).ToArray();

            Assert.Equal(21, iconsInfo.Length);

            var iconInfo = iconsInfo.Skip(1).First();
            Assert.Equal(61870, iconInfo.Key);
            Assert.Equal(19, iconInfo.Left);
            Assert.Equal(1, iconInfo.Top);
            Assert.Equal(39, iconInfo.Right);
            Assert.Equal(20, iconInfo.Bottom);
        });

        [Fact]
        public void WriteBackFontIconInfo() => Helpers.AssertStream(File.OpenRead(Inf2FileName), stream =>
        {
            var outStream = new MemoryStream();
            FontIconInfo.Write(outStream, FontIconInfo.Read(stream));
            return outStream;
        });
    }
}
