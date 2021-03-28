using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2
{
    public class Place
    {
        /// <summary>
        /// Refers to sys.bar
        /// </summary>
        public ushort MessageId { get; set; }

        /// <summary>
        /// Place name to be decoded as Shift-JIS.
        /// </summary>
        public byte[] Name { get; set; }

        private static IEnumerable<byte> ReadPlaceSubEntries(Stream stream)
        {
            while (true)
            {
                var ch = stream.ReadByte();
                if (ch <= 0)
                    yield break;

                yield return (byte)ch;
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
                var hasName = offsetIds[i] != offsetIds[i + 1];
                var name = hasName ?
                    ReadPlaceSubEntries(stream.SetPosition(offsetIds[i])).ToArray() :
                    new byte[0];

                yield return new Place
                {
                    MessageId = placeIds[i],
                    Name = name
                };
            }

            yield return new Place
            {
                MessageId = placeIds[placeCount - 1],
                Name = ReadPlaceSubEntries(stream.SetPosition(offsetIds[placeCount - 1])).ToArray()
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
                if (place.Name.Length > 0)
                    endOfFile += place.Name.Length + 1;
            }

            foreach (var place in places)
            {
                if (place.Name.Length > 0)
                {
                    stream.Write(place.Name);
                    stream.Write((byte)0);
                }
            }
        }

        public static Dictionary<string, List<Place>> Read(Stream stream) =>
            Bar.Read(stream)
                .Where(x => x.Type == Bar.EntryType.List)
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
                    Type = Bar.EntryType.List
                });
            };

            Bar.Write(stream, barEntries);

            foreach (var barEntry in barEntries)
                barEntry.Stream.Dispose();
        }
    }
}
