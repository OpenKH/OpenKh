using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public enum MemberVanilla
    {
        Sora,
        Donald,
        Goofy,
        WorldCharacter,
        FormValor,
        FormWisdom,
        FormTrinity,
        FormFinal,
        Antiform,
        Mickey,
        SoraHighPoly,
        FormValorHighPoly,
        FormWisdomHighPoly,
        FormTrinityHighPoly,
        FormFinalHighPoly,
        AntiformHighPoly,
    }

    public enum MemberFinalMix
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
        public const int MemberCountVanilla = 16;
        public const int MemberCountFinalMix = 18;

        public interface IEntry
        {
            short CheckStoryFlag { get; set; }
            short CheckStoryFlagNegation { get; set; }
            short[] Members { get; set; }
            byte CheckArea { get; set; }
            byte Padding { get; set; }
            int PlayerSize { get; set; }
            int FriendSize { get; set; }
            short WorldId { get; set; }
        }

        public class EntryVanilla : IEntry
        {
            [Data] public short WorldId { get; set; }
            [Data] public short CheckStoryFlag { get; set; }
            [Data] public short CheckStoryFlagNegation { get; set; }
            [Data] public byte CheckArea { get; set; }
            [Data] public byte Padding { get; set; }
            [Data] public int PlayerSize { get; set; }
            [Data] public int FriendSize { get; set; }
            [Data(Count = MemberCountVanilla)] public short[] Members { get; set; }
        }

        public class EntryFinalMix : IEntry
        {
            [Data] public short WorldId { get; set; }
            [Data] public short CheckStoryFlag { get; set; }
            [Data] public short CheckStoryFlagNegation { get; set; }
            [Data] public byte CheckArea { get; set; }
            [Data] public byte Padding { get; set; }
            [Data] public int PlayerSize { get; set; }
            [Data] public int FriendSize { get; set; }

            [Data(Count = MemberCountFinalMix)] public short[] Members { get; set; }
        }

        public class MemberIndices
        {
            [Data] public byte Player { get; set; }
            [Data] public byte Friend1 { get; set; }
            [Data] public byte Friend2 { get; set; }
            [Data] public byte FriendWorld { get; set; }
        }

        public List<IEntry> Entries { get; }
        public MemberIndices[] MemberIndexCollection { get; }

        public class MemtEntryPatch
        {
            public int Index { get; set; }
            public short WorldId { get; set; }
            public short CheckStoryFlag { get; set; }
            public string? FlagForWorld { get; set; }
            public short CheckStoryFlagNegation { get; set; }
            public string? NegationFlagForWorld { get; set; }
            public byte CheckArea { get; set; }
            public byte Padding { get; set; }
            public int PlayerSize { get; set; }
            public int FriendSize{ get; set; }
            public List<short> Members { get; set; }
        }


        public class MemberIndicesPatch
        {
            public int Index { get; set; }
            public byte Player { get; set; }
            public byte Friend1 { get; set; }
            public byte Friend2 { get; set; }
            public byte FriendWorld { get; set; }
        }

        public class MemtPatches
        {
            public List<MemtEntryPatch> MemtEntries { get; set; }
            public List<MemberIndicesPatch> MemberIndices { get; set; }
        }

        public Memt()
        {
            Entries = new List<IEntry>();
            MemberIndexCollection = new MemberIndices[7];
            for (int i = 0; i < MemberIndexCollection.Length; i++)
            {
                MemberIndexCollection[i] = new MemberIndices
                {
                    Player = 0,
                    Friend1 = 0,
                    Friend2 = 0,
                    FriendWorld = 0
                };
            }
        }

        internal Memt(Stream stream)
        {
            const int HeaderSize = 8;
            const int StructLengthVanilla = 48;
            const int MemberIndicesExpected = 7;

            var previousPosition = stream.Position;
            var version = stream.ReadInt32();
            var count = stream.ReadInt32();
            stream.Position = previousPosition;

            if (stream.Length - previousPosition == HeaderSize +
                count * StructLengthVanilla + MemberIndicesExpected * 4)
                Entries = BaseTable<EntryVanilla>.Read(stream).Cast<IEntry>().ToList();
            else
                Entries = BaseTable<EntryFinalMix>.Read(stream).Cast<IEntry>().ToList();

            MemberIndexCollection = Enumerable.Range(0, MemberIndicesExpected)
                .Select(x => BinaryMapping.ReadObject<MemberIndices>(stream))
                .ToArray();
        }

        public static Memt Read(Stream stream) => new Memt(stream);

        public static void Write(Stream stream, Memt memt)
        {
            var firstElement = memt.Entries.FirstOrDefault();
            if (firstElement is EntryVanilla)
                BaseTable<EntryVanilla>.Write(stream, Version, memt.Entries.Cast<EntryVanilla>());
            else if (firstElement is EntryFinalMix)
                BaseTable<EntryFinalMix>.Write(stream, Version, memt.Entries.Cast<EntryFinalMix>());

            foreach (var item in memt.MemberIndexCollection)
                BinaryMapping.WriteObject(stream, item);
        }
    }
}
