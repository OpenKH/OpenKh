using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class PlaceTests
    {
        private const string PlaceFileName = "kh2/res/place.bin";
        private const string OldPlaceFileName = "kh2/res/00place.bin";

        [Theory]
        [InlineData(PlaceFileName)]
        [InlineData(OldPlaceFileName)]
        public void HasTheRightAmountOfWorlds(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            Assert.Equal(19, Place.Read(stream).Count);
        });

        [Theory]
        [InlineData(PlaceFileName)]
        [InlineData(OldPlaceFileName)]
        public void HasTheRightWorldNames(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            var places = Place.Read(stream);
            Assert.Contains(places, x => x.Key == "al");
            Assert.Contains(places, x => x.Key == "bb");
            Assert.Contains(places, x => x.Key == "wm");
            Assert.Contains(places, x => x.Key == "zz");
        });

        [Theory]
        [InlineData(PlaceFileName)]
        [InlineData(OldPlaceFileName)]
        public void HasWorldTheRightAmountOfPlaces(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            Assert.Equal(16, Place.Read(stream)["bb"].Count);
        });

        [Fact]
        public void ReadPlaceName() => File.OpenRead(OldPlaceFileName).Using(stream =>
        {
            var places = Place.Read(stream)["bb"];
            Assert.Equal(18, places[0].Name.Length);
            Assert.Equal(0x83, places[0].Name[0]);
            Assert.Equal(0x8B, places[0].Name[17]);
        });

        [Theory]
        [InlineData(PlaceFileName)]
        [InlineData(OldPlaceFileName)]
        public void WriteBackTheSameFile(string fileName) => File.OpenRead(fileName).Using(stream =>
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
