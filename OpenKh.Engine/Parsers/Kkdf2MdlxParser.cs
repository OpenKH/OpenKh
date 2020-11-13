using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Ps2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenKh.Engine.Parsers
{
    public class Kkdf2MdlxParser
    {
        private class ImmutableMesh
        {
            public Mdlx.DmaChain DmaChain { get; }
            public List<VpuPacket> VpuPackets { get; }

            public int TextureIndex => DmaChain.TextureIndex;
            public bool IsOpaque => (DmaChain.RenderFlags & 1) == 0;

            public ImmutableMesh(Mdlx.DmaChain dmaChain)
            {
                DmaChain = dmaChain;
                VpuPackets = dmaChain.DmaVifs
                    .Select(dmaVif =>
                    {
                        var unpacker = new VifUnpacker(dmaVif.VifPacket);
                        unpacker.Run();

                        using (var stream = new MemoryStream(unpacker.Memory))
                            return VpuPacket.Read(stream);
                    })
                    .ToList();
            }
        }

        private class VertexAssignment
        {
            public int matrixIndex;
            public float weight = 1f;
            public Vector4 rawPos;
        }

        private class VertexRef
        {
            public int vertexIndex, uvIndex;

            public VertexRef(int vertexIndex, int uvIndex)
            {
                this.vertexIndex = vertexIndex;
                this.uvIndex = uvIndex;
            }
        }

        private class TriangleRef
        {
            public TriangleRef(VertexRef one, VertexRef two, VertexRef three)
            {
                list = new VertexRef[] { one, two, three };
            }

            public VertexRef[] list;
            public int textureIndex;
            public bool isOpaque;
        }

        private class ExportedMesh
        {
            public class Part
            {
                public int TextureIndex;
                public bool IsOpaque;
                public List<TriangleRef> triangleRefList = new List<TriangleRef>();
            }

            public List<Part> partList = new List<Part>();
            public List<Vector3> positionList = new List<Vector3>();
            public List<Vector2> uvList = new List<Vector2>();
        }

        private readonly List<ImmutableMesh> immultableMeshList;

        /// <summary>
        /// Build immutable parts from a submodel.
        /// </summary>
        /// <param name="submodel"></param>
        public Kkdf2MdlxParser(Mdlx.SubModel submodel)
        {
            immultableMeshList = submodel.DmaChains
                .Select(x => new ImmutableMesh(x))
                .ToList();
        }

        /// <summary>
        /// Build final model using immutable parts and given matrices.
        /// </summary>
        /// <returns></returns>
        public List<MeshDescriptor> ProcessVerticesAndBuildModel(Matrix4x4[] matrices)
        {
            var exportedMesh = new ExportedMesh();

            int vertexBaseIndex = 0;
            int uvBaseIndex = 0;
            VertexRef[] ringBuffer = new VertexRef[4];
            int ringIndex = 0;
            int[] triangleOrder = new int[] { 1, 3, 2 };
            foreach (ImmutableMesh meshRoot in immultableMeshList)
            {
                for (int i = 0; i < meshRoot.VpuPackets.Count; i++)
                {
                    VpuPacket mesh = meshRoot.VpuPackets[i];
                    var part = new ExportedMesh.Part
                    {
                        TextureIndex = meshRoot.TextureIndex,
                        IsOpaque = meshRoot.IsOpaque,
                    };

                    for (int x = 0; x < mesh.Indices.Length; x++)
                    {
                        var indexAssign = mesh.Indices[x];

                        VertexRef vertexRef = new VertexRef(
                            vertexBaseIndex + indexAssign.Index,
                            uvBaseIndex + x
                        );
                        ringBuffer[ringIndex] = vertexRef;
                        ringIndex = (ringIndex + 1) & 3;
                        var flag = indexAssign.Function;
                        if (flag == VpuPacket.VertexFunction.DrawTriangle ||
                            flag == VpuPacket.VertexFunction.DrawTriangleDoubleSided)
                        {
                            var triRef = new TriangleRef(
                                ringBuffer[(ringIndex - triangleOrder[0]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[1]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[2]) & 3]
                                );
                            part.triangleRefList.Add(triRef);
                        }
                        if (flag == VpuPacket.VertexFunction.DrawTriangleInverse ||
                            flag == VpuPacket.VertexFunction.DrawTriangleDoubleSided)
                        {
                            var triRef = new TriangleRef(
                                ringBuffer[(ringIndex - triangleOrder[0]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[2]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[1]) & 3]
                                );
                            part.triangleRefList.Add(triRef);
                        }
                    }

                    var matrixIndexList = meshRoot.DmaChain.DmaVifs[i].Alaxi;
                    var vertexIndex = 0;
                    var vertexAssignmentList = new VertexAssignment[mesh.Vertices.Length];
                    for (var indexToMatrixIndex = 0; indexToMatrixIndex < mesh.VertexRange.Length; indexToMatrixIndex++)
                    {
                        var verticesCount = mesh.VertexRange[indexToMatrixIndex];
                        for (var t = 0; t < verticesCount; t++)
                        {
                            var vertex = mesh.Vertices[vertexIndex];
                            vertexAssignmentList[vertexIndex++] = new VertexAssignment
                            {
                                matrixIndex = matrixIndexList[indexToMatrixIndex],
                                weight = vertex.W,
                                rawPos = new Vector4(vertex.X, vertex.Y, vertex.Z, vertex.W)
                            };
                        };
                    }

                    var vertexAssignmentsList = vertexAssignmentList
                        .Select(x => new VertexAssignment[] { x })
                        .ToArray();

                    exportedMesh.positionList.AddRange(
                        vertexAssignmentsList.Select(
                            vertexAssigns =>
                            {
                                Vector3 finalPos = Vector3.Zero;
                                if (vertexAssigns.Length == 1)
                                {
                                    // single joint
                                    finalPos = Vector3.Transform(
                                            ToVector3(vertexAssigns[0].rawPos),
                                            matrices[vertexAssigns[0].matrixIndex]
                                        );
                                }
                                else
                                {
                                    // multiple joints, using rawPos.W as blend weights
                                    foreach (VertexAssignment vertexAssign in vertexAssigns)
                                    {
                                        finalPos += ToVector3(
                                            Vector4.Transform(
                                                vertexAssign.rawPos,
                                                matrices[vertexAssign.matrixIndex]
                                            )
                                        );
                                    }
                                }
                                return finalPos;
                            }
                        )
                    );

                    exportedMesh.uvList.AddRange(
                        mesh.Indices.Select(x =>
                            new Vector2(x.U / 16 / 256.0f, x.V / 16 / 256.0f))
                    );

                    exportedMesh.partList.Add(part);

                    vertexBaseIndex += vertexAssignmentsList.Length;
                    uvBaseIndex += mesh.Indices.Length;
                }
            }

            var newList = new List<MeshDescriptor>();

            foreach (var part in exportedMesh.partList)
            {
                var vertices = new List<CustomVertex.PositionColoredTextured>();
                var indices = new List<int>();

                int triangleRefCount = part.triangleRefList.Count;
                for (int triIndex = 0; triIndex < triangleRefCount; triIndex++)
                {
                    TriangleRef triRef = part.triangleRefList[triIndex];
                    for (int i = 0; i < triRef.list.Length; i++)
                    {
                        VertexRef vertRef = triRef.list[i];
                        Vector3 pos = exportedMesh.positionList[vertRef.vertexIndex];
                        Vector2 uv = exportedMesh.uvList[vertRef.uvIndex];
                        indices.Add(vertices.Count);
                        vertices.Add(new CustomVertex.PositionColoredTextured(pos, -1, uv.X, uv.Y));
                    }
                }

                newList.Add(
                    new MeshDescriptor
                    {
                        IsOpaque = part.IsOpaque,
                        TextureIndex = part.TextureIndex,
                        Vertices = vertices.ToArray(),
                        Indices = indices.ToArray(),
                    }
                );
            }

            return newList;
        }

        private static Vector3 ToVector3(Vector4 pos) => new Vector3(pos.X, pos.Y, pos.Z);
    }
}
