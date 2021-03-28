using OpenKh.Common;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace OpenKh.Command.Arc
{
    [Command("OpenKh.Command.Arc")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Required]
        [Argument(0, "ARC file", "The ARC file to pack or unpack")]
        public string FileName { get; }

        [Required]
        [Argument(1, "ARC directory", "The ARC directory used as destination for unpakcing or source for packing")]
        public string DirectoryName { get; }

        [Option(ShortName = "p", LongName = "pack", Description = "Pack ARC")]
        public bool Pack { get; }

        private void OnExecute()
        {
            try
            {
                if (Pack)
                    Repack(FileName, DirectoryName);
                else
                    Unpack(FileName, DirectoryName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }

        private static void Unpack(string inputFile, string outputDirectory)
        {
            if (!File.Exists(inputFile))
                throw new FileNotFoundException("The specified file name cannot be found", inputFile);

            var entries = File.OpenRead(inputFile).Using(stream =>
            {
                if (!Bbs.Arc.IsValid(stream))
                    throw new InvalidDataException("The specified ARC file is not valid");

                return Bbs.Arc.Read(stream);
            });

            if (string.IsNullOrEmpty(outputDirectory))
                outputDirectory = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile));
            Directory.CreateDirectory(outputDirectory);

            foreach (var entry in entries.Where(x => !x.IsLink))
            {
                Console.WriteLine(entry.Name);

                File.Create(Path.Combine(outputDirectory, entry.Name)).Using(stream =>
                {
                    stream.Write(entry.Data, 0, entry.Data.Length);
                });
            }

            File.CreateText(Path.Combine(outputDirectory, "@ARC.txt")).Using(stream =>
            {
                foreach (var entry in entries)
                {
                    stream.WriteLine(entry.IsLink ? $"@{entry.Path}" : entry.Name);
                }
            });
        }

        private static void Repack(string outputFile, string inputDirectory)
        {
            if (!Directory.Exists(inputDirectory))
                throw new DirectoryNotFoundException("The specified input directory cannot be found");

            var arcEntriesFileName = Path.Combine(inputDirectory, "@ARC.txt");
            if (!File.Exists(arcEntriesFileName))
                throw new FileNotFoundException("The @ARC.txt descriptor cannot be found in the specified directory", arcEntriesFileName);

            var entries = File.ReadAllLines(arcEntriesFileName)
                .Select(x => GetEntry(inputDirectory, x));

            File.Create(outputFile).Using(stream => Bbs.Arc.Write(entries, stream));

        }

        private static Bbs.Arc.Entry GetEntry(string baseDirectory, string entryName)
        {
            if (entryName.FirstOrDefault() == '@')
            {
                var linkFileName = entryName.Substring(1);
                var directoryName = Path.GetDirectoryName(linkFileName).Replace('\\', '/');
                var fileName = Path.GetFileName(linkFileName);

                var directoryPointer = Bbs.Bbsa.GetDirectoryHash(directoryName);
                if (directoryPointer == uint.MaxValue)
                    throw new DirectoryNotFoundException($"The directory {directoryName} cannot be recognized by BBS engine.");

                return new Bbs.Arc.Entry
                {
                    DirectoryPointer = directoryPointer,
                    Name = fileName
                };
            }
            else
            {
                var fileName = Path.Combine(baseDirectory, entryName);
                return new Bbs.Arc.Entry
                {
                    DirectoryPointer = 0,
                    Data = File.ReadAllBytes(fileName),
                    Name = entryName
                };
            }
        }
    }
}
