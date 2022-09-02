using Assimp;
using McMaster.Extensions.CommandLineUtils;
using NLog;
using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Command.AnbMaker.Models;
using OpenKh.Command.AnbMaker.Utils;
using OpenKh.Common;
using OpenKh.Kh2;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using YamlDotNet.Core.Tokens;
using static OpenKh.Bbs.Bbsa;
using static OpenKh.Kh2.Motion;
using Key = OpenKh.Kh2.Motion.Key;

namespace OpenKh.Command.AnbMaker
{
    [Command("OpenKh.Command.AnbMaker")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(AnbCommand))]
    [Subcommand(typeof(ExportRawCommand))]
    [Subcommand(typeof(AnbExCommand))]
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
            finally
            {
                LogManager.Shutdown();
            }
        }

        private interface IFbxSourceItemSelector
        {
            string RootName { get; }
            string MeshName { get; }
            string AnimationName { get; }
        }

        private interface IMsetInjector
        {
            string MsetFile { get; }
            int MsetIndex { get; }
        }

        private class MsetInjector
        {
            internal void InjectMotionTo(IMsetInjector arg, byte[] motion)
            {
                if (string.IsNullOrEmpty(arg.MsetFile))
                {
                    return;
                }

                var logger = LogManager.GetLogger("MsetInjector");

                logger.Debug($"Going to inject new motion data into existing mset file");

                logger.Debug($"Loading {arg.MsetFile}");

                var (msetBar, msetLen) = File.OpenRead(arg.MsetFile).Using(stream => (Bar.Read(stream), stream.Length));

                logger.Debug($"{msetBar.Count} entries in mset");

                logger.Debug($"Locating bar entry #{arg.MsetIndex}");

                var msetBarEntry = msetBar[arg.MsetIndex];
                if (msetBarEntry.Type != Bar.EntryType.Anb)
                {
                    throw new Exception($"#{arg.MsetIndex}: {msetBarEntry.Type} must be EntryType.Anb!");
                }

                logger.Debug($"Loading anb");

                var anbBar = Bar.Read(msetBarEntry.Stream);

                logger.Debug($"{anbBar.Count} entries in anb");

                logger.Debug($"Locating bar entry having EntryType.Motion");

                var anbBarEntry = anbBar.Single(it => it.Type == Bar.EntryType.Motion);

                logger.Debug($"Found. Motion data: fromSize {anbBarEntry.Stream.Length:#,##0} newSize {motion.Length:#,##0}");

                anbBarEntry.Stream = new MemoryStream(motion).FromBegin();

                logger.Debug($"Packing new anb");

                var anbNewBarStream = new MemoryStream();
                Bar.Write(anbNewBarStream, anbBar);

                msetBarEntry.Stream = anbNewBarStream.FromBegin();

                logger.Debug($"Packing new mset");

                var msetBarNewStream = new MemoryStream();
                Bar.Write(msetBarNewStream, msetBar);

                logger.Debug($"Writing to mset");

                logger.Debug($"Mset file: fromSize {msetLen:#,##0} newSize {msetBarNewStream.Length:#,##0}");

                File.WriteAllBytes(
                    arg.MsetFile,
                    msetBarNewStream.ToArray()
                );

                logger.Debug($"Done");
            }
        }

        [HelpOption]
        [Command(Description = "fbx file: fbx to raw anb")]
        private class AnbCommand : IFbxSourceItemSelector, IMsetInjector
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
                raw.RawMotionHeader.FrameCount = (int)(frameCount * 64 / fbxAnim.TicksPerSecond); // in 64 fps?
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
                            * System.Numerics.Matrix4x4.CreateScale(scale.ToDotNetVector3())
                            * System.Numerics.Matrix4x4.CreateFromQuaternion(rotation.ToDotNetQuaternion())
                            * System.Numerics.Matrix4x4.CreateTranslation(translation.ToDotNetVector3())
                            * parentMatrix;

                        raw.AnimationMatrices.Add(absoluteMatrix);
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



        [HelpOption]
        [Command(Description = "fbx file: fbx to interpolated motion anb")]
        private class AnbExCommand : IFbxSourceItemSelector, IMsetInjector
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

                var ipm = InterpolatedMotion.CreateEmpty();

                var frameCount = (int)fbxAnim.DurationInTicks;

                ipm.InterpolatedMotionHeader.BoneCount = Convert.ToInt16(fbxArmatureBoneCount);
                ipm.InterpolatedMotionHeader.TotalBoneCount = Convert.ToInt16(fbxArmatureBoneCount);
                ipm.InterpolatedMotionHeader.FrameCount = (int)(frameCount * 64 / fbxAnim.TicksPerSecond); // in 64 fps?
                ipm.InterpolatedMotionHeader.FrameData.FrameStart = 0;
                ipm.InterpolatedMotionHeader.FrameData.FrameEnd = frameCount - 1;
                ipm.InterpolatedMotionHeader.FrameData.FramesPerSecond = (float)fbxAnim.TicksPerSecond;

                short AddKeyTime(float keyTime)
                {
                    var idx = ipm.KeyTimes.IndexOf(keyTime);
                    if (idx < 0)
                    {
                        idx = ipm.KeyTimes.Count;
                        ipm.KeyTimes.Add(keyTime);
                    }
                    return (short)Convert.ToUInt16(idx);
                }

                short AddKeyValue(float keyValue)
                {
                    var idx = ipm.KeyValues.IndexOf(keyValue);
                    if (idx < 0)
                    {
                        idx = ipm.KeyValues.Count;
                        ipm.KeyValues.Add(keyValue);
                    }
                    return (short)Convert.ToUInt16(idx);
                }

                static short CreateType_Time(Interpolation method, short keyValueIdx)
                {
                    if (16384 <= keyValueIdx)
                    {
                        throw new ArgumentOutOfRangeException("Too many KeyValue elements!");
                    }

                    return (short)((short)method | (short)(keyValueIdx << 2));
                    //return (short)((short)((short)(method) << 14) | (short)(keyValueIdx));
                }

                for (int boneIdx = 0; boneIdx < fbxArmatureBoneCount; boneIdx++)
                {
                    var name = fbxArmatureNodes[boneIdx].ArmatureNode.Name;

                    var animPosition = false;
                    var animRotation = false;
                    var animScaling = false;

                    var hit = fbxAnim.NodeAnimationChannels.FirstOrDefault(it => it.NodeName == name);
                    if (hit != null)
                    {
                        if (hit.PositionKeyCount != 0)
                        {
                            animPosition = true;

                            var numKeys = Convert.ToByte(hit.PositionKeyCount);

                            Key[] xKeys;
                            Key[] yKeys;
                            Key[] zKeys;

                            {
                                var lastKeyIdx = ipm.FCurveKeys.Count;

                                ipm.FCurvesForward.Add(
                                    new FCurve
                                    {
                                        JointId = Convert.ToInt16(boneIdx),
                                        Channel = (byte)(Channel.TRANSLATION_X),
                                        KeyStartId = Convert.ToInt16(lastKeyIdx),
                                        KeyCount = numKeys,
                                    }
                                );

                                ipm.FCurveKeys.AddRange(
                                    xKeys = Enumerable.Range(0, numKeys)
                                        .Select(_ => new Key { })
                                        .ToArray()
                                );
                            }

                            {
                                var lastKeyIdx = ipm.FCurveKeys.Count;

                                ipm.FCurvesForward.Add(
                                    new FCurve
                                    {
                                        JointId = Convert.ToInt16(boneIdx),
                                        Channel = (byte)(Channel.TRANSLATION_Y),
                                        KeyStartId = Convert.ToInt16(lastKeyIdx),
                                        KeyCount = numKeys,
                                    }
                                );

                                ipm.FCurveKeys.AddRange(
                                    yKeys = Enumerable.Range(0, numKeys)
                                        .Select(_ => new Key { })
                                        .ToArray()
                                );
                            }


                            {
                                var lastKeyIdx = ipm.FCurveKeys.Count;

                                ipm.FCurvesForward.Add(
                                    new FCurve
                                    {
                                        JointId = Convert.ToInt16(boneIdx),
                                        Channel = (byte)(Channel.TRANSLATION_Z),
                                        KeyStartId = Convert.ToInt16(lastKeyIdx),
                                        KeyCount = numKeys,
                                    }
                                );

                                ipm.FCurveKeys.AddRange(
                                    zKeys = Enumerable.Range(0, numKeys)
                                        .Select(_ => new Key { })
                                        .ToArray()
                                );
                            }

                            foreach (var (key, idx) in hit.PositionKeys.Select((key, idx) => (key, idx)))
                            {
                                var keyTimeIdx = AddKeyTime((float)key.Time);

                                xKeys[idx].Type_Time = CreateType_Time(Interpolation.Linear, keyTimeIdx);
                                yKeys[idx].Type_Time = CreateType_Time(Interpolation.Linear, keyTimeIdx);
                                zKeys[idx].Type_Time = CreateType_Time(Interpolation.Linear, keyTimeIdx);

                                xKeys[idx].ValueId = AddKeyValue(key.Value.X);
                                yKeys[idx].ValueId = AddKeyValue(key.Value.Y);
                                zKeys[idx].ValueId = AddKeyValue(key.Value.Z);
                            }
                        }

                        if (hit.RotationKeyCount != 0)
                        {
                            animRotation = true;

                            var numKeys = Convert.ToByte(hit.RotationKeyCount);

                            Key[] xKeys;
                            Key[] yKeys;
                            Key[] zKeys;

                            {
                                var lastKeyIdx = ipm.FCurveKeys.Count;

                                ipm.FCurvesForward.Add(
                                    new FCurve
                                    {
                                        JointId = Convert.ToInt16(boneIdx),
                                        Channel = (byte)(Channel.ROTATATION_X),
                                        KeyStartId = Convert.ToInt16(lastKeyIdx),
                                        KeyCount = numKeys,
                                    }
                                );

                                ipm.FCurveKeys.AddRange(
                                    xKeys = Enumerable.Range(0, numKeys)
                                        .Select(_ => new Key { })
                                        .ToArray()
                                );
                            }

                            {
                                var lastKeyIdx = ipm.FCurveKeys.Count;

                                ipm.FCurvesForward.Add(
                                    new FCurve
                                    {
                                        JointId = Convert.ToInt16(boneIdx),
                                        Channel = (byte)(Channel.ROTATATION_Y),
                                        KeyStartId = Convert.ToInt16(lastKeyIdx),
                                        KeyCount = numKeys,
                                    }
                                );

                                ipm.FCurveKeys.AddRange(
                                    yKeys = Enumerable.Range(0, numKeys)
                                        .Select(_ => new Key { })
                                        .ToArray()
                                );
                            }


                            {
                                var lastKeyIdx = ipm.FCurveKeys.Count;

                                ipm.FCurvesForward.Add(
                                    new FCurve
                                    {
                                        JointId = Convert.ToInt16(boneIdx),
                                        Channel = (byte)(Channel.ROTATATION_Z),
                                        KeyStartId = Convert.ToInt16(lastKeyIdx),
                                        KeyCount = numKeys,
                                    }
                                );

                                ipm.FCurveKeys.AddRange(
                                    zKeys = Enumerable.Range(0, numKeys)
                                        .Select(_ => new Key { })
                                        .ToArray()
                                );
                            }

                            foreach (var (key, idx) in hit.RotationKeys.Select((key, idx) => (key, idx)))
                            {
                                var keyTimeIdx = AddKeyTime((float)key.Time);

                                xKeys[idx].Type_Time = CreateType_Time(Interpolation.Linear, keyTimeIdx);
                                yKeys[idx].Type_Time = CreateType_Time(Interpolation.Linear, keyTimeIdx);
                                zKeys[idx].Type_Time = CreateType_Time(Interpolation.Linear, keyTimeIdx);

                                var angles = ToEulerAngles(key.Value);

                                static float FromEulerAngle(float angle) => angle;

                                xKeys[idx].ValueId = AddKeyValue(FromEulerAngle(angles.X));
                                yKeys[idx].ValueId = AddKeyValue(FromEulerAngle(angles.Y));
                                zKeys[idx].ValueId = AddKeyValue(FromEulerAngle(angles.Z));
                            }
                        }

                        if (hit.ScalingKeyCount != 0)
                        {
                            animScaling = true;

                            var numKeys = Convert.ToByte(hit.ScalingKeyCount);

                            Key[] xKeys;
                            Key[] yKeys;
                            Key[] zKeys;

                            {
                                var lastKeyIdx = ipm.FCurveKeys.Count;

                                ipm.FCurvesForward.Add(
                                    new FCurve
                                    {
                                        JointId = Convert.ToInt16(boneIdx),
                                        Channel = (byte)(Channel.SCALE_X),
                                        KeyStartId = Convert.ToInt16(lastKeyIdx),
                                        KeyCount = numKeys,
                                    }
                                );

                                ipm.FCurveKeys.AddRange(
                                    xKeys = Enumerable.Range(0, numKeys)
                                        .Select(_ => new Key { })
                                        .ToArray()
                                );
                            }

                            {
                                var lastKeyIdx = ipm.FCurveKeys.Count;

                                ipm.FCurvesForward.Add(
                                    new FCurve
                                    {
                                        JointId = Convert.ToInt16(boneIdx),
                                        Channel = (byte)(Channel.SCALE_Y),
                                        KeyStartId = Convert.ToInt16(lastKeyIdx),
                                        KeyCount = numKeys,
                                    }
                                );

                                ipm.FCurveKeys.AddRange(
                                    yKeys = Enumerable.Range(0, numKeys)
                                        .Select(_ => new Key { })
                                        .ToArray()
                                );
                            }


                            {
                                var lastKeyIdx = ipm.FCurveKeys.Count;

                                ipm.FCurvesForward.Add(
                                    new FCurve
                                    {
                                        JointId = Convert.ToInt16(boneIdx),
                                        Channel = (byte)(Channel.SCALE_Z),
                                        KeyStartId = Convert.ToInt16(lastKeyIdx),
                                        KeyCount = numKeys,
                                    }
                                );

                                ipm.FCurveKeys.AddRange(
                                    zKeys = Enumerable.Range(0, numKeys)
                                        .Select(_ => new Key { })
                                        .ToArray()
                                );
                            }

                            foreach (var (key, idx) in hit.ScalingKeys.Select((key, idx) => (key, idx)))
                            {
                                var keyTimeIdx = AddKeyTime((float)key.Time);

                                xKeys[idx].Type_Time = CreateType_Time(Interpolation.Linear, keyTimeIdx);
                                yKeys[idx].Type_Time = CreateType_Time(Interpolation.Linear, keyTimeIdx);
                                zKeys[idx].Type_Time = CreateType_Time(Interpolation.Linear, keyTimeIdx);

                                xKeys[idx].ValueId = AddKeyValue(key.Value.X);
                                yKeys[idx].ValueId = AddKeyValue(key.Value.Y);
                                zKeys[idx].ValueId = AddKeyValue(key.Value.Z);
                            }
                        }
                    }

                    ipm.Joints.Add(
                        new Joint
                        {
                            JointId = Convert.ToInt16(boneIdx),
                            Flags = (byte)((animPosition || animRotation || animScaling) ? (2 | 4 | 16 | 32) : 0),
                        }
                    );
                }

                logger.Debug($"{ipm.ConstraintActivations.Count:#,##0} ConstraintActivations");
                logger.Debug($"{ipm.Constraints.Count:#,##0} Constraints");
                logger.Debug($"{ipm.ExpressionNodes.Count:#,##0} ExpressionNodes");
                logger.Debug($"{ipm.Expressions.Count:#,##0} Expressions");
                logger.Debug($"{ipm.ExternalEffectors.Count:#,##0} ExternalEffectors");
                logger.Debug($"{ipm.FCurveKeys.Count:#,##0} FCurveKeys");
                logger.Debug($"{ipm.FCurvesForward.Count:#,##0} FCurvesForward");
                logger.Debug($"{ipm.FCurvesInverse.Count:#,##0} FCurvesInverse");
                logger.Debug($"{ipm.IKHelpers.Count:#,##0} IKHelpers");
                logger.Debug($"{ipm.InitialPoses.Count:#,##0} InitialPoses");
                logger.Debug($"{ipm.Joints.Count:#,##0} Joints");
                logger.Debug($"{ipm.KeyTangents.Count:#,##0} KeyTangents");
                logger.Debug($"{ipm.KeyTimes.Count:#,##0} KeyTimes");
                logger.Debug($"{ipm.KeyValues.Count:#,##0} KeyValues");

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

            /// <summary>
            /// ToEulerAngles
            /// </summary>
            /// <see cref="https://stackoverflow.com/a/70462919"/>
            private static Vector3 ToEulerAngles(Assimp.Quaternion q)
            {
                Vector3 angles = new();

                // roll / x
                double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
                double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
                angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

                // pitch / y
                double sinp = 2 * (q.W * q.Y - q.Z * q.X);
                if (Math.Abs(sinp) >= 1)
                {
                    angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
                }
                else
                {
                    angles.Y = (float)Math.Asin(sinp);
                }

                // yaw / z
                double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
                double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
                angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

                return angles;
            }
        }

        [HelpOption]
        [Command(Description = "raw anb file: bone and animation to fbx")]
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
}
