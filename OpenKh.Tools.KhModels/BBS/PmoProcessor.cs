using Assimp;
using ModelingToolkit.Objects;
using OpenKh.Bbs;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using static OpenKh.Ddd.PmoV4_2.Vertex;

namespace OpenKh.Tools.KhModels.BBS
{
    public class PmoProcessor
    {
        public static MtModel GetMtModel(Pmo pmo)
        {
            MtModel model = new MtModel();

            // Skeleton
            if(pmo.boneList != null)
            {
                for (int i = 0; i < pmo.boneList.Length; i++)
                {
                    Pmo.BoneData pmoBone = pmo.boneList[i];
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
            for (int i = 0; i < pmo.Meshes.Count; i++)
            {
                Pmo.MeshChunks pmoMesh = pmo.Meshes[i];
                MtMesh mesh = new MtMesh();
                mesh.Name = "Mesh"+i.ToString("D4");
                mesh.MaterialId = pmoMesh.TextureID;

                for(int j = 0; j < pmoMesh.vertices.Count; j++)
                {
                    MtVertex vertex = new MtVertex();

                    vertex.AbsolutePosition = pmoMesh.vertices[j];
                    vertex.AbsolutePosition *= pmo.header.ModelScale;

                    vertex.TextureCoordinates = new Vector3(pmoMesh.textureCoordinates[j].X, 1 - pmoMesh.textureCoordinates[j].Y, 1);

                    if(pmoMesh.jointWeights.Count > 0)
                    {
                        for (int k = 0; k < pmoMesh.jointWeights[j].weights.Count; k++)
                        {
                            if (pmoMesh.jointWeights[j].weights[k] > 0)
                            {
                                MtWeightPosition weight = new MtWeightPosition();
                                weight.Weight = pmoMesh.jointWeights[j].weights[k];
                                weight.JointIndex = pmoMesh.SectionInfo_opt1.SectionBoneIndices[k];
                                vertex.Weights.Add(weight);
                            }
                        }
                    }

                    mesh.Vertices.Add(vertex);
                }

                for(int j = 0; j < pmoMesh.Indices.Count; j += 3)
                {
                    MtFace face = new MtFace();
                    face.VertexIndices = new List<int> { pmoMesh.Indices[j], pmoMesh.Indices[j + 1], pmoMesh.Indices[j + 2] };
                    mesh.Faces.Add(face);
                }

                model.Meshes.Add(mesh);
            }

            // Materials
            for(int i = 0; i < pmo.texturesData.Count; i++)
            {
                Imaging.Tm2 image = pmo.texturesData[i];

                MtMaterial material = new MtMaterial();
                material.Name = "Texture" + i;
                material.DiffuseTextureFileName = "Texture" + i + ".png";
                material.DiffuseTextureBitmap = GetBitmap(image);
                model.Materials.Add(material);
            }

            return model;
        }

        public static Bitmap GetBitmap(Imaging.Tm2 image)
        {
            byte[] data = image.GetData();
            byte[] clut = image.GetClut();
            int width = image.Size.Width;
            int height = image.Size.Height;

            byte[] imageBgra = image.ToBgra32();

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = 4 * (y * width + x);
                    bitmap.SetPixel(x, y, Color.FromArgb(imageBgra[pixelIndex + 3], imageBgra[pixelIndex + 2], imageBgra[pixelIndex + 1], imageBgra[pixelIndex]));
                }
            }

            return bitmap;
        }

        private static Color GetColorFromCLUT(byte[] clut, int index)
        {
            int start = index * 4;
            byte red = clut[start];
            byte green = clut[start + 1];
            byte blue = clut[start + 2];
            byte alpha = clut[start + 3];

            Color color = Color.FromArgb(alpha, red, green, blue);

            return color;
        }
    }
}
