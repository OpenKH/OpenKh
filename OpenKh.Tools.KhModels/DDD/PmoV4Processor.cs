using ModelingToolkit.Objects;
using OpenKh.Ddd;
using System.Collections.Generic;
using System.Numerics;
using static OpenKh.Ddd.PmoV4_2;

namespace OpenKh.Tools.KhModels.DDD
{
    public class PmoV4Processor
    {
        public static MtModel GetMtModel(PmoV4 pmo)
        {
            MtModel model = new MtModel();

            // Skeleton
            if(pmo.boneList != null)
            {
                for (int i = 0; i < pmo.boneList.Length; i++)
                {
                    PmoV4.BoneData pmoBone = pmo.boneList[i];
                    MtJoint joint = new MtJoint();
                    joint.Name = pmoBone.JointName;
                    joint.ParentId = (pmoBone.ParentBoneIndex == 65535) ? null : pmoBone.ParentBoneIndex;
                    //joint.AbsoluteTransformationMatrix = iBone.Transform;
                    joint.RelativeTransformationMatrix = pmoBone.Transform;
                    model.Joints.Add(joint);
                }
                model.BuildBoneDataFromRelativeMatrices();
            }

            // Meshes
            if(pmo.Model0.Meshes != null)
            {
                for (int i = 0; i < pmo.Model0.Meshes.Count; i++)
                {
                    PmoV4.MeshChunks pmoMesh = pmo.Model0.Meshes[i];
                    MtMesh mesh = new MtMesh();
                    mesh.Name = "Mesh" + i;
                    mesh.MaterialId = pmoMesh.TextureID;

                    for (int j = 0; j < pmoMesh.vertices.Count; j++)
                    {
                        MtVertex vertex = new MtVertex();
                        //MtWeightPosition weight = new MtWeightPosition();
                        //weight.RelativePosition = pmoMesh.vertices[j];
                        //weight.Weight = pmoMesh.jointWeights[j].weights[0];
                        vertex.AbsolutePosition = pmoMesh.vertices[j];
                        vertex.AbsolutePosition *= pmo.header.ModelScale;
                        vertex.TextureCoordinates = new Vector3(pmoMesh.textureCoordinates[j].X, 1 - pmoMesh.textureCoordinates[j].Y, 1);
                        mesh.Vertices.Add(vertex);
                    }

                    for (int j = 0; j < pmoMesh.Indices.Count; j += 3)
                    {
                        MtFace face = new MtFace();
                        face.VertexIndices = new List<int> { pmoMesh.Indices[j], pmoMesh.Indices[j + 1], pmoMesh.Indices[j + 2] };
                        mesh.Faces.Add(face);
                    }

                    model.Meshes.Add(mesh);
                }
            }

            // Materials
            for (int i = 0; i < pmo.Textures.Count; i++)
            {
                MtMaterial material = new MtMaterial();
                material.Name = "Texture" + i;
                material.DiffuseTextureFileName = "Texture" + i + ".png";
                //material.DiffuseTextureBitmap = GetBitmap(image);
                model.Materials.Add(material);
            }

            return model;
        }

