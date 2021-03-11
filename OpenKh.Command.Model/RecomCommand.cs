using Assimp;
using McMaster.Extensions.CommandLineUtils;
using OpenKh.Recom;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenKh.Command.Model
{
    [Command(Description = "Import or export models from Kingdom Hearts RE:Chain of Memories")]
    public class RecomCommand
    {
        record VirtualVertex
        {
            public System.Numerics.Vector3 Position { get; init; }
            public System.Numerics.Vector2 Texture { get; init; }
            public float Weight { get; init; }
            public int BoneIndex { get; init; }
        }

        [Required]
        [FileExists]
        [Argument(0, Description = "Path to the MDL file")]
        public string Input { get; set; }

        //[Argument(1, Description = "Destination as FBX")]
        public string Output { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var scene = new Scene();

            var textures = TryReadFile(Input, "RTM", Rtm.Read);
            if (textures != null)
                PopulateMaterials(scene, Path.GetDirectoryName(Input), textures);

            var model = TryReadFile(Input, "MDL", Mdl.Read);
            if (model == null)
                return -1;
            PopulateMeshes(scene, model.First());

            //var animations = TryReadFile(Input, "ANN", Ann.Read);
            //if (animations != null)
            //    PopulateAnimations(scene, animations);

            using var ctx = new AssimpContext();
            ctx.ExportFile(scene, Path.ChangeExtension(Output ?? Input, "dae"), "collada");
            ctx.ExportFile(scene, Path.ChangeExtension(Output ?? Input, "fbx"), "fbx");

            return 0;
        }

        private static void PopulateMeshes(Scene scene, Mdl model)
        {
            var nodes = new Node[model.Bones.Count];
            for (var i = 0; i < model.Bones.Count; i++)
            {
                Node node;
                var bone = model.Bones[i];
                if (bone.Parent >= 0)
                {
                    nodes[i] = node = new Node(bone.Name, nodes[bone.Parent]);
                    nodes[bone.Parent].Children.Add(nodes[i]);
                }
                else
                    nodes[i] = node = new Node(bone.Name);

                node.Transform = Map(model.Bones[i].Matrix);
            }
            scene.RootNode = nodes[0];

            var indicesAccumulators = new List<int>();
            var indicesList = new List<List<int>>();
            var verticesList = new List<List<VirtualVertex>>();
            List<int> indices;
            foreach (var mesh in model.Meshes)
            {
                foreach (var vertex in mesh.Vertices)
                {
                    var materialIndex = vertex.Texture.MaterialIndex;
                    while (materialIndex >= indicesList.Count)
                    {
                        indicesAccumulators.Add(0);
                        indicesList.Add(new List<int>());
                        verticesList.Add(new List<VirtualVertex>());
                    }

                    verticesList[materialIndex].Add(new VirtualVertex
                    {
                        Position = vertex.Position.Position,
                        Texture = vertex.Texture.TextureUv,
                        Weight = 1f,
                        BoneIndex = vertex.Position.BoneIndex,
                    });

                    var accumulator = indicesAccumulators[materialIndex];
                    switch (vertex.PrimitiveType)
                    {
                        case -1: // Discard triangle
                            break;
                        case 0:
                            indices = indicesList[materialIndex];
                            indices.Add(accumulator - 2);
                            indices.Add(accumulator);
                            indices.Add(accumulator - 1);
                            break;
                        case 0x20: // flip winding
                            indices = indicesList[materialIndex];
                            indices.Add(accumulator - 2);
                            indices.Add(accumulator - 1);
                            indices.Add(accumulator);
                            break;
                        default:
                            break;
                    }
                    indicesAccumulators[materialIndex] = accumulator + 1;
                }
            }

            for (var meshIndex = 0; meshIndex < indicesAccumulators.Count; meshIndex++)
            {
                var meshName = model.Materials[meshIndex].Name;
                var assimpMesh = new Mesh
                {
                    PrimitiveType = PrimitiveType.Triangle,
                    Name = meshName,
                    MaterialIndex = meshIndex
                };

                foreach (var vertex in verticesList[meshIndex])
                {
                    var boneName = model.Bones[vertex.BoneIndex].Name;
                    var bone = assimpMesh.Bones.FirstOrDefault(x => x.Name == boneName);
                    if (bone == null)
                    {
                        bone = new Bone { Name = boneName };
                        assimpMesh.Bones.Add(bone);
                    }
                    bone.VertexWeights.Add(new VertexWeight(assimpMesh.Vertices.Count, vertex.Weight));

                    assimpMesh.Vertices.Add(new Vector3D(
                        vertex.Position.X,
                        vertex.Position.Y,
                        vertex.Position.Z));
                    assimpMesh.TextureCoordinateChannels[0].Add(new Vector3D(
                        vertex.Texture.X,
                        vertex.Texture.Y,
                        0.0f));
                }
                assimpMesh.SetIndices(indicesList[meshIndex].ToArray(), 3);

                var meshNode = new Node();
                meshNode.Name = meshName;
                meshNode.MeshIndices.Add(meshIndex);
                scene.RootNode.Children.Add(meshNode);
                scene.Meshes.Add(assimpMesh);
            }
        }

        private static void PopulateMaterials(Scene scene, string basePath, List<Rtm> textures)
        {
            foreach (var texture in textures)
            {
                var name = Path.GetFileNameWithoutExtension(texture.Name);
                scene.Materials.Add(new Material
                {
                    Name = name,
                    TextureDiffuse = new TextureSlot
                    {
                        FilePath = name,
                        TextureType = TextureType.Diffuse
                    }
                });

                var fileName = Path.ChangeExtension(texture.Name, "png");
                using var bitmap = CreateBitmap(texture.Textures.First());
                bitmap.Save(Path.Combine(basePath, fileName), ImageFormat.Png);
            }
        }

        //private static void PopulateAnimations(Scene scene, List<Ann> animations)
        //{

        //}

        private static T TryReadFile<T>(string path, string extension, Func<Stream, T> reader)
            where T : class
        {
            var realPath = Path.ChangeExtension(path, extension);
            if (!File.Exists(realPath))
                return null;

            using var stream = File.OpenRead(realPath);
            return reader(stream);
        }

        private static Matrix4x4 Map(System.Numerics.Matrix4x4 m)
        {
            var mm = new Matrix4x4(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44);
            mm.Transpose();
            return mm;
        }

        private static Bitmap CreateBitmap(Imaging.IImageRead imageRead)
        {
            var drawingPixelFormat = GetDrawingPixelFormat(imageRead.PixelFormat);
            var bitmap = new Bitmap(imageRead.Size.Width, imageRead.Size.Height, drawingPixelFormat);

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, drawingPixelFormat);

            var srcData = imageRead.GetData();
            var dstLength = Math.Min(srcData.Length, bitmapData.Stride * bitmapData.Height);
            Marshal.Copy(srcData, 0, bitmapData.Scan0, dstLength);

            bitmap.UnlockBits(bitmapData);

            var isIndexed = IsIndexed(imageRead.PixelFormat);
            if (isIndexed)
            {
                var palette = bitmap.Palette;
                var clut = imageRead.GetClut();
                var colorsCount = Math.Min(clut.Length / 4, palette.Entries.Length);

                for (var i = 0; i < colorsCount; i++)
                {
                    palette.Entries[i] = Color.FromArgb(
                        (byte)Math.Min(clut[i * 4 + 3] * 2, 0xff), // TODO alpha fix
                        clut[i * 4 + 0],
                        clut[i * 4 + 1],
                        clut[i * 4 + 2]);
                }

                bitmap.Palette = palette;
            }

            return bitmap;
        }

        private static PixelFormat GetDrawingPixelFormat(Imaging.PixelFormat pixelFormat) => pixelFormat switch
        {
            Imaging.PixelFormat.Indexed4 => PixelFormat.Format4bppIndexed,
            Imaging.PixelFormat.Indexed8 => PixelFormat.Format8bppIndexed,
            Imaging.PixelFormat.Rgba1555 => PixelFormat.Format32bppArgb,
            Imaging.PixelFormat.Rgb888 => PixelFormat.Format24bppRgb,
            Imaging.PixelFormat.Rgbx8888 => PixelFormat.Format32bppRgb,
            Imaging.PixelFormat.Rgba8888 => PixelFormat.Format32bppArgb,
            _ => throw new NotImplementedException($"The pixel format {pixelFormat} is not implemented."),
        };

        private static bool IsIndexed(Imaging.PixelFormat pixelFormat) => pixelFormat switch
        {
            Imaging.PixelFormat.Indexed4 or
            Imaging.PixelFormat.Indexed8 => true,
            _ => false,
        };
    }
}
