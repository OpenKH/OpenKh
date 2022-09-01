using Assimp;
using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Kh2;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Command.AnbMaker
{
    [Command("OpenKh.Command.AnbMaker")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(AnbCommand))]
    internal class Program
    {
        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "?";

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return 1;
            }
        }

        [HelpOption]
        [Command(Description = "fbx file: fbx to anb")]
        private class AnbCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "fbx input")]
            public string InputModel { get; set; }

            [Argument(1, Description = "anb output")]
            public string Output { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var assimp = new Assimp.AssimpContext();
                var scene = assimp.ImportFile(InputModel, Assimp.PostProcessSteps.None);

                Output = Path.GetFullPath(Output ?? Path.GetFileNameWithoutExtension(InputModel) + ".anb");

                Console.WriteLine($"Writing to: {Output}");

                //var fbxMesh = scene.Meshes.First();
                var fbxArmatureRoot = FindNodeByName(scene.RootNode, "kh_sk");
                var fbxArmatureNodes = FlattenNodes(fbxArmatureRoot);
                var fbxArmatureBoneCount = fbxArmatureNodes.Length;

                var fbxAnim = scene.Animations.First();

                var raw = RawMotion.CreateEmpty();

                //var frameCount = (int)fbxAnim.DurationInTicks;
                var frameCount = 1;

                raw.RawMotionHeader.BoneCount = fbxArmatureBoneCount;
                raw.RawMotionHeader.FrameCount = frameCount;
                raw.RawMotionHeader.TotalFrameCount = frameCount;
                raw.RawMotionHeader.FrameData.FrameStart = 0;
                raw.RawMotionHeader.FrameData.FrameEnd = frameCount - 1;
                raw.RawMotionHeader.FrameData.FramesPerSecond = (float)fbxAnim.TicksPerSecond;

                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    var matrices = new List<System.Numerics.Matrix4x4>();

                    for (int boneIdx = 0; boneIdx < fbxArmatureBoneCount; boneIdx++)
                    {
                        var parentIdx = fbxArmatureNodes[boneIdx].ParentIndex;

                        var parentMatrix = (parentIdx == -1)
                            ? System.Numerics.Matrix4x4.Identity
                            : matrices[parentIdx];

                        var absoluteMatrix = parentMatrix * GetDotNetMatrix(fbxArmatureNodes[boneIdx].ArmatureNode.Transform);

                        raw.AnimationMatrices.Add(absoluteMatrix);
                        matrices.Add(absoluteMatrix);
                    }
                }

                var rawMotionStream = new MemoryStream();
                RawMotion.Write(rawMotionStream, raw);

                var anbBarStream = new MemoryStream();
                Bar.Write(
                    anbBarStream,
                    new Bar.Entry[]
                    {
                        new Bar.Entry
                        {
                            Type = Bar.EntryType.Motion,
                            Name = "raw",
                            Stream = rawMotionStream,
                        }
                    }
                );

                File.WriteAllBytes(Output, anbBarStream.ToArray());

                return 0;
            }

            private System.Numerics.Matrix4x4 GetDotNetMatrix(Assimp.Matrix4x4 transform)
            {
                return new System.Numerics.Matrix4x4(
                    transform.A1, transform.A2, transform.A3, transform.A4,
                    transform.B1, transform.B2, transform.B3, transform.B4,
                    transform.C1, transform.C2, transform.C3, transform.C4,
                    transform.D1, transform.D2, transform.D3, transform.D4
                );
            }

            private record NodeRef
            {
                public int ParentIndex { get; set; }
                public Node ArmatureNode { get; set; }
            }

            private NodeRef[] FlattenNodes(Node topNode)
            {
                var list = new List<NodeRef>();

                var stack = new Stack<NodeRef>();
                stack.Push(new NodeRef { ParentIndex = -1, ArmatureNode = topNode });

                while (stack.Any())
                {
                    var nodeRef = stack.Pop();
                    var idx = list.Count;
                    list.Add(nodeRef);

                    foreach (var sub in nodeRef.ArmatureNode.Children.Reverse())
                    {
                        stack.Push(new NodeRef { ParentIndex = idx, ArmatureNode = sub, });
                    }
                }

                return list.ToArray();
            }

            private Node FindNodeByName(Node topNode, string name)
            {
                var stack = new Stack<Node>();
                stack.Push(topNode);

                while (stack.Any())
                {
                    var node = stack.Pop();
                    if (node.Name == name)
                    {
                        return node;
                    }

                    foreach (var sub in node.Children.Reverse())
                    {
                        stack.Push(sub);
                    }
                }

                throw new Exception($"Node '{name}' not found");
            }
        }
    }
}
