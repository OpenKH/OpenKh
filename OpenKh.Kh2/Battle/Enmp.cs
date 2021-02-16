using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Enmp
    {
        [Data] public short Id { get; set; }
        [Data] public short Level { get; set; }
        [Data(Count = 32)] public short[] Health { get; set; }
        [Data] public short Unknown44 { get; set; }
        [Data] public short Unknown46 { get; set; }
        [Data] public short PhysicalWeakness { get; set; }
        [Data] public short FireWeakness { get; set; }
        [Data] public short IceWeakness { get; set; }
        [Data] public short ThunderWeakness { get; set; }
        [Data] public short DarkWeakness { get; set; }
        [Data] public short Unknown52 { get; set; }
        [Data] public short ReflectWeakness { get; set; }
        [Data] public short Unknown56 { get; set; }
        [Data] public short Unknown58 { get; set; }
        [Data] public short Unknown5a { get; set; }

        public static List<Enmp> Read(Stream stream) => BaseTable<Enmp>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Enmp> items) =>
            BaseTable<Enmp>.Write(stream, 2, items);
    }
}
