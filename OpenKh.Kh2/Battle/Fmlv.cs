using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Fmlv
    {
        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
            [Data] public List<Level> Items { get; set; }
        }

        private readonly Header _header;

        public List<Level> Levels => _header.Items;
        public int Count => _header.Count;

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
            [Data] public byte LevelMovementAbility { get; set; }
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
                $"{FormFm} {FormLevel}: EXP {Exp}, Ability {Ability:X04} Lv. {LevelMovementAbility}";
        }

        public Fmlv(Stream stream)
        {
            BinaryMapping.SetMemberLengthMapping<Header>(nameof(Header.Items), (o, m) => o.Count);
            _header = BinaryMapping.ReadObject<Header>(stream);
        }

        public void Write(Stream stream)
        {
            BinaryMapping.WriteObject(stream, _header);
        }
    }
}
