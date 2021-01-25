using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Limt
    {
        [Data] public byte Id { get; set; }
        [Data] public byte Character { get; set; }
        [Data] public byte Summon { get; set; }
        [Data] public byte Group { get; set; }
        [Data(Count = 32)] public string FileName { get; set; }
        [Data] public uint SpawnId { get; set; }
        [Data] public ushort Command { get; set; }
        [Data] public ushort Limit { get; set; }
        [Data] public byte World { get; set; }
        [Data(Count = 19)] public byte[] Padding { get; set; }

        public static List<Limt> Read(Stream stream) => BaseTable<Limt>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Limt> items) =>
            BaseTable<Limt>.Write(stream, 2, items);
    }
}
