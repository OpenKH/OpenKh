using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    /// <summary>
    /// Low level reader/writer of COCT
    /// </summary>
    public class Coct
    {
        private interface IData { }

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

        public class Co1 : IData
        {
            [Data] public short Child1 { get; set; }
            [Data] public short Child2 { get; set; }
            [Data] public short Child3 { get; set; }
            [Data] public short Child4 { get; set; }
            [Data] public short Child5 { get; set; }
            [Data] public short Child6 { get; set; }
            [Data] public short Child7 { get; set; }
            [Data] public short Child8 { get; set; }
            [Data] public short MinX { get; set; }
            [Data] public short MinY { get; set; }
            [Data] public short MinZ { get; set; }
            [Data] public short MaxX { get; set; }
            [Data] public short MaxY { get; set; }
            [Data] public short MaxZ { get; set; }
            [Data] public ushort Collision2Start { get; set; }
            [Data] public ushort Collision2End { get; set; }
        }

        public class CollisionMesh : IData
        {
            [Data] public short MinX { get; set; }
            [Data] public short MinY { get; set; }
            [Data] public short MinZ { get; set; }
            [Data] public short MaxX { get; set; }
            [Data] public short MaxY { get; set; }
            [Data] public short MaxZ { get; set; }
            [Data] public ushort Collision3Start { get; set; }
            [Data] public ushort Collision3End { get; set; }
            [Data] public short v10 { get; set; }
            [Data] public short v12 { get; set; }
        }

        public class Co3 : IData
        {
            [Data] public short v00 { get; set; }
            [Data] public short Vertex1 { get; set; }
            [Data] public short Vertex2 { get; set; }
            [Data] public short Vertex3 { get; set; }
            [Data] public short Vertex4 { get; set; }
            [Data] public short Co5Index { get; set; }
            [Data] public short Co6Index { get; set; }
            [Data] public short Co7Index { get; set; }
        }

        public class Vector4 : IData
        {
            [Data] public float X { get; set; }
            [Data] public float Y { get; set; }
            [Data] public float Z { get; set; }
            [Data] public float W { get; set; }

            public override string ToString() =>
                $"V({X}, {Y}, {Z}, {W})";
        }

        /// <summary>
        /// Plane (x,y,z,d)
        /// </summary>
        public class Co5 : IData
        {
            [Data] public float X { get; set; }
            [Data] public float Y { get; set; }
            [Data] public float Z { get; set; }
            [Data] public float D { get; set; }

            public override string ToString() =>
                $"N({X}, {Y}, {Z}, {D})";
        }

        public class Co6 : IData
        {
            [Data] public short MinX { get; set; }
            [Data] public short MinY { get; set; }
            [Data] public short MinZ { get; set; }
            [Data] public short MaxX { get; set; }
            [Data] public short MaxY { get; set; }
            [Data] public short MaxZ { get; set; }
        }

        public class Co7 : IData
        {
            [Data] public int Unknown { get; set; }
        }

        public int Unknown08 { get; set; }
        public int Unknown0c { get; set; }
        public List<Co1> Collision1 { get; } = new List<Co1>();
        public List<CollisionMesh> Collision2 { get; } = new List<CollisionMesh>();
        public List<Co3> Collision3 { get; } = new List<Co3>();
        public List<Vector4> CollisionVertices { get; } = new List<Vector4>();
        public List<Co5> Collision5 { get; } = new List<Co5>();
        public List<Co6> Collision6 { get; } = new List<Co6>();
        public List<Co7> Collision7 { get; } = new List<Co7>();

        public Coct()
        {

        }

        private Coct(Stream stream)
        {
            if (!IsValid(stream))
                throw new InvalidDataException("Invalid header");

            var header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0));
            Unknown08 = header.Unknown08;
            Unknown0c = header.Unknown0c;

            Collision1 = ReactCoctEntry<Co1>(stream, header.Entries[1], Col1Size);
            Collision2 = ReactCoctEntry<CollisionMesh>(stream, header.Entries[2], Col2Size);
            Collision3 = ReactCoctEntry<Co3>(stream, header.Entries[3], Col3Size);
            CollisionVertices = ReactCoctEntry<Vector4>(stream, header.Entries[4], Col4Size);
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
            AddEntry(entries, CollisionVertices.Count * Col4Size, 0x10);
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
            WriteCoctEntry(stream, CollisionVertices);
            WriteCoctEntry(stream, Collision5);
            WriteCoctEntry(stream, Collision6);
            WriteCoctEntry(stream, Collision7);
        }

        private List<T> ReactCoctEntry<T>(Stream stream, Entry entry, int sizePerElement)
            where T : class, IData => ReadCoctEntry<T>(stream, entry.Offset, entry.Length / sizePerElement);

        private List<T> ReadCoctEntry<T>(Stream stream, int offset, int length)
            where T : class, IData
        {
            stream.Position = offset;
            return Enumerable.Range(0, length)
                .Select(_ => BinaryMapping.ReadObject<T>(stream))
                .ToList();
        }

        private void WriteCoctEntry<T>(Stream stream, IEnumerable<T> entries)
            where T : class, IData
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
            new Coct(stream.SetPosition(0));
    }
}
