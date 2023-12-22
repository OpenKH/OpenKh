using ModelingToolkit.Objects;
using OpenKh.Kh1;
using System.Numerics;
using static OpenKh.Kh1.Mdls;

namespace OpenKh.Tools.KhModels.KH1
{
    public class WpnProcessor
    {
        public static MtModel GetMtModel(Wpn wpn)
        {
            MtModel model = new MtModel();

            // Skeleton
            //for (int i = 0; i < wpn.ModelEnvFile.ModelHeader.JointCount; i++)
            //{
            //    MtJoint joint = new MtJoint();
            //    joint.ParentId = null;
            //    joint.RelativeScale = Vector3.One;
            //    joint.RelativeRotation = Vector3.Zero;
            //    joint.RelativeTranslation = Vector3.Zero;
            //    model.Joints.Add(joint);
            //}
            //model.CalculateFromRelativeData();

            AddBones(wpn, model);

            // Meshes
            foreach (MdlsMesh mdlsMesh in wpn.ModelEnvFile.Meshes)
            {
                MtMesh mesh = new MtMesh();
                mesh.MaterialId = mdlsMesh.Header.TextureIndex;

                for (int j = 0; j < mdlsMesh.packet.StripHeaders.Count; j++)
                {
                    int currentVertex = 0;
                    foreach (Mdls.MdlsVertex mdlsVertex in mdlsMesh.packet.TriangleStrips[j])
                    {
                        MtVertex vertex = new MtVertex();
                        MtWeightPosition weight = new MtWeightPosition();
                        weight.JointIndex = mdlsVertex.JointId;
                        weight.Weight = mdlsVertex.Weight;
                        weight.RelativePosition = new Vector3(mdlsVertex.TranslateX, mdlsVertex.TranslateY, mdlsVertex.TranslateZ);

                        vertex.Weights.Add(weight);

                        vertex.AbsolutePosition = Vector3.Transform(vertex.Weights[0].RelativePosition.Value, model.Joints[mdlsVertex.JointId].AbsoluteTransformationMatrix.Value);

                        vertex.TextureCoordinates = new Vector3(mdlsVertex.TexCoordU, 1 - mdlsVertex.TexCoordV, mdlsVertex.TexCoord1);

                        mesh.Vertices.Add(vertex);

                        currentVertex++;

                        if (currentVertex >= 3)
                        {
                            // Counterclockwise
                            if (mdlsMesh.packet.StripHeaders[j].Unknown == 0)
                            {
                                if (currentVertex % 2 == 0)
                                {
                                    MtFace face = new MtFace();
                                    face.VertexIndices.Add(mdlsVertex.Index);
                                    face.VertexIndices.Add(mdlsVertex.Index - 1);
                                    face.VertexIndices.Add(mdlsVertex.Index - 2);
                                    mesh.Faces.Add(face);
                                }
                                else
                                {
                                    MtFace face = new MtFace();
                                    face.VertexIndices.Add(mdlsVertex.Index);
                                    face.VertexIndices.Add(mdlsVertex.Index - 2);
                                    face.VertexIndices.Add(mdlsVertex.Index - 1);
                                    mesh.Faces.Add(face);
                                }
                            }
                            // Clockwise
                            else
                            {
                                if (currentVertex % 2 == 0)
                                {
                                    MtFace face = new MtFace();
                                    face.VertexIndices.Add(mdlsVertex.Index);
                                    face.VertexIndices.Add(mdlsVertex.Index - 2);
                                    face.VertexIndices.Add(mdlsVertex.Index - 1);
                                    mesh.Faces.Add(face);
                                }
                                else
                                {
                                    MtFace face = new MtFace();
                                    face.VertexIndices.Add(mdlsVertex.Index);
                                    face.VertexIndices.Add(mdlsVertex.Index - 1);
                                    face.VertexIndices.Add(mdlsVertex.Index - 2);
                                    mesh.Faces.Add(face);
                                }
                            }
                        }
                    }
                }
                model.Meshes.Add(mesh);
            }

            // Materials
            for (int i = 0; i < wpn.Images.Count; i++)
            {
                MdlsImage image = wpn.Images[i];
                MtMaterial material = new MtMaterial();
                material.Name = "Texture"+i;
                material.DiffuseTextureBitmap = image.bitmap;
                model.Materials.Add(material);
            }

            return model;
        }

