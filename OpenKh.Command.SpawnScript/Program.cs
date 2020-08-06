using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Kh2.Ard;
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
        typeof(DecompileCommand),
        typeof(CompileCommand),
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

        private class DecompileCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Spawnscript to decompile")]
            public string InputPath { get; set; }

            [Option(CommandOptionType.SingleValue, ShortName = "o", LongName = "output", Description = "Output file as text")]
            public string OutputPath { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                OutputPath ??= Path.Combine(Path.GetDirectoryName(InputPath), $"{Path.GetFileNameWithoutExtension(InputPath)}.txt");
                var spawnScript = File.OpenRead(InputPath).Using(Kh2.Ard.SpawnScript.Read);
                File.WriteAllText(OutputPath, SpawnScriptParser.Decompile(spawnScript));
                return 0;
            }
        }

        private class CompileCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Spawnscript source code to compile")]
            public string InputPath { get; set; }

            [Option(CommandOptionType.SingleValue, ShortName = "o", LongName = "output", Description = "Output file as spawnscript")]
            public string OutputPath { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                OutputPath ??= Path.Combine(Path.GetDirectoryName(InputPath), $"{Path.GetFileNameWithoutExtension(InputPath)}.spawnscript");
                var spawnScript = SpawnScriptParser.Compile(File.ReadAllText(InputPath));
                File.Create(OutputPath).Using(stream => Kh2.Ard.SpawnScript.Write(stream, spawnScript));
                return 0;
            }
        }

        private class UnpointCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Spawnpoint file to deserialize")]
            public string InputPath { get; set; }

            [Option(CommandOptionType.SingleValue, ShortName = "o", LongName = "output", Description = "Output file as YAML")]
            public string OutputPath { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                OutputPath ??= Path.Combine(Path.GetDirectoryName(InputPath), $"{Path.GetFileNameWithoutExtension(InputPath)}.yml");
                var spawnPoint = File.OpenRead(InputPath).Using(Kh2.Ard.SpawnPoint.Read);
                File.WriteAllText(OutputPath, Helpers.YamlSerialize(spawnPoint));
                return 0;
            }
        }

        private class RepointCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Spawnpoint file previously deserialized as YAML")]
            public string InputPath { get; set; }

            [Option(CommandOptionType.SingleValue, ShortName = "o", LongName = "output", Description = "Output file as spawnpoint")]
            public string OutputPath { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                OutputPath ??= Path.Combine(Path.GetDirectoryName(InputPath), $"{Path.GetFileNameWithoutExtension(InputPath)}.spawnpoint");
                var spawnPoint = Helpers.YamlDeserialize<List<Kh2.Ard.SpawnPoint>>(File.ReadAllText(InputPath));
                File.Create(OutputPath).Using(stream => Kh2.Ard.SpawnPoint.Write(stream, spawnPoint));
                return 0;
            }
        }
    }
}