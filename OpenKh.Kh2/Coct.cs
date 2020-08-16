using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;
using System.Numerics;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Utils;
using System.Collections;

namespace OpenKh.Kh2
{
    /// <summary>
    /// Low level reader/writer of COCT
    /// </summary>
    public class Coct
    {
        private class WriteCache<TValue, TKey> : IEnumerable<TValue>
        {
            private readonly List<TValue> _list = new List<TValue>();
            private readonly Dictionary<TKey, short> _dictionary = new Dictionary<TKey, short>();
            private readonly Func<TValue, TKey> _getKey;

            public int Count => _list.Count;

            public WriteCache(Func<TValue, TKey> getKey)
            {
                _getKey = getKey;
            }

            public void AddConstant(TValue item, short value)
            {
                _dictionary[_getKey(item)] = value;
            }

            public void Add(TValue item)
            {
                var key = _getKey(item);
                if (!_dictionary.ContainsKey(key))
                {
                    _dictionary[key] = (short)(_list.Count);
                    _list.Add(item);
                }
            }

            public short this[TValue item] => _dictionary[_getKey(item)];

            public IEnumerator<TValue> GetEnumerator() => _list.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
        }

        private interface IData { }

        private const uint MagicCode = 0x54434F43;
        private const int HeaderSize = 0x50;
        private const int Col1Size = 0x20;
        private const int Col2Size = 0x14;
        private const int Col3Size = 0x10;
        private const int Col4Size = 0x10;
        private const int Col5Size = 0x10;
        private const int BoundingBoxSize = 0xC;
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

        public class RawCollisionMeshGroup : IData
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

        private class RawCollision : IData
        {
            [Data] public short v00 { get; set; }
            [Data] public short Vertex1 { get; set; }
            [Data] public short Vertex2 { get; set; }
            [Data] public short Vertex3 { get; set; }
            [Data] public short Vertex4 { get; set; }
            [Data] public short PlaneIndex { get; set; }
            [Data] public short BoundingBoxIndex { get; set; }
            [Data] public short SurfaceFlagsIndex { get; set; }
        }

        private class RawCollisionMesh : IData
        {
            [Data] public BoundingBoxInt16 BoundingBox { get; set; }
            [Data] public ushort CollisionStart { get; set; }
            [Data] public ushort CollisionEnd { get; set; }
            [Data] public short v10 { get; set; }
            [Data] public short v12 { get; set; }
        }

        public class CollisionMeshGroup : IData
        {
            public short Child1 { get; set; } = -1;
            public short Child2 { get; set; } = -1;
            public short Child3 { get; set; } = -1;
            public short Child4 { get; set; } = -1;
            public short Child5 { get; set; } = -1;
            public short Child6 { get; set; } = -1;
            public short Child7 { get; set; } = -1;
            public short Child8 { get; set; } = -1;
            public BoundingBoxInt16 BoundingBox { get; set; }
            public List<CollisionMesh> Meshes { get; set; }
        }

        public class CollisionMesh
        {
            public BoundingBoxInt16 BoundingBox { get; set; }
            public List<Collision> Collisions { get; set; }
            public short v10 { get; set; }
            public short v12 { get; set; }
        }

        public class Collision : IData
        {
            public short v00 { get; set; }
            public short Vertex1 { get; set; } = -1;
            public short Vertex2 { get; set; } = -1;
            public short Vertex3 { get; set; } = -1;
            public short Vertex4 { get; set; } = -1;
            public Plane Plane { get; set; }
            public BoundingBoxInt16 BoundingBox { get; set; }
            public SurfaceFlags SurfaceFlags { get; set; }
        }

        public class SurfaceFlags : IData
        {
            [Data] public int Flags { get; set; }
        }

        public int Unknown08 { get; set; }
        public int Unknown0c { get; set; }
        public List<CollisionMeshGroup> CollisionMeshGroupList { get; } = new List<CollisionMeshGroup>();
        public List<Vector4> VertexList { get; } = new List<Vector4>();

