using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

namespace OpenKh.Command.SpawnScript
{
    [Command("OpenKh.Command.SpawnScript")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(UnscriptCommand),
        typeof(RescriptCommand),
        typeof(UnpointCommand),
        typeof(RepointCommand))]
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
                Console.WriteLine($"The file or path {e.FileName} cannot be found. The program will now exit.");
                return 2;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return -1;
            }
        }

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        private class UnscriptCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "File of a spawnscript file")]
            public string InputPath { get; set; }

            [Required]
            [Argument(1, Description = "Output file name as YAML")]
            public string OutputPath { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var spawnScript = File.OpenRead(InputPath).Using(Kh2.Ard.SpawnScript.Read);
                File.WriteAllText(OutputPath, Helpers.YamlSerialize(spawnScript));
                return 0;
            }
        }

        private class RescriptCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "File of a YAML spawnscript file")]
            public string InputPath { get; set; }

            [Required]
            [Argument(1, Description = "Output file name")]
            public string OutputPath { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var spawnScript = Helpers.YamlDeserialize<List<Kh2.Ard.SpawnScript>>(File.ReadAllText(InputPath));
                File.Create(OutputPath).Using(stream => Kh2.Ard.SpawnScript.Write(stream, spawnScript));
                return 0;
            }
        }

        private class UnpointCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "File of a spawnpoint file")]
            public string InputPath { get; set; }

            [Required]
            [Argument(1, Description = "Output file name as YAML")]
            public string OutputPath { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var spawnPoint = File.OpenRead(InputPath).Using(Kh2.Ard.SpawnPoint.Read);
                File.WriteAllText(OutputPath, Helpers.YamlSerialize(spawnPoint));
                return 0;
            }
        }

        private class RepointCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "File of a YAML spawnpoint file")]
            public string InputPath { get; set; }

            [Required]
            [Argument(1, Description = "Output file name")]
            public string OutputPath { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var spawnPoint = Helpers.YamlDeserialize<List<Kh2.Ard.SpawnPoint>>(File.ReadAllText(InputPath));
                File.Create(OutputPath).Using(stream => Kh2.Ard.SpawnPoint.Write(stream, spawnPoint));
                return 0;
            }
        }
    }
}