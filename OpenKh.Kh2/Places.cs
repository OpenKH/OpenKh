using OpenKh.Common;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenKh.Kh2
{
    public class Place
    {
        public ushort MessageId { get; set; }

        private static IEnumerable<Place> ReadPlaceEntry(Stream stream)
        {
            if (stream.Length < 4)
                yield break;

            var messageId = stream.ReadUInt16();
            var startOffset = stream.ReadUInt16();
            yield return new Place
            {
                MessageId = messageId
            };

            while (stream.Position < startOffset)
            {
                yield return new Place
                {
                    MessageId = stream.ReadUInt16()
                };

                var offset = stream.ReadUInt16(); // skip
            }
        }

        private static void Write(Stream stream, List<Place> places)
        {
            var endOfFile = (ushort)(places.Count * 4);
            foreach (var place in places)
            {
                stream.Write(place.MessageId);
                stream.Write(endOfFile);
                endOfFile -= 4;
            }
        }

        public static Dictionary<string, List<Place>> Read(Stream stream) =>
            Bar.Read(stream)
                .Where(x => x.Type == Bar.EntryType.Binary)
                .ToDictionary(x => x.Name, x => ReadPlaceEntry(x.Stream).ToList());

        public static void Write(Stream stream, Dictionary<string, List<Place>> entries)
        {
            var barEntries = new List<Bar.Entry>();
            barEntries.Add(new Bar.Entry
            {
                Name = "tmp",
                Stream = new MemoryStream()
            });

            foreach (var placeGroup in entries)
            {
                var memoryStream = new MemoryStream();
                Write(memoryStream, placeGroup.Value);

                barEntries.Add(new Bar.Entry
                {
                    Name = placeGroup.Key,
                    Stream = memoryStream,
                    Type = Bar.EntryType.Binary
                });
            };

            Bar.Write(stream, barEntries);

            foreach (var barEntry in barEntries)
                barEntry.Stream.Dispose();
        }
    }
}
