using OpenKh.Bbs;
using OpenKh.Common;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class EventTableTests
    {
        private const string FilePath = "Bbs/res/event-table.bin";

        [Fact]
        public void ReadEntriesCountCorrectly() => File.OpenRead(FilePath).Using(stream =>
        {
            var events = Event.Read(stream);
            Assert.Equal(92, events.Count);
        });

        [Fact]
        public void ParseSingleEntryCorrectly() => File.OpenRead(FilePath).Using(stream =>
        {
            var events = Event.Read(stream);
            var @event = events[22];
            Assert.Equal(534, @event.Id);
            Assert.Equal(301, @event.EventIndex);
            Assert.Equal(4, @event.World);
            Assert.Equal(7, @event.Room);
            Assert.Equal(61, @event.Unknown06);
        });

        [Fact]
        public void WritesBackCorrectly() => File.OpenRead(FilePath).Using(stream =>
            Helpers.AssertStream(stream, x =>
            {
                var events = Event.Read(stream);

                var outStream = new MemoryStream();
                Event.Write(outStream, events);

                return outStream;
            }));
    }
}
