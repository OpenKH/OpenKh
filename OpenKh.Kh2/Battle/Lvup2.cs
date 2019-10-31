using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Lvup2
    {
        public class Character
        {
            [Data] public int Unknown00 { get; set; }
            [Data(Count = 99)] public List<Level> Levels { get; set; }

            public class Level
            {
                [Data] public int Exp { get; set; }
                [Data] public byte Strength { get; set; }
                [Data] public byte Magic { get; set; }
                [Data] public byte Defense{ get; set; }
                [Data] public byte Ap { get; set; }
                [Data] public short SwordAbility { get; set; }
                [Data] public short ShieldAbility { get; set; }
                [Data] public short StaffAbility { get; set; }
                [Data] public short Padding { get; set; }
            }
        }

        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Count { get => Characters.TryGetCount(); set => Characters = Characters.CreateOrResize(value); }
            [Data(Count = 0x38)] public byte[] Unknown08 { get; set; }
            [Data(Count = 13)] public List<Character> Characters { get; set; }
        }


        static Lvup2()
        {
            //BinaryMapping.SetMemberLengthMapping<Header>(nameof(Header.Characters), (o, m) => o.Count);
        }

        public static List<Character> Read(Stream stream) =>
            BinaryMapping.ReadObject<Header>(stream).Characters;

        //public static void Write(Stream stream, List<Character> entries) =>
        //    BinaryMapping.WriteObject(stream, new Header
        //    {
        //        MagicCode = 1,
        //        Entries = entries
        //    });
    }
}
