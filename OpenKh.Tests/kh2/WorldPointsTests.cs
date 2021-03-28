using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class WorldPointsTests
    {
        private const string FileName = "kh2/res/00worldpoint.bin";

        [Fact]
        public void ReadTest()
        {
            var items = File.OpenRead(FileName).Using(WorldPoint.Read);
            Assert.Equal(3, items.Count);
            Assert.Equal(1, items[0].World);
            Assert.Equal(1, items[0].Area);
            Assert.Equal(99, items[0].Entrance);
            Assert.Equal(1, items[1].World);
            Assert.Equal(2, items[1].Area);
            Assert.Equal(99, items[1].Entrance);
        }

        [Fact]
        public void WriteTest() => File.OpenRead(FileName).Using(stream =>
        {
            Helpers.AssertStream(stream, x =>
            {
                var outStream = new MemoryStream();
                WorldPoint.Write(outStream, WorldPoint.Read(x));

                return outStream;
            });
        });
    }
}
