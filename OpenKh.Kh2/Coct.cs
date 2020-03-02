using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
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

        private class _Co1 : IData
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
            [Data] public ushort Collision2Start { get; set; }
            [Data] public ushort Collision2End { get; set; }
        }

        public class Co1
        {
            public short v00 { get; set; }
            public short v02 { get; set; }
            public short v04 { get; set; }
            public short v06 { get; set; }
            public short v08 { get; set; }
            public short v0a { get; set; }
            public short v0c { get; set; }
            public short v0e { get; set; }
            public short v10 { get; set; }
            public short v12 { get; set; }
            public short v14 { get; set; }
            public short v16 { get; set; }
            public short v18 { get; set; }
            public short v1a { get; set; }

            public List<CollisionMesh> Meshes { get; set; }
        }

        private class _CollisionMesh : IData
        {
            [Data] public short v00 { get; set; }
            [Data] public short v02 { get; set; }
            [Data] public short v04 { get; set; }
            [Data] public short v06 { get; set; }
            [Data] public short v08 { get; set; }
            [Data] public short v0a { get; set; }
            [Data] public ushort Collision3Start { get; set; }
            [Data] public ushort Collision3End { get; set; }
            [Data] public short v10 { get; set; }
            [Data] public short v12 { get; set; }
        }

        public class CollisionMesh
        {
            public short v00 { get; set; }
            public short v02 { get; set; }
            public short v04 { get; set; }
            public short v06 { get; set; }
            public short v08 { get; set; }
            public short v0a { get; set; }
            public short v10 { get; set; }
            public short v12 { get; set; }

            public List<Co3> Items { get; set; }
        }

        public class Co3 : IData
        {
            [Data] public short v00 { get; set; }
            [Data] public short Vertex1 { get; set; }
            [Data] public short Vertex2 { get; set; }
            [Data] public short Vertex3 { get; set; }
            [Data] public short Vertex4 { get; set; }
            [Data] public short v0a { get; set; }
            [Data] public short v0c { get; set; }
            [Data] public short v0e { get; set; }
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
            [Data] public short v00 { get; set; }
            [Data] public short v02 { get; set; }
            [Data] public short v04 { get; set; }
            [Data] public short v06 { get; set; }
            [Data] public short v08 { get; set; }
            [Data] public short v0a { get; set; }
        }

        public class Co7 : IData
        {
            [Data] public int Unknown { get; set; }
        }

        public int Unknown08 { get; set; }
        public int Unknown0c { get; set; }
        public List<Co1> Collision1 { get; }
        public List<Vector4> CollisionVertices { get; }
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

            var collision1 = ReactCoctEntry<_Co1>(stream, header.Entries[1], Col1Size);
            var collision2 = ReactCoctEntry<_CollisionMesh>(stream, header.Entries[2], Col2Size);
            var collision3 = ReactCoctEntry<Co3>(stream, header.Entries[3], Col3Size);
            CollisionVertices = ReactCoctEntry<Vector4>(stream, header.Entries[4], Col4Size);
            Collision5 = ReactCoctEntry<Co5>(stream, header.Entries[5], Col5Size);
            Collision6 = ReactCoctEntry<Co6>(stream, header.Entries[6], Col6Size);
            Collision7 = ReactCoctEntry<Co7>(stream, header.Entries[7], Col7Size);

            Collision1 = collision1.Select(x => GetCo1(x, collision2, collision3)).ToList();
        }

        private static Co1 GetCo1(_Co1 item, List<_CollisionMesh> collision2, List<Co3> co3)
        {
            var mapped = Map(item);
            mapped.Meshes = new List<CollisionMesh>(item.Collision2End);
            for (int i = item.Collision2Start; i < item.Collision2End; i++)
                mapped.Meshes.Add(GetCo2(collision2[i], co3));

            return mapped;
        }

        private static CollisionMesh GetCo2(_CollisionMesh co2, List<Co3> co3)
        {
            var mapped = Map(co2);
            mapped.Items = new List<Co3>();
            for (int i = co2.Collision3Start; i < co2.Collision3End; i++)
                mapped.Items.Add(co3[i]);

            return mapped;
        }

        public void Write(Stream stream)
        {
            var collision1 = new List<_Co1>();
            var collision2 = new List<_CollisionMesh>();
            var collision3 = new List<Co3>();
            foreach (var item1 in Collision1)
            {
                var mapped1 = Map(item1);
                if (item1.Meshes.Count != 0)
                {
                    mapped1.Collision2Start = (ushort)collision2.Count;
                    foreach (var item2 in item1.Meshes)
                    {
                        var mapped2 = Map(item2);
                        mapped2.Collision3Start = (ushort)collision3.Count;
                        collision3.AddRange(item2.Items);
                        mapped2.Collision3End = (ushort)collision3.Count;

                        collision2.Add(mapped2);
                    }
                    mapped1.Collision2End = (ushort)collision2.Count;
                }
                else
                {
                    mapped1.Collision2Start = 0;
                    mapped1.Collision2End = 0;
                }
                collision1.Add(mapped1);
            }

            var entries = new List<Entry>(8);
            AddEntry(entries, HeaderSize, 1);
            AddEntry(entries, collision1.Count * Col1Size, 0x10);
            AddEntry(entries, collision2.Count * Col2Size, 0x10);
            AddEntry(entries, collision3.Count * Col3Size, 4);
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

            WriteCoctEntry(stream, collision1);
            WriteCoctEntry(stream, collision2);
            WriteCoctEntry(stream, collision3);
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

        private static Co1 Map(_Co1 x) => new Co1
        {
            v00 = x.v00,
            v02 = x.v02,
            v04 = x.v04,
            v06 = x.v06,
            v08 = x.v08,
            v0a = x.v0a,
            v0c = x.v0c,
            v0e = x.v0e,
            v10 = x.v10,
            v12 = x.v12,
            v14 = x.v14,
            v16 = x.v16,
            v18 = x.v18,
            v1a = x.v1a,
        };

        private static _Co1 Map(Co1 x) => new _Co1
        {
            v00 = x.v00,
            v02 = x.v02,
            v04 = x.v04,
            v06 = x.v06,
            v08 = x.v08,
            v0a = x.v0a,
            v0c = x.v0c,
            v0e = x.v0e,
            v10 = x.v10,
            v12 = x.v12,
            v14 = x.v14,
            v16 = x.v16,
            v18 = x.v18,
            v1a = x.v1a,
        };

        private static CollisionMesh Map(_CollisionMesh x) => new CollisionMesh
        {
            v00 = x.v00,
            v02 = x.v02,
            v04 = x.v04,
            v06 = x.v06,
            v08 = x.v08,
            v0a = x.v0a,
            v10 = x.v10,
            v12 = x.v12,
        };

        private static _CollisionMesh Map(CollisionMesh x) => new _CollisionMesh
        {
            v00 = x.v00,
            v02 = x.v02,
            v04 = x.v04,
            v06 = x.v06,
            v08 = x.v08,
            v0a = x.v0a,
            v10 = x.v10,
            v12 = x.v12,
        };
    }
}
