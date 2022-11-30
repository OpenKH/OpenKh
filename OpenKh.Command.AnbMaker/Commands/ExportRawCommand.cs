using Assimp;
using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Command.AnbMaker.Utils;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Command.AnbMaker.Commands
{
    [HelpOption]
    [Command(Description = "raw anb file: bone and animation to fbx")]
    internal class ExportRawCommand
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

            var sampleRaw = motionSetList.First().raw;

            var sampleExport = new RawMotionExporter(sampleRaw).Export;

            Assimp.Scene scene = new Assimp.Scene();
            scene.RootNode = new Assimp.Node("RootNode");

            var mat = new Assimp.Material();
            mat.Name = "Dummy";

            var matIdx = scene.Materials.Count;
            scene.Materials.Add(mat);

            var fbxMesh = new Mesh($"Mesh", PrimitiveType.Polygon);
            fbxMesh.MaterialIndex = matIdx;

            var fbxBones = new List<Bone>();

            var fbxBoneCount = sampleExport.BoneCount;

            var fbxSkeletonRoot = new Node("Skeleton");
            scene.RootNode.Children.Add(fbxSkeletonRoot);

            foreach (var boneIdx in Enumerable.Range(0, sampleExport.BoneCount))
            {
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
                    $"Bone{boneIdx}",
                    Matrix3x3.Identity,
                    Enumerable.Range(topVertIdx, bottomVertIdx - topVertIdx)
                        .Select(idx => new VertexWeight(idx, 1))
                        .ToArray()
                );
                fbxBones.Add(fbxBone);
                fbxMesh.Bones.Add(fbxBone);

                var fbxSkeletonBone = new Node($"Bone{boneIdx}");
                fbxSkeletonRoot.Children.Add(fbxSkeletonBone);
            }

            scene.Meshes.Add(fbxMesh);
            scene.RootNode.MeshIndices.Add(scene.Meshes.Count - 1);

            foreach (var motionSet in motionSetList)
            {
                var thisExport = new RawMotionExporter(motionSet.raw).Export;

                var fbxAnim = new Assimp.Animation();
                fbxAnim.Name = motionSet.name;
                fbxAnim.DurationInTicks = thisExport.FrameCount;
                fbxAnim.TicksPerSecond = thisExport.FramesPerSecond;

                foreach (var (bone, boneIdx) in thisExport.Bones.Select((bone, boneIdx) => (bone, boneIdx)))
                {
                    var fbxAnimChannel = new NodeAnimationChannel();
                    fbxAnimChannel.NodeName = $"Bone{boneIdx}";

                    foreach (var frame in bone.KeyFrames)
                    {
                        var matrix = frame.AbsoluteMatrix;

                        System.Numerics.Matrix4x4.Decompose(
                            matrix,
                            out Vector3 scale,
                            out System.Numerics.Quaternion rotation,
                            out Vector3 translation
                        );

                        fbxAnimChannel.PositionKeys.Add(
                            new VectorKey(
                                frame.KeyTime,
                                translation.ToAssimpVector3D()
                            )
                        );

                        fbxAnimChannel.RotationKeys.Add(
                            new QuaternionKey(
                                frame.KeyTime,
                                rotation.ToAssimpQuaternion()
                            )
                        );

                        fbxAnimChannel.ScalingKeys.Add(
                            new VectorKey(
                                frame.KeyTime,
                                scale.ToAssimpVector3D()
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
