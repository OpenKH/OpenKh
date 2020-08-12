using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2.Battle
{
    public class Lvpm
    {
        public short HpMultiplier { get; set; } // (Hp * HpMultiplier + 99) / 100
        public short Unknown02 { get; set; }
        public short Unknown04 { get; set; }
        public short Unknown06 { get; set; }
        public short Attack { get; set; }
        public short Unknown0a { get; set; }

        public static List<Lvpm> Read(Stream stream) => BaseTable<Lvpm>.Read(stream).Items;

        public static void Write(Stream stream, IEnumerable<Lvpm> items) =>
            new BaseTable<Lvpm>
            {
                Id = 2,
                Items = items.ToList()
            }.Write(stream);
    }
}
