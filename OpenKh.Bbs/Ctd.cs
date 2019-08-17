using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class Ctd
    {
        private const int MagicCode = 0x44544340;
        private const int Version = 1;
        private const int HeaderLength = 0x20;
        private const int Entry1Length = 0xC;
        private const int Entry2Length = 0x20;

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
            [Data] public short Id { get; set; }
            [Data] public short Unknown02 { get; set; }
            [Data] public int Offset { get; set; }
            [Data] public int Entry2Index { get; set; }

            public byte[] Data { get; set; }
            public string Text
            {
                get => Encoding.UTF8.GetString(Data);
                set => Data = Encoding.UTF8.GetBytes(value);
            }

            public override string ToString() =>
                $"{Id:X04} {Unknown02:X04} {Entry2Index:X08}: {Text}";
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

        public short Unknown { get; set; }
        public List<Entry> Entries1 { get; set; }
        public List<Entry2> Entries2 { get; set; }

        public string GetString(int id)
        {
            var entry = Entries1.FirstOrDefault(x => x.Id == id);
            if (entry == null)
                return null;

            return entry.Text;
        }

        public void Write(Stream stream)
        {
            var entry1Offset = HeaderLength;
            var entry2Offset = Helpers.Align(entry1Offset + Entries1.Count * Entry1Length, 16);
            var textOffset = entry2Offset + Entries2.Count * Entry2Length;

            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Version = Version,
                Unknown08 = 0,
                Unknown0a = Unknown,
                Entry2Count = (short)Entries2.Count,
                Entry1Count = (short)Entries1.Count,
                Entry1Offset = entry1Offset,
                Entry2Offset = entry2Offset,
                TextOffset = textOffset,
                Unknown1c = 0,
            });

            stream.Position = entry1Offset;
            foreach (var item in Entries1)
                BinaryMapping.WriteObject(stream, item);

            stream.Position = entry2Offset;
            foreach (var item in Entries2)
                BinaryMapping.WriteObject(stream, item);

            stream.Position = textOffset;
            foreach (var entry in Entries1)
            {
                stream.Write(entry.Data, 0, entry.Data.Length);
                stream.WriteByte(0);
            }
        }

        private Ctd(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream);
            Unknown = header.Unknown0a;

            stream.Position = header.Entry1Offset;
            Entries1 = Enumerable.Range(0, header.Entry1Count)
                .Select(x => BinaryMapping.ReadObject<Entry>(stream))
                .ToList();

            stream.Position = header.Entry2Offset;
            Entries2 = Enumerable.Range(0, header.Entry2Count)
                .Select(x => BinaryMapping.ReadObject<Entry2>(stream))
                .ToList();

            foreach (var entry in Entries1)
            {
                stream.Position = entry.Offset;
                entry.Data = ReadUntilTerminator(stream);
            }
        }

        private byte[] ReadUntilTerminator(Stream stream)
        {
            var byteList = new List<byte>(100);

            while (stream.Position < stream.Length)
            {
                var ch = stream.ReadByte();
                if (ch <= 0)
                    break;

                byteList.Add((byte)ch);
            }

            return byteList.ToArray();
        }

        public static Ctd Read(Stream stream) => new Ctd(stream);

        public static bool IsValid(Stream stream) =>
            new BinaryReader(stream).ReadInt32() == MagicCode;
    }
}
