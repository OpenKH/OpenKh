using OpenKh.Common;
using System;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Ps2
{
    public class VpuPacket
    {
        public enum VertexFunction
        {
            DrawTriangleDoubleSided = 0x00,
            Stock = 0x10,
            DrawTriangle = 0x20,
            DrawTriangleInverse = 0x30
        }

        public class VpuHeader
        {
            [Data] public int Type { get; set; }
            [Data] public int VertexColorPtrInc { get; set; }
            [Data] public int MagicNumber { get; set; }
            [Data] public int VertexBufferPointer { get; set; }
            [Data] public int TriStripNodeCount { get; set; }
            [Data] public int TriStripNodeOffset { get; set; }
            [Data] public int MatrixCountOffset { get; set; }
            [Data] public int MatrixOffset { get; set; }
            [Data] public int ColorCount { get; set; }
            [Data] public int ColorOffset { get; set; }
            [Data] public int WeightGroupCount { get; set; }
            [Data] public int WeightGroupCountOffset { get; set; }
            [Data] public int VertexCoordCount { get; set; }
            [Data] public int VertexCoordOffset { get; set; }
            [Data] public int VertexIndexOffset { get; set; }
            [Data] public int MatrixCount { get; set; }
        }

        /*
         * SKELETAL REFERENCE
         * Header[1]
         * StripNodes[] - U; V; VertexIndex; Function (1 int each) | TriStripNodeOffset (Always 0x04)
         * VertexCoords[] - X; Y; Z; W; (1 int each, W is weight asignment) | VertexCoordOffset ! W = 0 for single bones
         * MatrixCounts[] - Amount of vertices assigned to the bone in this position (1 int each) | MatrixCountOffset
         * VertexIndices[] - Weight groups (1 int per weight) | WeightGroupCountOffset + VertexIndexOffset ! Only for multiple bones
         * Matrices[] - Transform matrices of the bones | MatrixOffset
         */

        public class VertexIndex
        {
            [Data] public int U { get; set; }
            [Data] public int V { get; set; }
            [Data] public int Index { get; set; }
            [Data] public VertexFunction Function { get; set; }

            public override string ToString() =>
                $"{U / 4096.0f:F}, {V / 4096.0f:F}, {Index:X}, {Function}";
        }

        public class VertexColor
        {
            [Data] public int R { get; set; }
            [Data] public int G { get; set; }
            [Data] public int B { get; set; }
            [Data] public int A { get; set; }

            public override string ToString() =>
                $"{R:X}, {G:X}, {B:X}, {A:X}";
        }

        public class VertexCoord
        {
            [Data] public float X { get; set; }
            [Data] public float Y { get; set; }
            [Data] public float Z { get; set; }
            [Data] public float W { get; set; }

            public override string ToString() =>
                $"{X:F}, {Y:F}, {Z:F}, {W:F}";
        }
        
        public VertexIndex[] Indices { get; }
        public VertexColor[] Colors { get; }
        public VertexCoord[] Vertices { get; }
        public int[] VertexRange { get; }
        public int VertexWeightedCount { get; }
        public int[][][] VertexWeightedIndices { get; }

        private VpuPacket(Stream stream)
        {
            var vpu = BinaryMapping.ReadObject<VpuHeader>(stream);

            VertexRange = Read(stream, vpu.MatrixCountOffset, vpu.MatrixCount, ReadInt32);
            Indices = Read(stream, vpu.TriStripNodeOffset, vpu.TriStripNodeCount, ReadIndex);
            Colors = Read(stream, vpu.ColorOffset, vpu.ColorCount, ReadColor);
            Vertices = Read(stream, vpu.VertexCoordOffset, vpu.VertexCoordCount, ReadVertex);

            if (vpu.WeightGroupCount > 0)
            {
                var countPerAmount = Read(stream, vpu.WeightGroupCountOffset, vpu.WeightGroupCount, ReadInt32);
                VertexWeightedCount = countPerAmount.Sum();

                VertexWeightedIndices = countPerAmount
                    .Select((count, amount) =>
                    {
                        stream.AlignPosition(0x10);
                        return Enumerable
                            .Range(0, count)
                            .Select(x => Enumerable.Range(0, amount + 1).Select(y => stream.ReadInt32()).ToArray())
                            .ToArray();
                    })
                    .ToArray();

            }

            //Debug.Assert(vpu.VertexCount == Box.Sum());
        }

        private static int ReadInt32(Stream stream) => stream.ReadInt32();

        private static VertexIndex ReadIndex(Stream stream) => new VertexIndex
        {
            U = stream.ReadInt32(),
            V = stream.ReadInt32(),
            Index = stream.ReadInt32(),
            Function = (VertexFunction)stream.ReadInt32()
        };

        private static VertexColor ReadColor(Stream stream) => new VertexColor
        {
            R = stream.ReadInt32(),
            G = stream.ReadInt32(),
            B = stream.ReadInt32(),
            A = stream.ReadInt32(),
        };

        private static VertexCoord ReadVertex(Stream stream) => new VertexCoord
        {
            X = stream.ReadSingle(),
            Y = stream.ReadSingle(),
            Z = stream.ReadSingle(),
            W = stream.ReadSingle(),
        };

        private static T[] Read<T>(Stream stream, int offset, int count, Func<Stream, T> func)
        {
            stream.SetPosition(offset * 0x10);
            var array = new T[count];
            for (var i = 0; i < count; i++)
                array[i] = func(stream);

            return array;
        }

        public static VpuPacket Read(Stream stream) =>
            new VpuPacket(stream);

        public static VpuHeader Header(Stream stream) =>
            BinaryMapping.ReadObject<VpuHeader>(stream);
    }
}
