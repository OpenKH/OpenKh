using OpenKh.Ps2;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Engine
{
    public class MdlxLoaderTests
    {
        [Fact]
        public void NEW_ShouldGetImmutableMesh()
        {
            var alaxi = new int[]
            {
                0x35, 0x2, 0x18, 0x29, 0x0c, 0x31, 0x3f, 0x14, 0x22, 0x16
            };

            var data = File.ReadAllBytes("res/obj_vif_unpack.bin");
            var vpu = VpuPacket.Read(new MemoryStream(data));
            var immutableMesh = vpu.GetWeightedVertices(vpu.GetFromMatrixIndices(alaxi));
            var weightedVertices = immutableMesh
                .Select(x => x.Select(y => new
                {
                    y.MatrixIndex,
                    Vertex = vpu.Vertices[y.VertexIndex]
                }).ToArray())
                .ToArray();

            Assert.NotNull(immutableMesh);
            Assert.Equal(0x4c, immutableMesh.Length);
            Assert.Equal(0x4f, vpu.Indices.Length);

            Assert.Equal(0x35, weightedVertices[0][0].MatrixIndex);
            AssertEqual(weightedVertices[0][0].Vertex,
                11.55658f, -10.695402f, -1.8525791f, 0);

            Assert.Equal(0x35, weightedVertices[1][0].MatrixIndex);
            AssertEqual(weightedVertices[1][0].Vertex,
                12.341873f, -6.8589644f, -6.79047f, 0);

            Assert.Equal(0x16, weightedVertices[75][0].MatrixIndex);
            AssertEqual(weightedVertices[75][0].Vertex,
                -0.29302156f, -0.004016876f, 13.689127f, 0);

            AssertEqual(vpu.Indices[0], 0x10, 0x27, 0f, 0.26171875f);
            AssertEqual(vpu.Indices[1], 0x10, 0x28, 0.02734375f, 0.24609375f);
            AssertEqual(vpu.Indices[78], 0x30, 0x36, 0.5234375f, 0.49609375f);
        }

        [Fact]
        public void NEW_ShouldGetImmutableMeshHighDef()
        {
            var alaxi = new int[]
            {
                0x43, 0x8c, 0xdb, 0xd9, 0x89, 0x42, 0x41, 0x02, 0xd7, 0xd3
            };

            var data = File.ReadAllBytes("res/obj_vif_unpack_alt.bin");
            var vpu = VpuPacket.Read(new MemoryStream(data));
            var immutableMesh = vpu.GetWeightedVertices(vpu.GetFromMatrixIndices(alaxi));
            var weightedVertices = immutableMesh
                .Select(x => x.Select(y => new
                {
                    y.MatrixIndex,
                    Vertex = vpu.Vertices[y.VertexIndex]
                }).ToArray())
                .ToArray();

            Assert.NotNull(immutableMesh);
            Assert.Equal(0x4a, immutableMesh.Length);
            Assert.Equal(0x4f, vpu.Indices.Length);

            Assert.Equal(0x89, weightedVertices[0][0].MatrixIndex);
            AssertEqual(weightedVertices[0][0].Vertex,
                6.9705353f, 0.5387306f, -4.146785f, 1f);

            Assert.Equal(0x89, weightedVertices[1][0].MatrixIndex);
            AssertEqual(weightedVertices[1][0].Vertex,
                6.1054993f, 1.1867695f, -3.7489133f, 1f);

            Assert.Equal(0x8c, weightedVertices[73][0].MatrixIndex);
            AssertEqual(weightedVertices[73][0].Vertex,
                9.300911f, -0.66315675f, -5.8931007f, 0.5f);

            Assert.Equal(0xd3, weightedVertices[73][1].MatrixIndex);
            AssertEqual(weightedVertices[73][1].Vertex,
                3.4593887f, -1.39147f, -1.8666306f, 0.5f);
        }

        private static void AssertEqual(VpuPacket.VertexIndex v,
            int expectedVertexFlag,
            int expectedIndexToVertexAssignment,
            float expectedU,
            float expectedV)
        {
            Assert.Equal(expectedVertexFlag, (int)v.Function);
            Assert.Equal(expectedIndexToVertexAssignment, v.Index);
            Assert.Equal(expectedU, v.U / 16 / 256.0f);
            Assert.Equal(expectedV, v.V / 16 / 256.0f);
        }

        private static void AssertEqual(
            VpuPacket.VertexCoord v,
            float expectedX,
            float expectedY,
            float expectedZ,
            float expectedW)
        {
            Assert.Equal(expectedX, v.X);
            Assert.Equal(expectedY, v.Y);
            Assert.Equal(expectedZ, v.Z);
            Assert.Equal(expectedW, v.W);
        }
    }
}
