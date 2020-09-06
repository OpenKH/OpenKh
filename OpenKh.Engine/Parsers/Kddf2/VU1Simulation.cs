using OpenKh.Common;
using OpenKh.Ps2;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class VU1Simulation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrixIndexList">The limited count of matrices are transferred to VU1 due to memory limitation.</param>
        /// <returns></returns>
        public static ImmutableMesh Run(byte[] vu1mem, int tops, int textureIndex, int[] matrixIndexList)
        {
            var si = new MemoryStream(vu1mem, true);
            var br = new BinaryReader(si);

            si.Position = tops * 0x10;

            var vpu = VpuPacket.Header(si);

            if (vpu.Type != 1 && vpu.Type != 2) throw new ProtInvalidTypeException();

            int v04 = vpu.Unknown04;
            int v08 = vpu.Unknown08;
            int v0c = vpu.Unknown1cLocation;
            int offMatrices = vpu.Unknown1cLocation; // off matrices

            // Shady logic here???? It MIGHT cause problems:
            // If vpu.Type != 1, those 0x10 bytes will be skipped. Therefore,
            // stuff that it's at 0x40, will be at 0x30 instead.
            int cntvertscolor = (vpu.Type == 1) ? vpu.ColorCount : 0;
            int offvertscolor = (vpu.Type == 1) ? vpu.ColorLocation : 0;

            int v38 = vpu.Unknown38; // 

            si.Position = (vpu.UnkBoxLocation + tops) * 0x10;
            var vertexCountPerMatrix = Enumerable.Range(0, vpu.UnkBoxCount)
                .Select(x => si.ReadInt32())
                .ToList();
            Debug.Assert(vpu.VertexCount == vertexCountPerMatrix.Sum(x => x));

            var mesh = new ImmutableMesh
            {
                textureIndex = textureIndex
            };

            var vertexAssignmentList = new VertexAssignment[vpu.VertexCount];

            int vertexIndex = 0;
            si.Position = (vpu.VertexLocation + tops) * 0x10;
            for (var indexToMatrixIndex = 0; indexToMatrixIndex < vertexCountPerMatrix.Count; indexToMatrixIndex++)
            {
                var verticesCount = vertexCountPerMatrix[indexToMatrixIndex];
                for (var t = 0; t < verticesCount; t++)
                {
                    var vertex = ReadVector4(br);
                    vertexAssignmentList[vertexIndex++] = new VertexAssignment
                    {
                        matrixIndex = matrixIndexList[indexToMatrixIndex],
                        weight = vertex.W,
                        rawPos = vertex,
                    };
                }
            }
            mesh.vertexAssignmentsList = vertexAssignmentList
                .Select(x => new VertexAssignment[] { x })
                .ToArray();

            si.Position = (vpu.IndexLocation + tops) * 0x10;
            mesh.indexAssignmentList = Enumerable.Range(0, vpu.IndexCount)
                .Select(x => new IndexAssignment(
                    new Vector2(br.ReadInt32() / 16 / 256.0f, br.ReadInt32() / 16 / 256.0f),
                    br.ReadInt32(),
                    br.ReadInt32()))
                .ToArray();

            if (vpu.Type == 1 && vpu.VertexMixerCount > 0)
            {
                si.Position = (vpu.VertexMixerOffset + tops) * 0x10;
                var mixerCounts = Enumerable.Range(0, vpu.VertexMixerCount)
                    .Select(x => br.ReadInt32()).ToList();

                var newVertexAssignList = new VertexAssignment[mixerCounts.Sum()][];
                var inputVertexIndex = 0;

                for (var i = 0; i < mixerCounts.Count; i++)
                {
                    si.Position = (si.Position + 15) & (~15);
                    for (var j = 0; j < mixerCounts[i]; j++)
                    {
                        newVertexAssignList[inputVertexIndex++] = Enumerable
                            .Range(0, i + 1)
                            .Select(x => vertexAssignmentList[br.ReadInt32()])
                            .ToArray();
                    }
                }

                mesh.vertexAssignmentsList = newVertexAssignList;
            }

            return mesh;
        }

        private static Vector4 ReadVector4(BinaryReader reader) => new Vector4(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle());

    }
}
