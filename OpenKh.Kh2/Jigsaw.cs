using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Jigsaw
    {
        [Data] public byte Picture { get; set; }
        [Data] public byte Part { get; set; }
        [Data] public ushort Text { get; set; } //z_un_002a2de8, binary addition 0x8000
        [Data] public byte World { get; set; }
        [Data] public byte Room { get; set; }
        [Data] public byte JigsawIdWorld { get; set; }
        [Data] public byte Unk07 { get; set; } //has also something to do with pos and orientation
        [Data] public ushort Unk08 { get; set; } //z_un_001d9d88, starting pos and orientation

        public static List<Jigsaw> Read(Stream stream) => BaseTable<Jigsaw>.Read(stream).Items;
        public static void Write(Stream stream, IEnumerable<Jigsaw> items) => new BaseTable<Jigsaw>
        {
            Id = 2,
            Items = items.ToList()
        }.Write(stream);
    }
}
