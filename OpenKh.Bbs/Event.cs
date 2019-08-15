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
            [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
            [Data] public List<Event> Items { get; set; }

            static Header()
            {
                BinaryMapping.SetMemberLengthMapping<Header>(nameof(Items), (o, m) => o.Count);
            }
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

        public static List<Event> Read(Stream stream) =>
            BinaryMapping.ReadObject<Header>(stream).Items;

        public static void Write(Stream stream, IEnumerable<Event> events) =>
            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Items = events.ToList()
            });
    }
}
