using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.System
{
    public class Trsr
    {
        public enum TrsrType : byte
        {
            Chest,
            Event
        }

        [Data] public ushort Id { get; set; }
        [Data] public ushort ItemId { get; set; }
        [Data] public TrsrType Type { get; set; }
        [Data] public byte World { get; set; }
        [Data] public byte Room { get; set; }
        [Data] public byte RoomChestIndex { get; set; }
        [Data] public short EventId { get; set; }
        [Data] public short OverallChestIndex { get; set; }

        public static List<Trsr> Read(Stream stream) => BaseShortTable<Trsr>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Trsr> items) =>
            BaseShortTable<Trsr>.Write(stream, 3, items);
    }
}
