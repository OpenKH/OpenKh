using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2.Battle
{
    public class Lvpm
    {
        public short HpMultiplier { get; set; } // (Hp * HpMultiplier + 99) / 100
        public short Strength { get; set; }
        public short Defense { get; set; }
        public short MaxStrength { get; set; }
        public short MinStrength { get; set; }
        public short Experience { get; set; }

        public static List<Lvpm> Read(Stream stream) => BaseTable<Lvpm>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Lvpm> items) =>
            BaseTable<Lvpm>.Write(stream, 2, items);
    }
}
