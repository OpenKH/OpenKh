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

            public List<Vector4[]> vertices;
            public List<VertexIndexWeighted[][]> vertexAssignments;
        }

        private readonly List<ImmutableMesh> immultableMeshList;
        private readonly ExportedMesh immutableExportedMesh;

        /// <summary>
        /// Build immutable parts from a submodel.
        /// </summary>
        /// <param name="submodel"></param>
        public Kkdf2MdlxParser(Mdlx.SubModel submodel)
        {
            immultableMeshList = submodel.DmaChains
                .Select(x => new ImmutableMesh(x))
                .ToList();

            immutableExportedMesh = PreProcessVerticesAndBuildModel();
        }

        private ExportedMesh PreProcessVerticesAndBuildModel()
        {
            var exportedMesh = new ExportedMesh();
            exportedMesh.vertexAssignments = new List<VertexIndexWeighted[][]>();
            exportedMesh.vertices = new List<Vector4[]>();

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

                    var vertices = mesh.Vertices
                        .Select(vertex => new Vector4(vertex.X, vertex.Y, vertex.Z, vertex.W))
                        .ToArray();
                    exportedMesh.vertices.Add(vertices);

                    var matrixIndexList = meshRoot.DmaChain.DmaVifs[i].Alaxi;
                    var vertexAssignmentsList = mesh.GetWeightedVertices(mesh.GetFromMatrixIndices(matrixIndexList));
                    exportedMesh.vertexAssignments.Add(vertexAssignmentsList);

                    exportedMesh.uvList.AddRange(
                        mesh.Indices.Select(x =>
                            new Vector2(x.U / 16 / 256.0f, x.V / 16 / 256.0f))
                    );

                    exportedMesh.partList.Add(part);

                    vertexBaseIndex += vertexAssignmentsList.Length;
                    uvBaseIndex += mesh.Indices.Length;
                }
            }

            return exportedMesh;
        }

        public List<MeshDescriptor> ProcessVerticesAndBuildModel(Matrix4x4[] matrices)
        {
            immutableExportedMesh.positionList.Clear();
            for (var i = 0; i < immutableExportedMesh.vertexAssignments.Count; i++)
            {
                var vertexAssignments = immutableExportedMesh.vertexAssignments[i];
                var vertices = immutableExportedMesh.vertices[i];

                immutableExportedMesh.positionList.AddRange(
                    vertexAssignments.Select(
                        vertexAssigns =>
                        {
                            Vector3 finalPos = Vector3.Zero;
                            if (vertexAssigns.Length == 1)
                            {
                                // single joint
                                finalPos = Vector3.Transform(
                                ToVector3(vertices[vertexAssigns[0].VertexIndex]),
                                matrices[vertexAssigns[0].MatrixIndex]);
                            }
                            else
                            {
                                // multiple joints, using rawPos.W as blend weights
                                foreach (var vertexAssign in vertexAssigns)
                                {
                                    finalPos += ToVector3(
                                        Vector4.Transform(
                                            vertices[vertexAssign.VertexIndex],
                                            matrices[vertexAssign.MatrixIndex]
                                        ));
                                }
                            }
                            return finalPos;
                        }
                    )
                );
            }

            var newList = new List<MeshDescriptor>();
            foreach (var part in immutableExportedMesh.partList)
            {
                var vertices = new List<PositionColoredTextured>();
                var indices = new List<int>();

                int triangleRefCount = part.triangleRefList.Count;
                for (int triIndex = 0; triIndex < triangleRefCount; triIndex++)
                {
                    TriangleRef triRef = part.triangleRefList[triIndex];
                    for (int i = 0; i < triRef.list.Length; i++)
                    {
                        VertexRef vertRef = triRef.list[i];
                        Vector3 pos = immutableExportedMesh.positionList[vertRef.vertexIndex];
                        Vector2 uv = immutableExportedMesh.uvList[vertRef.uvIndex];
                        indices.Add(vertices.Count);
                        vertices.Add(new PositionColoredTextured(pos, -1, uv.X, uv.Y));
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
