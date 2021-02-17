using OpenKh.Common.Utils;
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
            get => BitsUtil.Int.GetBits(Unk0, 4, 4);
            set => Unk0 = (byte)BitsUtil.Int.SetBits(Unk0, 4, 4, value);
        }

        public int FormLevel
        {
            get => BitsUtil.Int.GetBits(Unk0, 0, 4);
            set => Unk0 = (byte)BitsUtil.Int.SetBits(Unk0, 0, 4, value);
        }

        public int AbilityLevel
        {
            get => BitsUtil.Int.GetBits(Unk1, 0, 4);
            set => Unk1 = (byte)BitsUtil.Int.SetBits(Unk1, 0, 4, value);
        }

        public int AntiRate
        {
            get => BitsUtil.Int.GetBits(Unk1, 4, 4);
            set => Unk1 = (byte)BitsUtil.Int.SetBits(Unk1, 4, 4, value);
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
