using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Fmlv
    {
        public enum FormVanilla
        {
            Summon,
            Valor,
            Wisdom,
            Master,
            Final,
            AntiForm
        }
        public enum FormFm
        {
            Summon,
            Valor,
            Wisdom,
            Limit,
            Master,
            Final,
            AntiForm
        }

        public class Level
        {
            [Data] public byte Unk0 { get; set; }
            [Data] public byte LevelGrowthAbility { get; set; }
            [Data] public short Ability { get; set; }
            [Data] public int Exp { get; set; }

            public int FormId
            {
                get => Unk0 >> 4;
                set => Unk0 = (byte)((Unk0 & 0x0F) | (value << 4));
            }

            public int FormLevel
            {
                get => Unk0 & 0xF;
                set => Unk0 = (byte)((Unk0 & 0xF0) | (value & 0xF));
            }

            public FormVanilla FormVanilla
            {
                get => (FormVanilla)FormId;
                set => FormId = (int)value;
            }

            public FormFm FormFm
            {
                get => (FormFm)FormId;
                set => FormId = (int)value;
            }

            public override string ToString() =>
                $"{FormFm} {FormLevel}: EXP {Exp}, Ability {Ability:X04} Lv. {LevelGrowthAbility}";
        }

        public static List<Level> Read(Stream stream) => BaseTable<Level>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Level> items) =>
            BaseTable<Level>.Write(stream, 2, items);
    }
}
