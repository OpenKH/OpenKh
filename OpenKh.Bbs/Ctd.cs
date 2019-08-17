using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class Ctd
    {
        private const int MagicCode = 0x44544340;

        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Version { get; set; }
            [Data] public short Unknown08 { get; set; }
            [Data] public short Unknown0a { get; set; }
            [Data] public short Entry2Count { get; set; }
            [Data] public short Entry1Count { get; set; }
            [Data] public int Entry1Offset { get; set; }
            [Data] public int Entry2Offset { get; set; }
            [Data] public int TextOffset { get; set; }
            [Data] public int Unknown1c { get; set; }
        }

        public class Entry
        {
            public short Unknown00 { get; set; }
            public short Unknown02 { get; set; }
            public int Unknown04 { get; set; }
            public int Unknown08 { get; set; }
        }

        public class Entry2
        {
            [Data] public ushort textX { get; set; }
            [Data] public ushort textY { get; set; }
            [Data] public ushort winW { get; set; }
            [Data] public ushort winH { get; set; }
            [Data] public byte formatType1 { get; set; }
            [Data] public byte dialogType { get; set; }
            [Data] public byte formatType2 { get; set; }
            [Data] public byte unk1 { get; set; }
            [Data] public ushort fontSize { get; set; }
            [Data] public ushort unk2 { get; set; }
            [Data] public ushort fontSeparation { get; set; } // NOT TESTED
            [Data] public ushort unk3 { get; set; }
            [Data] public ushort unk4 { get; set; }
            [Data] public ushort unk5 { get; set; }
            [Data] public ushort unk6 { get; set; }
            [Data] public ushort color { get; set; }
            [Data] public ushort unk7 { get; set; }
            [Data] public ushort unk8 { get; set; }
        }

        private readonly Header _header;

        public List<Entry> Entries1 { get; set; }
        public List<Entry2> Entries2 { get; set; }

        private Ctd(Stream stream)
        {
            _header = BinaryMapping.ReadObject<Header>(stream);

            stream.Position = _header.Entry1Offset;
            Entries1 = Enumerable.Range(0, _header.Entry1Count)
                .Select(x => BinaryMapping.ReadObject<Entry>(stream))
                .ToList();

            stream.Position = _header.Entry2Offset;
            Entries2 = Enumerable.Range(0, _header.Entry2Count)
                .Select(x => BinaryMapping.ReadObject<Entry2>(stream))
                .ToList();
        }

        public static Ctd Read(Stream stream) => new Ctd(stream);

        public static bool IsValid(Stream stream) =>
            new BinaryReader(stream).ReadInt32() == MagicCode;
    }
}
