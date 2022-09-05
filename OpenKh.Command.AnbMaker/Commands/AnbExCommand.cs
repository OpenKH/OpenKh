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
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Command.AnbMaker.Commands
{
    [HelpOption]
    [Command(Description = "fbx file: fbx to interpolated motion anb")]
    internal class AnbExCommand : IFbxSourceItemSelector, IMsetInjector
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

        [Option(Description = "specify animation name to read bone data", ShortName = "a")]
        public string AnimationName { get; set; }

        [Option(Description = "apply scaling to each source node", ShortName = "x")]
        public float NodeScaling { get; set; } = 1;

        [Option(Description = "optionally inject new motion into mset directly", ShortName = "w")]
        public string MsetFile { get; set; }

        [Option(Description = "zero based target index of bar entry in mset file", ShortName = "i")]
        public int MsetIndex { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var logger = LogManager.GetLogger("InterpolatedMotionMaker");

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
            var fbxArmatureNodes = AssimpHelper.FlattenNodes(fbxArmatureRoot);
            var fbxArmatureBoneCount = fbxArmatureNodes.Length;

            bool IsAnimationNameMatched(string animName) =>
                string.IsNullOrEmpty(AnimationName)
                    ? true
                    : animName == AnimationName;

            var fbxAnim = scene.Animations.First(anim => IsAnimationNameMatched(anim.Name));

            AScalarKey GetAScalarKey(double time, float value) =>
                new AScalarKey
                {
                    Time = (float)time,
                    Value = value,
                };

            var builder = new InterpolatedMotionBuilder(
                (int)fbxAnim.DurationInTicks,
                (float)fbxAnim.TicksPerSecond,
                fbxArmatureBoneCount,
                NodeScaling,
                boneIdx =>
                {
                    var name = fbxArmatureNodes[boneIdx].ArmatureNode.Name;
                    var hit = fbxAnim.NodeAnimationChannels.FirstOrDefault(it => it.NodeName == name);
                    if (hit != null)
                    {
                        var pseudoRotations = hit.RotationKeys
                            .Select(
                                key => new PseudoRotation
                                {
                                    time = key.Time,
                                    rotation = key.Value.ToDotNetQuaternion().ToEulerAngles()
                                }
                            )
                            .ToArray();

                        return new AChannel
                        {
                            ScaleXKeys = hit.ScalingKeys
                                .Select(it => GetAScalarKey(it.Time, it.Value.X))
                                .ToArray(),

                            ScaleYKeys = hit.ScalingKeys
                                .Select(it => GetAScalarKey(it.Time, it.Value.Y))
                                .ToArray(),

                            ScaleZKeys = hit.ScalingKeys
                                .Select(it => GetAScalarKey(it.Time, it.Value.Z))
                                .ToArray(),

                            RotationXKeys = pseudoRotations
                                .Select(it => GetAScalarKey(it.time, it.rotation.X))
                                .ToArray(),
                            RotationYKeys = pseudoRotations
                                .Select(it => GetAScalarKey(it.time, it.rotation.Y))
                                .ToArray(),
                            RotationZKeys = pseudoRotations
                                .Select(it => GetAScalarKey(it.time, it.rotation.Z))
                                .ToArray(),

                            PositionXKeys = hit.PositionKeys
                                .Select(it => GetAScalarKey(it.Time, it.Value.X))
                                .ToArray(),

                            PositionYKeys = hit.PositionKeys
                                .Select(it => GetAScalarKey(it.Time, it.Value.Y))
                                .ToArray(),

                            PositionZKeys = hit.PositionKeys
                                .Select(it => GetAScalarKey(it.Time, it.Value.Z))
                                .ToArray(),
                        };
                    }

                    return null;
                }
            );

            var ipm = builder.ipm;

            logger.Debug($"{ipm.ConstraintActivations.Count,6:#,##0} ConstraintActivations");
            logger.Debug($"{ipm.Constraints.Count,6:#,##0} Constraints");
            logger.Debug($"{ipm.ExpressionNodes.Count,6:#,##0} ExpressionNodes");
            logger.Debug($"{ipm.Expressions.Count,6:#,##0} Expressions");
            logger.Debug($"{ipm.ExternalEffectors.Count,6:#,##0} ExternalEffectors");
            logger.Debug($"{ipm.FCurveKeys.Count,6:#,##0} FCurveKeys");
            logger.Debug($"{ipm.FCurvesForward.Count,6:#,##0} FCurvesForward");
            logger.Debug($"{ipm.FCurvesInverse.Count,6:#,##0} FCurvesInverse");
            logger.Debug($"{ipm.IKHelpers.Count,6:#,##0} IKHelpers");
            logger.Debug($"{ipm.InitialPoses.Count,6:#,##0} InitialPoses");
            logger.Debug($"{ipm.Joints.Count,6:#,##0} Joints");
            logger.Debug($"{ipm.KeyTangents.Count,6:#,##0} KeyTangents");
            logger.Debug($"{ipm.KeyTimes.Count,6:#,##0} KeyTimes");
            logger.Debug($"{ipm.KeyValues.Count,6:#,##0} KeyValues");

            var motionStream = (MemoryStream)ipm.toStream();

            var anbBarStream = new MemoryStream();
            Bar.Write(
                anbBarStream,
                new Bar.Entry[]
                {
                        new Bar.Entry
                        {
                            Type = Bar.EntryType.Motion,
                            Name = "A999",
                            Stream = motionStream,
                        }
                }
            );

            File.WriteAllBytes(Output, anbBarStream.ToArray());
            File.WriteAllBytes(Output + ".raw", motionStream.ToArray());

            logger.Debug($"Motion data generation successful");

            new MsetInjector().InjectMotionTo(this, motionStream.ToArray());

            return 0;
        }

        private class PseudoRotation
        {
            internal double time;
            internal Vector3 rotation;
        }
    }
}
