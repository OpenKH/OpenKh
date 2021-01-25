using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2.Battle
{
    public class Lvpm
    {
        public ushort HpMultiplier { get; set; } // (Hp * HpMultiplier + 99) / 100
        public ushort Strength { get; set; }
        public ushort Defense { get; set; }
        public ushort MaxStrength { get; set; }
        public ushort MinStrength { get; set; }
        public ushort Experience { get; set; }

        public static List<Lvpm> Read(Stream stream) => BaseTable<Lvpm>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Lvpm> items) =>
            BaseTable<Lvpm>.Write(stream, 2, items);
    }
}
