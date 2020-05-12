using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

                var offset = stream.ReadUInt16();
            }
        }

        public static Dictionary<string, List<Place>> Read(Stream stream) =>
            Bar.Read(stream).ToDictionary(x => x.Name, x => ReadPlaceEntry(x.Stream).ToList());

        public static void Write(Stream stream, Dictionary<string, List<Place>> entries)
        {

        }
    }
}
