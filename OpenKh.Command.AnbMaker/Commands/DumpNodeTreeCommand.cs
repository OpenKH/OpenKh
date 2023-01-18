using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.AnbMaker.Extensions;
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
    [Command(Description = "fbx file: dump node tree")]
    internal class DumpNodeTreeCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "fbx input")]
        public string? InputModel { get; set; }

        [Option(Description = "print decomposed scale-rotation-translation", ShortName = "d", LongName = "decompose")]
        public bool Decompose { get; set; }

        [Option(Description = "print transform matrix", ShortName = "m", LongName = "matrix")]
        public bool Matrix { get; set; }

        [Option(Description = "print absolute", ShortName = "a", LongName = "absolute")]
        public bool Absolute { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(InputModel, Assimp.PostProcessSteps.None);

            void Walk(Assimp.Node node, Matrix4x4 parent, int depth)
            {
                var more = "";

                var thisMatrix = node.Transform.ToDotNetMatrix4x4() * parent;

                if (Decompose)
                {
                    System.Numerics.Matrix4x4.Decompose(
                        thisMatrix,
                        out Vector3 scale,
                        out System.Numerics.Quaternion rotation,
                        out Vector3 translation
                    );

                    more += $" S{scale} R{(ToDegree(rotation.ToEulerAngles()))} T{translation}";
                }

                if (Matrix)
                {
                    more += thisMatrix;
                }

                Console.WriteLine(new string(' ', depth) + "+ " + node.Name + more);

                if (node.HasMeshes)
                {
                    foreach (var meshIdx in node.MeshIndices)
                    {
                        var mesh = scene.Meshes[meshIdx];
                        Console.WriteLine(new string(' ', depth + 1) + "$ " + mesh.Name);

                        if (mesh.HasBones)
                        {
                            foreach (var bone in mesh.Bones)
                            {
                                var boneMore = "";

                                var boneMatrix = bone.OffsetMatrix.ToDotNetMatrix4x4();

                                if (Decompose)
                                {
                                    System.Numerics.Matrix4x4.Decompose(
                                        boneMatrix,
                                        out Vector3 scale,
                                        out System.Numerics.Quaternion rotation,
                                        out Vector3 translation
                                    );

                                    boneMore += $" S{scale} R{(ToDegree(rotation.ToEulerAngles()))} T{translation}";
                                }

                                if (Matrix)
                                {
                                    boneMore += " " + boneMatrix;
                                }

                                Console.WriteLine(new string(' ', depth + 1) + " + " + bone.Name + boneMore);
                            }
                        }
                    }
                }

                if (node.HasChildren)
                {
                    foreach (var child in node.Children)
                    {
                        Walk(
                            child,
                            Absolute ? thisMatrix : Matrix4x4.Identity,
                            depth + 1
                        );
                    }
                }
            }

            Walk(scene.RootNode, Matrix4x4.Identity, 0);

            return 0;
        }

        private static Vector3 ToDegree(Vector3 vec)
        {
            var f = (float)(1.0f / Math.PI * 180);
            return new Vector3(
                (int)(vec.X * f),
                (int)(vec.Y * f),
                (int)(vec.Z * f)
            );
        }
    }
}
