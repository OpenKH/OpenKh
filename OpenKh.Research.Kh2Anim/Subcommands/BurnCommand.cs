using McMaster.Extensions.CommandLineUtils;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Kh2Anim.Mset.Interfaces;
using OpenKh.Research.Kh2Anim.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Research.Kh2Anim.Subcommands
{
    [HelpOption]
    [Command(Description = "Burn hard coded motion and print computed (x, y) of bones.")]
    public class BurnCommand
    {
        [Option(CommandOptionType.SingleValue, ShortName = "o", Description = "Output file: model.mset")]
        public string OutputMset { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var FramePerSecond = 30f;
            var Pre = 0;
            var Post = 0;
            var FrameCount = 0;
            var FrameEnd = 100;
            var FrameLoop = 0;
            var TotalFrameCount = 100;

            var mdlxFile = new MemoryStream(File.ReadAllBytes("rawData/5_Nodes.mdlx"));

            // bones:
            //   mdlx 0 1 2 3 4
            //   mset 5 6 7 8

            var motion = Motion.CreateInterpolatedFromScratch();
            var interpolated = motion.Interpolated;
            {
                interpolated.IKHelpers.Add(
                    new IKHelperTable
                    {
                        Index = 5,
                        ParentIndex = -1,
                        ScaleX = 2,
                        ScaleY = 2,
                        ScaleZ = 1,
                    }
                );
                interpolated.IKHelpers.Add(
                    new IKHelperTable
                    {
                        Index = 6,
                        ParentIndex = 5,
                        ScaleX = 2,
                        ScaleY = 2,
                        ScaleZ = 1,
                        TranslateY = 10,
                    }
                );
                interpolated.IKHelpers.Add(
                    new IKHelperTable
                    {
                        Index = 7,
                        ParentIndex = 6,
                        ScaleX = 2,
                        ScaleY = 2,
                        ScaleZ = 1,
                        TranslateY = 10,
                    }
                );
                interpolated.IKHelpers.Add(
                    new IKHelperTable
                    {
                        Index = 8,
                        ParentIndex = 7,
                        ScaleX = 2,
                        ScaleY = 2,
                        ScaleZ = 1,
                        TranslateY = 400,
                    }
                );
            }
            {
                interpolated.IKChains.Add(
                    new IKChainTable
                    {
                        Unk00 = 1,
                        Unk01 = 1,
                        ModelBoneIndex = 2,
                        IKHelperIndex = 6,
                        Table8Index = -1,
                    }
                );
                //interpolated.IKChains.Add(
                //    new IKChainTable
                //    {
                //        Unk00 = 3,
                //        Unk01 = 1,
                //        ModelBoneIndex = 3,
                //        IKHelperIndex = 8,
                //        Table8Index = -1,
                //    }
                //);
                interpolated.Joints.Add(new JointTable { JointIndex = 5, });
                interpolated.Joints.Add(new JointTable { JointIndex = 6, });
                interpolated.Joints.Add(new JointTable { JointIndex = 7, });
                interpolated.Joints.Add(new JointTable { JointIndex = 8, });
                interpolated.Joints.Add(new JointTable { JointIndex = 0, });
                interpolated.Joints.Add(new JointTable { JointIndex = 1, });
                interpolated.Joints.Add(new JointTable { JointIndex = 2, });
                interpolated.Joints.Add(new JointTable { JointIndex = 3, });
                interpolated.Joints.Add(new JointTable { JointIndex = 4, });
            }
            {
                interpolated.Timeline.Add(new TimelineTable { KeyFrame = 0, Value = 0, Interpolation = Interpolation.Linear });
                interpolated.Timeline.Add(new TimelineTable { KeyFrame = 20, Value = -6.28f, Interpolation = Interpolation.Linear });
                interpolated.Timeline.Add(new TimelineTable { KeyFrame = 40, Value = 0, Interpolation = Interpolation.Linear });
                interpolated.Timeline.Add(new TimelineTable { KeyFrame = 60, Value = 0, Interpolation = Interpolation.Linear });
                interpolated.Timeline.Add(new TimelineTable { KeyFrame = 80, Value = 0, Interpolation = Interpolation.Linear });
                interpolated.IKHelperAnimation.Add(
                    new BoneAnimationTable
                    {
                        JointIndex = 2, // 5 +x
                        Channel = (byte)(5 | ((3 & Pre) << 4) | ((3 & Post) << 6)),
                        TimelineStartIndex = 0,
                        TimelineCount = 5,
                    }
                );

                //interpolated.Timeline.Add(new TimelineTable { KeyFrame = 0, Value = -100f, Interpolation = Interpolation.Linear });
                //interpolated.Timeline.Add(new TimelineTable { KeyFrame = 20, Value = 0f, Interpolation = Interpolation.Linear });
                //interpolated.Timeline.Add(new TimelineTable { KeyFrame = 40, Value = 100f, Interpolation = Interpolation.Linear });
                //interpolated.Timeline.Add(new TimelineTable { KeyFrame = 60, Value = 0f, Interpolation = Interpolation.Linear });
                //interpolated.Timeline.Add(new TimelineTable { KeyFrame = 80, Value = -100f, Interpolation = Interpolation.Linear });
                //interpolated.IKHelperAnimation.Add(
                //    new BoneAnimationTable
                //    {
                //        JointIndex = 2, // 5 +x
                //        Channel = (byte)(4 | ((3 & Pre) << 4) | ((3 & Post) << 6)),
                //        TimelineStartIndex = 5,
                //        TimelineCount = 5,
                //    }
                //);
            }
            interpolated.BoundingBoxMaxX = 1000;
            interpolated.BoundingBoxMaxY = 1000;
            interpolated.BoundingBoxMaxZ = 1000;
            interpolated.BoundingBoxMinX = -1000;
            interpolated.BoundingBoxMinY = -1000;
            interpolated.BoundingBoxMinZ = -1000;
            interpolated.BoneCount = 5;
            interpolated.FrameCount = FrameCount;
            interpolated.FrameEnd = FrameEnd;
            interpolated.FrameLoop = FrameLoop;
            interpolated.FramePerSecond = FramePerSecond;
            interpolated.TotalFrameCount = TotalFrameCount;
            interpolated.Footer.Unknown = new int[9];

            var motionBin = new MemoryStream();
            Motion.Write(motionBin, motion);

            motionBin.Position = 0;

            var anbFile = new MemoryStream();

            var anbBarEntries = new Bar.Entry[]
            {
                new Bar.Entry
                {
                    Type = Bar.EntryType.Motion,
                    Stream = motionBin,
                    Name = "A000",
                },
            };

            Bar.Write(
                anbFile,
                anbBarEntries
            );

            anbFile.Position = 0;

            var msetFile = new MemoryStream();

            Bar.Write(
                msetFile,
                new Bar.Entry[]
                {
                    new Bar.Entry
                    {
                        Type = Bar.EntryType.Anb,
                        Stream = anbFile,
                        Name = "A000",
                    },
                }
            );

            msetFile.Position = 0;

            if (!string.IsNullOrEmpty(OutputMset))
            {
                File.WriteAllBytes(OutputMset, msetFile.ToArray());
            }

            motionBin.Position = 0;

            var animReader = new AnimReader(motionBin);

            mdlxFile.Position = 0;

            var provider = (IAnimMatricesProvider)new EmuBasedAnimMatricesProvider(animReader, mdlxFile, motionBin);
            for (int x = 0; x <= 100; x++)
            {
                var matrices = provider.ProvideMatrices((x == 0) ? 0 : 1);

                Console.WriteLine(
                    string.Join(
                        ",",
                        matrices
                            .Select(
                                it =>
                                {
                                    var pos = Vector3.Transform(Vector3.Zero, it);
                                    return $"({pos.X,5:0},{pos.Y,5:0})";
                                }
                            )
                    )
                );
            }

            return 0;
        }
    }
}
