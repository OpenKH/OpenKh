using Assimp;
using McMaster.Extensions.CommandLineUtils;
using NLog;
using OpenKh.Command.AnbMaker.Commands.Interfaces;
using OpenKh.Command.AnbMaker.Commands.Utils;
using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Command.AnbMaker.Utils;
using OpenKh.Command.AnbMaker.Utils.Builder;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Command.AnbMaker.Utils.Builder.RawMotionBuilder;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Command.AnbMaker.Commands
{
    [HelpOption]
    [Command(Description = "fbx file: fbx to raw anb")]
    internal class AnbCommand : IFbxSourceItemSelector, IMsetInjector
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "fbx input")]
        public string InputModel { get; set; }

        [Argument(1, Description = "anb output")]
        public string Output { get; set; }

        [Option(Description = "specify root armature node name", ShortName = "r")]
        public string RootName { get; set; }

        [Option(Description = "specify mesh name to read bone data", ShortName = "m")]
        public string MeshName { get; set; }

        [Option(Description = "apply scaling to each source node", ShortName = "x")]
        public float NodeScaling { get; set; } = 1;

        [Option(Description = "specify animation name to read bone data", ShortName = "a")]
        public string AnimationName { get; set; }

        [Option(Description = "optionally inject new motion into mset directly", ShortName = "w")]
        public string MsetFile { get; set; }

        [Option(Description = "zero based target index of bar entry in mset file", ShortName = "i")]
        public int MsetIndex { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var logger = LogManager.GetLogger("RawMotionMaker");

            Output = Path.GetFullPath(Output ?? Path.GetFileNameWithoutExtension(InputModel) + ".anb");

            Console.WriteLine($"Writing to: {Output}");

            var parms = new UseAssimpForRaw(
                InputModel,
                MeshName,
                RootName,
                AnimationName,
                NodeScaling
            )
                .Parameters;

            foreach (var parm in parms.Take(1))
            {
                var builder = new RawMotionBuilder(
                    parm
                );

                logger.Debug($"(frameCount {parm.DurationInTicks:#,##0}) x (boneCount {parm.BoneCount:#,##0}) -> {builder.Raw.AnimationMatrices.Count:#,##0} matrices ({64 * builder.Raw.AnimationMatrices.Count:#,##0} bytes)");

                var rawMotionStream = new MemoryStream();
                RawMotion.Write(rawMotionStream, builder.Raw);

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

                logger.Debug("Raw motion data generation successful");

                new MsetInjector().InjectMotionTo(this, rawMotionStream.ToArray());
            }

            return 0;
        }

        internal class UseAssimpForRaw
        {
            public IEnumerable<RawMotionBuilder.Parameter> Parameters { get; }

            public UseAssimpForRaw(
                string inputModel,
                string meshName,
                string rootName,
                string animationName,
                float nodeScaling
            )
            {
                var assimp = new Assimp.AssimpContext();
                var scene = assimp.ImportFile(inputModel, Assimp.PostProcessSteps.None);

                var outputList = new List<RawMotionBuilder.Parameter>();

                bool IsMeshNameMatched(string it) =>
                    string.IsNullOrEmpty(meshName)
                        ? true
                        : it == meshName;

                bool IsAnimationNameMatched(string it) =>
                    string.IsNullOrEmpty(animationName)
                        ? true
                        : it == animationName;

                foreach (var fbxMesh in scene.Meshes.Where(mesh => IsMeshNameMatched(mesh.Name)))
                {
                    var fbxArmatureRoot = scene.RootNode.FindNode(rootName ?? "bone000"); //"kh_sk"
                    var fbxArmatureNodes = AssimpHelper.FlattenNodes(fbxArmatureRoot);
                    var fbxArmatureBoneCount = fbxArmatureNodes.Length;

                    foreach (var fbxAnim in scene.Animations.Where(anim => IsAnimationNameMatched(anim.Name)))
                    {
                        outputList.Add(
                            new Parameter
                            {
                                DurationInTicks = (int)fbxAnim.DurationInTicks,
                                TicksPerSecond = (float)fbxAnim.TicksPerSecond,
                                BoneCount = fbxArmatureNodes.Length,
                                NodeScaling = nodeScaling,
                                Bones = fbxArmatureNodes
                                    .Select(
                                        node => new ABone
                                        {
                                            NodeName = node.ArmatureNode.Name,
                                            ParentIndex = node.ParentIndex,
                                        }
                                    )
                                    .ToArray(),

                                GetRelativeMatrix = (frameIdx, boneIdx) =>
                                {
                                    var name = fbxArmatureNodes[boneIdx].ArmatureNode.Name;

                                    var hit = fbxAnim.NodeAnimationChannels.FirstOrDefault(it => it.NodeName == name);

                                    var translation = (hit == null)
                                        ? new Vector3D(0, 0, 0)
                                        : hit.PositionKeys.GetInterpolatedVector(frameIdx);

                                    var rotation = (hit == null)
                                        ? new Assimp.Quaternion(w: 1, 0, 0, 0)
                                        : hit.RotationKeys.GetInterpolatedQuaternion(frameIdx);

                                    var scale = (hit == null)
                                        ? new Vector3D(1, 1, 1)
                                        : hit.ScalingKeys.GetInterpolatedVector(frameIdx);

                                    return System.Numerics.Matrix4x4.Identity
                                        * System.Numerics.Matrix4x4.CreateFromQuaternion(rotation.ToDotNetQuaternion())
                                        * System.Numerics.Matrix4x4.CreateScale(scale.ToDotNetVector3())
                                        * System.Numerics.Matrix4x4.CreateTranslation(translation.ToDotNetVector3())
                                        ;
                                }
                            }
                        );
                    }
                }

                Parameters = outputList;
            }
        }
    }
}
