using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class Sklt
    {
        [Data] public uint CharacterId { get; set; }
        [Data] public short Bone1 { get; set; }
        [Data] public short Bone2 { get; set; }

        public static List<Sklt> Read(Stream stream) => BaseTable<Sklt>.Read(stream);
        public static void Write(Stream stream, IEnumerable<Sklt> entries) =>
            BaseTable<Sklt>.Write(stream, 2, entries);
    }
}
