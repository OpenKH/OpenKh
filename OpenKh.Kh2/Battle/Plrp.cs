using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Plrp
    {
        [Data] public ushort Id { get; set; }
        [Data] public byte Character { get; set; } 
        [Data] public byte Hp { get; set; }
        [Data] public byte Mp { get; set; }
        [Data] public byte Ap { get; set; }
        [Data] public byte Strength { get; set; }
        [Data] public byte Magic { get; set; }
        [Data] public byte Defense { get; set; }
        [Data] public byte ArmorSlotMax { get; set; }
        [Data] public byte AccessorySlotMax { get; set; }
        [Data] public byte ItemSlotMax { get; set; }
        [Data(Count = 32)] public List<ushort> Items { get; set; }
        [Data(Count = 52)] public byte[] Padding { get; set; }

        public static List<Plrp> Read(Stream stream) => BaseTable<Plrp>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Plrp> items) =>
            BaseTable<Plrp>.Write(stream, 1, items);
    }
}
