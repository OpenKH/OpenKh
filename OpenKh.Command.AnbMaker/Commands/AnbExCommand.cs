using McMaster.Extensions.CommandLineUtils;
using NLog;
using OpenKh.Command.AnbMaker.Commands.Interfaces;
using OpenKh.Command.AnbMaker.Commands.Utils;
using OpenKh.Command.AnbMaker.Utils.AssimpAnimSource;
using OpenKh.Command.AnbMaker.Utils.Builder;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using OpenKh.Command.AnbMaker.Utils.JsonAnimSource;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        public string OutputMset { get; set; }

        [Option(Description = "specify root armature node name", ShortName = "r")]
        public string RootName { get; set; }

        [Option(Description = "specify mesh name to read bone data", ShortName = "m")]
        public string MeshName { get; set; }

        [Option(Description = "specify animation name to read bone data", ShortName = "a")]
        public string AnimationName { get; set; }

        [Option(Description = "apply scaling to each source node", ShortName = "x", LongName = "node-scaling")]
        public float NodeScaling { get; set; } = 1;

        [Option(Description = "apply scaling to each bone position", ShortName = "p", LongName = "position-scaling")]
        public float PositionScaling { get; set; } = 1;

        [Option(Description = "optionally inject new motion into mset directly", ShortName = "w")]
        public string MsetFile { get; set; }

        [Option(Description = "zero based target index of bar entry in mset file", ShortName = "i")]
        public int MsetIndex { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            var logger = LogManager.GetLogger("InterpolatedMotionMaker");

            Output = Path.GetFullPath(Output ?? Path.GetFileNameWithoutExtension(InputModel) + ".anb");
            OutputMset = Path.GetFullPath(OutputMset ?? Path.GetFileNameWithoutExtension(InputModel) + ".mset");
            Console.WriteLine($"Writing to: {Output}");

            IEnumerable<BasicSourceMotion> parms;
            if (Path.GetExtension(InputModel).ToLowerInvariant() == ".json")
            {
                parms = new UseJson(
                    inputModel: InputModel,
                    meshName: MeshName,
                    rootName: RootName,
                    animationName: AnimationName,
                    nodeScaling: 1
                )
                    .Parameters;
            }
            else
            {
                parms = new UseAssimp(
                    inputModel: InputModel,
                    meshName: MeshName,
                    rootName: RootName,
                    animationName: AnimationName,
                    nodeScaling: NodeScaling,
                    positionScaling: PositionScaling
                )
                    .Parameters;
            }

            foreach (var parm in parms.Take(1))
            {
                var builder = new InterpolatedMotionBuilder(parm);

                var ipm = builder.Ipm;

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
                        },
                        new Bar.Entry
                        {
                            Type = Bar.EntryType.MotionTriggers,
                            Name = "A999",
                            Stream = new MemoryStream()  // Replace null with MemoryStream containing "0"
                        }
                    }
                );

                var msetBarStream = new MemoryStream();
                Bar.Write(
                msetBarStream,
                    new Bar.Entry[]
                    {
                        new Bar.Entry
                        {
                            Type = Bar.EntryType.Anb,
                            Name = "A999",
                            Stream = anbBarStream
                        }
                    }
               );

                File.WriteAllBytes(Output, anbBarStream.ToArray());
                File.WriteAllBytes(Output + ".raw", motionStream.ToArray());
                File.WriteAllBytes(OutputMset, msetBarStream.ToArray());

                logger.Debug($"Motion data generation successful");

                new MsetInjector().InjectMotionTo(this, motionStream.ToArray());
            }

            return 0;
        }
    }
}