        private readonly BuildHelper buildHelper;

        public Coct()
        {
            buildHelper = new BuildHelper(this);
        }

        private Coct(Stream stream)
            : this()
        {
            if (!IsValid(stream))
                throw new InvalidDataException("Invalid header");

            var header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0));
            Unknown08 = header.Unknown08;
            Unknown0c = header.Unknown0c;

            var rawCollisionMeshGroupList = ReactCoctEntry<RawCollisionMeshGroup>(stream, header.Entries[1], Col1Size);
            var rawCollisionMeshList = ReactCoctEntry<RawCollisionMesh>(stream, header.Entries[2], Col2Size);
            var rawCollisionList = ReactCoctEntry<RawCollision>(stream, header.Entries[3], Col3Size);
            VertexList = ReadValueEntry<Vector4>(stream, header.Entries[4], Col4Size, ReadVector4);
            var planeList = ReadValueEntry<Plane>(stream, header.Entries[5], Col5Size, ReadPlane);
            var boundingBoxes = ReadValueEntry(stream, header.Entries[6], BoundingBoxSize, ReadBoundingBoxInt16);
            var surfaceFlagsList = ReactCoctEntry<SurfaceFlags>(stream, header.Entries[7], Col7Size);

            var collisionList = rawCollisionList
                .Select(x => new Collision
                {
                    v00 = x.v00,
                    Vertex1 = x.Vertex1,
                    Vertex2 = x.Vertex2,
                    Vertex3 = x.Vertex3,
                    Vertex4 = x.Vertex4,
                    Plane = planeList[x.PlaneIndex],
                    BoundingBox = x.BoundingBoxIndex >= 0 ? boundingBoxes[x.BoundingBoxIndex] : BoundingBoxInt16.Invalid,
                    SurfaceFlags = surfaceFlagsList[x.SurfaceFlagsIndex],
                })
                .ToList();

            var collisionMeshList = rawCollisionMeshList
                .Select(x => new CollisionMesh
                {
                    BoundingBox = x.BoundingBox,
                    Collisions = Enumerable.Range(x.CollisionStart, x.CollisionEnd - x.CollisionStart)
                        .Select(i => collisionList[i])
                        .ToList(),
                    v10 = x.v10,
                    v12 = x.v12
                })
                .ToList();

            CollisionMeshGroupList = rawCollisionMeshGroupList
                .Select(x => new CollisionMeshGroup
                {
                    Child1 = x.Child1,
                    Child2 = x.Child2,
                    Child3 = x.Child3,
                    Child4 = x.Child4,
                    Child5 = x.Child5,
                    Child6 = x.Child6,
                    Child7 = x.Child7,
                    Child8 = x.Child8,
                    BoundingBox = x.BoundingBox,
                    Meshes = Enumerable.Range(x.CollisionMeshStart, x.CollisionMeshEnd - x.CollisionMeshStart)
                        .Select(i => collisionMeshList[i])
                        .ToList(),
                })
                .ToList();
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
            var collisionList = new List<Collision>();
            var collisionMeshList = new List<RawCollisionMesh>();
            var collisionMeshGroupList = new List<RawCollisionMeshGroup>();
            foreach (var node in CollisionMeshGroupList)
            {
                var start = node.Meshes.Count > 0 ? collisionMeshList.Count : 0;
                collisionMeshGroupList.Add(new RawCollisionMeshGroup
                {
                    Child1 = node.Child1,
                    Child2 = node.Child2,
                    Child3 = node.Child3,
                    Child4 = node.Child4,
                    Child5 = node.Child5,
                    Child6 = node.Child6,
                    Child7 = node.Child7,
                    Child8 = node.Child8,
                    BoundingBox = node.BoundingBox,
                    CollisionMeshStart = (ushort)start,
                    CollisionMeshEnd = (ushort)(start + node.Meshes.Count)
                });

                foreach (var mesh in node.Meshes)
                {
                    collisionMeshList.Add(new RawCollisionMesh
                    {
                        BoundingBox = mesh.BoundingBox,
                        CollisionStart = (ushort)collisionList.Count,
                        CollisionEnd = (ushort)(collisionList.Count + mesh.Collisions.Count),
                        v10 = mesh.v10,
                        v12 = mesh.v12
                    });

                    collisionList.AddRange(mesh.Collisions);
                }
            }

