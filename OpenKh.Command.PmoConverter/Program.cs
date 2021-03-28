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
        [Argument(0, "Convert File", "The file to convert to PMO.")]
        public string FileName { get; }

        [Required]
        [Argument(1, "Converted File", "The resulting converted PMO.")]
        public string outFileName { get; }

        public static List<string> TexList { get; set; }
        public static List<Tm2> TextureData { get; set; }

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
            foreach (MeshDescriptor d in Descriptors)
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

                chunk.SectionInfo = new Pmo.MeshSection();
                chunk.SectionInfo.Attribute = 0;
                chunk.SectionInfo.VertexCount = (ushort)desc.Vertices.Length;
                chunk.SectionInfo.TextureID = (byte)desc.TextureIndex;
                chunk.SectionInfo.VertexFlags = 0x30000000; // 0011 000 0 0 00 000 0 000 0 00 00 11 00 000 01

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
                chunk.SectionInfo.VertexFlags = BitsUtil.Int.SetBits(chunk.SectionInfo.VertexFlags, 0, 2, (uint)TextureCoordinateFormat);
                chunk.SectionInfo.VertexFlags = BitsUtil.Int.SetBits(chunk.SectionInfo.VertexFlags, 7, 2, (uint)VertexFormat);

                chunk.SectionInfo.VertexSize += 0; // Weights.
                chunk.SectionInfo.VertexSize += (TextureCoordinateFormat == Pmo.CoordinateFormat.FLOAT_32_BITS) ? (byte)8 : (byte)((int)TextureCoordinateFormat * 2); // Texture Coordinates
                if (chunk.SectionInfo.VertexSize % 4 != 0)
                    chunk.SectionInfo.VertexSize += 2;
                chunk.SectionInfo.VertexSize += UsesUniformColor ? (byte)0 : (byte)4; // VertexColor
                chunk.SectionInfo.VertexSize += (VertexFormat == Pmo.CoordinateFormat.FLOAT_32_BITS) ? (byte)12 : (byte)((int)VertexFormat * 3); // Vertices


                for (int v = 0; v < desc.Indices.Length; v++)
                {
                    int index = vertIndices[v];

                    Vector4 Color = new Vector4();
                    Color.X = desc.Vertices[index].R * 2;
                    Color.Y = desc.Vertices[index].G * 2;
                    Color.Z = desc.Vertices[index].B * 2;
                    Color.W = desc.Vertices[index].A * 2;
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
                }

                pmo.Meshes.Add(chunk);
            }

            // Header.
            pmo.header = new Pmo.Header();
            pmo.header.MagicCode = 0x4F4D50;
            pmo.header.TextureCount = (ushort)TextureData.Count; // TODO.
            pmo.header.Unk0A = 0x80;
            pmo.header.MeshOffset0 = 0xA0 + ((uint)pmo.header.TextureCount * 0x20);
            pmo.header.VertexCount = (ushort)indicesVertexCount;
            pmo.header.TriangleCount = (ushort)indicesVertexCount;
            pmo.header.TriangleCount /= 3;
            pmo.header.ModelScale = 1.0f;
            pmo.header.BoundingBox = new float[32];

            // Texture block.
            if (TextureData.Count > 0)
            {
                pmo.textureInfo = new Pmo.TextureInfo[TextureData.Count];

                for (int t = 0; t < TextureData.Count; t++)
                {
                    Tm2 tm = TextureData[t];
                    pmo.textureInfo[t] = new Pmo.TextureInfo();
                    pmo.textureInfo[t].TextureName = TexList[t];
                    pmo.textureInfo[t].Unknown = new UInt32[4];
                    pmo.texturesData.Add(TextureData[t]);
                }
            }

            return pmo;
        }

        public static bool UsesUniformDiffuseFlag(MeshDescriptor desc)
        {
            bool bDiffuseFlag = true;
            Vector4 InitialColor = new Vector4(desc.Vertices[0].R, desc.Vertices[0].G, desc.Vertices[0].B, desc.Vertices[0].A);
            Vector4 CompareColor = new Vector4();

            foreach (PositionColoredTextured vert in desc.Vertices)
            {
                CompareColor = new Vector4(vert.R, vert.G, vert.B, vert.A);

                if (CompareColor != InitialColor)
                    return false;
            }

            return bDiffuseFlag;
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
            if (is8bits)
                return Pmo.CoordinateFormat.NORMALIZED_8_BITS;

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
            if (is16bits)
                return Pmo.CoordinateFormat.NORMALIZED_16_BITS;

            return Pmo.CoordinateFormat.FLOAT_32_BITS;
        }

        public static MeshGroup FromFbx(string filePath)
        {
            const float Scale = 1.0f;
            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(filePath, Assimp.PostProcessSteps.PreTransformVertices);
            var baseFilePath = Path.GetDirectoryName(filePath);
            TexList = new List<string>();
            TextureData = new List<Tm2>();

            foreach (Assimp.Material mat in scene.Materials)
            {
                TexList.Add(Path.GetFileName(mat.TextureDiffuse.FilePath));
                Stream str = File.OpenRead(TexList[TexList.Count - 1]);

                PngImage png = new PngImage(str);
                Tm2 tmImage = Tm2.Create(png);
                TextureData.Add(tmImage);
            }

            int childCount = scene.RootNode.ChildCount;

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
                            vertices[i].R = 1.0f;
                            vertices[i].G = 1.0f;
                            vertices[i].B = 1.0f;
                            vertices[i].A = 1.0f;
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
