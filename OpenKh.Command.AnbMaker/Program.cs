using Assimp;
using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Kh2;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using static OpenKh.Bbs.Bbsa;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Command.AnbMaker
{
    [Command("OpenKh.Command.AnbMaker")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(AnbCommand))]
    [Subcommand(typeof(ExportRawCommand))]
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

            [Option(Description = "specify root armature node name")]
            public string RootName { get; set; }

            [Option(Description = "specify mesh name to read bone data")]
            public string MeshName { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var assimp = new Assimp.AssimpContext();
                var scene = assimp.ImportFile(InputModel, Assimp.PostProcessSteps.None);

                Output = Path.GetFullPath(Output ?? Path.GetFileNameWithoutExtension(InputModel) + ".anb");

                Console.WriteLine($"Writing to: {Output}");

                bool IsMeshNameMatched(string meshName) =>
                    string.IsNullOrEmpty(MeshName)
                        ? true
                        : meshName == MeshName;

                var fbxMesh = scene.Meshes.First(mesh => IsMeshNameMatched(mesh.Name));
                var fbxArmatureRoot = scene.RootNode.FindNode(RootName ?? "bone000"); //"kh_sk"
                var fbxArmatureNodes = FlattenNodes(fbxArmatureRoot);
                var fbxArmatureBoneCount = fbxArmatureNodes.Length;

                var fbxAnim = scene.Animations.First();

                var raw = RawMotion.CreateEmpty();

                var frameCount = (int)fbxAnim.DurationInTicks;

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

                        var name = fbxArmatureNodes[boneIdx].ArmatureNode.Name;

                        var hit = fbxAnim.NodeAnimationChannels.FirstOrDefault(it => it.NodeName == name);

                        var translation = (hit == null)
                            ? new Vector3D(0, 0, 0)
                            : GetInterpolatedValue(hit.PositionKeys, frameIdx);

                        var rotation = (hit == null)
                            ? new Assimp.Quaternion(1, 0, 0, 0)
                            : GetInterpolatedValue(hit.RotationKeys, frameIdx);

                        var scale = (hit == null)
                            ? new Vector3D(1, 1, 1)
                            : GetInterpolatedValue(hit.ScalingKeys, frameIdx);

                        var absoluteMatrix = System.Numerics.Matrix4x4.Identity
                            * System.Numerics.Matrix4x4.CreateScale(ToDotNet(scale))
                            * System.Numerics.Matrix4x4.CreateFromQuaternion(ToDotNet(rotation))
                            * System.Numerics.Matrix4x4.CreateTranslation(ToDotNet(translation))
                            * parentMatrix;

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
                File.WriteAllBytes(Output + ".raw", rawMotionStream.ToArray());

                return 0;
            }

            private Assimp.Quaternion GetInterpolatedValue(List<QuaternionKey> keys, double time)
            {
                if (keys.Any())
                {
                    if (time < keys.First().Time)
                    {
                        return keys.First().Value;
                    }
                    if (keys.Last().Time < time)
                    {
                        return keys.Last().Value;
                    }

                    for (int x = 0, cx = keys.Count - 1; x < cx; x++)
                    {
                        var time0 = keys[x].Time;
                        var time1 = keys[x + 1].Time;

                        if (time0 <= time && time <= time1)
                        {
                            float ratio = (float)((time - time0) / (time1 - time0));

                            return Assimp.Quaternion.Slerp(
                                keys[x].Value,
                                keys[x + 1].Value,
                                ratio
                            );
                        }
                    }
                }

                return new Assimp.Quaternion(1, 0, 0, 0);
            }

            private Vector3D GetInterpolatedValue(List<VectorKey> keys, double time)
            {
                if (keys.Any())
                {
                    if (time < keys.First().Time)
                    {
                        return keys.First().Value;
                    }
                    if (keys.Last().Time < time)
                    {
                        return keys.Last().Value;
                    }

                    for (int x = 0, cx = keys.Count - 1; x < cx; x++)
                    {
                        var time0 = keys[x].Time;
                        var time1 = keys[x + 1].Time;

                        if (time0 <= time && time <= time1)
                        {
                            float ratio = (float)((time - time0) / (time1 - time0));

                            return (keys[x].Value * (1f - ratio)) + (keys[x + 1].Value * ratio);
                        }
                    }
                }

                return new Vector3D(0, 0, 0);
            }

            private System.Numerics.Quaternion ToDotNet(Assimp.Quaternion a) =>
                new System.Numerics.Quaternion(a.X, a.Y, a.Z, a.W);

            private Vector3 ToDotNet(Vector3D a) =>
                new Vector3(a.X, a.Y, a.Z);

            private record NodeRef
            {
                public int ParentIndex { get; set; }
                public Node ArmatureNode { get; set; }

                public NodeRef(int parentIndex, Node armatureNode)
                {
                    ParentIndex = parentIndex;
                    ArmatureNode = armatureNode;
                }
            }

            private NodeRef[] FlattenNodes(Node topNode)
            {
                var list = new List<NodeRef>();

                var stack = new Stack<NodeRef>();
                stack.Push(new NodeRef(-1, topNode));

                while (stack.Any())
                {
                    var nodeRef = stack.Pop();
                    var idx = list.Count;
                    list.Add(nodeRef);

                    foreach (var sub in nodeRef.ArmatureNode.Children.Reverse())
                    {
                        stack.Push(new NodeRef(idx, sub));
                    }
                }

                return list.ToArray();
            }
        }

        [HelpOption]
        [Command(Description = "raw anb file: to fbx")]
        private class ExportRawCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "anb input")]
            public string InputMotion { get; set; }

            [Argument(1, Description = "fbx output")]
            public string OutputFbx { get; set; }

            private class MotionSet
            {
                internal RawMotion raw;
                internal string name;
            }

            protected int OnExecute(CommandLineApplication app)
            {
                OutputFbx = Path.GetFullPath(OutputFbx ?? Path.GetFileNameWithoutExtension(InputMotion) + ".motion.fbx");

                Console.WriteLine($"Writing to: {OutputFbx}");

                var fileStream = new MemoryStream(
                    File.ReadAllBytes(InputMotion)
                );

                var motionSetList = new List<MotionSet>();

                string FilterName(string name) => name.Trim();

                var barFile = Bar.Read(fileStream);
                if (barFile.Any(it => it.Type == Bar.EntryType.Anb))
                {
                    // this is mset
                    motionSetList.AddRange(
                        barFile
                            .Where(barEntry => barEntry.Type == Bar.EntryType.Anb && barEntry.Stream.Length >= 16)
                            .SelectMany(
                                (barEntry, barEntryIndex) =>
                                    Bar.Read(barEntry.Stream)
                                        .Where(subBarEntry => subBarEntry.Type == Bar.EntryType.Motion)
                                        .Select(
                                            (subBarEntry, subBarEntryIndex) => new MotionSet
                                            {
                                                raw = new RawMotion(subBarEntry.Stream),
                                                name = $"{barEntryIndex}_{FilterName(barEntry.Name)}_{subBarEntryIndex}_{FilterName(subBarEntry.Name)}",
                                            }
                                        )
                            )
                    );
                }
                else if (barFile.Any(barEntry => barEntry.Type == Bar.EntryType.Motion))
                {
                    // this is anb
                    motionSetList.AddRange(
                        barFile
                            .Where(barEntry => barEntry.Type == Bar.EntryType.Motion)
                            .Select(
                                (barEntry, barEntryIndex) =>
                                    new MotionSet
                                    {
                                        raw = new RawMotion(barEntry.Stream),
                                        name = $"{barEntryIndex}_{FilterName(barEntry.Name)}",
                                    }
                            )
                            .ToArray()
                    );
                }
                else
                {
                    Console.Error.WriteLine("Error. Specify valid file of either mset or anb.");
                    return 1;
                }

                var raw = motionSetList.First().raw;

                Assimp.Scene scene = new Assimp.Scene();
                scene.RootNode = new Assimp.Node("RootNode");

                var mat = new Assimp.Material();
                mat.Name = "Dummy";

                var matIdx = scene.Materials.Count;
                scene.Materials.Add(mat);

                var fbxMesh = new Mesh($"Mesh", PrimitiveType.Polygon);
                fbxMesh.MaterialIndex = matIdx;

                var fbxBones = new List<Bone>();

                var fbxBoneCount = raw.RawMotionHeader.BoneCount;

                var fbxSkeletonRoot = new Node("Skeleton");
                scene.RootNode.Children.Add(fbxSkeletonRoot);

                for (int idx = 0; idx < fbxBoneCount; idx++)
                {
                    var matrix = raw.AnimationMatrices[idx];

                    var topVertIdx = fbxMesh.Vertices.Count;

                    void AddVert(float x, float y, float z) =>
                        fbxMesh.Vertices.Add(
                            new Assimp.Vector3D(x, y, z)
                        );

                    float boxLen = 1;

                    AddVert(-boxLen, -boxLen, -boxLen);
                    AddVert(+boxLen, -boxLen, -boxLen);
                    AddVert(-boxLen, +boxLen, -boxLen);
                    AddVert(+boxLen, +boxLen, -boxLen);
                    AddVert(-boxLen, -boxLen, +boxLen);
                    AddVert(+boxLen, -boxLen, +boxLen);
                    AddVert(-boxLen, +boxLen, +boxLen);
                    AddVert(+boxLen, +boxLen, +boxLen);

                    var bottomVertIdx = fbxMesh.Vertices.Count;

                    void AddFace(params int[] indices) =>
                        fbxMesh.Faces.Add(
                            new Face(
                                indices
                                    .Select(idx => topVertIdx + idx)
                                    .ToArray()
                            )
                        );

                    // left handed
                    AddFace(0, 1, 3, 2); // bottom
                    AddFace(4, 6, 7, 5); // top
                    AddFace(0, 4, 5, 1); // N
                    AddFace(3, 7, 5, 1); // E
                    AddFace(2, 6, 7, 3); // S
                    AddFace(0, 4, 6, 2); // W

                    var fbxBone = new Bone(
                        $"Bone{idx}",
                        Matrix3x3.Identity,
                        Enumerable.Range(topVertIdx, bottomVertIdx - topVertIdx)
                            .Select(idx => new VertexWeight(idx, 1))
                            .ToArray()
                    );
                    fbxBones.Add(fbxBone);
                    fbxMesh.Bones.Add(fbxBone);

                    var fbxSkeletonBone = new Node($"Bone{idx}");
                    fbxSkeletonRoot.Children.Add(fbxSkeletonBone);
                }

                scene.Meshes.Add(fbxMesh);
                scene.RootNode.MeshIndices.Add(scene.Meshes.Count - 1);

                foreach (var motionSet in motionSetList)
                {
                    var thisRaw = motionSet.raw;

                    var total = thisRaw.RawMotionHeader.TotalFrameCount;

                    var fbxAnim = new Assimp.Animation();
                    fbxAnim.Name = motionSet.name;
                    fbxAnim.DurationInTicks = total;
                    fbxAnim.TicksPerSecond = thisRaw.RawMotionHeader.FrameData.FramesPerSecond;

                    for (int boneIdx = 0; boneIdx < thisRaw.RawMotionHeader.BoneCount; boneIdx++)
                    {
                        var fbxAnimChannel = new NodeAnimationChannel();
                        fbxAnimChannel.NodeName = $"Bone{boneIdx}";

                        for (int step = 0; step < total; step++)
                        {
                            var time = step / fbxAnim.TicksPerSecond;

                            var matrix = thisRaw.AnimationMatrices[fbxBoneCount * step + boneIdx];

                            System.Numerics.Matrix4x4.Decompose(
                                matrix,
                                out Vector3 scale,
                                out System.Numerics.Quaternion rotation,
                                out Vector3 translation
                            );

                            fbxAnimChannel.PositionKeys.Add(
                                new VectorKey(
                                    time,
                                    new Vector3D(translation.X, translation.Y, translation.Z)
                                )
                            );

                            fbxAnimChannel.RotationKeys.Add(
                                new QuaternionKey(
                                    time,
                                    new Assimp.Quaternion(rotation.W, rotation.X, rotation.Y, rotation.Z)
                                )
                            );

                            fbxAnimChannel.ScalingKeys.Add(
                                new VectorKey(
                                    time,
                                    new Vector3D(scale.X, scale.Y, scale.Z)
                                )
                            );
                        }

                        fbxAnim.NodeAnimationChannels.Add(fbxAnimChannel);

                    }

                    scene.Animations.Add(fbxAnim);

                    // One animation per one fbx.
                    // Multiple animations can be stored.
                    // But Blender cannot import no more than one animation per one node.
                    Assimp.AssimpContext context = new Assimp.AssimpContext();
                    context.ExportFile(scene, OutputFbx + $".{motionSet.name}.fbx", "fbx");

                    scene.Animations.Remove(fbxAnim);
                }

                return 0;
            }
        }
    }
}
