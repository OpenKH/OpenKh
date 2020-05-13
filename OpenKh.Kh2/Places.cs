using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2
{
    public class Place
    {
        public ushort MessageId { get; set; }

        public List<ushort> SubPlaces { get; set; }

        private static IEnumerable<ushort> ReadPlaceSubEntries(Stream stream)
        {
            while (true)
            {
                var ch = stream.ReadByte();
                if (ch <= 0)
                    yield break;

                yield return (ushort)(ch | (stream.ReadByte() << 8));
            }
        }

        private static IEnumerable<Place> ReadPlaceEntry(Stream stream)
        {
            if (stream.Length < 4)
                yield break;

            var placeCount = stream.SetPosition(2).ReadUInt16() / 4;
            stream.SetPosition(0);

            var placeIds = new List<ushort>(placeCount);
            var offsetIds = new List<int>(placeCount);
            for (var i = 0; i < placeCount; i++)
            {
                placeIds.Add(stream.ReadUInt16());
                offsetIds.Add(stream.ReadUInt16() + i * 4);
            }

            for (var i = 0; i < placeCount - 1; i++)
            {
                var hasSubPlaces = offsetIds[i] != offsetIds[i + 1];
                var subPlaces = hasSubPlaces ?
                    ReadPlaceSubEntries(stream.SetPosition(offsetIds[i])).ToList() :
                    new List<ushort>();

                yield return new Place
                {
                    MessageId = placeIds[i],
                    SubPlaces = subPlaces
                };
            }

            yield return new Place
            {
                MessageId = placeIds[placeCount - 1],
                SubPlaces = ReadPlaceSubEntries(stream.SetPosition(offsetIds[placeCount - 1])).ToList()
            };
        }

        private static void Write(Stream stream, List<Place> places)
        {
            var endOfFile = places.Count * 4;
            foreach (var place in places)
            {
                stream.Write(place.MessageId);
                stream.Write((ushort)endOfFile);

                endOfFile -= 4;
                if (place.SubPlaces.Count > 0)
                    endOfFile += place.SubPlaces.Count * 2 + 1;
            }

            foreach (var place in places)
            {
                // If no subplaces are found, then it's a place.bin and not a 00place.bin
                if (place.SubPlaces.Count > 0)
                {
                    foreach (var subplace in place.SubPlaces)
                        stream.Write(subplace);
                    stream.Write((byte)0);
                }
            }
        }

        public static Dictionary<string, List<Place>> Read(Stream stream) =>
            Bar.Read(stream)
                .Where(x => x.Type == Bar.EntryType.Binary)
                .ToDictionary(x => x.Name, x => ReadPlaceEntry(x.Stream).ToList());

        public static void Write(Stream stream, Dictionary<string, List<Place>> entries)
        {
            var barEntries = new List<Bar.Entry>
            {
                new Bar.Entry
                {
                    Name = "tmp",
                    Stream = new MemoryStream()
                }
            };

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
