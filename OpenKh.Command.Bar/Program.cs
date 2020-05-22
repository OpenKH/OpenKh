using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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
        class BarRoot
        {
            [JsonProperty] public string OriginalFileName { get; set; }

            [JsonProperty] public List<BarDesc> Entries { get; set; }
        }

        class BarDesc
        {
            [JsonProperty] public string FileName { get; set; }
            [JsonProperty] public string InternalName { get; set; }
            [JsonProperty] public int TypeId { get; set; }
            [JsonProperty] public int LinkIndex { get; set; }
            [JsonIgnore] public Stream Stream { get; set; }

            public override string ToString() => FileName;
        }

        private const string InputProjectDesc = "BAR project file (eg. P_EX100.json).";
        private const string InputBarDesc = "Kingdom Hearts II BAR file.";
        private const string OutputBarDesc = "Name of the BAR file that will be created.";
        private const string OutputDirDesc = "Path where the content will be extracted.";
        private const string InvalidBarText = "The specified file is not a BAR file.";
        private const string SuppressProjectCreationDesc = "Do not generate a project when unpacking.";
        private static readonly Exception InvalidBarFileException = new InvalidDataException(InvalidBarText);

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

        private static List<Kh2.Bar.Entry> ReadEntries(string fileName)
        {
            using var stream = File.OpenRead(fileName);
            if (!Kh2.Bar.IsValid(stream))
                throw InvalidBarFileException;

            stream.Position = 0;
            return Kh2.Bar.Read(stream);
        }

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
                var fileNameWithExt = Path.GetFileName(InputBar);
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(InputBar);

                if (string.IsNullOrEmpty(OutputDir))
                    OutputDir = Path.Combine(Path.GetDirectoryName(InputBar), fileNameWithoutExt);

                var barEntries = ReadEntries(InputBar);
                var project = new BarRoot
                {
                    OriginalFileName = Path.GetFileName(InputBar),
                    Entries = barEntries
                        .Select(x => new BarDesc
                        {
                            FileName = $"{x.Name}.{Helpers.GetSuggestedExtension(x.Type)}",
                            InternalName = x.Name,
                            TypeId = (int)x.Type,
                            LinkIndex = x.Index,
                            Stream = x.Stream,
                        })
                        .ToList()
                };

                foreach (var entryGroup in project.Entries
                    .Where(x => x.LinkIndex == 0)
                    .GroupBy(x => $"{x.InternalName}_{x.TypeId}"))
                {
                    var items = entryGroup.ToArray();
                    if (items.Length > 1)
                    {
                        for (var i = 0; i < items.Length; i++)
                        {
                            var ext = Helpers.GetSuggestedExtension((Kh2.Bar.EntryType)items[0].TypeId);
                            items[i].FileName = $"{items[0].InternalName}_{i}.{ext}";
                        }
                    }
                }

                if (!SuppressProjectCreation)
                {
                    var projectFileName = Path.Combine(OutputDir, $"{fileNameWithExt}.json");
                    if (!Directory.Exists(OutputDir))
                        Directory.CreateDirectory(OutputDir);

                    File.WriteAllText(projectFileName, JsonConvert.SerializeObject(project, Formatting.Indented));
                }

                foreach (var entry in project.Entries.Where(x => x.LinkIndex == 0))
                {
                    var outputFileName = Path.Combine(OutputDir, entry.FileName);

                    using var stream = File.Create(outputFileName);
                    entry.Stream.Position = 0;
                    entry.Stream.CopyTo(stream);
                }

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
                var project = JsonConvert.DeserializeObject<BarRoot>(File.ReadAllText(InputProject));
                var streams = project.Entries
                    .Where(x => x.LinkIndex == 0)
                    .ToDictionary(x => x.FileName,
                        x => File.OpenRead(Path.Combine(baseDirectory, x.FileName)));

                if (string.IsNullOrEmpty(OutputFile))
                    OutputFile = Path.Combine(baseDirectory, project.OriginalFileName);
                if (!File.Exists(OutputFile) && Directory.Exists(OutputFile) &&
                    File.GetAttributes(OutputFile).HasFlag(FileAttributes.Directory))
                    OutputFile = Path.Combine(OutputFile, project.OriginalFileName);

                using var outputStream = File.Create(OutputFile);
                Kh2.Bar.Write(outputStream, project.Entries
                    .Select(x => new Kh2.Bar.Entry
                    {
                        Name = x.InternalName,
                        Type = (Kh2.Bar.EntryType)x.TypeId,
                        Index = x.LinkIndex,
                        Stream = x.LinkIndex == 0 ? streams[x.FileName] : null
                    }));

                foreach (var pair in streams)
                    pair.Value.Dispose();

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
