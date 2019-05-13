using OpenKh.Kh2;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace GenerateKh2FilesMarkdown
{
    [Command(Description = "Generates a markdown file with all the file entries read from the specified IDX files")]
    class Program
    {
        [Option("-o|--output", "Output file name", CommandOptionType.SingleValue, LongName = "output")]
        [Required]
        public string OutputFileName { get; set; }

        [Option("-r|--recursive", "Add underlying IDX to the process", CommandOptionType.NoValue, LongName = "recursive")]
        [Required]
        public bool IsRecursive { get; set; }

        [Option(ShortName = "--no-unnamed", Description = "Excluse from the output all the files without a file name discovered")]
        public bool ExcludeUnNamed { get; set; }

        [Option(ShortName = "--only-unnamed", Description = "Generates a specific list of un-named files only")]
        public bool OnlyUnNamed { get; set; }

        public static void Main(string[] args)
        {
            //CommandLineApplication.Execute<Program>(args);
            new Program()
            {
                OutputFileName = "../../../../docs/kh2/files.md",
                IsRecursive = true,
                ExcludeUnNamed = true
            }.OnExecute();
            new Program()
            {
                OutputFileName = "../../../../docs/kh2/files-unknown.md",
                IsRecursive = true,
                OnlyUnNamed = true
            }.OnExecute();
        }

        private void OnExecute()
        {
            var idxEntries = IdxTooling.GetEntries();

            if (ExcludeUnNamed)
                idxEntries = idxEntries
                    .Where(x => x.FileName != null);

            if (OnlyUnNamed)
                idxEntries = idxEntries
                    .Where(x => x.FileName == null)
                    .OrderByDescending(x => x.Games);

            var myData = idxEntries.ToList();

            Console.WriteLine($"Generating {OutputFileName}...");
            using (var stream = File.CreateText(OutputFileName))
            {
                stream.WriteLine("| Name | Hash32 | Hash16 | Games |");
                stream.WriteLine("|------|--------|--------|-------|");
                foreach (var item in idxEntries)
                {
                    stream.WriteLine($"|{item.FileName}|{item.Hash32:X08}|{item.Hash16:X04}|{item.Games}");
                }
            }

            using (var stream = File.CreateText("files.txt"))
            {
                foreach (var file in idxEntries)
                {
                    if (file.FileName != null)
                    {
                        stream.WriteLine(file.FileName);
                    }
                }
            }

            System.Console.WriteLine($"Discovered {idxEntries.Count(x => x.FileName != null)} of {myData.Count}");
        }
    }
}
