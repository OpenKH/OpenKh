using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Common.Exceptions;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Research.Kh2Anim.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Serialization;

namespace OpenKh.Research.Kh2Anim
{
    [Command("OpenKh.Research.Kh2Anim")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (InvalidFileException e)
            {
                Console.WriteLine(e.Message);
                return 3;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 2;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return -1;
            }
        }

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

        class AnbBarWrapper
        {
            public int IndexInMset { get; set; }
            public List<Bar.Entry> BarEntries { get; set; }
        }

        protected int OnExecute(CommandLineApplication app)
        {
            var mdlxBytes = File.ReadAllBytes(InputMdlx);

            OutputDir ??= Path.Combine(Path.GetDirectoryName(InputAnim), Path.GetFileNameWithoutExtension(InputAnim));

            Directory.CreateDirectory(OutputDir);

            var unkBarEntries = File.OpenRead(InputAnim).Using(Bar.Read);

            var anbBarEntriesList = unkBarEntries.Any(entry => entry.Type == Bar.EntryType.Motion)
                // Input is `.anb`
                ? new AnbBarWrapper[] {
                    new AnbBarWrapper {
                        BarEntries = unkBarEntries,
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

            anbBarEntriesList
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

                        var timeArray = Enumerable.Range(0, (int)provider.FrameEnd)
                            .Select(it => (float)it);

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
                                        var matrixArray = provider.ProvideMatrices(0);
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

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
