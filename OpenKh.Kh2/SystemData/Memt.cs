using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public enum Member
    {
        Sora,
        Donald,
        Goofy,
        WorldCharacter,
        FormValor,
        FormWisdom,
        FormLimit,
        FormTrinity,
        FormFinal,
        Antiform,
        Mickey,
        SoraHighPoly,
        FormValorHighPoly,
        FormWisdomHighPoly,
        FormLimitHighPoly,
        FormTrinityHighPoly,
        FormFinalHighPoly,
        AntiformHighPoly,
    }

    public class Memt
    {
        private const int Version = 5;

        public class Entry
        {
            [Data] public short WorldId { get; set; }
            [Data] public short CheckStoryFlag { get; set; }
            [Data] public short CheckStoryFlagNegation { get; set; }
            [Data] public short Unk06 { get; set; }
            [Data] public short Unk08 { get; set; }
            [Data] public short Unk0A { get; set; }
            [Data] public short Unk0C { get; set; }
            [Data] public short Unk0E { get; set; }
            [Data(Count = 18)] public short[] Members { get; set; }
        }

        public class MemberIndices
        {
            [Data] public byte Player { get; set; }
            [Data] public byte Friend1 { get; set; }
            [Data] public byte Friend2 { get; set; }
            [Data] public byte FriendWorld { get; set; }
        }

        public List<Entry> Entries { get; }
        public MemberIndices[] MemberIndexCollection { get; }

        internal Memt(Stream stream)
        {
            Entries = BaseTable<Entry>.Read(stream);

            MemberIndexCollection = Enumerable.Range(0, 7)
                .Select(x => BinaryMapping.ReadObject<MemberIndices>(stream))
                .ToArray();
        }

        public static Memt Read(Stream stream) => new Memt(stream);

        public static void Write(Stream stream, Memt memt)
        {
            BaseTable<Entry>.Write(stream, Version, memt.Entries);
            foreach (var item in memt.MemberIndexCollection)
                BinaryMapping.WriteObject(stream, item);
        }
    }
}
