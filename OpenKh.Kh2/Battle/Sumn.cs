using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Sumn
    {
        [Data] public ushort Command { get; set; }
        [Data] public ushort Item { get; set; }
        [Data] public uint Entity1 { get; set; }
        [Data] public uint Entity2 { get; set; }
        [Data] public ushort LimitCommand { get; set; }
        [Data(Count = 50)] public byte[] Padding { get; set; }

        public static List<Sumn> Read(Stream stream) => BaseTable<Sumn>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Sumn> items) =>
            BaseTable<Sumn>.Write(stream, 2, items);
    }
}
