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

            foreach (var subDir in "anm ard bgm dbg effect event field2d file gumibattle gumiblock gumimenu itempic libretto limit magic map menu minigame msg msn npack obj se vagstream voice remastered".Split(' '))
            {
                var firstDirPath = Path.Combine(sourceDir, subDir);
                if (Directory.Exists(firstDirPath))
                {
                    assets.AddRange(
                        Directory.EnumerateFiles(firstDirPath, "*", SearchOption.AllDirectories)
                            .Select(
                                filePath =>
                                {
                                    var relativePath = Path.GetRelativePath(sourceDir, filePath).Replace('/', '\\');
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
