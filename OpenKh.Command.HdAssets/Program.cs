using OpenKh.Common.Archives;
using System;
using System.IO;
using System.Linq;

namespace OpenKh.Command.HdAssets
{
    class Program
    {
        const string Prefix = "_ASSET_";

        static void Main(string[] args)
        {
            var path = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            if (File.Exists(path))
                UnpackFile(path);
            else
                UnpackDirectory(path);
        }

        private static void UnpackDirectory(string path)
        {
            var fileList = Directory
                .GetFiles(path, "*", SearchOption.AllDirectories)
                .Where(x => x.IndexOf(Prefix) != 0)
                .ToList();

            foreach (var filePath in fileList)
                UnpackFile(filePath);
        }


        private static void UnpackFile(string filePath)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            Console.Write($"{filePath}... ");

            using (var stream = File.OpenRead(filePath))
            {
                HdAsset asset;

                try
                {
                    // Avoids to throw a false positive to files that are not an HdAsset
                    asset = HdAsset.Read(stream);
                }
                catch
                {
                    Console.WriteLine("ERROR!");
                    return;
                }

                foreach (var entry in asset.Entries)
                {
                    var outDir = Path.GetFileNameWithoutExtension(filePath);
                    var outFileName = Path.Combine(directoryName, $"{Prefix}{outDir}", entry.Name);
                    Directory.CreateDirectory(Path.GetDirectoryName(outFileName));

                    using (var outStream = File.Create(outFileName))
                    {
                        entry.Stream.Position = 0;
                        entry.Stream.CopyTo(outStream);
                    }
                }
            }

            Console.WriteLine("Ok!");
        }
    }
}
