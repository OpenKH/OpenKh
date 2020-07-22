using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;
using System.Numerics;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Utils;

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

        public class CollisionMeshGroup : IData
        {
            [Data] public short Child1 { get; set; } = -1;
            [Data] public short Child2 { get; set; } = -1;
            [Data] public short Child3 { get; set; } = -1;
            [Data] public short Child4 { get; set; } = -1;
            [Data] public short Child5 { get; set; } = -1;
            [Data] public short Child6 { get; set; } = -1;
            [Data] public short Child7 { get; set; } = -1;
            [Data] public short Child8 { get; set; } = -1;
            [Data] public BoundingBoxInt16 BoundingBox { get; set; }
            [Data] public ushort CollisionMeshStart { get; set; }
            [Data] public ushort CollisionMeshEnd { get; set; }
        }

        public class CollisionMesh : IData
        {
            [Data] public BoundingBoxInt16 BoundingBox { get; set; }
            [Data] public ushort CollisionStart { get; set; }
            [Data] public ushort CollisionEnd { get; set; }
            [Data] public short v10 { get; set; }
            [Data] public short v12 { get; set; }
        }

        public class Collision : IData
        {
            [Data] public short v00 { get; set; }
            [Data] public short Vertex1 { get; set; } = -1;
            [Data] public short Vertex2 { get; set; } = -1;
            [Data] public short Vertex3 { get; set; } = -1;
            [Data] public short Vertex4 { get; set; } = -1;
            [Data] public short PlaneIndex { get; set; }
            [Data] public short BoundingBoxIndex { get; set; }
            [Data] public short SurfaceFlagsIndex { get; set; }
        }

        public class SurfaceFlags : IData
        {
            [Data] public int Flags { get; set; }
        }

        public int Unknown08 { get; set; }
        public int Unknown0c { get; set; }
        public List<CollisionMeshGroup> CollisionMeshGroupList { get; } = new List<CollisionMeshGroup>();
        public List<CollisionMesh> CollisionMeshList { get; } = new List<CollisionMesh>();
        public List<Collision> CollisionList { get; } = new List<Collision>();
        public List<Vector4> VertexList { get; } = new List<Vector4>();
        public List<Plane> PlaneList { get; } = new List<Plane>();
        public List<BoundingBoxInt16> BoundingBoxList { get; } = new List<BoundingBoxInt16>();
        public List<SurfaceFlags> SurfaceFlagsList { get; } = new List<SurfaceFlags>();

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

            CollisionMeshGroupList = ReactCoctEntry<CollisionMeshGroup>(stream, header.Entries[1], Col1Size);
            CollisionMeshList = ReactCoctEntry<CollisionMesh>(stream, header.Entries[2], Col2Size);
            CollisionList = ReactCoctEntry<Collision>(stream, header.Entries[3], Col3Size);
            VertexList = ReadValueEntry<Vector4>(stream, header.Entries[4], Col4Size, ReadVector4);
            PlaneList = ReadValueEntry<Plane>(stream, header.Entries[5], Col5Size, ReadPlane);
            BoundingBoxList = ReadValueEntry<BoundingBoxInt16>(stream, header.Entries[6], Col6Size, ReadBoundingBoxInt16);
            SurfaceFlagsList = ReactCoctEntry<SurfaceFlags>(stream, header.Entries[7], Col7Size);
        }

        private BoundingBoxInt16 ReadBoundingBoxInt16(Stream arg)
        {
            return new BoundingBoxInt16(
                new Vector3Int16(
                    x: arg.ReadInt16(),
                    y: arg.ReadInt16(),
                    z: arg.ReadInt16()
                ),
                new Vector3Int16(
                    x: arg.ReadInt16(),
                    y: arg.ReadInt16(),
                    z: arg.ReadInt16()
                )
            );
        }

        private Plane ReadPlane(Stream arg)
        {
            return new Plane(
                x: arg.ReadSingle(),
                y: arg.ReadSingle(),
                z: arg.ReadSingle(),
                d: arg.ReadSingle()
            );
        }

        private Vector4 ReadVector4(Stream arg)
        {
            return new Vector4(
                x: arg.ReadSingle(),
                y: arg.ReadSingle(),
                z: arg.ReadSingle(),
                w: arg.ReadSingle()
            );
        }

        public void Write(Stream stream)
        {
            var entries = new List<Entry>(8);
            AddEntry(entries, HeaderSize, 1);
            AddEntry(entries, CollisionMeshGroupList.Count * Col1Size, 0x10);
            AddEntry(entries, CollisionMeshList.Count * Col2Size, 0x10);
            AddEntry(entries, CollisionList.Count * Col3Size, 4);
            AddEntry(entries, VertexList.Count * Col4Size, 0x10);
            AddEntry(entries, PlaneList.Count * Col5Size, 0x10);
            AddEntry(entries, BoundingBoxList.Count * Col6Size, 0x10);
            AddEntry(entries, SurfaceFlagsList.Count * Col7Size, 4);

            stream.Position = 0;
            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Version = 1,
                Unknown08 = Unknown08,
                Unknown0c = Unknown0c,
                Entries = entries.ToArray()
            });

            WriteCoctEntry(stream, CollisionMeshGroupList);
            WriteCoctEntry(stream, CollisionMeshList);
            WriteCoctEntry(stream, CollisionList);
            stream.AlignPosition(0x10);
            WriteValueEntry(stream, VertexList, WriteVector4);
            WriteValueEntry(stream, PlaneList, WritePlane);
            WriteValueEntry(stream, BoundingBoxList, WriteBoundingBoxInt16);
            WriteCoctEntry(stream, SurfaceFlagsList);
        }

        private void WriteBoundingBoxInt16(Stream arg1, BoundingBoxInt16 arg2)
        {
            var writer = new BinaryWriter(arg1);
            writer.Write(arg2.Minimum.X);
            writer.Write(arg2.Minimum.Y);
            writer.Write(arg2.Minimum.Z);
            writer.Write(arg2.Maximum.X);
            writer.Write(arg2.Maximum.Y);
            writer.Write(arg2.Maximum.Z);
        }

        private void WritePlane(Stream arg1, Plane arg2)
        {
            var writer = new BinaryWriter(arg1);
            writer.Write(arg2.Normal.X);
            writer.Write(arg2.Normal.Y);
            writer.Write(arg2.Normal.Z);
            writer.Write(arg2.D);
        }

        private void WriteVector4(Stream arg1, Vector4 arg2)
        {
            var writer = new BinaryWriter(arg1);
            writer.Write(arg2.X);
            writer.Write(arg2.Y);
            writer.Write(arg2.Z);
            writer.Write(arg2.W);
        }

        private List<T> ReactCoctEntry<T>(Stream stream, Entry entry, int sizePerElement)
            where T : class, IData => ReactCoctEntry<T>(stream, entry.Offset, entry.Length / sizePerElement);

        private List<T> ReactCoctEntry<T>(Stream stream, int offset, int length)
            where T : class, IData
        {
            stream.Position = offset;
            return Enumerable.Range(0, length)
                .Select(_ => BinaryMapping.ReadObject<T>(stream))
                .ToList();
        }

        private List<T> ReadValueEntry<T>(Stream stream, Entry entry, int sizePerElement, Func<Stream, T> readFunc)
            => ReadValueEntry<T>(stream, entry.Offset, entry.Length / sizePerElement, readFunc);

        private List<T> ReadValueEntry<T>(Stream stream, int offset, int length, Func<Stream, T> readFunc)
        {
            stream.Position = offset;
            return Enumerable.Range(0, length)
                .Select(_ => readFunc(stream))
                .ToList();
        }

        private void WriteCoctEntry<T>(Stream stream, IEnumerable<T> entries)
            where T : class, IData
        {
            foreach (var entry in entries)
                BinaryMapping.WriteObject(stream, entry);
        }

        private void WriteValueEntry<T>(Stream stream, IEnumerable<T> entries, Action<Stream, T> writeFunc)
        {
            foreach (var entry in entries)
                writeFunc(stream, entry);
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

        public class BuildHelper
        {
            private readonly Coct coct;

            public BuildHelper(Coct coct)
            {
                this.coct = coct;
            }

            public short AllocateBoundingBox(
                BoundingBoxInt16 bbox
            )
            {
                var index = Convert.ToInt16(coct.BoundingBoxList.Count);

                coct.BoundingBoxList.Add(bbox);

                return index;
            }

            public short AllocatePlane(float x, float y, float z, float d)
            {
                var index = Convert.ToInt16(coct.PlaneList.Count);

                coct.PlaneList.Add(new Plane(x, y, z, d));

                return index;
            }

            public short AllocatePlane(Plane plane)
            {
                var index = Convert.ToInt16(coct.PlaneList.Count);

                coct.PlaneList.Add(plane);

                return index;
            }

            public short AllocateSurfaceFlags(int flags)
            {
                var index = Convert.ToInt16(coct.SurfaceFlagsList.Count);

                var ent7 = new SurfaceFlags
                {
                    Flags = flags,
                };
                coct.SurfaceFlagsList.Add(ent7);

                return index;
            }

            public short AllocateVertex(float x, float y, float z, float w = 1)
            {
                var index = Convert.ToInt16(coct.VertexList.Count);

                var ent4 = new Vector4
                {
                    X = x,
                    Y = y,
                    Z = z,
                    W = w
                };
                coct.VertexList.Add(ent4);

                return index;
            }

            public void CompletePlane(Collision ent3)
            {
                var v0 = coct.VertexList[ent3.Vertex1].ToVector3();
                var v1 = coct.VertexList[ent3.Vertex2].ToVector3();
                var v2 = coct.VertexList[ent3.Vertex3].ToVector3();

                ent3.PlaneIndex = AllocatePlane(Plane.CreateFromVertices(v0, v1, v2));
            }

            public void CompleteBBox(Collision ent3)
            {
                var index = AllocateBoundingBox(
                    BoundingBox.FromPoints(
                        coct.VertexList[ent3.Vertex1].ToVector3(),
                        coct.VertexList[ent3.Vertex2].ToVector3(),
                        coct.VertexList[ent3.Vertex3].ToVector3(),
                        coct.VertexList[ent3.Vertex4].ToVector3()
                    )
                        .ToBoundingBoxInt16()
                );

                ent3.BoundingBoxIndex = index;
            }

            public void CompleteBBox(CollisionMesh ent2)
            {
                ent2.BoundingBox = Enumerable.Range(
                    ent2.CollisionStart,
                    ent2.CollisionEnd - ent2.CollisionStart
                )
                    .Select(index => coct.CollisionList[index].BoundingBoxIndex)
                    .Where(co6Index => co6Index != -1)
                    .Select(co6Index => coct.BoundingBoxList[co6Index])
                    .MergeAll();
            }

            public void CompleteBBox(CollisionMeshGroup ent1)
            {
                ent1.BoundingBox = Enumerable.Range(
                    ent1.CollisionMeshStart,
                    ent1.CollisionMeshEnd - ent1.CollisionMeshStart
                )
                    .Select(index => coct.CollisionMeshList[index].BoundingBox)
                    .MergeAll();
            }
        }

        public static Coct Read(Stream stream) =>
            new Coct(stream.SetPosition(0));
    }
}