            var bbCache = new WriteCache<BoundingBoxInt16, string>(x => x.ToString());
            bbCache.AddConstant(BoundingBoxInt16.Invalid, -1);
            foreach (var item in collisionList)
                bbCache.Add(item.BoundingBox);

            var surfaceCache = new WriteCache<SurfaceFlags, int>(x => x.Flags);
            foreach (var item in collisionList)
                surfaceCache.Add(item.SurfaceFlags);

            var planeCache = new WriteCache<Plane, string>(x => x.ToString());
            foreach (var item in collisionList)
                planeCache.Add(item.Plane);

            var entries = new List<Entry>(8);
            AddEntry(entries, HeaderSize, 1);
            AddEntry(entries, CollisionMeshGroupList.Count * Col1Size, 0x10);
            AddEntry(entries, collisionMeshList.Count * Col2Size, 0x10);
            AddEntry(entries, collisionList.Count * Col3Size, 4);
            AddEntry(entries, VertexList.Count * Col4Size, 0x10);
            AddEntry(entries, planeCache.Count * Col5Size, 0x10);
            AddEntry(entries, bbCache.Count * BoundingBoxSize, 0x10);
            AddEntry(entries, surfaceCache.Count * Col7Size, 4);

            stream.Position = 0;
            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Version = 1,
                Unknown08 = Unknown08,
                Unknown0c = Unknown0c,
                Entries = entries.ToArray()
            });

            WriteCoctEntry(stream, collisionMeshGroupList);
            WriteCoctEntry(stream, collisionMeshList);
            WriteCoctEntry(stream, collisionList
                .Select(x => new RawCollision
                {
                    v00 = x.v00,
                    Vertex1 = x.Vertex1,
                    Vertex2 = x.Vertex2,
                    Vertex3 = x.Vertex3,
                    Vertex4 = x.Vertex4,
                    PlaneIndex = planeCache[x.Plane],
                    BoundingBoxIndex = bbCache[x.BoundingBox],
                    SurfaceFlagsIndex = surfaceCache[x.SurfaceFlags],
                }));
            stream.AlignPosition(0x10);
            WriteValueEntry(stream, VertexList, WriteVector4);
            WriteValueEntry(stream, planeCache, WritePlane);
            WriteValueEntry(stream, bbCache, WriteBoundingBoxInt16);
            WriteCoctEntry(stream, surfaceCache);
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
            private readonly SortedDictionary<string, int> vertexIndexMap = new SortedDictionary<string, int>();

            public BuildHelper(Coct coct)
            {
                this.coct = coct;
            }

            public short AllocateVertex(float x, float y, float z, float w = 1)
            {
                var ent4 = new Vector4
                {
                    X = x,
                    Y = y,
                    Z = z,
                    W = w
                };

                var key = ent4.ToString();

                if (!vertexIndexMap.TryGetValue(key, out int index))
                {
                    index = Convert.ToInt16(coct.VertexList.Count);

                    coct.VertexList.Add(ent4);

                    vertexIndexMap[key] = index;
                }

                return Convert.ToInt16(index);
            }

            public void CompletePlane(Collision ent3)
            {
                var v0 = coct.VertexList[ent3.Vertex1].ToVector3();
                var v1 = coct.VertexList[ent3.Vertex2].ToVector3();
                var v2 = coct.VertexList[ent3.Vertex3].ToVector3();
                ent3.Plane = Plane.CreateFromVertices(v0, v1, v2);
            }

            public void CompleteBBox(Collision ent3, int inflate = 0)
            {
                ent3.BoundingBox = BoundingBox.FromPoints(
                    coct.VertexList[ent3.Vertex1].ToVector3(),
                    coct.VertexList[ent3.Vertex2].ToVector3(),
                    coct.VertexList[ent3.Vertex3].ToVector3(),
                    coct.VertexList[(ent3.Vertex4 == -1) ? ent3.Vertex3 : ent3.Vertex4].ToVector3()
                )
                .ToBoundingBoxInt16()
                .InflateWith(Convert.ToInt16(inflate));
            }

            public void CompleteBBox(CollisionMesh ent2)
            {
                ent2.BoundingBox = ent2.Collisions
                    .Where(collision => collision.BoundingBox != BoundingBoxInt16.Invalid)
                    .Select(x => x.BoundingBox)
                    .MergeAll();
            }

            public void CompleteBBox(CollisionMeshGroup ent1)
            {
                ent1.BoundingBox = ent1.Meshes
                    .Select(x => x.BoundingBox)
                    .Concat(SelectBoundingBoxList(ent1))
                    .MergeAll();
            }

            private IEnumerable<BoundingBoxInt16> SelectBoundingBoxList(CollisionMeshGroup ent1)
            {
                if (ent1.Child1 != -1)
                {
                    yield return coct.CollisionMeshGroupList[ent1.Child1].BoundingBox;

                    if (ent1.Child2 != -1)
                    {
                        yield return coct.CollisionMeshGroupList[ent1.Child2].BoundingBox;

                        if (ent1.Child3 != -1)
                        {
                            yield return coct.CollisionMeshGroupList[ent1.Child3].BoundingBox;

                            if (ent1.Child4 != -1)
                            {
                                yield return coct.CollisionMeshGroupList[ent1.Child4].BoundingBox;

                                if (ent1.Child5 != -1)
                                {
                                    yield return coct.CollisionMeshGroupList[ent1.Child5].BoundingBox;

                                    if (ent1.Child6 != -1)
                                    {
                                        yield return coct.CollisionMeshGroupList[ent1.Child6].BoundingBox;

                                        if (ent1.Child7 != -1)
                                        {
                                            yield return coct.CollisionMeshGroupList[ent1.Child7].BoundingBox;

                                            if (ent1.Child8 != -1)
                                            {
                                                yield return coct.CollisionMeshGroupList[ent1.Child8].BoundingBox;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public void FlushCache()
            {
                vertexIndexMap.Clear();
            }
        }

        public void ReverseMeshGroup()
        {
            var maxIndex = CollisionMeshGroupList.Count - 1;

            CollisionMeshGroupList.Reverse();

            short ReverseChildIndex(short child)
            {
                if (child != -1)
                {
                    child = Convert.ToInt16(maxIndex - child);
                }
                return child;
            }

            foreach (var one in CollisionMeshGroupList)
            {
                one.Child1 = ReverseChildIndex(one.Child1);
                one.Child2 = ReverseChildIndex(one.Child2);
                one.Child3 = ReverseChildIndex(one.Child3);
                one.Child4 = ReverseChildIndex(one.Child4);
                one.Child5 = ReverseChildIndex(one.Child5);
                one.Child6 = ReverseChildIndex(one.Child6);
                one.Child7 = ReverseChildIndex(one.Child7);
                one.Child8 = ReverseChildIndex(one.Child8);
            }
        }

        public static Coct Read(Stream stream) =>
            new Coct(stream.SetPosition(0));

        public Collision Complete(Collision it, int inflate = 0)
        {
            buildHelper.CompleteBBox(it, inflate);
            buildHelper.CompletePlane(it);
            return it;
        }

        public CollisionMesh Complete(CollisionMesh it)
        {
            buildHelper.CompleteBBox(it);
            return it;
        }

        public CollisionMeshGroup CompleteAndAdd(CollisionMeshGroup it)
        {
            buildHelper.CompleteBBox(it);
            CollisionMeshGroupList.Add(it);
            return it;
        }

        public void FlushCache() => buildHelper.FlushCache();
    }
}
