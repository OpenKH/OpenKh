using OpenKh.Kh2;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Command.IdxImg
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

        [Option(ShortName = "i", LongName = "input", Description = "IDX input")]
        public string Input { get; }

        private void OnExecute()
        {
            Idx idx = OpenIdx(Input);

            string imgPath = Input.Replace(".idx", ".img", StringComparison.InvariantCultureIgnoreCase);
            using (var imgStream = File.OpenRead(imgPath))
            {
                var img = new Img(imgStream, idx, false);
                var idxName = Path.GetFileNameWithoutExtension(Input);
                var outputDir = Path.Combine(Path.GetDirectoryName(Input), idxName);
                foreach (var idxFileName in ExtractIdx(img, idx, Path.Combine(outputDir, "KH2")))
                {
                    idxName = Path.GetFileNameWithoutExtension(idxFileName);
                    ExtractIdx(img, OpenIdx(idxFileName), Path.Combine(outputDir, idxName));
                }
            }
        }

        public List<string> ExtractIdx(Img img, Idx idx, string basePath)
        {
            var idxs = new List<string>();

            foreach (var entry in idx.GetNameEntries())
            {
                var fileName = entry.Name;
                if (fileName == null)
                    fileName = $"@noname/{entry.Entry.Hash32:X08}-{entry.Entry.Hash16:X04}";

                Console.WriteLine(fileName);

                var outputFile = Path.Combine(basePath, fileName);
                var outputDir = Path.GetDirectoryName(outputFile);
                if (Directory.Exists(outputDir) == false)
                    Directory.CreateDirectory(outputDir);

                using (var file = File.Create(outputFile))
                {
                    // TODO handle decompression
                    img.FileOpen(entry.Entry).CopyTo(file);
                }

                if (Path.GetExtension(fileName) == ".idx")
                    idxs.Add(outputFile);
            }

            return idxs;
        }

        private Idx OpenIdx(string fileName)
        {
            using (var idxStream = File.OpenRead(fileName))
                return Idx.Read(idxStream);
        }
    }
}
