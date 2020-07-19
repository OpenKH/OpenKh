using McMaster.Extensions.CommandLineUtils;
using OpenKh.Kh2;
using OpenKh.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Linq;

namespace OpenKh.Command.DoctChanger
{
    [Command("OpenKh.Command.DoctChanger")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(NoDoctCommand), typeof(UseThisDoctCommand))]
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
        [Command(Description = "map file: remove doct")]
        private class NoDoctCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Input map dir")]
            public string Input { get; set; }

            [Required]
            [DirectoryExists]
            [Argument(1, Description = "Output map dir")]
            public string Output { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                foreach (var mapIn in Directory.GetFiles(Input, "*.map"))
                {
                    Console.WriteLine(mapIn);

                    var mapOut = Path.Combine(Output, Path.GetFileName(mapIn));

                    var entries = File.OpenRead(mapIn).Using(s => Bar.Read(s))
                        .Where(it => it.Type != Bar.EntryType.MeshOcclusion)
                        .ToArray();

                    File.Create(mapOut).Using(s => Bar.Write(s, entries));
                }
                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "map file: use your doct")]
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

    }
}
