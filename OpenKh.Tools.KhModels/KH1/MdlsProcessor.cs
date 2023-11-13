using ModelingToolkit.Objects;
using OpenKh.Kh1;
using System.Numerics;
using static OpenKh.Kh1.Mdls;

namespace OpenKh.Tools.KhModels.KH1
{
    public class MdlsProcessor
    {
        public static MtModel GetMtModel(Mdls mdls)
        {
            MtModel model = new MtModel();

            // Skeleton
            for (int i = 0; i < mdls.Joints.Count; i++)
            {
                MdlsJoint mdlsJoint = mdls.Joints[i];
                MtJoint joint = new MtJoint();
                joint.Name = "Bone" + i;
                joint.ParentId = (mdlsJoint.ParentId == 0x000003FF) ? null : (int)mdlsJoint.ParentId;
                joint.RelativeScale = new Vector3(mdlsJoint.ScaleX, mdlsJoint.ScaleY, mdlsJoint.ScaleZ);
                joint.RelativeRotation = new Vector3(mdlsJoint.RotateX, mdlsJoint.RotateY, mdlsJoint.RotateZ);
                joint.RelativeTranslation = new Vector3(mdlsJoint.TranslateX, mdlsJoint.TranslateY, mdlsJoint.TranslateZ);
                model.Joints.Add(joint);
            }
            model.CalculateFromRelativeData();

            // Meshes
            foreach (MdlsMesh mdlsMesh in mdls.Meshes)
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
            for(int i = 0; i < mdls.Images.Count; i++)
            {
                MdlsImage image = mdls.Images[i];
                MtMaterial material = new MtMaterial();
                material.Name = "Texture" + i;
                material.DiffuseTextureBitmap = image.bitmap;
                model.Materials.Add(material);
            }

            return model;
        }
    }
}
