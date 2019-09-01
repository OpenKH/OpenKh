using OpenKh.Common;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;

namespace OpenKh.Command.Arc
{
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

        [Argument(0, "ARC file", "The ARC file to pack or unpack")]
        public string FileName { get; }

        [Option(ShortName = "p", LongName = "pack", Description = "Pack ARC")]
        public bool Pack { get; }

        private void OnExecute()
        {
            try
            {
                if (Pack)
                    Repack(FileName);
                else
                    Unpack(FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }

        private static void Unpack(string input)
        {
            if (!File.Exists(input))
                throw new FileNotFoundException("The specified file name cannot be found", input);

            var entries = File.OpenRead(input).Using(stream =>
            {
                if (!Bbs.Arc.IsValid(stream))
                    throw new InvalidDataException("The specified ARC file is not valid");

                return Bbs.Arc.Read(stream);
            });

            var outputDirectory = Path.Combine(Path.GetDirectoryName(input), Path.GetFileNameWithoutExtension(input));
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

        private static void Repack(string input)
        {
            throw new NotImplementedException();
        }
    }
}
