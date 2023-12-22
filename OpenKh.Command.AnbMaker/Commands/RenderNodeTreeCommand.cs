using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Commands
{
    [HelpOption]
    [Command(Description = "fbx file: render node tree to bitmaps")]
    internal class RenderNodeTreeCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "fbx input")]
        public string? InputModel { get; set; }

        private class TreeRenderer : IDisposable
        {
            private readonly SKBitmap _bitmap;
            private readonly SKCanvas _canvas;
            private readonly SKPaint _pen;

            public TreeRenderer()
            {
                _bitmap = new SKBitmap(1000, 1000);
                _canvas = new SKCanvas(_bitmap);

                _canvas.Clear(SKColors.White);

                _pen = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Blue,
                    StrokeWidth = 1,
                    StrokeCap = SKStrokeCap.Round,
                };
            }

            public void Dispose()
            {
                _pen.Dispose();
                _canvas.Dispose();
                _bitmap.Dispose();
            }

            public void DrawParentToChild(SKPoint from, SKPoint to)
            {
                _canvas.DrawLine(from, to, _pen);
            }

            public void SaveToPng(Stream stream)
            {
                _bitmap.Encode(SKEncodedImageFormat.Png, 100)
                    .SaveTo(stream);
            }
        }

        protected int OnExecute(CommandLineApplication app)
        {
            using var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(InputModel, Assimp.PostProcessSteps.None);

            using var worldTreeRenderer = new TreeRenderer();
            using var bonesTreeRenderer = new TreeRenderer();

            var boneTreeWritten = false;

            SKPoint GetPoint(Vector3 vec) =>
                new SKPoint(
                    500 + vec.X,
                    500 - vec.Y
                    );

            void Walk(Assimp.Node node, Matrix4x4 parent, int depth)
            {
                var thisMatrix = node.Transform.ToDotNetMatrix4x4() * parent;

                {
                    Matrix4x4.Decompose(
                        thisMatrix,
                        out Vector3 thisScale,
                        out Quaternion thisRotation,
                        out Vector3 thisTranslation
                    );

                    Matrix4x4.Decompose(
                        parent,
                        out Vector3 parentScale,
                        out Quaternion parentRotation,
                        out Vector3 parentTranslation
                    );

                    worldTreeRenderer.DrawParentToChild(GetPoint(parentTranslation), GetPoint(thisTranslation));
                }

                if (node.HasChildren)
                {
                    foreach (var child in node.Children)
                    {
                        Walk(
                            child,
                            thisMatrix,
                            depth + 1
                        );
                    }
                }

                if (node.HasMeshes)
                {
                    foreach (var meshIdx in node.MeshIndices)
                    {
                        var mesh = scene.Meshes[meshIdx];

                        if (!boneTreeWritten)
                        {
                            foreach (var bone in mesh.Bones)
                            {
                                Matrix4x4.Decompose(
                                    bone.OffsetMatrix.ToDotNetMatrix4x4(),
                                    out Vector3 thisScale,
                                    out Quaternion thisRotation,
                                    out Vector3 thisTranslation
                                );

                                bonesTreeRenderer.DrawParentToChild(
                                    GetPoint(Vector3.Zero),
                                    GetPoint(thisTranslation * 100)
                                );
                            }

                            boneTreeWritten = true;
                        }
                    }
                }
            }

            Walk(scene.RootNode, Matrix4x4.Identity, 0);

            Console.WriteLine("Saving something to world.png");
            File.Create("world.png").Using(
                stream => worldTreeRenderer.SaveToPng(stream)
            );

            Console.WriteLine("Saving something to bones.png");
            File.Create("bones.png").Using(
                stream => bonesTreeRenderer.SaveToPng(stream)
            );

            return 0;
        }

    }
}
