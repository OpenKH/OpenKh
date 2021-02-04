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
        [Data] public short MaxDamage { get; set; }
        [Data] public short MinDamage { get; set; }
        [Data] public short PhysicalWeakness { get; set; }
        [Data] public short FireWeakness { get; set; }
        [Data] public short IceWeakness { get; set; }
        [Data] public short ThunderWeakness { get; set; }
        [Data] public short DarkWeakness { get; set; }
        [Data] public short SpecialWeakness { get; set; }
        [Data] public short MaxWeakness { get; set; }
        [Data] public short Experience { get; set; }
        [Data] public short Prize { get; set; }
        [Data] public short BonusLevel { get; set; }

        public static List<Enmp> Read(Stream stream) => BaseTable<Enmp>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Enmp> items) =>
            BaseTable<Enmp>.Write(stream, 2, items);
    }
}
