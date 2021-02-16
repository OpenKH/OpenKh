using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

namespace OpenKh.Command.Bar
{
    [Command("OpenKh.Command.Bar")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(UnpackCommand),
        typeof(PackCommand),
        typeof(ListCommand))]
    class Program
    {
        private const string InputProjectDesc = "BAR project file (eg. P_EX100.json).";
        private const string InputBarDesc = "Kingdom Hearts II BAR file.";
        private const string OutputBarDesc = "Name of the BAR file that will be created.";
        private const string OutputDirDesc = "Path where the content will be extracted.";
        private const string SuppressProjectCreationDesc = "Do not generate a project when unpacking.";

        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
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

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Command(Description = "Unpack the content of a BAR file and generate a project")]
        private class UnpackCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = InputBarDesc)]
            public string InputBar { get; set; }

            [Option(CommandOptionType.SingleValue, Description = OutputDirDesc, ShortName = "o", LongName = "output")]
            public string OutputDir { get; set; }

            [Option(CommandOptionType.NoValue, Description = SuppressProjectCreationDesc, ShortName = "s", LongName = "skip")]
            public bool SuppressProjectCreation { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                if (string.IsNullOrEmpty(OutputDir))
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(InputBar);
                    OutputDir = Path.Combine(Path.GetDirectoryName(InputBar), fileNameWithoutExt);
                }

                Core.ExportProject(InputBar, OutputDir, SuppressProjectCreation);

                return 0;
            }
        }

        [Command(Description = "Repack a BAR from its project file")]
        private class PackCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = InputProjectDesc)]
            public string InputProject { get; set; }

            [Option(CommandOptionType.SingleValue, Description = OutputBarDesc, ShortName = "o", LongName = "output")]
            public string OutputFile { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var baseDirectory = Path.GetDirectoryName(InputProject);
                var binarc = Core.ImportProject(InputProject, out var originalFileName);

                if (string.IsNullOrEmpty(OutputFile))
                    OutputFile = Path.Combine(baseDirectory, originalFileName);

                if (!File.Exists(OutputFile) && Directory.Exists(OutputFile) &&
                    File.GetAttributes(OutputFile).HasFlag(FileAttributes.Directory))
                    OutputFile = Path.Combine(OutputFile, originalFileName);

                using var outputStream = File.Create(OutputFile);
                Kh2.Bar.Write(outputStream, binarc);

                foreach (var entry in binarc)
                    entry.Stream?.Dispose();

                return 0;
            }
        }

        [Command(Description = "Print content of a BAR file")]
        private class ListCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = InputBarDesc)]
            public string InputBar { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                using var stream = File.OpenRead(InputBar);
                foreach (var entry in Kh2.Bar.Read(stream))
                    Console.WriteLine($"{entry.Name}, {entry.Type}, {entry.Index}");

                return 0;
            }
        }
    }
}
