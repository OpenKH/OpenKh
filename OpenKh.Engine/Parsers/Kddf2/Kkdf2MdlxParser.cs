using OpenKh.Common;
using OpenKh.Engine.Maths;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class Kkdf2MdlxParser
    {
        public class CI
        {
            public int[] Indices;
            public int TextureIndex, SegmentIndex;
        }

        public List<CI> MeshDescriptors { get; } = new List<CI>();

        public SortedDictionary<int, Model> Models { get; } = new SortedDictionary<int, Model>();

        public Matrix[] Matrices;

        private readonly List<ImmutableMesh> immultableMeshList;

        /// <summary>
        /// Build immutable parts from a submodel.
        /// </summary>
        /// <param name="submodel"></param>
        public Kkdf2MdlxParser(Mdlx.SubModel submodel)
        {
            immultableMeshList = new List<ImmutableMesh>();
            foreach (Mdlx.DmaVif dmaVif in submodel.DmaChains.SelectMany(dmaChain => dmaChain.DmaVifs))
            {
                const int tops = 0x40, top2 = 0x220;

                var unpacker = new VifUnpacker(dmaVif.VifPacket)
                {
                    Vif1_Tops = tops
                };
                unpacker.Run();

                var mesh = VU1Simulation.Run(unpacker.Memory, Matrices, tops, top2, dmaVif.TextureIndex, dmaVif.Alaxi);
                immultableMeshList.Add(mesh);
            }
        }

        /// <summary>
        /// Build intial T-pose matrices.
        /// </summary>
        public Kkdf2MdlxParser BuildTPoseMatrices(Mdlx.SubModel model, Matrix initialMatrix)
        {
            var boneList = model.Bones.ToArray();
            Matrices = new Matrix[boneList.Length];
            {
                var absTranslationList = new Vector3[Matrices.Length];
                var absRotationList = new Quaternion[Matrices.Length];
                for (int x = 0; x < Matrices.Length; x++)
                {
                    Quaternion absRotation;
                    Vector3 absTranslation;
                    var oneBone = boneList[x];
                    var parent = oneBone.Parent;
                    if (parent < 0)
                    {
                        absRotation = Quaternion.Identity;
                        absTranslation = Vector3.Zero;
                    }
                    else
                    {
                        absRotation = absRotationList[parent];
                        absTranslation = absTranslationList[parent];
                    }

                    var localTranslation = Vector3.TransformCoordinate(new Vector3(oneBone.TranslationX, oneBone.TranslationY, oneBone.TranslationZ), Matrix.RotationQuaternion(absRotation));
                    absTranslationList[x] = absTranslation + localTranslation;

                    var localRotation = Quaternion.Identity;
                    if (oneBone.RotationX != 0) localRotation *= (Quaternion.RotationAxis(new Vector3(1, 0, 0), oneBone.RotationX));
                    if (oneBone.RotationY != 0) localRotation *= (Quaternion.RotationAxis(new Vector3(0, 1, 0), oneBone.RotationY));
                    if (oneBone.RotationZ != 0) localRotation *= (Quaternion.RotationAxis(new Vector3(0, 0, 1), oneBone.RotationZ));
                    absRotationList[x] = localRotation * absRotation;
                }
                for (int x = 0; x < Matrices.Length; x++)
                {
                    var absMatrix = initialMatrix;
                    absMatrix *= Matrix.RotationQuaternion(absRotationList[x]);
                    absMatrix *= Matrix.Translation(absTranslationList[x]);
                    Matrices[x] = absMatrix;
                }
            }

            return this;
        }

        /// <summary>
        /// Build final model using immutable parts and given matrices.
        /// </summary>
        /// <returns></returns>
        public Kkdf2MdlxParser ProcessVerticesAndBuildModel()
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
                            var triRef = new TriangleRef(mesh.textureIndex,
                                ringBuffer[(ringIndex - triangleOrder[0]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[1]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[2]) & 3]
                                );
                            exportedMesh.triangleRefList.Add(triRef);
                        }
                        if (flag == 0x30 || flag == 0x00)
                        {
                            var triRef = new TriangleRef(mesh.textureIndex,
                                ringBuffer[(ringIndex - triangleOrder[0]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[2]) & 3],
                                ringBuffer[(ringIndex - triangleOrder[1]) & 3]
                                );
                            exportedMesh.triangleRefList.Add(triRef);
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
                                    finalPos = Vector3.TransformCoordinate(
                                    VCUt.V4To3(
                                        vertexAssigns[0].rawPos
                                    ),
                                    Matrices[vertexAssigns[0].matrixIndex]
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
                                                Matrices[vertexAssign.matrixIndex]
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

                    vertexBaseIndex += mesh.vertexAssignmentsList.Length;
                    uvBaseIndex += mesh.indexAssignmentList.Length;
                }
            }

            {
                Models.Clear();

                int triangleRefCount = exportedMesh.triangleRefList.Count;
                for (int triIndex = 0; triIndex < triangleRefCount; triIndex++)
                {
                    TriangleRef triRef = exportedMesh.triangleRefList[triIndex];
                    Model model;
                    if (Models.TryGetValue(triRef.textureIndex, out model) == false)
                    {
                        Models[triRef.textureIndex] = model = new Model();
                    }
                    for (int i = 0; i < triRef.list.Length; i++)
                    {
                        VertexRef vertRef = triRef.list[i];
                        Vector3 pos = exportedMesh.positionList[vertRef.vertexIndex];
                        Vector2 uv = exportedMesh.uvList[vertRef.uvIndex];
                        model.Vertices.Add(new CustomVertex.PositionColoredTextured(pos, -1, uv.X, uv.Y));
                    }
                }
            }

            return this;
        }
    }
}
