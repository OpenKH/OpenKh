using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class Event
    {
        private const int MagicCode = 1;

        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Count { get; set; }
        }

        [Data] public ushort Id { get; set; }
        [Data] public ushort EventIndex { get; set; }
        [Data] public byte World { get; set; }
        [Data] public byte Room { get; set; }
        [Data] public ushort Unknown06 { get; set; }

        public static bool IsValid(Stream stream)
        {
            var prevPosition = stream.Position;
            var magicCode = new BinaryReader(stream).ReadInt32();
            stream.Position = prevPosition;

            return magicCode == MagicCode;
        }

        public static List<Event> Read(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream);
            return Enumerable.Range(0, header.Count)
                .Select(_ => BinaryMapping.ReadObject<Event>(stream))
                .ToList();
        }

        public static void Write(Stream stream, IEnumerable<Event> events)
        {
            var list = events.ToList();
            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Count = list.Count
            });
            foreach (var item in list)
                BinaryMapping.WriteObject(stream, item);
        }
    }
}