        public static MtModel GetMtModel(PmoV4_2 pmo)
        {
            MtModel model = new MtModel();

            // Skeleton
            if (pmo.Skel != null)
            {
                for (int i = 0; i < pmo.Skel.Bones.Count; i++)
                {
                    PmoV4_2.BoneData pmoBone = pmo.Skel.Bones[i];
                    MtJoint joint = new MtJoint();
                    joint.Name = pmoBone.JointName;
                    joint.ParentId = (pmoBone.ParentBoneIndex == 65535) ? null : pmoBone.ParentBoneIndex;
                    //joint.AbsoluteTransformationMatrix = iBone.Transform;
                    joint.RelativeTransformationMatrix = pmoBone.Transform;
                    model.Joints.Add(joint);
                }
                model.BuildBoneDataFromRelativeMatrices();
            }

            List<MeshPartition> allMeshPartitions = new List<MeshPartition>();
            allMeshPartitions.AddRange(pmo.MeshPartitionList1);
            allMeshPartitions.AddRange(pmo.MeshPartitionList2);

            // Meshes
            for (int i = 0; i < allMeshPartitions.Count; i++)
            {
                MeshPartition partition = allMeshPartitions[i];
                MtMesh mesh = new MtMesh();
                mesh.Name = "Mesh" + i;
                mesh.MaterialId = partition.Header.TextureID;

                foreach(PmoV4_2.Vertex pmoVertex in partition.Vertices)
                {
                    MtVertex vertex = new MtVertex();

                    vertex.AbsolutePosition = new Vector3(pmoVertex.PositionX, pmoVertex.PositionY, pmoVertex.PositionZ);
                    vertex.AbsolutePosition *= pmo.Header.ModelScale;
                    vertex.TextureCoordinates = new Vector3(pmoVertex.TexCoordU, 1 - pmoVertex.TexCoordV, 1);

                    if(pmoVertex.VWeight != null)
                    {
                        if (pmoVertex.VWeight.Weight0 > 0)
                        {
                            MtWeightPosition weight = new MtWeightPosition();
                            weight.JointIndex = partition.Header.BoneIndices[pmoVertex.VWeight.Joint0 / 3];
                            weight.Weight = pmoVertex.VWeight.Weight0 / 128f;
                            vertex.Weights.Add(weight);
                        }
                        if (pmoVertex.VWeight.Weight1 > 0)
                        {
                            MtWeightPosition weight = new MtWeightPosition();
                            weight.JointIndex = partition.Header.BoneIndices[pmoVertex.VWeight.Joint1 / 3];
                            weight.Weight = pmoVertex.VWeight.Weight1 / 128f;
                            vertex.Weights.Add(weight);
                        }
                        if (pmoVertex.VWeight.Weight2 > 0)
                        {
                            MtWeightPosition weight = new MtWeightPosition();
                            weight.JointIndex = partition.Header.BoneIndices[pmoVertex.VWeight.Joint2 / 3];
                            weight.Weight = pmoVertex.VWeight.Weight2 / 128f;
                            vertex.Weights.Add(weight);
                        }
                        if (pmoVertex.VWeight.Weight3 > 0)
                        {
                            MtWeightPosition weight = new MtWeightPosition();
                            weight.JointIndex = partition.Header.BoneIndices[pmoVertex.VWeight.Joint3 / 3];
                            weight.Weight = pmoVertex.VWeight.Weight3 / 128f;
                            vertex.Weights.Add(weight);
                        }
                    }

                    mesh.Vertices.Add(vertex);
                }

                if (partition.Header.isTriangleStrip)
                {
                    for (int j = 0; j < partition.Vertices.Count - 2; j++)
                    {
                        if (partition.Vertices[j].ColorA == 0x00 || partition.Vertices[j + 1].ColorA == 0x00 || partition.Vertices[j + 2].ColorA == 0x00)
                            continue;
                
                        MtFace face = new MtFace();
                        if(j % 2 == 0)
                        {
                            face.VertexIndices = new List<int> { j, j + 1, j + 2 };
                        }
                        else
                        {
                            face.VertexIndices = new List<int> { j + 1, j, j + 2 };
                        }
                        mesh.Faces.Add(face);
                    }
                }
                else
                {
                    for (int j = 0; j < partition.Vertices.Count - 2; j += 3)
                    {
                        MtFace face = new MtFace();
                        face.VertexIndices = new List<int> { j, j + 1, j + 2 };
                        mesh.Faces.Add(face);
                    }
                }

                model.Meshes.Add(mesh);
            }

            // Materials
            for (int i = 0; i < pmo.Materials.Count; i++)
            {
                MtMaterial material = new MtMaterial();
                material.Name = "Texture" + i;
                material.DiffuseTextureFileName = "Texture" + i + ".png";
                //material.DiffuseTextureBitmap = GetBitmap(image);
                model.Materials.Add(material);
            }

            return model;
        }
    }
}
