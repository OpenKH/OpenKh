using ModelingToolkit.Objects;
using OpenKh.Common;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using OpenKh.Ps2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenKh.Tools.KhModels.KIH2
{
    public class MapProcessor
    {
        public static List<MtModel> GetMtModels(Bar barFile)
        {
            List<MtModel> models = new List<MtModel>();

            List<Mdlx> modelFiles = new List<Mdlx>();
            List<ModelTexture> modelTextures = new List<ModelTexture>();

            foreach (Bar.Entry barEntry in barFile)
            {
                try
                {
                    switch (barEntry.Type)
                    {
                        case Bar.EntryType.Model:
                            modelFiles.Add(Mdlx.Read(barEntry.Stream));
                            break;
                        case Bar.EntryType.ModelTexture:
                            ModelTexture mapTexture = ModelTexture.Read(barEntry.Stream);
                            modelTextures.Add(mapTexture);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e) { }
            }

            if (modelFiles.Count > 0)
            {
                for(int i = 0; i < modelFiles.Count; i++)
                {
                    Mdlx mapModelFile = modelFiles[i];
                    MtModel model = new MtModel();
                    model.Name = "Model" + i.ToString("D4");
                    // Meshes
                    if(mapModelFile.ModelBackground != null)
                    {
                        for (int j = 0; j < mapModelFile.ModelBackground.Chunks.Count; j++)
                        {
                            ModelBackground.ModelChunk mapMesh = mapModelFile.ModelBackground.Chunks[j];
                            MeshDescriptor meshDesc = ParseChunk(mapMesh);
                            MtMesh mesh = new MtMesh();
                            mesh.Name = "Mesh" + j.ToString("D4");
                            mesh.MaterialId = meshDesc.TextureIndex;
                            foreach (PositionColoredTextured mapVertex in meshDesc.Vertices)
                            {
                                MtVertex vertex = new MtVertex();
                                vertex.AbsolutePosition = new Vector3(mapVertex.X, mapVertex.Y, mapVertex.Z);
                                vertex.TextureCoordinates = new Vector3(mapVertex.Tu, 1 - mapVertex.Tv, 0);
                                vertex.Color = new Vector4(mapVertex.R, mapVertex.G, mapVertex.B, mapVertex.A);
                                mesh.Vertices.Add(vertex);
                            }

                            for (int k = 0; k < meshDesc.Indices.Length - 1; k += 3)
                            {
                                MtFace face = new MtFace();
                                face.VertexIndices.Add(meshDesc.Indices[k]);
                                face.VertexIndices.Add(meshDesc.Indices[k + 1]);
                                face.VertexIndices.Add(meshDesc.Indices[k + 2]);
                                mesh.Faces.Add(face);
                            }
                            model.Meshes.Add(mesh);
                        }
                    }
                    
                    models.Add(model);
                }
            }

            if(modelFiles.Count == modelTextures.Count)
            {
                // Materials
                if (modelTextures.Count > 0)
                {
                    for (int i = 0; i < modelTextures.Count; i++)
                    {
                        ModelTexture mapTextureFile = modelTextures[i];
                        MtModel model = models[i];

                        for (int j = 0; j < mapTextureFile.Images.Count; j++)
                        {
                            ModelTexture.Texture texture = mapTextureFile.Images[j];
                            MtMaterial material = new MtMaterial();
                            material.Name = "Texture" + j;
                            material.DiffuseTextureFileName = "Texture" + j;
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
                                        Color pixelColor = Color.FromArgb(a, r, g, b);
                                        material.DiffuseTextureBitmap.SetPixel(x, y, pixelColor);
                                    }
                                }
                            }

                            model.Materials.Add(material);
                        }
                    }
                }
            }

            return models;
        }

        // Taken from OpenKh.Engine.MdlxParser
        private static MeshDescriptor ParseChunk(ModelBackground.ModelChunk vifPacketDescriptor)
        {
            var vertices = new List<PositionColoredTextured>();
            var indices = new List<int>();
            var unpacker = new VifUnpacker(vifPacketDescriptor.VifPacket);

            var indexBuffer = new int[4];
            var recentIndex = 0;
            while (unpacker.Run() != VifUnpacker.State.End)
            {
                var vpu = new MemoryStream(unpacker.Memory, false)
                    .Using(stream => VpuPacket.Read(stream));

                var baseVertexIndex = vertices.Count;
                for (var i = 0; i < vpu.Indices.Length; i++)
                {
                    var vertexIndex = vpu.Indices[i];
                    var position = new Vector3(
                        vpu.Vertices[vertexIndex.Index].X,
                        vpu.Vertices[vertexIndex.Index].Y,
                        vpu.Vertices[vertexIndex.Index].Z);

                    int colorR, colorG, colorB, colorA;
                    if (vpu.Colors.Length != 0)
                    {
                        colorR = vpu.Colors[i].R;
                        colorG = vpu.Colors[i].G;
                        colorB = vpu.Colors[i].B;
                        colorA = vpu.Colors[i].A;
                    }
                    else
                    {
                        colorR = 0x80;
                        colorG = 0x80;
                        colorB = 0x80;
                        colorA = 0x80;
                    }

                    vertices.Add(new PositionColoredTextured(
                        vpu.Vertices[vertexIndex.Index].X,
                        vpu.Vertices[vertexIndex.Index].Y,
                        vpu.Vertices[vertexIndex.Index].Z,
                        (short)(ushort)vertexIndex.U / 4096.0f,
                        (short)(ushort)vertexIndex.V / 4096.0f,
                        colorR / 128f,
                        colorG / 128f,
                        colorB / 128f,
                        colorA / 128f));

                    indexBuffer[(recentIndex++) & 3] = baseVertexIndex + i;
                    switch (vertexIndex.Function)
                    {
                        case VpuPacket.VertexFunction.DrawTriangleDoubleSided:
                            indices.Add(indexBuffer[(recentIndex - 1) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 3) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 2) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 1) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 2) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 3) & 3]);
                            break;
                        case VpuPacket.VertexFunction.Stock:
                            break;
                        case VpuPacket.VertexFunction.DrawTriangle:
                            indices.Add(indexBuffer[(recentIndex - 1) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 3) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 2) & 3]);
                            break;
                        case VpuPacket.VertexFunction.DrawTriangleInverse:
                            indices.Add(indexBuffer[(recentIndex - 1) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 2) & 3]);
                            indices.Add(indexBuffer[(recentIndex - 3) & 3]);
                            break;
                    }
                }
            }

            return new MeshDescriptor
            {
                Vertices = vertices.ToArray(),
                Indices = indices.ToArray(),
                TextureIndex = vifPacketDescriptor.TextureId,
                IsOpaque = vifPacketDescriptor.TransparencyFlag == 0,
            };
        }
    }
}
