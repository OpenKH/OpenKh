using OpenKh.Common;
using OpenKh.Recom;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Recom
{
    public class RtmTests
    {
        private const string FileName = "Recom/res/texture.rtm";

        [Fact]
        public void ReadTest()
        {
            var rtm = File.OpenRead(FileName).Using(Rtm.Read).ToList();

            Assert.Single(rtm);
            Assert.Equal("openkh_tex.tm2", rtm[0].Name);
            Assert.NotNull(rtm[0].Textures);
        }

        [Fact]
        public void WritesBackCorrectly() => File.OpenRead(FileName).Using(stream =>
            Helpers.AssertStream(stream, x =>
            {
                var outStream = new MemoryStream();
                Rtm.Write(outStream, Rtm.Read(stream));

                return outStream;
            }));
    }
}
