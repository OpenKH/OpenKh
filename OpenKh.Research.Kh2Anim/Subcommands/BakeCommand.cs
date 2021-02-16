using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Research.Kh2Anim.Models;
using OpenKh.Research.Kh2Anim.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Xml.Serialization;

namespace OpenKh.Research.Kh2Anim.Subcommands
{
    [HelpOption]
    [Command(Description = "Bake motion as Matrix4x4, and then write them into xml file")]
    class BakeCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "Input file: model.mdlx")]
        public string InputMdlx { get; set; }

        [Required]
        [FileExists]
        [Argument(1, Description = "Input file: model.{mset,anb}")]
        public string InputAnim { get; set; }

        [Argument(2, Description = "XML Output dir")]
        public string OutputDir { get; set; }

        [Option(CommandOptionType.SingleValue, Description = "Frames per second")]
        public float FPS { get; set; } = 0;

        protected int OnExecute(CommandLineApplication app)
        {
            var mdlxBytes = File.ReadAllBytes(InputMdlx);

            OutputDir ??= Path.Combine(Path.GetDirectoryName(InputAnim), Path.GetFileNameWithoutExtension(InputAnim));

            Directory.CreateDirectory(OutputDir);

            var unkBarEntries = File.OpenRead(InputAnim).Using(Bar.Read);

            ConvertAnbOrMsetIntoMset(unkBarEntries)
                .ForEach(
                    wrapper =>
                    {
                        var anbIndir = new AnbIndir(wrapper.BarEntries);

                        if (!anbIndir.HasAnimationData)
                        {
                            return;
                        }

                        var provider = anbIndir.GetAnimProvider(
                            new MemoryStream(mdlxBytes, false)
                        );

                        var xmlFile = Path.Combine(OutputDir, $"{wrapper.IndexInMset}.xml");

                        if (FPS == 0)
                        {
                            FPS = 30;
                        }

                        var gameTimeDelta = 1 / FPS;

                        var timeArray = Enumerable.Range(0, (int)(provider.FrameEnd * FPS))
                            .Select(it => (float)(it * gameTimeDelta));

                        var export = new MotionExport
                        {
                            FrameCount = provider.FrameCount,
                            FrameEnd = provider.FrameEnd,
                            FrameLoop = provider.FrameLoop,
                            FramePerSecond = provider.FramePerSecond,
                            MatrixCount = provider.MatrixCount,

                            Frame = timeArray
                                .Select(
                                    time =>
                                    {
                                        var matrixArray = provider.ProvideMatrices(gameTimeDelta);
                                        return new MotionExport.FrameClass
                                        {
                                            Time = time,
                                            Matrices = MatrixArrayToString(matrixArray),
                                        };
                                    }
                                )
                                .ToArray(),
                        };

                        File.Create(xmlFile).Using(
                            stream => new XmlSerializer(typeof(MotionExport)).Serialize(stream, export)
                        );
                    }
                );

            return 0;
        }

        private string MatrixArrayToString(Matrix4x4[] matrixArray)
        {
            return string.Join(
                ",",
                matrixArray
                    .Select(
                        matrix => string.Join(
                            " "
                            , $"{matrix.M11}"
                            , $"{matrix.M12}"
                            , $"{matrix.M13}"
                            , $"{matrix.M14}"
                            , $"{matrix.M21}"
                            , $"{matrix.M22}"
                            , $"{matrix.M23}"
                            , $"{matrix.M24}"
                            , $"{matrix.M31}"
                            , $"{matrix.M32}"
                            , $"{matrix.M33}"
                            , $"{matrix.M34}"
                            , $"{matrix.M41}"
                            , $"{matrix.M42}"
                            , $"{matrix.M43}"
                            , $"{matrix.M44}"
                        )
                    )
            );
        }

        private static IEnumerable<AnbBarWrapper> ConvertAnbOrMsetIntoMset(IEnumerable<Bar.Entry> unkBarEntries)
        {
            return unkBarEntries.Any(entry => entry.Type == Bar.EntryType.Motion)
                // Input is `.anb`
                ? new AnbBarWrapper[] {
                    new AnbBarWrapper {
                        BarEntries = unkBarEntries.ToList(),
                        IndexInMset = 0,
                    }
                }
                // Input is `.mset`
                : unkBarEntries
                    .Select(
                        (msetBarEntry, index) => new AnbBarWrapper
                        {
                            IndexInMset = index,
                            BarEntries = (msetBarEntry.Stream.Length != 0)
                                ? Bar.Read(msetBarEntry.Stream)
                                : new List<Bar.Entry>()
                        }
                    );
        }
    }
}
