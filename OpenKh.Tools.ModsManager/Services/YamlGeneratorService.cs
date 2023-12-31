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

        public async Task RefillAssetFilesAsync(
            List<AssetFile> assetFiles,
            string sourceDir
        )
        {
            string NormalizePath(string path) => path?.Replace('\\', '/');

            IEnumerable<AssetFile> EnumerateAllSources(IEnumerable<AssetFile> list)
            {
                if (list != null)
                {
                    foreach (var one in list)
                    {
                        yield return one;

                        foreach (var child in EnumerateAllSources(one.Source))
                        {
                            yield return child;
                        }
                    }
                }
            }

            foreach (var asset in EnumerateAllSources(assetFiles))
            {
                asset.Name = NormalizePath(asset.Name);
            }

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
                                    var relativePath = Path.GetRelativePath(sourceDir, filePath).Replace('\\', '/');
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
                if (!EnumerateAllSources(assetFiles).Any(it => it.Name == asset.Name))
                {
                    assetFiles.Add(asset);
                }
            }
        }
    }
}
