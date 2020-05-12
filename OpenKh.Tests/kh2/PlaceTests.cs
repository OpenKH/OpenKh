using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class PlaceTests
    {
        private const string FileName = "kh2/res/00place.bin";

        [Fact]
        public void HasTheRightAmountOfWorlds() => Common.FileOpenRead(FileName, stream =>
        {
            Assert.Equal(20, Place.Read(stream).Count);
        });

        [Fact]
        public void HasTheRightWorldNames() => Common.FileOpenRead(FileName, stream =>
        {
            var places = Place.Read(stream);
            Assert.Contains(places, x => x.Key == "tmp");
            Assert.Contains(places, x => x.Key == "al");
            Assert.Contains(places, x => x.Key == "bb");
            Assert.Contains(places, x => x.Key == "wm");
            Assert.Contains(places, x => x.Key == "zz");
        });

        [Fact]
        public void HasWorldTheRightAmountOfPlaces() => Common.FileOpenRead(FileName, stream =>
        {
            Assert.Equal(16, Place.Read(stream)["bb"].Count);
        });

        [Fact]
        public void WriteBackTheSameFile() => Common.FileOpenRead(FileName, stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Place.Write(outStream, Place.Read(inStream));

                return outStream;
            });
        });
    }
}
