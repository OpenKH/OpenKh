using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Contextes;
using System;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class FontContextTests
    {
        [Fact]
        public void LoadEnglishSystemFontTest()
        {
            LoadFontTest(512, 256, 4, "sys", x => x.ImageSystem);
            LoadFontTest(512, 256, 4, "sys", x => x.ImageSystem2);
        }

        [Fact]
        public void LoadJapaneseSystemFontTest()
        {
            LoadFontTest(512, 512, 4, "sys", x => x.ImageSystem);
            LoadFontTest(512, 512, 4, "sys", x => x.ImageSystem2);
        }

        [Fact]
        public void LoadEnglishEventFontTest()
        {
            LoadFontTest(512, 512, 4, "evt", x => x.ImageEvent);
            LoadFontTest(512, 512, 4, "evt", x => x.ImageEvent2);
        }

        [Fact]
        public void LoadJapaneseEventFontTest()
        {
            LoadFontTest(512, 1024, 4, "evt", x => x.ImageEvent);
            LoadFontTest(512, 1024, 4, "evt", x => x.ImageEvent2);
        }

        [Fact]
        public void LoadIconFontTest() =>
            LoadFontTest(256, 160, 8, "icon", x => x.ImageIcon);

        private static void LoadFontTest(
            int expectedWidth,
            int expectedHeight,
            int bitsPerPixel,
            string name,
            Func<FontContext, IImage> getter)
        {
            var expectedLength = expectedWidth * expectedHeight * bitsPerPixel / 8;

            var fontContext = new FontContext();
            var entry = new Bar.Entry
            {
                Name = name,
                Type = Bar.EntryType.RawBitmap,
                Stream = CreateStream(expectedLength)
            };

            fontContext.Read(new Bar.Entry[] { entry });

            var image = getter(fontContext);
            Assert.NotNull(image);
            Assert.Equal(expectedWidth, image.Size.Width);
            Assert.Equal(expectedHeight, image.Size.Height);
            Assert.Equal(expectedLength, entry.Stream.Position);
        }

        private static Stream CreateStream(int length)
        {
            var stream = new MemoryStream(length);
            stream.Write(new byte[length]);
            stream.Position = 0;

            return stream;
        }
    }
}
