using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.TextureFooter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class TextureFooterDataTests
    {
        [Theory]
        [InlineData("kh2/res/dummy1.footer.bin", 2, 0)]
        [InlineData("kh2/res/dummy2.footer.bin", 0, 1)]
        public void ReadAndCheckSomeParams(string binFile, int numUvsc, int numTexa)
        {
            var footer = File.OpenRead(binFile).Using(TextureFooterData.Read);
            Assert.Equal(numUvsc, footer.UvscList.Count);
            Assert.Equal(numTexa, footer.TextureAnimationList.Count);
        }

        [Fact]
        public void CreateDummy()
        {
            {
                var footer = new TextureFooterData();
                var stream = new MemoryStream();
                footer.Write(stream);
            }
            {
                var footer = new TextureFooterData();
                footer.UvscList.Add(new UvScroll { });
                var stream = new MemoryStream();
                footer.Write(stream);
            }
            {
                var footer = new TextureFooterData();
                footer.TextureAnimationList.Add(new TextureAnimation { });
                var stream = new MemoryStream();
                footer.Write(stream);
            }
        }
    }
}
