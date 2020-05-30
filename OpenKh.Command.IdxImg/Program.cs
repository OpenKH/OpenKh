using OpenKh.Kh2;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OpenKh.Command.IdxImg
{
    [Command("OpenKh.Command.IdxImg")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(ExtractCommand), typeof(ListCommand))]
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

        private class ExtractCommand
        {
            private Program Parent { get; set; }

            [Required]
            [FileExists]
            [Argument(0, Description = "Kingdom Hearts II IDX file, paired with a IMG")]
            public string InputIdx { get; set; }

            [FileExists]
            [Option(CommandOptionType.SingleValue, Description = "Custom Kingdom Hearts II IMG file", ShortName = "m", LongName = "img")]
            public string InputImg { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Path where the content will be extracted", ShortName = "o", LongName = "output")]
            public string OutputDir { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Extract all the sub-IDX recursively", ShortName = "r", LongName = "recursive")]
            public bool Recursive { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Split sub-IDX when extracting recursively", ShortName = "s", LongName = "split")]
            public bool Split { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var inputImg = InputImg ?? InputIdx.Replace(".idx", ".img", StringComparison.InvariantCultureIgnoreCase);
                var outputDir = OutputDir ?? Path.Combine(Path.GetFullPath(inputImg), "extract");

                var idxEntries = OpenIdx(InputIdx);

                using (var imgStream = File.OpenRead(inputImg))
                {
                    var img = new Img(imgStream, idxEntries, false);
                    var idxName = Path.GetFileNameWithoutExtension(InputIdx);

                    var subIdxPath = ExtractIdx(img, idxEntries, Recursive && Split ? Path.Combine(outputDir, "KH2") : outputDir);
                    if (Recursive)
                    {
                        foreach (var idxFileName in subIdxPath)
                        {
                            idxName = Path.GetFileNameWithoutExtension(idxFileName);
                            ExtractIdx(img, OpenIdx(idxFileName), Split ? Path.Combine(outputDir, idxName) : outputDir);
                        }
                    }
                }

                return 0;
            }

            public static List<string> ExtractIdx(Img img, IEnumerable<Idx.Entry> idxEntries, string basePath)
            {
                var idxs = new List<string>();

                foreach (var entry in idxEntries)
                {
                    var fileName = IdxName.Lookup(entry);
                    if (fileName == null)
                        fileName = $"@noname/{entry.Hash32:X08}-{entry.Hash16:X04}";

                    Console.WriteLine(fileName);

                    var outputFile = Path.Combine(basePath, fileName);
                    var outputDir = Path.GetDirectoryName(outputFile);
                    if (Directory.Exists(outputDir) == false)
                        Directory.CreateDirectory(outputDir);

                    using (var file = File.Create(outputFile))
                    {
                        // TODO handle decompression
                        img.FileOpen(entry).CopyTo(file);
                    }

                    if (Path.GetExtension(fileName) == ".idx")
                        idxs.Add(outputFile);
                }

                return idxs;
            }
        }

        private class ListCommand
        {
            private Program Parent { get; set; }

            [Required]
            [FileExists]
            [Argument(0, Description = "Kingdom Hearts II IDX file, paired with a IMG")]
            public string InputIdx { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Sort file list by their position in the IMG", ShortName = "s", LongName = "sort")]
            public bool Sort { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var entries = OpenIdx(InputIdx);
                if (Sort)
                    entries = entries.OrderBy(x => x.Offset);

                foreach (var entry in entries)
                    Console.WriteLine(IdxName.Lookup(entry) ?? $"@{entry.Hash32:X08}-{entry.Hash16:X04}");

                return 0;
            }
        }

        private static IEnumerable<Idx.Entry> OpenIdx(string fileName)
        {
            using var idxStream = File.OpenRead(fileName);
            return Idx.Read(idxStream);
        }
    }
}
