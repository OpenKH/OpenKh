using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Coct
    {
        private const uint MagicCode = 0x54434F43;
        private const int HeaderSize = 0x50;
        private const int Col1Size = 0x20;
        private const int Col2Size = 0x14;
        private const int Col3Size = 0x10;
        private const int Col4Size = 0x10;
        private const int Col5Size = 0x10;
        private const int Col6Size = 0xC;
        private const int Col7Size = 0x4;

        private class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public int Version { get; set; }
            [Data] public int Unknown08 { get; set; }
            [Data] public int Unknown0c { get; set; }
            [Data(Count = 8)] public Entry[] Entries { get; set; }
        }

        private class Entry
        {
            [Data] public int Offset { get; set; }
            [Data] public int Length { get; set; }
        }

        public class Co1
        {
            [Data] public short v00 { get; set; }
            [Data] public short v02 { get; set; }
            [Data] public short v04 { get; set; }
            [Data] public short v06 { get; set; }
            [Data] public short v08 { get; set; }
            [Data] public short v0a { get; set; }
            [Data] public short v0c { get; set; }
            [Data] public short v0e { get; set; }
            [Data] public short v10 { get; set; }
            [Data] public short v12 { get; set; }
            [Data] public short v14 { get; set; }
            [Data] public short v16 { get; set; }
            [Data] public short v18 { get; set; }
            [Data] public short v1a { get; set; }
            [Data] public short v1c { get; set; }
            [Data] public short v1e { get; set; }
        }

        public class Co2
        {
            [Data] public short v00 { get; set; }
            [Data] public short v02 { get; set; }
            [Data] public short v04 { get; set; }
            [Data] public short v06 { get; set; }
            [Data] public short v08 { get; set; }
            [Data] public short v0a { get; set; }
            [Data] public short v0c { get; set; }
            [Data] public short v0e { get; set; }
            [Data] public short v10 { get; set; }
            [Data] public short v12 { get; set; }
        }

        public class Co3
        {
            [Data] public short v00 { get; set; }
            [Data] public short v02 { get; set; }
            [Data] public short v04 { get; set; }
            [Data] public short v06 { get; set; }
            [Data] public short v08 { get; set; }
            [Data] public short v0a { get; set; }
            [Data] public short v0c { get; set; }
            [Data] public short v0e { get; set; }
        }

        public class Vector4
        {
            [Data] public float X { get; set; }
            [Data] public float Y { get; set; }
            [Data] public float Z { get; set; }
            [Data] public float W { get; set; }
        }

        public class Co5
        {
            [Data] public float X { get; set; }
            [Data] public float Y { get; set; }
            [Data] public float Z { get; set; }
            [Data] public float D { get; set; }
        }

        public class Co6
        {
            [Data] public short v00 { get; set; }
            [Data] public short v02 { get; set; }
            [Data] public short v04 { get; set; }
            [Data] public short v06 { get; set; }
            [Data] public short v08 { get; set; }
            [Data] public short v0a { get; set; }
        }

        public class Co7
        {
            [Data] public int Unknown { get; set; }
        }

        public int Unknown08 { get; set; }
        public int Unknown0c { get; set; }
        public List<Co1> Collision1 { get; }
        public List<Co2> Collision2 { get; }
        public List<Co3> Collision3 { get; }
        public List<Vector4> Collision4 { get; }
        public List<Co5> Collision5 { get; }
        public List<Co6> Collision6 { get; }
        public List<Co7> Collision7 { get; }

        private Coct(Stream stream)
        {
            if (!IsValid(stream))
                throw new InvalidDataException("Invalid header");

            var header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0));
            Unknown08 = header.Unknown08;
            Unknown0c = header.Unknown0c;

            Collision1 = ReactCoctEntry<Co1>(stream, header.Entries[1], Col1Size);
            Collision2 = ReactCoctEntry<Co2>(stream, header.Entries[2], Col2Size);
            Collision3 = ReactCoctEntry<Co3>(stream, header.Entries[3], Col3Size);
            Collision4 = ReactCoctEntry<Vector4>(stream, header.Entries[4], Col4Size);
            Collision5 = ReactCoctEntry<Co5>(stream, header.Entries[5], Col5Size);
            Collision6 = ReactCoctEntry<Co6>(stream, header.Entries[6], Col6Size);
            Collision7 = ReactCoctEntry<Co7>(stream, header.Entries[7], Col7Size);
        }

        public void Write(Stream stream)
        {
            var entries = new List<Entry>(8);
            AddEntry(entries, HeaderSize, 1);
            AddEntry(entries, Collision1.Count * Col1Size, 0x10);
            AddEntry(entries, Collision2.Count * Col2Size, 0x10);
            AddEntry(entries, Collision3.Count * Col3Size, 4);
            AddEntry(entries, Collision4.Count * Col4Size, 0x10);
            AddEntry(entries, Collision5.Count * Col5Size, 0x10);
            AddEntry(entries, Collision6.Count * Col6Size, 0x10);
            AddEntry(entries, Collision7.Count * Col7Size, 4);

            stream.Position = 0;
            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Version = 1,
                Unknown08 = Unknown08,
                Unknown0c = Unknown0c,
                Entries = entries.ToArray()
            });

            WriteCoctEntry(stream, Collision1);
            WriteCoctEntry(stream, Collision2);
            WriteCoctEntry(stream, Collision3);
            stream.AlignPosition(0x10);
            WriteCoctEntry(stream, Collision4);
            WriteCoctEntry(stream, Collision5);
            WriteCoctEntry(stream, Collision6);
            WriteCoctEntry(stream, Collision7);
        }

        private List<T> ReactCoctEntry<T>(Stream stream, Entry entry, int sizePerElement)
            where T : class => ReadCoctEntry<T>(stream, entry.Offset, entry.Length / sizePerElement);

        private List<T> ReadCoctEntry<T>(Stream stream, int offset, int length)
            where T : class
        {
            stream.Position = offset;
            return Enumerable.Range(0, length)
                .Select(_ => BinaryMapping.ReadObject<T>(stream))
                .ToList();
        }

        private void WriteCoctEntry<T>(Stream stream, IEnumerable<T> entries)
            where T : class
        {
            foreach (var entry in entries)
                BinaryMapping.WriteObject(stream, entry);
        }

        private static void AddEntry(List<Entry> entries, int newEntryLength, int alignment)
        {
            var lastEntry = entries.LastOrDefault();
            entries.Add(new Entry
            {
                Offset = Helpers.Align((lastEntry?.Offset + lastEntry?.Length) ?? 0, alignment),
                Length = newEntryLength
            });
        }

        public static bool IsValid(Stream stream)
        {
            if (stream.SetPosition(0).ReadInt32() != MagicCode ||
                stream.ReadInt32() != 1 ||
                stream.Length < 0x50)
                return false;

            return true;
        }

        public static Coct Read(Stream stream) =>
            new Coct(stream);
    }
}
