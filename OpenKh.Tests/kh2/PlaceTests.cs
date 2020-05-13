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
        public void HasTheRightAmountOfWorlds(string fileName) => Common.FileOpenRead(fileName, stream =>
        {
            Assert.Equal(19, Place.Read(stream).Count);
        });

        [Theory]
        [InlineData(PlaceFileName)]
        [InlineData(OldPlaceFileName)]
        public void HasTheRightWorldNames(string fileName) => Common.FileOpenRead(fileName, stream =>
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
        public void HasWorldTheRightAmountOfPlaces(string fileName) => Common.FileOpenRead(fileName, stream =>
        {
            Assert.Equal(16, Place.Read(stream)["bb"].Count);
        });

        [Fact]
        public void ReadSubPlaces() => Common.FileOpenRead(OldPlaceFileName, stream =>
        {
            var places = Place.Read(stream)["bb"];
            Assert.Equal(9, places[0].SubPlaces.Count);
            Assert.Equal(0x4783, places[0].SubPlaces[0]);
            Assert.Equal(0x8B83, places[0].SubPlaces[8]);

            Assert.Single(places[15].SubPlaces);
            Assert.Equal(0xB48B, places[15].SubPlaces[0]);
        });

        [Theory]
        [InlineData(PlaceFileName)]
        [InlineData(OldPlaceFileName)]
        public void WriteBackTheSameFile(string fileName) => Common.FileOpenRead(fileName, stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Place.Write(outStream, Place.Read(inStream));

                outStream.Position = 0;
                using var tmp = File.Create("D:/place.bin");
                outStream.CopyTo(tmp);

                return outStream;
            });
        });
    }
}
