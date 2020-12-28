using McMaster.Extensions.CommandLineUtils;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Kh2Anim.Mset.Interfaces;
using OpenKh.Research.Kh2Anim.Models;
using OpenKh.Research.Kh2Anim.TypeConverters;
using OpenKh.Research.Kh2Anim.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Research.Kh2Anim.Subcommands
{
    [HelpOption]
    [Command(Description = "Fry fake motion")]
    class FryCommand
    {
        [Option(CommandOptionType.SingleValue, ShortName = "o", Description = "Output file: model.mset")]
        public string OutputMset { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "l", Description = "Unknown")]
        public float FrameLoop { get; set; } = 0;

        [Option(CommandOptionType.SingleValue, ShortName = "e", Description = "Unknown")]
        public float FrameEnd { get; set; } = 100;

        [Option(CommandOptionType.SingleValue, ShortName = "c", Description = "Unknown")]
        public float FrameCount { get; set; } = 0;

        [Option(CommandOptionType.SingleValue, ShortName = "a", Description = "Unknown")]
        public int TotalFrameCount { get; set; } = 100;

        [Option(CommandOptionType.SingleValue, ShortName = "f", Description = "FPS")]
        public float FramePerSecond { get; set; } = 1;

        [Option(CommandOptionType.MultipleValue, ShortName = "t", Description = "Interpolation,FrameTime,Value,TangentEaseIn,TangentEaseOut\n(Nearest|Linear|Hermite|Zero),0.00,0.00,0.00,0.00")]
        public string[] Timeline { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "p", Description = "Output gnuplot file set with this file path prefix")]
        public string Prefix { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "u", Description = "Unknown")]
        public byte Pre { get; set; } = 0;

        [Option(CommandOptionType.SingleValue, ShortName = "v", Description = "Unknown")]
        public byte Post { get; set; } = 0;

        [Option(CommandOptionType.SingleValue, ShortName = "n", Description = "StaticPose value")]
        public float InitialValue { get; set; } = 0;

        [Option(CommandOptionType.SingleValue, ShortName = "m", Description = "The value directly set to matrix")]
        public float MatrixValue { get; set; } = 0;

        protected int OnExecute(CommandLineApplication app)
        {
            FramePerSecond = Math.Max(1, FramePerSecond);

            if (Timeline == null || Timeline?.Length == 0)
            {
                Timeline = new string[] { $"Linear,{FrameLoop},0,0,0", $"Linear,{FrameEnd},100,0,0" };
            }

            var mdlxFile = MdlxMaker.CreateMdlxHaving2Bones(tx: MatrixValue);

            var motion = Motion.CreateInterpolatedFromScratch();
            var interpolated = motion.Interpolated;
            interpolated.ModelBoneAnimation.Add(
                new BoneAnimationTable
                {
                    JointIndex = 1,
                    Channel = (byte)(6 | ((3 & Pre) << 4) | ((3 & Post) << 6)),
                    TimelineStartIndex = 0,
                    TimelineCount = Convert.ToByte(Timeline.Length),
                }
            );
            interpolated.BoneCount = 2;
            interpolated.FrameCount = FrameCount;
            interpolated.FrameEnd = FrameEnd;
            interpolated.FrameLoop = FrameLoop;
            interpolated.FramePerSecond = FramePerSecond;
            interpolated.StaticPose.Add(
                new Motion.StaticPoseTable
                {
                    BoneIndex = 1,
                    Channel = 6,
                    Value = InitialValue,
                }
            );
            interpolated.Joints.Add(new JointTable { JointIndex = 0, });
            interpolated.Joints.Add(new JointTable { JointIndex = 1, });
            interpolated.TotalFrameCount = TotalFrameCount;
            interpolated.Footer.Unknown = new int[9];

            var converter = new TimelineTableConverter();

            interpolated.Timeline.AddRange(
                (Timeline ?? new string[0])
                    .Select(text => converter.ConvertFromInvariantString(text))
                    .Cast<TimelineTable>()
            );

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

            var outFiles = new Dictionary<string, object>();

            {
                var dat = new StringWriter();
                var gnuplot = new StringWriter();
                gnuplot.WriteLine($"set terminal png");
                gnuplot.WriteLine($"set title '{Prefix}'");
                gnuplot.WriteLine($"set output '{Prefix}.png'");
                gnuplot.WriteLine($"plot '{Prefix}.dat' index 0 title 'out' with lines, \\");
                gnuplot.WriteLine($"     '' index 1 title 'in' with points");

                outFiles[$"{Prefix}.dat"] = dat;
                outFiles[$"{Prefix}.txt"] = gnuplot;

                var provider = (IAnimMatricesProvider)new EmuBasedAnimMatricesProvider(animReader, mdlxFile, motionBin);

                dat.WriteLine("# X Y");

                foreach (var x in Enumerable.Range(0, (int)((FrameEnd - FrameLoop + 1) * FramePerSecond)))
                {
                    var matrices = provider.ProvideMatrices((x == 0) ? 0 : 1);

                    var xVal = FrameLoop + (x / FramePerSecond);
                    var yVal = matrices[1].M41;

                    dat.WriteLine($"{xVal} {yVal}");
                }

                dat.WriteLine();
                dat.WriteLine();

                dat.WriteLine("# X Y");

                interpolated.Timeline
                    .ForEach(
                        indir =>
                        {
                            dat.WriteLine($"{indir.KeyFrame} {indir.Value}");
                        }
                    );
            }

            if (!string.IsNullOrEmpty(Prefix))
            {
                outFiles.ForEach(
                    pair => File.WriteAllText(
                        pair.Key,
                        "" + pair.Value,
                        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                    )
                );

                Console.WriteLine($"Run: gnuplot {Prefix}.txt");
            }

            return 0;
        }

    }
}
