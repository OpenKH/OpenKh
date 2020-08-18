using McMaster.Extensions.CommandLineUtils;
using OpenKh.Kh2;
using OpenKh.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Linq;
using OpenKh.Kh2.Utils;
using OpenKh.Command.DoctChanger.Utils;

namespace OpenKh.Command.DoctChanger
{
    [Command("OpenKh.Command.DoctChanger")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(UseThisDoctCommand), typeof(CreateDoctForMapCommand)
        , typeof(CreateDummyDoctCommand)
        , typeof(ReadDoctCommand), typeof(ReadMapDoctCommand)
        , typeof(ShowStatsCommand)
        , typeof(DumpDoctCommand))]
    class Program
    {
        static int Main(string[] args)
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

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }


        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;


        [HelpOption]
        [Command(Description = "map file: replace doct with your doct")]
        private class UseThisDoctCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Input map dir")]
            public string Input { get; set; }

            [Required]
            [DirectoryExists]
            [Argument(1, Description = "Output map dir")]
            public string Output { get; set; }

            [Required]
            [FileExists]
            [Argument(2, Description = "DOCT file input")]
            public string DoctIn { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var doctBin = File.ReadAllBytes(DoctIn);

                foreach (var mapIn in Directory.GetFiles(Input, "*.map"))
                {
                    Console.WriteLine(mapIn);

                    var mapOut = Path.Combine(Output, Path.GetFileName(mapIn));

                    var entries = File.OpenRead(mapIn).Using(s => Bar.Read(s))
                        .Select(
                            it =>
                            {
                                if (it.Type == Bar.EntryType.MeshOcclusion)
                                {
                                    it.Stream = new MemoryStream(doctBin, false);
                                }

                                return it;
                            }
                        )
                        .ToArray();

                    File.Create(mapOut).Using(s => Bar.Write(s, entries));
                }
                return 0;
            }
        }

        private static void SummaryDoct(Doct doct, string file, TextWriter writer)
        {
            writer.WriteLine($"# DOCT ({Path.GetFileName(file)})");
            writer.WriteLine();
            writer.WriteLine($"- Version: {doct.Header.Version}");
            writer.WriteLine($"- Unk2: {doct.Header.Unk2}");
            writer.WriteLine();

            writer.WriteLine("## Entry1");
            writer.WriteLine();
            writer.WriteLine("```");

            foreach (var pair in doct.Entry1List.Select((it, index) => (it, index)))
            {
                writer.WriteLine($"{pair.index,4}:{pair.it}");
            }
            writer.WriteLine("```");

            writer.WriteLine();

            writer.WriteLine("## Entry2");
            writer.WriteLine();
            writer.WriteLine("```");

            foreach (var pair in doct.Entry2List.Select((it, index) => (it, index)))
            {
                writer.WriteLine($"{pair.index,4}:{pair.it}");
            }

            writer.WriteLine("```");
        }

        [HelpOption]
        [Command(Description = "doct file: read")]
        private class ReadDoctCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "DOCT file input")]
            public string DoctIn { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var doct = File.OpenRead(DoctIn).Using(s => Doct.Read(s));

                SummaryDoct(doct, DoctIn, Console.Out);

                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "doct file: dump")]
        private class DumpDoctCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "DOCT file input")]
            public string DoctIn { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var doct = File.OpenRead(DoctIn).Using(s => Doct.Read(s));

                new DumpDoctUtil(doct, Console.Out);

                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "map file: read doct")]
        private class ReadMapDoctCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Map file input")]
            public string MapIn { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var doctBin = File.ReadAllBytes(MapIn);

                var entries = File.OpenRead(MapIn).Using(s => Bar.Read(s));

                var doctEntry = entries.Single(it => it.Type == Bar.EntryType.MeshOcclusion);

                var doct = Doct.Read(doctEntry.Stream);

                SummaryDoct(doct, MapIn, Console.Out);

                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "doct file: create dummy")]
        private class CreateDummyDoctCommand
        {
            [Required]
            [Argument(0, Description = "DOCT file output")]
            public string DoctOut { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var doct = new Doct();

                doct.Entry1List.Add(new Doct.Entry1 { });
                doct.Entry2List.Add(new Doct.Entry2 { });

                File.Create(DoctOut).Using(s => Doct.Write(s, doct));

                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "map file: create and set unoptimized doct for rendering entire map")]
        private class CreateDoctForMapCommand
        {
            [Required]
            [Argument(0, Description = "Map file input")]
            public string MapIn { get; set; }

            [Argument(1, Description = "Map file output. Default output is same file name at current folder")]
            public string MapOut { get; set; }

            private static bool IsMapModel(Bar.Entry entry) => entry.Type == Bar.EntryType.Model && entry.Name == "MAP";
            private static bool IsDoct(Bar.Entry entry) => entry.Type == Bar.EntryType.MeshOcclusion;

            protected int OnExecute(CommandLineApplication app)
            {
                MapOut = Path.GetFullPath(MapOut ?? Path.GetFileName(MapIn));

                Console.WriteLine($"Output map file: {MapOut}");

                var entries = File.OpenRead(MapIn).Using(s => Bar.Read(s).ToArray());

                var mapModel = Mdlx.Read(entries.Single(IsMapModel).Stream);

                var numVifPackets = mapModel.MapModel.VifPackets.Count;
                var numAlb2Groups = mapModel.MapModel.vifPacketRenderingGroup.Count;

                Console.WriteLine($"numVifPackets: {numVifPackets:#,##0}");
                Console.WriteLine($"numAlb2Groups: {numAlb2Groups:#,##0}");

                Console.WriteLine($"Note: this tool will build a unoptimized doct that renders all ALB2 {numAlb2Groups:#,##0} groups.");

                var doctStream = new MemoryStream();
                {
                    var doct = new Doct();

                    doct.Entry1List.Add(
                        new Doct.Entry1
                        {
                            Entry2Index = 0,
                            Entry2LastIndex = Convert.ToUInt16(numAlb2Groups), // max 65535
                        }
                    );

                    const float WorldBounds = 18000;

                    doct.Entry2List.AddRange(
                        Enumerable.Range(0, numAlb2Groups)
                            .Select(
                                index => new Doct.Entry2
                                {
                                    BoundingBox = new BoundingBox(
                                        new System.Numerics.Vector3(-WorldBounds),
                                        new System.Numerics.Vector3(WorldBounds)
                                    )
                                }
                            )
                    );

                    Doct.Write(doctStream, doct);

                    doctStream.Position = 0;
                }

                foreach (var entry in entries.Where(IsDoct))
                {
                    Console.WriteLine("DOCT entry replaced.");
                    entry.Stream = doctStream;
                }

                File.Create(MapOut).Using(s => Bar.Write(s, entries));

                Console.WriteLine("Output map file is written successfully.");
                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "doct file: show stats")]
        private class ShowStatsCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input map/doct file (decided by file extension: `.map` or not)")]
            public string InputFile { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var isMap = Path.GetExtension(InputFile).ToLowerInvariant() == ".map";

                if (isMap)
                {
                    foreach (var entry in File.OpenRead(InputFile).Using(Bar.Read)
                        .Where(entry => false
                            || entry.Type == Bar.EntryType.MeshOcclusion
                        )
                    )
                    {
                        Console.WriteLine($"# {entry.Name}:{entry.Index} ({entry.Type})");
                        PrintSummary(Doct.Read(entry.Stream));
                        Console.WriteLine();
                    }
                }
                else
                {
                    PrintSummary(File.OpenRead(InputFile).Using(Doct.Read));
                }

                return 0;
            }

            private void PrintSummary(Doct coct)
            {
                Console.WriteLine($"{coct.Entry1List.Count,8:#,##0} drawing mesh groups.");
                Console.WriteLine($"{coct.Entry2List.Count,8:#,##0} drawing meshes.");
            }
        }
    }
}
