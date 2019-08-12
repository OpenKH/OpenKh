using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Command.Bbsa
{
    public class Program
    {
        static void Main(string[] args)
        {
            var argument = args.FirstOrDefault() ?? Directory.GetCurrentDirectory();
            try
            {
                ProcessArgument(argument);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
            }
        }

        private static void ProcessArgument(string argument)
        {
            if (Directory.Exists(argument))
            {
                if (!DoesContainBbsa(argument))
                    throw new ArchiveNotFoundException(argument, 0);
                ExtractByFolder(argument);
            }
            else
            {
                ExtractByPattern(argument);
            }
        }

        private static bool DoesContainBbsa(string path) =>
            File.Exists(Path.Combine(path, "BBS0.DAT"));

        private static void ExtractByFolder(string path) =>
            ExtractByPattern(Path.Combine(path, "BBS"));

        private static void ExtractByPattern(string prefix)
        {
            var bbsaFileNames = Enumerable.Range(0, 5)
                .Select(x => $"{prefix}{x}.DAT");
            var outputDir = Path.Combine(Path.GetDirectoryName(prefix), Path.GetFileName(prefix));

            ExtractArchives(bbsaFileNames, outputDir);
        }

        private static void ExtractArchives(IEnumerable<string> bbsaFileNames, string outputDir)
        {
            var streams = bbsaFileNames
                .Select(x => File.OpenRead(x))
                .ToArray();

            var bbsa = Bbs.Bbsa.Read(streams[0]);
            foreach (var file in bbsa.Files)
            {
                var bbsaFileStream = file.OpenStream(i => streams[i]);
                if (bbsaFileStream == null)
                    continue;

                var destinationFileName = Path.Combine(outputDir, file.Name);
                var destinationFolder = Path.GetDirectoryName(destinationFileName);
                if (!Directory.Exists(destinationFolder))
                    Directory.CreateDirectory(destinationFolder);

                Console.WriteLine(file.Name);

                using (var outStream = File.Create(destinationFileName))
                    bbsaFileStream.CopyTo(outStream);
            }
        }
    }
}
