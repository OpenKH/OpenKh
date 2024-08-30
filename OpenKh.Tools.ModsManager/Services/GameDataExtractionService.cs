using OpenKh.Common;
using OpenKh.Kh1;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xe.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    public class GameDataExtractionService
    {
        private const int BufferSize = 65536;
        private const string REMASTERED_FILES_FOLDER_NAME = "remastered";

        public class BadConfigurationException : Exception
        {
            public BadConfigurationException(string message) : base(message)
            {

            }
        }

        public async Task ExtractKh2Ps2EditionAsync(
            string isoLocation,
            string gameDataLocation,
            Action<float> onProgress)
        {
            var fileBlocks = File.OpenRead(isoLocation).Using(stream =>
            {
                var bufferedStream = new BufferedStream(stream);
                var idxBlock = IsoUtility.GetFileOffset(bufferedStream, "KH2.IDX;1");
                var imgBlock = IsoUtility.GetFileOffset(bufferedStream, "KH2.IMG;1");
                return (idxBlock, imgBlock);
            });

            if (fileBlocks.idxBlock == -1 || fileBlocks.imgBlock == -1)
            {
                throw new BadConfigurationException(
                    $"Unable to find the files KH2.IDX and KH2.IMG in the ISO at '{isoLocation}'. The extraction will stop."
                );
            }

            onProgress(0);

            await Task.Run(() =>
            {
                using var isoStream = File.OpenRead(isoLocation);

                var idxOffset = fileBlocks.idxBlock * 0x800L;
                var idx = Idx.Read(new SubStream(isoStream, idxOffset, isoStream.Length - idxOffset));

                var imgOffset = fileBlocks.imgBlock * 0x800L;
                var imgStream = new SubStream(isoStream, imgOffset, isoStream.Length - imgOffset);
                var img = new Img(imgStream, idx, true);

                var fileCount = img.Entries.Count;
                var fileProcessed = 0;
                foreach (var fileEntry in img.Entries)
                {
                    var fileName = IdxName.Lookup(fileEntry) ?? $"@{fileEntry.Hash32:08X}_{fileEntry.Hash16:04X}";
                    using var stream = img.FileOpen(fileEntry);
                    var fileDestination = Path.Combine(gameDataLocation, "kh2", fileName);
                    var directoryDestination = Path.GetDirectoryName(fileDestination);
                    if (!Directory.Exists(directoryDestination))
                    {
                        Directory.CreateDirectory(directoryDestination);
                    }
                    File.Create(fileDestination).Using(dstStream => stream.CopyTo(dstStream, BufferSize));

                    fileProcessed++;
                    onProgress((float)fileProcessed / fileCount);
                }

                onProgress(1.0f);
            });
        }

        public async Task ExtractKhPcEditionAsync(
            string gameDataLocation,
            Action<float> onProgress,
            Func<string, string> getKhFilePath,
            Func<string, string> getKh3dFilePath,
            bool extractkh1,
            bool extractkh2,
            bool extractbbs,
            bool extractrecom,
            bool extractkh3d,
            Func<Exception, Task<bool>> ifRetry,
            CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                var _nameListkh1 = new string[]
                {
                    "first",
                    "second",
                    "third",
                    "fourth",
                    "fifth"
                };
                var _nameListkh2 = new string[]
                {
                    "first",
                    "second",
                    "third",
                    "fourth",
                    "fifth",
                    "sixth"
                };
                var _nameListbbs = new string[]
                {
                    "first",
                    "second",
                    "third",
                    "fourth"
                };
                var _nameListkh3d = new string[]
                {
                    "first",
                    "second",
                    "third",
                    "fourth"
                };

                var _totalFiles = 0;
                var _procTotalFiles = 0;

                onProgress(0);

                if (extractkh1)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        using var _stream = new FileStream(getKhFilePath("kh1_" + _nameListkh1[i] + ".hed"), System.IO.FileMode.Open);
                        var _hedFile = OpenKh.Egs.Hed.Read(_stream);
                        _totalFiles += _hedFile.Count();
                    }
                }
                if (extractkh2)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        using var _stream = new FileStream(getKhFilePath("kh2_" + _nameListkh2[i] + ".hed"), System.IO.FileMode.Open);
                        var _hedFile = OpenKh.Egs.Hed.Read(_stream);
                        _totalFiles += _hedFile.Count();
                    }
                }
                if (extractbbs)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        using var _stream = new FileStream(getKhFilePath("bbs_" + _nameListbbs[i] + ".hed"), System.IO.FileMode.Open);
                        var _hedFile = OpenKh.Egs.Hed.Read(_stream);
                        _totalFiles += _hedFile.Count();
                    }
                }
                if (extractrecom)
                {
                    using var _stream = new FileStream(getKhFilePath("Recom.hed"), System.IO.FileMode.Open);
                    var _hedFile = OpenKh.Egs.Hed.Read(_stream);
                    _totalFiles += _hedFile.Count();
                }
                if (extractkh3d)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        using var _stream = new FileStream(getKh3dFilePath("kh3d_" + _nameListbbs[i] + ".hed"), System.IO.FileMode.Open);
                        var _hedFile = OpenKh.Egs.Hed.Read(_stream);
                        _totalFiles += _hedFile.Count();
                    }
                }

                async Task ProcessHedStreamAsync(string outputDir, Stream hedStream, Stream img)
                {
                    await Task.Yield();

                    foreach (var entry in OpenKh.Egs.Hed.Read(hedStream))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                    retry:
                        try
                        {
                            var hash = OpenKh.Egs.Helpers.ToString(entry.MD5);
                            if (!OpenKh.Egs.EgsTools.Names.TryGetValue(hash, out var fileName))
                                fileName = $"{hash}.dat";

                            var outputFileName = Path.Combine(outputDir, fileName);

                            OpenKh.Egs.EgsTools.CreateDirectoryForFile(outputFileName);

                            var hdAsset = new OpenKh.Egs.EgsHdAsset(img.SetPosition(entry.Offset));

                            File.Create(outputFileName).Using(stream => stream.Write(hdAsset.OriginalData));

                            outputFileName = Path.Combine(outputDir, REMASTERED_FILES_FOLDER_NAME, fileName);

                            if (!ConfigurationService.SkipRemastered)
                            {

                                foreach (var asset in hdAsset.Assets)
                                {
                                    var outputFileNameRemastered = Path.Combine(OpenKh.Egs.EgsTools.GetHDAssetFolder(outputFileName), asset);

                                    OpenKh.Egs.EgsTools.CreateDirectoryForFile(outputFileNameRemastered);

                                    var assetData = hdAsset.RemasteredAssetsDecompressedData[asset];
                                    File.Create(outputFileNameRemastered).Using(stream => stream.Write(assetData));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (await ifRetry(ex))
                            {
                                goto retry;
                            }
                        }

                        _procTotalFiles++;

                        onProgress((float)_procTotalFiles / _totalFiles);
                    }
                }

                if (extractkh1)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        var outputDir = Path.Combine(gameDataLocation, "kh1");
                        using var hedStream = File.OpenRead(getKhFilePath("kh1_" + _nameListkh1[i] + ".hed"));
                        using var img = File.OpenRead(getKhFilePath("kh1_" + _nameListkh1[i] + ".pkg"));

                        await ProcessHedStreamAsync(outputDir, hedStream, img);
                    }
                }
                if (extractkh2)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        var outputDir = Path.Combine(gameDataLocation, "kh2");
                        using var hedStream = File.OpenRead(getKhFilePath("kh2_" + _nameListkh2[i] + ".hed"));
                        using var img = File.OpenRead(getKhFilePath("kh2_" + _nameListkh2[i] + ".pkg"));

                        await ProcessHedStreamAsync(outputDir, hedStream, img);
                    }
                }
                if (extractbbs)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var outputDir = Path.Combine(gameDataLocation, "bbs");
                        using var hedStream = File.OpenRead(getKhFilePath("bbs_" + _nameListbbs[i] + ".hed"));
                        using var img = File.OpenRead(getKhFilePath("bbs_" + _nameListbbs[i] + ".pkg"));

                        await ProcessHedStreamAsync(outputDir, hedStream, img);
                    }
                }
                if (extractrecom)
                {
                    for (int i = 0; i < 1; i++)
                    {
                        var outputDir = Path.Combine(gameDataLocation, "Recom");
                        using var hedStream = File.OpenRead(getKhFilePath("Recom.hed"));
                        using var img = File.OpenRead(getKhFilePath("Recom.pkg"));

                        await ProcessHedStreamAsync(outputDir, hedStream, img);
                    }
                }
                if (extractkh3d)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var outputDir = Path.Combine(gameDataLocation, "kh3d");
                        using var hedStream = File.OpenRead(getKh3dFilePath("kh3d_" + _nameListkh3d[i] + ".hed"));
                        using var img = File.OpenRead(getKh3dFilePath("kh3d_" + _nameListkh3d[i] + ".pkg"));

                        await ProcessHedStreamAsync(outputDir, hedStream, img);
                    }
                }
                onProgress(1);
            });
        }
    }
}
