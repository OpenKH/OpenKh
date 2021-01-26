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

        [Data] public byte Unk0 { get; set; }
        [Data] public byte Unk1 { get; set; }
        [Data] public ushort Ability { get; set; }
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

        public int AbilityLevel
        {
            get => Unk1 >> 4;
            set => Unk1 = (byte)((Unk1 & 0x0F) | (value << 4));
        }

        public int AntiRate
        {
            get => Unk1 & 0xF;
            set => Unk1 = (byte)((Unk1 & 0xF0) | (value & 0xF));
        }

        public FormVanilla VanillaForm
        {
            get => (FormVanilla)FormId;
            set => FormId = (int)value;
        }

        public FormFm FinalMixForm
        {
            get => (FormFm)FormId;
            set => FormId = (int)value;
        }

        public static List<Fmlv> Read(Stream stream) => BaseTable<Fmlv>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Fmlv> items) =>
            BaseTable<Fmlv>.Write(stream, 2, items);
    }
}
