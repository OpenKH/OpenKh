using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Ps2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class Kkdf2MdlxParser
    {
        public class CI
        {
            public int[] Indices;
            public int TextureIndex, SegmentIndex;
            public bool IsOpaque;
        }

        public List<CI> MeshDescriptors { get; } = new List<CI>();

        private readonly List<ImmutableMesh> immultableMeshList;

        /// <summary>
        /// Build immutable parts from a submodel.
        /// </summary>
        /// <param name="submodel"></param>
        public Kkdf2MdlxParser(Mdlx.SubModel submodel)
        {
            immultableMeshList = new List<ImmutableMesh>();
            foreach (Mdlx.DmaChain dmaChain in submodel.DmaChains)
            {
                foreach (Mdlx.DmaVif dmaVif in dmaChain.DmaVifs)
                {
                    const int tops = 0x00;

                    var unpacker = new VifUnpacker(dmaVif.VifPacket)
                    {
                        Vif1_Tops = tops
                    };
                    unpacker.Run();

                    var mesh = VU1Simulation.Run(unpacker.Memory, tops, dmaVif.TextureIndex, dmaVif.Alaxi);
                    mesh.isOpaque = (dmaChain.RenderFlags & 1) == 0;
                    immultableMeshList.Add(mesh);
                }
            }
        }

        /// <summary>
        /// Build final model using immutable parts and given matrices.
        /// </summary>
        /// <returns></returns>
        public List<MeshDescriptor> ProcessVerticesAndBuildModel(Matrix4x4[] matrices)
        {
            var exportedMesh = new ExportedMesh();

            {
                int vertexBaseIndex = 0;
                int uvBaseIndex = 0;
                VertexRef[] ringBuffer = new VertexRef[4];
                int ringIndex = 0;
                int[] triangleOrder = new int[] { 1, 3, 2 };
                foreach (ImmutableMesh mesh in immultableMeshList)
                {
                    var part = new ExportedMesh.Part
                    {
                        meshRef = mesh,
                    };

                    for (int x = 0; x < mesh.indexAssignmentList.Length; x++)
                    {
                        var indexAssign = mesh.indexAssignmentList[x];

                        VertexRef vertexRef = new VertexRef(
                            vertexBaseIndex + indexAssign.indexToVertexAssignment,
                            uvBaseIndex + x
                        );
                        ringBuffer[ringIndex] = vertexRef;
                        ringIndex = (ringIndex + 1) & 3;
                        int flag = indexAssign.vertexFlag;
                        if (flag == 0x20 || flag == 0x00)
                        {
                            var triRef = new TriangleRef(
                                ringBuffer[(ringIndex - triangleOrder[0]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[1]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[2]) & 3]
                                );
                            part.triangleRefList.Add(triRef);
                        }
                        if (flag == 0x30 || flag == 0x00)
                        {
                            var triRef = new TriangleRef(
                                ringBuffer[(ringIndex - triangleOrder[0]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[2]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[1]) & 3]
                                );
                            part.triangleRefList.Add(triRef);
                        }
                    }

                    exportedMesh.positionList.AddRange(
                        mesh.vertexAssignmentsList.Select(
                            vertexAssigns =>
                            {
                                Vector3 finalPos = Vector3.Zero;
                                if (vertexAssigns.Length == 1)
                                {
                                    // single joint
                                    finalPos = Vector3.Transform(
                                        VCUt.V4To3(
                                            vertexAssigns[0].rawPos
                                        ),
                                        matrices[vertexAssigns[0].matrixIndex]
                                    );
                                }
                                else
                                {
                                    // multiple joints, using rawPos.W as blend weights
                                    foreach (VertexAssignment vertexAssign in vertexAssigns)
                                    {
                                        finalPos += VCUt.V4To3(
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
                        mesh.indexAssignmentList
                            .Select(indexAssign => indexAssign.uv)
                    );

                    exportedMesh.partList.Add(part);

                    vertexBaseIndex += mesh.vertexAssignmentsList.Length;
                    uvBaseIndex += mesh.indexAssignmentList.Length;
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
                        IsOpaque = part.meshRef.isOpaque,
                        TextureIndex = part.meshRef.textureIndex,
                        Vertices = vertices.ToArray(),
                        Indices = indices.ToArray(),
                    }
                );
            }

            return newList;
        }

        public IEnumerable<ImmutableMesh> GetUnprocessedMeshList() => new ReadOnlyCollection<ImmutableMesh>(immultableMeshList);
    }
}
