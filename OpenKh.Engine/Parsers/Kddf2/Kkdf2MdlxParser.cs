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

        public Kkdf2MdlxParser(List<Mdlx.SubModel> submodels)
        {
            const float globalScale = 1f;

            foreach (var submodel in submodels)
            {
                var boneList = submodel.Bones.ToArray();
                var matrices = new Matrix[boneList.Length];
                {
                    var absTranslationList = new Vector3[matrices.Length];
                    var absRotationList = new Quaternion[matrices.Length];
                    for (int x = 0; x < matrices.Length; x++)
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
                    for (int x = 0; x < matrices.Length; x++)
                    {
                        var absMatrix = Matrix.RotationQuaternion(absRotationList[x]);
                        absMatrix *= Matrix.Translation(absTranslationList[x]);
                        matrices[x] = absMatrix;
                    }
                }

                var middleMeshList = new List<MiddleMesh>();
                var projectionMatrix = Matrix.Identity;
                foreach (Mdlx.DmaVif dmaVif in submodel.DmaChains.SelectMany(dmaChain => dmaChain.DmaVifs))
                {
                    const int tops = 0x40, top2 = 0x220;

                    var unpacker = new VifUnpacker(dmaVif.VifPacket)
                    {
                        Vif1_Tops = tops
                    };
                    unpacker.Run();

                    var middleMesh = VU1Simulation.Run(unpacker.Memory, matrices, tops, top2, dmaVif.TextureIndex, dmaVif.Alaxi, projectionMatrix);
                    middleMeshList.Add(middleMesh);
                }

                if (true)
                {
                    ExportedMesh exportedMesh = new ExportedMesh();
                    if (true)
                    {
                        int svi = 0;
                        int sti = 0;
                        VertexRef[] ringBuffer = new VertexRef[4];
                        int ringIndex = 0;
                        int[] triangleOrder = new int[] { 1, 3, 2 };
                        foreach (MiddleMesh mesh in middleMeshList)
                        {
                            for (int x = 0; x < mesh.vertexIndexMappingList.Length; x++)
                            {
                                VertexRef vertexRef = new VertexRef(svi + mesh.vertexIndexMappingList[x], sti + x);
                                ringBuffer[ringIndex] = vertexRef;
                                ringIndex = (ringIndex + 1) & 3;
                                int flag = mesh.vertexFlagList[x];
                                if (flag == 0x20 || flag == 0x00)
                                {
                                    TriangleRef triRef = new TriangleRef(mesh.textureIndex,
                                        ringBuffer[(ringIndex - triangleOrder[0]) & 3],
                                        ringBuffer[(ringIndex - triangleOrder[1]) & 3],
                                        ringBuffer[(ringIndex - triangleOrder[2]) & 3]
                                        );
                                    exportedMesh.triangleRefList.Add(triRef);
                                }
                                if (flag == 0x30 || flag == 0x00)
                                {
                                    TriangleRef triRef = new TriangleRef(mesh.textureIndex,
                                        ringBuffer[(ringIndex - triangleOrder[0]) & 3],
                                        ringBuffer[(ringIndex - triangleOrder[2]) & 3],
                                        ringBuffer[(ringIndex - triangleOrder[1]) & 3]
                                        );
                                    exportedMesh.triangleRefList.Add(triRef);
                                }
                            }
                            for (int x = 0; x < mesh.rawPositionList.Length; x++)
                            {
                                if (mesh.assignedJointsList[x] == null)
                                {
                                    exportedMesh.positionList.Add(Vector3.Zero);
                                    exportedMesh.jointList.Add(new JointAssignment[0]);
                                    continue;
                                }
                                if (mesh.assignedJointsList[x].Length == 1)
                                {
                                    JointAssignment assignment = mesh.assignedJointsList[x][0];
                                    assignment.factor = 1.0f;
                                    Vector3 finalPos = Vector3.TransformCoordinate(VCUt.V4To3(mesh.rawPositionList[assignment.vertexIndex]), matrices[assignment.matrixIndex]);
                                    exportedMesh.positionList.Add(finalPos);
                                }
                                else
                                {
                                    Vector3 finalPos = Vector3.Zero;
                                    foreach (JointAssignment assignment in mesh.assignedJointsList[x])
                                    {
                                        finalPos += VCUt.V4To3(Vector4.Transform(mesh.rawPositionList[assignment.vertexIndex], matrices[assignment.matrixIndex]));
                                    }
                                    exportedMesh.positionList.Add(finalPos);
                                }
                                exportedMesh.jointList.Add(mesh.assignedJointsList[x]);
                            }
                            for (int x = 0; x < mesh.uvList.Length; x++)
                            {
                                Vector2 vst = mesh.uvList[x];
                                vst.Y = 1.0f - vst.Y; // !
                                exportedMesh.uvList.Add(vst);
                            }
                            svi += mesh.rawPositionList.Length;
                            sti += mesh.uvList.Length;
                        }
                    }

                    {   // position xyz
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
                                Vector3 pos = (exportedMesh.positionList[vertRef.vertexIndex] * globalScale);
                                Vector2 uv = exportedMesh.uvList[vertRef.uvIndex];
                                model.Vertices.Add(new CustomVertex.PositionColoredTextured(pos, -1, uv.X, 1 - uv.Y));
                            }
                        }
                    }
                }
                break;
            }
        }
    }
}
