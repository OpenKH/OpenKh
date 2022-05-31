using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Parsers;
using OpenKh.Imaging;

using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using OpenKh.Bbs;
using System.Collections.Generic;
using System.Numerics;
using OpenKh.Common.Utils;

namespace OpenKh.Command.PmoConverter
{
    [Command("OpenKh.Command.PmoConverter")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Required]
        [Argument(0, "Convert File", "The FBX to convert to PMO. (Textures preferrably converted to TM2.)")]
        public string FileName { get; }

        [Required]
        [Argument(1, "Converted File", "The resulting converted PMO.")]
        public string outFileName { get; }

        public static List<string> TexList { get; set; }
        public static List<Tm2> TextureData { get; set; }
        public static List<Assimp.Bone> BoneData { get; set; }
        public static List<Assimp.Node> NodeData { get; set; }


        private void OnExecute()
        {
            try
            {
                Convert(FileName, outFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }

        private static void Convert(string fileIn, string fileOut)
        {
            MeshGroup p = FromFbx(fileIn);
            Pmo pmo = MeshGroup2PMO(p);
            using Stream stream = File.Create(fileOut);
            Pmo.Write(stream, pmo);
            stream.Close();
        }

        private static Pmo MeshGroup2PMO(MeshGroup meshGroup)
        {
            Pmo pmo = new Pmo();

            List<MeshDescriptor> Descriptors = meshGroup.MeshDescriptors;
            
            // Max 65K vertices.
            uint descriptorVertexCount = 0;
            uint indicesVertexCount = 0;
            foreach(MeshDescriptor d in Descriptors)
            {
                descriptorVertexCount += (uint)d.Vertices.Length;
                indicesVertexCount += (uint)d.Indices.Length;
            }

            // Mesh data.
            for (int i = 0; i < Descriptors.Count; i++)
            {
                MeshDescriptor desc = Descriptors[i];
                int[] vertIndices = desc.Indices;
                Pmo.MeshChunks chunk = new Pmo.MeshChunks();

                // Obtain info for PMO Vertex Flag.
                bool UsesUniformColor = UsesUniformDiffuseFlag(desc);
                Pmo.CoordinateFormat TextureCoordinateFormat = GetTextureCoordinateFormat(desc);
                Pmo.CoordinateFormat VertexFormat = GetVertexFormat(desc);
                Pmo.CoordinateFormat WeightFormat = Pmo.CoordinateFormat.FLOAT_32_BITS;

                chunk.SectionInfo = new Pmo.MeshSection();
                chunk.SectionInfo.Attribute = 0;
                chunk.SectionInfo.VertexCount = (ushort)desc.Vertices.Length;
                chunk.SectionInfo.TextureID = (byte)desc.TextureIndex;
                chunk.SectionInfo.VertexFlags = 0x30000000; // 0011 000 0 0 00 000 0 000 0 00 00 11 00 000 01
                //chunk.SectionInfo_opt1 = new Pmo.MeshSectionOptional1();
                //chunk.SectionInfo_opt1.SectionBoneIndices = GetBoneInfluences(desc);

                // Set extra flags.
                if (UsesUniformColor)
                {
                    var UniformColor = (uint)(Math.Min(byte.MaxValue, desc.Vertices[0].A * 256f));
                    UniformColor += (uint)(Math.Min(byte.MaxValue, desc.Vertices[0].B * 256f)) << 8;
                    UniformColor += (uint)(Math.Min(byte.MaxValue, desc.Vertices[0].G * 256f)) << 16;
                    UniformColor += (uint)(Math.Min(byte.MaxValue, desc.Vertices[0].R * 256f)) << 24;
                    chunk.SectionInfo.VertexFlags = BitsUtil.Int.SetBit(chunk.SectionInfo.VertexFlags, 24, true);
                    chunk.SectionInfo_opt2 = new Pmo.MeshSectionOptional2();
                    chunk.SectionInfo_opt2.DiffuseColor = UniformColor;
                } 
                else
                    chunk.SectionInfo.VertexFlags = BitsUtil.Int.SetBits(chunk.SectionInfo.VertexFlags, 2, 3, (uint)0x7);
                //chunk.SectionInfo.VertexFlags = BitsUtil.Int.SetBits(chunk.SectionInfo.VertexFlags, 0, 2, (uint)TextureCoordinateFormat);
                //chunk.SectionInfo.VertexFlags = BitsUtil.Int.SetBits(chunk.SectionInfo.VertexFlags, 7, 2, (uint)VertexFormat);

                uint texFormat = 3;
                uint posFormat = 3;
                uint weightFormat = 1;

                chunk.SectionInfo.VertexFlags = BitsUtil.Int.SetBits(chunk.SectionInfo.VertexFlags, 0, 2, texFormat);
                chunk.SectionInfo.VertexFlags = BitsUtil.Int.SetBits(chunk.SectionInfo.VertexFlags, 7, 2, posFormat);
                //chunk.SectionInfo.VertexFlags = BitsUtil.Int.SetBits(chunk.SectionInfo.VertexFlags, 9, 2, weightFormat);
                //chunk.SectionInfo.VertexFlags = BitsUtil.Int.SetBits(chunk.SectionInfo.VertexFlags, 14, 3, 4);

                //chunk.SectionInfo.VertexSize += 4; // Weights.
                TextureCoordinateFormat = (Pmo.CoordinateFormat)texFormat;
                chunk.SectionInfo.VertexSize += (TextureCoordinateFormat == Pmo.CoordinateFormat.FLOAT_32_BITS) ? (byte)8  : (byte)((int)TextureCoordinateFormat * 2); // Texture Coordinates
                if (chunk.SectionInfo.VertexSize % 4 != 0)
                    chunk.SectionInfo.VertexSize += 2;

                chunk.SectionInfo.VertexSize += UsesUniformColor ? (byte)0 : (byte)4; // VertexColor

                VertexFormat = (Pmo.CoordinateFormat)posFormat;
                chunk.SectionInfo.VertexSize += (VertexFormat == Pmo.CoordinateFormat.FLOAT_32_BITS) ? (byte)12 : (byte)((int)VertexFormat * 3); // Vertices
                if (chunk.SectionInfo.VertexSize % 4 != 0)
                    chunk.SectionInfo.VertexSize += 2;

                for (int v = 0; v < desc.Indices.Length; v++)
                {
                    int index = vertIndices[v];

                    Vector4 Color = new Vector4();
                    Color.X = desc.Vertices[index].R * 256;
                    Color.Y = desc.Vertices[index].G * 256;
                    Color.Z = desc.Vertices[index].B * 256;
                    Color.W = 128;
                    chunk.colors.Add(Color);

                    Vector3 vec;
                    vec.X = desc.Vertices[index].X / 10000.0f;
                    vec.Y = desc.Vertices[index].Y / 10000.0f;
                    vec.Z = desc.Vertices[index].Z / 10000.0f;
                    chunk.vertices.Add(vec);

                    Vector2 Coords;
                    Coords.X = desc.Vertices[index].Tu;
                    Coords.Y = desc.Vertices[index].Tv;
                    chunk.textureCoordinates.Add(Coords);

                    /*
                    Pmo.WeightData weights = new Pmo.WeightData();
                    weights.weights = new List<float>();
                    weights.coordFormat = Pmo.CoordinateFormat.NORMALIZED_8_BITS;
                    weights.weights.Add(100.0f);

                    
                    int weightsAdded = 0;
                    foreach (var bn in BoneData)
                    {
                        // Check each vertex weight
                        foreach(Assimp.VertexWeight vertW in bn.VertexWeights)
                        {
                            // Check supported bones.
                            foreach(byte tempIndices in chunk.SectionInfo_opt1.SectionBoneIndices)
                            {
                                byte tst = tempIndices;
                                // Check if supported bone is weighted to this vertex.
                                if(tst == vertW.VertexID)
                                {
                                    if(weightsAdded < 4)
                                    {
                                        weights.weights.Add(vertW.Weight);
                                        weightsAdded++;
                                    }
                                }
                            }
                        }
                    }

                    // Make sure there's always 4 weights.
                    for(int vt = weightsAdded; vt < 4; v++)
                    {
                        weights.weights.Add(0.0f);
                    }*/

                    //chunk.jointWeights.Add(weights);
                }

                pmo.Meshes.Add(chunk);
            }

            // Header.
            pmo.header = new Pmo.Header();
            pmo.header.MagicCode = 0x4F4D50;
            pmo.header.Number = 1;
            pmo.header.Group = 1;
            pmo.header.Version = 3;
            pmo.header.TextureCount = (byte)TextureData.Count; // TODO.
            pmo.header.Flag = 0x8000;
            pmo.header.MeshOffset0 = 0xA0 + ((uint)pmo.header.TextureCount * 0x20);
            pmo.header.VertexCount = (ushort)indicesVertexCount;
            pmo.header.TriangleCount = (ushort)indicesVertexCount;
            pmo.header.TriangleCount /= 3;
            pmo.header.ModelScale = 1.0f;

            float[] commonBoundBox =
                new float[32]
                {
                    -1.0f, -1.0f, -1.0f, 1.0f,
                    1.0f, -1.0f, -1.0f, 1.0f,
                    -1.0f, -1.0f, 1.0f, 1.0f,
                    1.0f, -1.0f, 1.0f, 1.0f,
                    1.0f, -1.0f, 1.0f, 1.0f,
                    1.0f, 1.0f, -1.0f, 1.0f,
                    -1.0f, 1.0f, 1.0f, 1.0f,
                    1.0f, 1.0f, 1.0f, 1.0f
                };

            pmo.header.BoundingBox = commonBoundBox;

            // Texture block.
            if(TextureData.Count > 0)
            {
                pmo.textureInfo = new Pmo.TextureInfo[TextureData.Count];

                for (int t = 0; t < TextureData.Count; t++)
                {
                    Tm2 tm = TextureData[t];
                    pmo.textureInfo[t] = new Pmo.TextureInfo();
                    pmo.textureInfo[t].TextureName = TexList[t];
                    pmo.textureInfo[t].Unknown = new UInt32[4];
                    pmo.texturesData.Add( TextureData[t] );
                }
            }

            /*
            pmo.header.SkeletonOffset = pmo.header.MeshOffset0 + 0;
            pmo.skeletonHeader = new Pmo.SkeletonHeader();
            pmo.boneList = new Pmo.BoneData[0];
            pmo.skeletonHeader.MagicValue = 0x4E4F42;
            pmo.skeletonHeader.BoneCount = (ushort)BoneData.Count;
            pmo.skeletonHeader.SkinnedBoneCount = (ushort)BoneData.Count;
            pmo.skeletonHeader.nStdBone = 2;

            Pmo.BoneData RootBone = new Pmo.BoneData();
            RootBone.ParentBoneIndex = 0;
            RootBone.BoneIndex = 0;
            RootBone.JointName = "Root";

            pmo.boneList = new Pmo.BoneData[1];
            pmo.boneList[0] = RootBone;
            */

            /*pmo.boneList = new Pmo.BoneData[BoneData.Count];

            for(int b = 0; b < pmo.boneList.Length; b++)
            {
                Pmo.BoneData bn = new Pmo.BoneData();
                bn.BoneIndex = (ushort)b;

                Assimp.Node curNode = new Assimp.Node();
                ushort p = 0;
                foreach(var nd in NodeData)
                {
                    p++;
                    if(nd.Name == BoneData[b].Name)
                    {
                        curNode = nd;
                        p--;
                        break;
                    }
                }

                bn.ParentBoneIndex = p;
                bn.JointName = BoneData[b].Name;

                Matrix4x4 mtx = new Matrix4x4();
                mtx.M11 = BoneData[b].OffsetMatrix.A1;
                mtx.M12 = BoneData[b].OffsetMatrix.A2;
                mtx.M13 = BoneData[b].OffsetMatrix.A3;
                mtx.M14 = BoneData[b].OffsetMatrix.A4;
                mtx.M21 = BoneData[b].OffsetMatrix.B1;
                mtx.M22 = BoneData[b].OffsetMatrix.B2;
                mtx.M23 = BoneData[b].OffsetMatrix.B3;
                mtx.M24 = BoneData[b].OffsetMatrix.B4;
                mtx.M31 = BoneData[b].OffsetMatrix.C1;
                mtx.M32 = BoneData[b].OffsetMatrix.C2;
                mtx.M33 = BoneData[b].OffsetMatrix.C3;
                mtx.M34 = BoneData[b].OffsetMatrix.C4;
                mtx.M41 = BoneData[b].OffsetMatrix.D1;
                mtx.M42 = BoneData[b].OffsetMatrix.D2;
                mtx.M43 = BoneData[b].OffsetMatrix.D3;
                mtx.M44 = BoneData[b].OffsetMatrix.D4;

                bn.Transform = mtx;
                bn.InverseTransform = mtx;
                pmo.boneList[b] = bn;
            }*/

            return pmo;
        }

        public static bool UsesUniformDiffuseFlag(MeshDescriptor desc)
        {
            Vector4 InitialColor = new Vector4(desc.Vertices[0].R, desc.Vertices[0].G, desc.Vertices[0].B, desc.Vertices[0].A);

            foreach (PositionColoredTextured vert in desc.Vertices)
            {
                Vector4 CompareColor = new Vector4(vert.R, vert.G, vert.B, vert.A);

                if (CompareColor != InitialColor)
                    return false;
            }

            return true;
        }

        public static Pmo.CoordinateFormat GetTextureCoordinateFormat(MeshDescriptor desc)
        {
            Vector2 ResizedVector = new Vector2();

            // Check if 0 bits per coordinate.
            bool is0bits = true;
            foreach (PositionColoredTextured vert in desc.Vertices)
            {
                ResizedVector = new Vector2(vert.Tu, vert.Tv);

                if (ResizedVector.X != 0 || ResizedVector.Y != 0)
                {
                    is0bits = false;
                    break;
                }
            }
            if (is0bits)
                return Pmo.CoordinateFormat.NO_VERTEX;

            // Check if 8 bits per coordinate.
            bool is8bits = true;
            foreach (PositionColoredTextured vert in desc.Vertices)
            {
                ResizedVector = new Vector2(vert.Tu * 128.0f, vert.Tv * 128.0f);

                if (ResizedVector.X > 255 || ResizedVector.Y > 255)
                {
                    is8bits = false;
                    break;
                }
            }
            if (is8bits)
                return Pmo.CoordinateFormat.NORMALIZED_8_BITS;

            // Check if 16 bits per coordinate.
            bool is16bits = true;
            foreach (PositionColoredTextured vert in desc.Vertices)
            {
                ResizedVector = new Vector2(vert.Tu * 32768.0f, vert.Tv * 32768.0f);

                if (ResizedVector.X > 65535 || ResizedVector.Y > 65535)
                {
                    is16bits = false;
                    break;
                }
            }
            if (is16bits)
                return Pmo.CoordinateFormat.NORMALIZED_16_BITS;

            return Pmo.CoordinateFormat.FLOAT_32_BITS;
        }

        public static Pmo.CoordinateFormat GetVertexFormat(MeshDescriptor desc)
        {
            Vector3 ResizedVector = new Vector3();

            // Check if 8 bits per coordinate.
            bool is8bits = true;
            foreach (PositionColoredTextured vert in desc.Vertices)
            {
                ResizedVector = new Vector3(((vert.X / 100.0f) * 128.0f), ((vert.Y / 100.0f) * 128.0f), ((vert.Z / 100.0f) * 128.0f));

                if (ResizedVector.X > 255 || ResizedVector.Y > 255 || ResizedVector.Z > 255)
                {
                    is8bits = false;
                    break;
                }
            }
            if (is8bits) return Pmo.CoordinateFormat.NORMALIZED_8_BITS;

            // Check if 16 bits per coordinate.
            bool is16bits = true;
            foreach (PositionColoredTextured vert in desc.Vertices)
            {
                ResizedVector = new Vector3(((vert.X / 100.0f) * 32768.0f), ((vert.Y / 100.0f) * 32768.0f), ((vert.Z / 100.0f) * 32768.0f));

                if (ResizedVector.X > 65535 || ResizedVector.Y > 65535 || ResizedVector.Z > 65535)
                {
                    is16bits = false;
                    break;
                }
            }
            if (is16bits) return Pmo.CoordinateFormat.NORMALIZED_16_BITS;

            return Pmo.CoordinateFormat.FLOAT_32_BITS;
        }

        public static byte[] GetBoneInfluences(MeshDescriptor desc)
        {
            byte[] boneInfluences = new byte[8];
            
            foreach(Assimp.Bone bn in BoneData)
            {
                
            }

            boneInfluences = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            return boneInfluences;
        }
        public static MeshGroup FromFbx(string filePath)
        {
            const float Scale = 1.0f;
            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(filePath, Assimp.PostProcessSteps.PreTransformVertices);
            var BoneScene = assimp.ImportFile(filePath);
            var baseFilePath = Path.GetDirectoryName(filePath);
            TexList = new List<string>();
            TextureData = new List<Tm2>();
            BoneData = new List<Assimp.Bone>();
            NodeData = new List<Assimp.Node>();

            foreach(Assimp.Material mat in scene.Materials)
            {
                Stream str = null;
                var name = Path.GetFileNameWithoutExtension(mat.TextureDiffuse.FilePath);
                var FinalName = "";
                bool isTm2 = false;
                if (name != "" || name != null)
                {
                    if(!File.Exists(name + ".tm2"))
                    {
                        FinalName = name + ".png";
                        str = File.OpenRead(FinalName);
                    }
                    else
                    {
                        isTm2 = true;
                        FinalName = name + ".tm2";
                        str = File.OpenRead(FinalName);
                    }
                }
                
                if(str != null)
                {
                    TexList.Add(Path.GetFileNameWithoutExtension(mat.TextureDiffuse.FilePath));

                    if (isTm2)
                    {
                        var rTim = Tm2.Read(str);
                        TextureData.Add(rTim.ToArray()[0]);
                    }
                    else
                    {
                        PngImage png = new PngImage(str);
                        Tm2 tmImage = Tm2.Create(png);
                        TextureData.Add(tmImage);
                    } 
                }
            }

            Assimp.Bone rBone = new Assimp.Bone();
            foreach(var m in BoneScene.Meshes)
            {
                foreach(var bn in m.Bones)
                {
                    if (!BoneData.Contains(bn))
                        BoneData.Add(bn);
                }
            }

            NodeData.AddRange(BoneScene.RootNode.Children.ToList());

            return new MeshGroup()
            {
                MeshDescriptors = scene.Meshes
                    .Select(x =>
                    {
                        var vertices = new PositionColoredTextured[x.Vertices.Count];
                        for (var i = 0; i < vertices.Length; i++)
                        {
                            vertices[i].X = x.Vertices[i].X * Scale;
                            vertices[i].Y = x.Vertices[i].Y * Scale;
                            vertices[i].Z = x.Vertices[i].Z * Scale;
                            vertices[i].Tu = x.TextureCoordinateChannels[0][i].X;
                            vertices[i].Tv = 1.0f - x.TextureCoordinateChannels[0][i].Y;
                            vertices[i].R = x.VertexColorChannels[0][i].R;
                            vertices[i].G = x.VertexColorChannels[0][i].G;
                            vertices[i].B = x.VertexColorChannels[0][i].B;
                            vertices[i].A = x.VertexColorChannels[0][i].A;
                        }
                        
                        return new MeshDescriptor
                        {
                            Vertices = vertices,
                            Indices = x.GetIndices(),
                            IsOpaque = true,
                            TextureIndex = x.MaterialIndex
                        };
                    }).ToList()
            };
        }
    }
}
