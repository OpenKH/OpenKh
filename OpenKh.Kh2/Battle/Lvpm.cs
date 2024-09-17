using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Lvpm
    {
        [Data] public ushort HpMultiplier { get; set; } // (Hp * HpMultiplier + 99) / 100
        [Data] public ushort Strength { get; set; }
        [Data] public ushort Defense { get; set; }
        [Data] public ushort MaxStrength { get; set; }
        [Data] public ushort MinStrength { get; set; }
        [Data] public ushort Experience { get; set; }

        //Default
        public static List<Lvpm> Read(Stream stream) => BaseList<Lvpm>.Read(stream, 99);

        //Override for having a custom amount of entries
        public static List<Lvpm> Read(Stream stream, int count) => BaseList<Lvpm>.Read(stream, count);

        public static void Write(Stream stream, IEnumerable<Lvpm> items) => BaseList<Lvpm>.Write(stream, items);

        public Lvpm() { }

        public Lvpm(ushort HP, ushort Str, ushort Def, ushort MaxStr, ushort MinStr, ushort Exp)
        {
            HpMultiplier = HP;
            Strength = Str;
            Defense = Def;
            MaxStrength = MaxStr;
            MinStrength = MinStr;
            Experience = Exp;
        }
    }
}
