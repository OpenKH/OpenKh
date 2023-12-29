using OpenKh.Patcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public class YamlGeneratorService
    {
        /// <returns>Return final yml blob if it is good to save. Otherwise return `null` if it is not fit to save.</returns>
        public delegate Task<byte[]> GetDiffAsyncDelegate(byte[] rawInput, byte[] rawOutput);

        public async Task GenerateAsync(
            string ymlFile,
            GetDiffAsyncDelegate diffAsync
        )
        {
            var rawInput = await File.ReadAllBytesAsync(ymlFile);

            var mod = await Task.Run(() => File.Exists(ymlFile))
                ? Metadata.Read(new MemoryStream(rawInput, false))
                : new Metadata();

            if (mod.Assets == null)
            {
                mod.Assets = new List<AssetFile>();
            }

            mod.Assets.Clear();

            var sourceDir = Path.GetDirectoryName(ymlFile);

            var assets = new List<AssetFile>();

            void TryToSearchDir(string firstDir)
            {
                var firstDirPath = Path.Combine(sourceDir, firstDir);
                if (Directory.Exists(firstDirPath))
                {
                    assets.AddRange(
                        Directory.EnumerateFiles(firstDirPath, "*", SearchOption.AllDirectories)
                            .Select(
                                filePath =>
                                {
                                    var relativePath = Path.GetRelativePath(firstDir, filePath);
                                    return new AssetFile
                                    {
                                        Name = relativePath,
                                        Method = "copy",
                                        Source = new List<AssetFile>(
                                            new AssetFile[]
                                            {
                                                new AssetFile
                                                {
                                                    Name = relativePath,
                                                }
                                            }
                                        ),
                                    };
                                }
                            )
                    );
                }
            }

            TryToSearchDir("field2d");
            TryToSearchDir("menu");

            foreach (var asset in assets)
            {
                if (!mod.Assets.Any(it => it.Name == asset.Name))
                {
                    mod.Assets.Add(asset);
                }
            }

            {
                var temp = new MemoryStream();
                mod.Write(temp);
                var rawOutput = temp.ToArray();

                var rawFinalOutput = await diffAsync(rawInput, rawOutput);
                if (rawFinalOutput != null)
                {
                    await File.WriteAllBytesAsync(ymlFile, rawFinalOutput);
                }
            }
        }
    }
}
