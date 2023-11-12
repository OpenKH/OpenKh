using ModelingToolkit.Objects;
using OpenKh.AssimpUtils;
using OpenKh.Engine.Extensions;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace OpenKh.Tools.KhModels.KIH2
{
    public class MdlxProcessor
    {
        public static MtModel GetMtModel(Bar barFile)
        {
            MtModel model = new MtModel();

            ModelSkeletal modelFile = null;
            ModelTexture textureFile = null;

            foreach (Bar.Entry barEntry in barFile)
            {
                try
                {
                    switch (barEntry.Type)
                    {
                        case Bar.EntryType.Model:
                            modelFile = ModelSkeletal.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelTexture:
                            textureFile = ModelTexture.Read(barEntry.Stream);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e) { }
            }

            if(modelFile != null)
            {
                // Skeleton
                for (int i = 0; i < modelFile.Bones.Count; i++)
                {
                    ModelCommon.Bone bone = modelFile.Bones[i];
                    MtJoint joint = new MtJoint();
                    joint.Name = "Bone" + i;
                    joint.ParentId = (bone.ParentIndex == -1) ? null : bone.ParentIndex;
                    joint.RelativeScale = new Vector3(bone.ScaleX, bone.ScaleY, bone.ScaleZ);
                    joint.RelativeRotation = new Vector3(bone.RotationX, bone.RotationY, bone.RotationZ);
                    joint.RelativeTranslation = new Vector3(bone.TranslationX, bone.TranslationY, bone.TranslationZ);
                    model.Joints.Add(joint);
                }
                model.CalculateFromRelativeData();

                // Meshes
                for (int i = 0; i < modelFile.Groups.Count; i++)
                {
                    ModelSkeletal.SkeletalGroup group = modelFile.Groups[i];

                    MtMesh mesh = new MtMesh();
                    mesh.Name = "Mesh" + i;
                    mesh.MaterialId = (int)group.Header.TextureIndex;

                    foreach(ModelCommon.UVBVertex mdlxVertex in group.Mesh.Vertices)
                    {
                        MtVertex vertex = new MtVertex();
                        vertex.AbsolutePosition = mdlxVertex.Position;
                        vertex.TextureCoordinates = new Vector3(mdlxVertex.U / 4096.0f, 1 - (mdlxVertex.V / 4096.0f), 1);
                        foreach(ModelCommon.BPosition bonePosition in mdlxVertex.BPositions)
                        {
                            MtWeightPosition weight = new MtWeightPosition();
                            weight.JointIndex = bonePosition.BoneIndex;
                            weight.Weight = bonePosition.Position.W == 0 ? 1 : bonePosition.Position.W;
                            vertex.Weights.Add(weight);
                        }
                        mesh.Vertices.Add(vertex);
                    }
                    foreach (List<int> mdlxTriangle in group.Mesh.Triangles)
                    {
                        MtFace face = new MtFace();
                        face.VertexIndices = mdlxTriangle;
                        mesh.Faces.Add(face);
                    }

                    model.Meshes.Add(mesh);
                }
            }

            // Materials
            if (textureFile != null)
            {
                for(int i = 0; i < textureFile.Images.Count; i++)
                {
                    ModelTexture.Texture texture = textureFile.Images[i];
                    MtMaterial material = new MtMaterial();
                    material.Name = "Texture" + i;
                    material.Width = texture.Size.Width;
                    material.Height = texture.Size.Height;
                    material.Data = texture.GetData();
                    material.Clut = texture.GetClut();
                    material.ColorSize = 4;

                    material.DiffuseTextureBitmap = new Bitmap(material.Width, material.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    using (MemoryStream stream = new MemoryStream(texture.AsRgba8888()))
                    {
                        for (int y = 0; y < material.Height; y++)
                        {
                            for (int x = 0; x < material.Width; x++)
                            {
                                int r = stream.ReadByte();
                                int g = stream.ReadByte();
                                int b = stream.ReadByte();
                                int a = stream.ReadByte();
                                Color pixelColor = Color.FromArgb(a,r,g,b);
                                material.DiffuseTextureBitmap.SetPixel(x, y, pixelColor);
                            }
                        }
                    }


                    model.Materials.Add(material);
                }
            }

            return model;
        }
    }
}
