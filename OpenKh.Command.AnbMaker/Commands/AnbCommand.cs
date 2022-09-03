using Assimp;
using McMaster.Extensions.CommandLineUtils;
using NLog;
using OpenKh.Command.AnbMaker.Commands.Interfaces;
using OpenKh.Command.AnbMaker.Commands.Utils;
using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Command.AnbMaker.Utils;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(InputModel, Assimp.PostProcessSteps.None);

            Output = Path.GetFullPath(Output ?? Path.GetFileNameWithoutExtension(InputModel) + ".anb");

            var nodeFix = System.Numerics.Matrix4x4.CreateScale(
                NodeScaling
            );

            Console.WriteLine($"Writing to: {Output}");

            bool IsMeshNameMatched(string meshName) =>
                string.IsNullOrEmpty(MeshName)
                    ? true
                    : meshName == MeshName;

            var fbxMesh = scene.Meshes.First(mesh => IsMeshNameMatched(mesh.Name));
            var fbxArmatureRoot = scene.RootNode.FindNode(RootName ?? "bone000"); //"kh_sk"
            var fbxArmatureNodes = AssimpHelper.FlattenNodes(fbxArmatureRoot);
            var fbxArmatureBoneCount = fbxArmatureNodes.Length;

            bool IsAnimationNameMatched(string animName) =>
                string.IsNullOrEmpty(AnimationName)
                    ? true
                    : animName == AnimationName;

            var fbxAnim = scene.Animations.First(anim => IsAnimationNameMatched(anim.Name));

            var raw = RawMotion.CreateEmpty();

            var frameCount = (int)fbxAnim.DurationInTicks;

            raw.RawMotionHeader.BoneCount = fbxArmatureBoneCount;
            raw.RawMotionHeader.FrameCount = (int)(frameCount * 60 / fbxAnim.TicksPerSecond); // in 1/60 sec
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
                        : hit.PositionKeys.GetInterpolatedVector(frameIdx);

                    var rotation = (hit == null)
                        ? new Assimp.Quaternion(w: 1, 0, 0, 0)
                        : hit.RotationKeys.GetInterpolatedQuaternion(frameIdx);

                    var scale = (hit == null)
                        ? new Vector3D(1, 1, 1)
                        : hit.ScalingKeys.GetInterpolatedVector(frameIdx);

                    var absoluteMatrix = System.Numerics.Matrix4x4.Identity
                        * System.Numerics.Matrix4x4.CreateFromQuaternion(rotation.ToDotNetQuaternion())
                        * System.Numerics.Matrix4x4.CreateScale(scale.ToDotNetVector3())
                        * System.Numerics.Matrix4x4.CreateTranslation(translation.ToDotNetVector3())
                        * parentMatrix;

                    raw.AnimationMatrices.Add(nodeFix * absoluteMatrix);
                    matrices.Add(absoluteMatrix);
                }
            }

            logger.Debug($"(frameCount {frameCount:#,##0}) x (boneCount {fbxArmatureBoneCount:#,##0}) -> {raw.AnimationMatrices.Count:#,##0} matrices ({64 * raw.AnimationMatrices.Count:#,##0} bytes)");

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

            logger.Debug("Raw motion data generation successful");

            new MsetInjector().InjectMotionTo(this, rawMotionStream.ToArray());

            return 0;
        }
    }
}