        public static void AddBones(Wpn wpn, MtModel model)
        {
            // For Sora's Keyblades. Taken from Sora's last 13 bones
            if(wpn.ModelEnvFile.ModelHeader.JointCount == 13)
            {
                MtJoint joint0 = new MtJoint();
                joint0.Name = "Bone0";
                joint0.ParentId = null;
                joint0.RelativeScale = Vector3.One;
                joint0.RelativeRotation = Vector3.Zero;
                joint0.RelativeTranslation = Vector3.Zero;
                model.Joints.Add(joint0);

                MtJoint joint1 = new MtJoint();
                joint1.Name = "Bone1";
                joint1.ParentId = 0;
                joint1.RelativeScale = Vector3.One;
                joint1.RelativeRotation = Vector3.Zero;
                joint1.RelativeTranslation = Vector3.Zero;
                model.Joints.Add(joint1);

                MtJoint joint2 = new MtJoint();
                joint2.Name = "Bone2";
                joint2.ParentId = 1;
                joint2.RelativeScale = Vector3.One;
                joint2.RelativeRotation = new Vector3(3.1415927f, 0, 3.1415923f);
                joint2.RelativeTranslation = Vector3.Zero;
                model.Joints.Add(joint2);

                MtJoint joint3 = new MtJoint();
                joint3.Name = "Bone3";
                joint3.ParentId = 2;
                joint3.RelativeScale = Vector3.One;
                joint3.RelativeRotation = Vector3.Zero;
                joint3.RelativeTranslation = new Vector3(21.10731f, 0, 0);
                model.Joints.Add(joint3);

                MtJoint joint4 = new MtJoint();
                joint4.Name = "Bone4";
                joint4.ParentId = 3;
                joint4.RelativeScale = Vector3.One;
                joint4.RelativeRotation = Vector3.Zero;
                joint4.RelativeTranslation = new Vector3(6.72208f, 0, 0);
                model.Joints.Add(joint4);

                MtJoint joint5 = new MtJoint();
                joint5.Name = "Bone5";
                joint5.ParentId = 4;
                joint5.RelativeScale = Vector3.One;
                joint5.RelativeRotation = Vector3.Zero;
                joint5.RelativeTranslation = new Vector3(2.953814f, 0, 0);
                model.Joints.Add(joint5);

                MtJoint joint6 = new MtJoint();
                joint6.Name = "Bone6";
                joint6.ParentId = 5;
                joint6.RelativeScale = Vector3.One;
                joint6.RelativeRotation = Vector3.Zero;
                joint6.RelativeTranslation = new Vector3(2.907539f, 0, 0);
                model.Joints.Add(joint6);

                MtJoint joint7 = new MtJoint();
                joint7.Name = "Bone7";
                joint7.ParentId = 6;
                joint7.RelativeScale = Vector3.One;
                joint7.RelativeRotation = Vector3.Zero;
                joint7.RelativeTranslation = new Vector3(3.116349f, 0, 0);
                model.Joints.Add(joint7);

                MtJoint joint8 = new MtJoint();
                joint8.Name = "Bone8";
                joint8.ParentId = 7;
                joint8.RelativeScale = Vector3.One;
                joint8.RelativeRotation = Vector3.Zero;
                joint8.RelativeTranslation = new Vector3(2.979294f, 0, 0);
                model.Joints.Add(joint8);

                MtJoint joint9 = new MtJoint();
                joint9.Name = "Bone9";
                joint9.ParentId = 8;
                joint9.RelativeScale = Vector3.One;
                joint9.RelativeRotation = Vector3.Zero;
                joint9.RelativeTranslation = new Vector3(3.162624f, 0, 0);
                model.Joints.Add(joint9);

                MtJoint joint10 = new MtJoint();
                joint10.Name = "Bone10";
                joint10.ParentId = 9;
                joint10.RelativeScale = Vector3.One;
                joint10.RelativeRotation = Vector3.Zero;
                joint10.RelativeTranslation = new Vector3(2.999702f, 0, 0);
                model.Joints.Add(joint10);

                MtJoint joint11 = new MtJoint();
                joint11.Name = "Bone11";
                joint11.ParentId = 10;
                joint11.RelativeScale = Vector3.One;
                joint11.RelativeRotation = Vector3.Zero;
                joint11.RelativeTranslation = new Vector3(3.261963f, 0, 0);
                model.Joints.Add(joint11);

                MtJoint joint12 = new MtJoint();
                joint12.Name = "Bone12";
                joint12.ParentId = 11;
                joint12.RelativeScale = Vector3.One;
                joint12.RelativeRotation = Vector3.Zero;
                joint12.RelativeTranslation = new Vector3(7.035393f, 0, 0);
                model.Joints.Add(joint12);
            }
            else
            {
                for (int i = 0; i < wpn.ModelEnvFile.ModelHeader.JointCount; i++)
                {
                    MtJoint joint = new MtJoint();
                    joint.Name = "Bone" + i;
                    joint.ParentId = null;
                    joint.RelativeScale = Vector3.One;
                    joint.RelativeRotation = Vector3.Zero;
                    joint.RelativeTranslation = Vector3.Zero;
                    model.Joints.Add(joint);
                }
            }
            
            model.CalculateFromRelativeData();
        }
    }
}
