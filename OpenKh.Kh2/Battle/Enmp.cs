using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Enmp
    {
        [Data] public ushort Id { get; set; }
        [Data] public ushort Level { get; set; }
        [Data(Count = 32)] public short[] Health { get; set; }
        [Data] public ushort MaxDamage { get; set; }
        [Data] public ushort MinDamage { get; set; }
        [Data] public ushort PhysicalWeakness { get; set; }
        [Data] public ushort FireWeakness { get; set; }
        [Data] public ushort IceWeakness { get; set; }
        [Data] public ushort ThunderWeakness { get; set; }
        [Data] public ushort DarkWeakness { get; set; }
        [Data] public ushort LightWeakness { get; set; }
        [Data] public ushort GeneralWeakness { get; set; }
        [Data] public ushort Experience { get; set; }
        [Data] public ushort Prize { get; set; }
        [Data] public ushort BonusLevel { get; set; }

        public static List<Enmp> Read(Stream stream) => BaseTable<Enmp>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Enmp> items) =>
            BaseTable<Enmp>.Write(stream, 2, items);
    }
}
