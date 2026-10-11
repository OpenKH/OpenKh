#pragma warning disable S4790   // Disabled because the hashing algorithm used in this class is not being used for security.
#pragma warning disable CS4014  // Disabled because this class contains non-awaited tasks.

using Avalonia.Controls;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using OpenKh.Common;
using OpenKh.Egs;
using OpenKh.Kh1;
using OpenKh.Kh2;
using OpenKh.Patcher;
using OpenKh.Tools.ModManager.Classes;
using OpenKh.Tools.ModManager.Models;
using SharpYaml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xe.IO;

using Loggers = Serilog.Log;

namespace OpenKh.Tools.ModManager.Services
{
    public static class ModService
    {
        // We need a cancellation token to interrupt what we are doing should the user not want to do that anymore.
        public static CancellationTokenSource CancelTokenSource = new CancellationTokenSource();
        public static CancellationToken CancelToken = CancelTokenSource.Token;

        /// <summary>
        /// Renews the cancellation token for tasks to be able to run.
        /// </summary>
        private static void RenewCancelToken()
        {
            if (CancelToken.IsCancellationRequested)
            {
                CancelTokenSource.Dispose();

                CancelTokenSource = new CancellationTokenSource();
                CancelToken = CancelTokenSource.Token;
            }
        }

        /// <summary>
        /// Resolves the hash for a mod and returns it as a hex string.
        /// </summary>
        /// <param name="currentMod">The mod to resolve the hash from.</param>
        /// <param name="currentConfig">The current active configuration.</param>
        /// <returns>The MD5 hash for the path of the mod as a hex string.</returns>
        public static string ResolveMD5(ModModel currentMod, Config? currentConfig)
        {
            var _fetchRelativePath = currentConfig != null ? Path.GetRelativePath(PathService.ResolveMod(currentConfig), currentMod.ModPath) : currentMod.ModPath;
            var _fetchBytes = Encoding.ASCII.GetBytes(_fetchRelativePath.Replace("\\", "/"));
            var _fetchHash = MD5.HashData(_fetchBytes);

            return Convert.ToHexString(_fetchHash);
        }

        /// <summary>
        /// Extract data from the given games asynchronously.
        /// </summary>
        /// <param name="extractGames">Toggle list fore the games to extract. In order: KH1, KH2, COM, BBS, DDD.</param>
        /// <param name="currentConfig">The current active configuration.</param>
        /// <param name="isPlatformPC">If the extraction will be done for the PC editions of games.</param>
        /// <param name="reportProgress">[Optional] Callback function for this function.</param>
        /// <returns> 0x00 if it succeeds, 0x01 if it fails, 0x03 if it is cancelled.</returns>
        public static async Task<byte> Extract(List<bool> extractGames, Config currentConfig, bool isPlatformPC, Func<int, int, bool>? reportProgress = null)
        {
            // Reset the cancel token if it was called prior.
            RenewCancelToken();

            // If the extraction needs to be done for PC:
            if (isPlatformPC)
            {
                // Construct the shorthands of the games declares for extraction.
                var _fetchExtractList = new List<string>()
                {
                    extractGames[0] ? "kh1" : "",
                    extractGames[1] ? "kh2" : "",
                    extractGames[2] ? "Recom" : "",
                    extractGames[3] ? "bbs" : "",
                    extractGames[4] ? "kh3d" : ""
                }.Where(x => !String.IsNullOrEmpty(x)).ToList();

                // Declare the progress variables.
                var _fetchFilesCurrent = 0;
                var _fetchFilesTotal = 0;

                // Resolve the data folder.
                var _fetchDataPath = PathService.ResolveData(currentConfig, true);

                // If the data folder is null, resort to the default of /modmanager/extract.
                if (String.IsNullOrEmpty(_fetchDataPath))
                {
                    currentConfig.Frontend.DataPath = Path.Combine(AppContext.BaseDirectory, "extract");
                    _fetchDataPath = currentConfig.Frontend.DataPath;
                }

                // Start a task for header traversal:
                await Task.Run(() =>
                {
                    // For every game that is declared for extraction:
                    foreach (var _fetchExtractGame in _fetchExtractList)
                    {
                        // Fetch the game path accounting for DDD and the game region. If the region was not able to be processed, consider it to be International.
                        var _fetchGamePath = _fetchExtractGame == "kh3d" ? PathService.ResolvePath28(currentConfig) : PathService.ResolvePath1525(currentConfig);
                        var _resolveJP = PathService.ResolveRegionJP(currentConfig) ?? false;

                        // Fetch all the header files for the current game.
                        var _fetchPackagePath = Path.Combine(_fetchGamePath, "Image", currentConfig.Frontend.TargetPlatform == Platform.STEAM ? "dt" : (_resolveJP ? "jp" : "en"));
                        var _fetchHeaderFiles = Directory.GetFiles(_fetchPackagePath).Where(x => x.Contains(_fetchExtractGame) && x.EndsWith(".hed"));

                        // For every header file:
                        foreach (var _fetchHeader in _fetchHeaderFiles)
                        {
                            // Open and read all the files. Fetch the entry count, note it for progress, and close the file.
                            using (var _fetchHedStream = new FileStream(_fetchHeader, FileMode.Open))
                            {
                                var _fetchFiles = Hed.Read(_fetchHedStream);
                                _fetchFilesTotal += _fetchFiles.Count();
                            }

                            // If cancellation was requested, break out.
                            if (CancelToken.IsCancellationRequested)
                                break;
                        }

                        // If cancellation was requested, break out.
                        if (CancelToken.IsCancellationRequested)
                            break;
                    }
                }, CancelToken);

                // If cancellation was requested, cancel the process.
                if (CancelToken.IsCancellationRequested)
                    return 0x03;

                await Task.Run(() =>
                {
                    // Start a parallel for every header parsed:
                    Parallel.ForEach(_fetchExtractList.AsParallel(), (_fetchExtractGame, _fetchStateGame) =>
                    {
                        // Fetch the game path accounting for DDD and the game region. If the region was not able to be processed, consider it to be International.
                        var _fetchGamePath = _fetchExtractGame == "kh3d" ? PathService.ResolvePath28(currentConfig) : PathService.ResolvePath1525(currentConfig);
                        var _resolveJP = PathService.ResolveRegionJP(currentConfig) ?? false;

                        // Fetch all the header files for the current game.
                        var _fetchPackagePath = Path.Combine(_fetchGamePath, "Image", currentConfig.Frontend.TargetPlatform == Platform.STEAM ? "dt" : (_resolveJP ? "jp" : "en"));
                        var _fetchHeaderFiles = Directory.GetFiles(_fetchPackagePath).Where(x => x.Contains(_fetchExtractGame) && x.EndsWith(".hed"));

                        // Start a parallel for every header parsed:
                        Parallel.ForEach(_fetchHeaderFiles.AsParallel(), (_fetchHeader, _fetchStateHeader) =>
                        {
                            // Resolve the package name derived from the header.
                            var _fetchPackage = Path.ChangeExtension(_fetchHeader, ".pkg");

                            // Open the header for reading, and read it as a HED class.
                            using (var _fetchHedStream = new FileStream(_fetchHeader, FileMode.Open))
                            {
                                var _fetchFiles = Hed.Read(_fetchHedStream);

                                // Open the derivitive package for shared reading.
                                using (var _fetchPkgStream = new FileStream(_fetchPackage, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    foreach (var _fetchFile in _fetchFiles)
                                    {
                                        // Increment the file progress.
                                        _fetchFilesCurrent++;

                                        // Fetch the hex string of the file hash and check if the hash is known.
                                        var _fetchHashText = Convert.ToHexString(_fetchFile.MD5);
                                        var _fetchNameValue = EgsTools.Names.FirstOrDefault(x => x.Key == _fetchHashText).Value;

                                        // If it is, fetch the name. Otherwise construct a name from the hash string.
                                        var _fetchFileName = String.IsNullOrEmpty(_fetchNameValue) ? $"{_fetchHashText}.dat" : _fetchNameValue;

                                        // Resolve the extraction target for the file and its directory.
                                        var _fetchFilePath = Path.Combine(_fetchDataPath, _fetchExtractGame, _fetchFileName);
                                        var _fetchFileDir = Path.GetDirectoryName(_fetchFilePath);

                                        // If the target directory does not exist, create it.
                                        if (!Directory.Exists(_fetchFileDir))
                                            Directory.CreateDirectory(_fetchFileDir);

                                        // Set the stream position and fetch the file as an HDAsset.
                                        _fetchPkgStream.SetPosition(_fetchFile.Offset);
                                        var _fetchData = new EgsHdAsset(_fetchPkgStream);

                                        // Write the file to the disk.
                                        File.WriteAllBytes(_fetchFilePath, _fetchData.OriginalData);

                                        // If the file has any remastered assets:
                                        if (_fetchData.Assets.Count() != 0x00)
                                        {
                                            // Construct the remastered extraction path.
                                            var _fetchRemasterPath = Path.Combine(_fetchDataPath, _fetchExtractGame, "remastered", _fetchFileName);

                                            foreach (var _fetchAsset in _fetchData.Assets)
                                            {
                                                // Fetch the asset target path and directory.
                                                var _fetchAssetPath = Path.Combine(_fetchRemasterPath, _fetchAsset);
                                                var _fetchAssetDir = Path.GetDirectoryName(_fetchAssetPath);

                                                // If the directory does not exist, create it.
                                                if (!Directory.Exists(_fetchAssetDir))
                                                    Directory.CreateDirectory(_fetchAssetDir);

                                                // Fetch the data for the asset and write it.
                                                var _fetchAssetData = _fetchData.RemasteredAssetsDecompressedData[_fetchAsset];
                                                File.WriteAllBytes(_fetchAssetPath, _fetchAssetData);

                                                if (CancelToken.IsCancellationRequested)
                                                    break;
                                            }

                                        }

                                        // Callback to the progress feedback function.

                                        if (reportProgress != null)
                                            reportProgress(_fetchFilesCurrent, _fetchFilesTotal);

                                        if (CancelToken.IsCancellationRequested)
                                            break;
                                    }

                                }
                            }

                            if (CancelToken.IsCancellationRequested)
                                _fetchStateHeader.Stop();
                        });

                        if (CancelToken.IsCancellationRequested)
                            _fetchStateGame.Stop();
                    });
                }, CancelToken);

                if (CancelToken.IsCancellationRequested)
                    return 0x03;
            }

            else
            {
                for (int i = 0x00; i < 0x05; i++)
                {
                    if (!extractGames[i])
                        continue;

                    var _fetchPath = currentConfig.Emulator.RomPath[i];

                    await Task.Run(async () =>
                    {
                        var _fetchDataPath = PathService.ResolveData(currentConfig, true);

                        // If the data folder is null, resort to the default of /modmanager/extract.
                        if (String.IsNullOrEmpty(_fetchDataPath))
                        {
                            currentConfig.Frontend.DataPath = Path.Combine(AppContext.BaseDirectory, "extract");
                            _fetchDataPath = currentConfig.Frontend.DataPath;
                        }

                        using (var _fetchStreamISO = new FileStream(_fetchPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            switch (i)
                            {
                                case 0x00:
                                {
                                    var _fetchOffsetIDX = IsoUtility.GetFileOffset(_fetchStreamISO, "KINGDOM.IDX;1");
                                    var _fetchOffsetIMG = IsoUtility.GetFileOffset(_fetchStreamISO, "SYSTEM.CNF;1");

                                    var _fetchSubStrIDX = new SubStream(_fetchStreamISO, _fetchOffsetIDX * 0x800, _fetchStreamISO.Length - (_fetchOffsetIDX * 0x800));
                                    var _fetchSubStrIMG = new SubStream(_fetchStreamISO, _fetchOffsetIMG * 0x800, _fetchStreamISO.Length - (_fetchOffsetIMG * 0x800));

                                    var _fetchIDX = Idx1.Read(_fetchSubStrIDX);
                                    var _fetchIMG = new Img1(_fetchSubStrIMG, _fetchIDX, 0x00);

                                    var _fetchCount = _fetchIMG.Entries.Count;
                                    var _fetchProcessed = 0x00;

                                    Parallel.ForEach(_fetchIMG.Entries.AsParallel(), (_fetchEntry, _fetchToken) =>
                                    {
                                        var _fetchTargetName = Idx1Name.Lookup(_fetchEntry.Value) ?? $"@noname/{_fetchEntry.Value.Hash:X08}";

                                        using (var _fetchStreamFILE = _fetchIMG.FileOpen(_fetchEntry.Value))
                                        {
                                            if (_fetchStreamFILE != null)
                                            {
                                                var _targetFilePath = Path.Combine(_fetchDataPath, "kh1", _fetchTargetName);
                                                var _targetFileDirectory = Path.GetDirectoryName(_targetFilePath);

                                                if (!Directory.Exists(_targetFileDirectory))
                                                    Directory.CreateDirectory(_targetFileDirectory);

                                                var _fetchData = _fetchStreamFILE.ReadAllBytes();
                                                File.WriteAllBytes(_targetFilePath, _fetchData);
                                            }
                                        }

                                        _fetchProcessed++;

                                        if (reportProgress != null)
                                        {
                                            var _fetchProgress = reportProgress(_fetchProcessed, _fetchCount);

                                            if (!_fetchProgress || CancelToken.IsCancellationRequested)
                                                _fetchToken.Stop();
                                        }
                                    });
                                } break;
                                case 0x01:
                                {
                                    var _fetchOffsetIDX = IsoUtility.GetFileOffset(_fetchStreamISO, "KH2.IDX;1");
                                    var _fetchOffsetIMG = IsoUtility.GetFileOffset(_fetchStreamISO, "KH2.IMG;1");

                                    var _fetchSubStrIDX = new SubStream(_fetchStreamISO, _fetchOffsetIDX * 0x800, _fetchStreamISO.Length - (_fetchOffsetIDX * 0x800));
                                    var _fetchSubStrIMG = new SubStream(_fetchStreamISO, _fetchOffsetIMG * 0x800, _fetchStreamISO.Length - (_fetchOffsetIMG * 0x800));

                                    var _fetchIDX = Idx.Read(_fetchSubStrIDX);
                                    var _fetchIMG = new Img(_fetchSubStrIMG, _fetchIDX, true);

                                    var _fetchCount = _fetchIMG.Entries.Count;
                                    var _fetchProcessed = 0x00;

                                    Parallel.ForEach(_fetchIMG.Entries.AsParallel(), (_fetchEntry, _fetchToken) =>
                                    {
                                        var _fetchTargetName = IdxName.Lookup(_fetchEntry) ?? $"@{_fetchEntry.Hash32:08X}_{_fetchEntry.Hash16:04X}";

                                        using (var _fetchStreamFILE = _fetchIMG.FileOpen(_fetchEntry))
                                        {
                                            var _targetFilePath = Path.Combine(_fetchDataPath, "kh2", _fetchTargetName);
                                            var _targetFileDirectory = Path.GetDirectoryName(_targetFilePath);

                                            if (!Directory.Exists(_targetFileDirectory))
                                                Directory.CreateDirectory(_targetFileDirectory);

                                            var _fetchData = _fetchStreamFILE.ReadAllBytes();
                                            File.WriteAllBytes(_targetFilePath, _fetchData);
                                        }

                                        _fetchProcessed++;

                                        if (reportProgress != null)
                                        {
                                            var _fetchProgress = reportProgress(_fetchProcessed, _fetchCount);

                                            if (!_fetchProgress || CancelToken.IsCancellationRequested)
                                                _fetchToken.Stop();
                                        }
                                    });
                                }
                                break;
                            }
                        }
                    }, CancelToken);
                }
            }

            // If the function made it this far, it is a surefire success.
            return 0x00;
        }

        /// <summary>
        /// Installs a mod from the specified Git platform, or from GitHub if no platform is given.
        /// </summary>
        /// <param name="repoName">The name of the repository to install.</param>
        /// <param name="currentConfig">The current active configuration.</param>
        /// <param name="reportProgress">[Optional] Callback function for this function.</param>
        /// <returns> 0x00 if it succeeds, 0x01 if it fails, 0x03 if it is cancelled.</returns>
        public static async Task<byte> InstallGit(string repoName, Config currentConfig, Func<long, long, bool, bool>? reportProgress = null)
        {
            // Reset the cancel token if it was called prior.
            RenewCancelToken();

            // Fetch the platfrom from the given string.
            var _fetchPlatform = repoName.Contains('@') ? repoName.Split('@').Last() : null;
            repoName = _fetchPlatform != null ? repoName.Replace("@" + _fetchPlatform, "") : repoName;

            // Fetch the branch from the given string.
            var _fetchBranch = repoName.Contains(':') ? repoName.Split(':').Last() : null;
            repoName = _fetchBranch != null ? repoName.Replace(":" + _fetchBranch, "") : repoName;

            // Fetch the author and the name from the given string.
            var _fetchAuthor = repoName.Split('/').First();
            var _fetchName = repoName.Split('/').Last();

            // P.S. - I know my string handling is SHIT and can break. If anyone knows how to do this in C# with regex I highly advise you to fix this.

            var _fetchModPath = PathService.ResolveMod(currentConfig);
            var _fetchCurrentModDir = Path.Combine(_fetchModPath, _fetchAuthor, _fetchName);

            // If the directory does not exist, create it. 
            if (!Directory.Exists(_fetchCurrentModDir))
                Directory.CreateDirectory(_fetchCurrentModDir);

            // If it does:
            else
            {
                // Ask the user if they want to overwrite the mod.
                var _fetchResult = await DialogService.ShowQuestion(null, "Overwrite Existing Mod?", "The mod you are trying to install already exists. Do you wish to overwrite it?");

                // If they don't, cancel.
                if (_fetchResult == false)
                    return 0x03;

                // If they do, delete the old mod.
                await Task.Run(() =>
                {
                    Directory.Delete(_fetchCurrentModDir, true);
                    Directory.CreateDirectory(_fetchCurrentModDir);
                }, CancellationToken.None);
            }

            // Try to see if "mod.yml" actually exists.
            var _fetchMetadataResponse = await GitService.CheckFile(_fetchPlatform == null ? "github.com" : _fetchPlatform.ToLower(), _fetchAuthor, _fetchName, "mod.yml", _fetchBranch);

            // If the file doesn't exist, abort.
            if (!_fetchMetadataResponse)
            {
                Directory.Delete(_fetchCurrentModDir, true);
                return 0x01;
            }

            // Otherwise, clone the mod.
            // Use GitService.FetchSource if it's GitHub, use GitService.FetchZipball otherwise.
            // This only supports GitHub, Gitea Instances, and Forgejo Instances at this moment.

            await Task.Run(async () =>
            {
                if (_fetchPlatform != null && _fetchPlatform.ToLower() != "github.com")
                {
                    var _fetchZipball = await GitService.FetchZipball(_fetchPlatform, _fetchAuthor, _fetchName, CancelToken, _fetchBranch, reportProgress);

                    if (_fetchZipball != null)
                    {
                        using (var _fetchMemStr = new MemoryStream(_fetchZipball))
                        {
                            var _fetchArchive = new ZipArchive(_fetchMemStr);

                            if (_fetchArchive != null)
                            {
                                for (int i = 1; i < _fetchArchive.Entries.Count; i++)
                                {
                                    // Fetch the current entry.
                                    var _fetchEntry = _fetchArchive.Entries[i];
                                    var _fetchRoot = _fetchArchive.Entries[0].FullName;

                                    var _normalizePath = Path.GetRelativePath(_fetchRoot, _fetchEntry.FullName);
                                    _normalizePath = _normalizePath.Replace("\\", "/");

                                    // If the entry is a directory (yes, really): Move on to the next one.
                                    if (_normalizePath.EndsWith('/'))
                                        continue;

                                    if (_normalizePath.Contains("../") || _normalizePath.Contains("/.."))
                                        continue;

                                    // Construct the target paths for the file and the directory.
                                    var _fetchFileTarget = Path.Combine(_fetchCurrentModDir, _normalizePath);
                                    var _fetchDirectory = Path.Combine(_fetchCurrentModDir, Path.GetDirectoryName(_normalizePath));

                                    // Should the target directory not exist, we make it exist.
                                    if (!Directory.Exists(_fetchDirectory))
                                        Directory.CreateDirectory(_fetchDirectory);

                                    // Extract the file.
                                    _fetchEntry.ExtractToFile(_fetchFileTarget, true);

                                    // If the progress feedback exists:
                                    if (reportProgress != null)
                                    {
                                        // Feedback to the progress and see the result.
                                        var _fetchProgress = reportProgress(i + 1, _fetchArchive.Entries.Count, true);

                                        // If the result is false, meaning cancellation requested, break out immediately.
                                        if (!_fetchProgress)
                                            break;
                                    }

                                    // Otherwise manually check for cancellation and break out if it's requested.
                                    else if (CancelToken.IsCancellationRequested)
                                        break;
                                }
                            }
                        }
                    }
                }

                else
                {
                    var _fetchSource = await GitService.FetchSource(_fetchAuthor, _fetchName, CancelToken, _fetchBranch, reportProgress);

                    if (_fetchSource != null)
                    {
                        for (int i = 0; i < _fetchSource.Count; i++)
                        {
                            // Fetch the current entry.
                            var _fetchEntry = _fetchSource.ElementAt(i);

                            var _fetchFilePath = _fetchEntry.Key;
                            var _fetchFileData = _fetchEntry.Value;

                            // Construct the target paths for the file and the directory.
                            var _fetchFileTarget = Path.Combine(_fetchCurrentModDir, _fetchFilePath);
                            var _fetchDirectory = Path.Combine(_fetchCurrentModDir, Path.GetDirectoryName(_fetchFilePath));

                            // Should the target directory not exist, we make it exist.
                            if (!Directory.Exists(_fetchDirectory))
                                Directory.CreateDirectory(_fetchDirectory);

                            // Extract the file.
                            await File.WriteAllBytesAsync(_fetchFileTarget, _fetchFileData);

                            // If the progress feedback exists:
                            if (reportProgress != null)
                            {
                                // Feedback to the progress and see the result.
                                var _fetchProgress = reportProgress(i + 1, _fetchSource.Count, true);

                                // If the result is false, meaning cancellation requested, break out immediately.
                                if (!_fetchProgress)
                                    break;
                            }

                            // Otherwise manually check for cancellation and break out if it's requested.
                            else if (CancelToken.IsCancellationRequested)
                                break;
                        }
                    }
                }

            }, CancelToken);

            // After the task finishes/aborts, if we requested cancellation, abort.
            if (CancelToken.IsCancellationRequested)
            {
                Directory.Delete(_fetchCurrentModDir, true);
                return 0x03;
            }

            // If not, we need to submit the Git information.

            var _fetchGitInfo = new GitModel
            {
                Author = _fetchAuthor,
                Repository = _fetchName,
                Platform = _fetchPlatform ?? "github.com",
                Branch = _fetchBranch,
                LatestCommit = await GitService.FetchLatestCommit(_fetchPlatform == null ? "github.com" : _fetchPlatform.ToLower(), _fetchAuthor, _fetchName)
            };

            var _fetchGitInfoPath = Path.Combine(_fetchCurrentModDir, "git_info.yml");
            var _fetchSerial = YamlSerializer.Serialize(_fetchGitInfo);

            File.WriteAllText(_fetchGitInfoPath, _fetchSerial);

            return 0x00;
        }

        /// <summary>
        /// Installs a mod from the specified Git platform, or from GitHub if no platform is given.
        /// </summary>
        /// <param name="fileName">The name of the local file to install.</param>
        /// <param name="currentConfig">The current active configuration.</param>
        /// <param name="reportProgress">[Optional] Callback function for this function.</param>
        /// <returns> 0x00 if it succeeds, 0x01 if it fails, 0x03 if it is cancelled.</returns>
        public static async Task<byte> InstallLocal(string fileName, Config currentConfig, Func<int, int, bool>? reportProgress = null)
        {
            // Reset the cancel token if it was called prior.
            RenewCancelToken();

            var _fetchModPath = PathService.ResolveMod(currentConfig);

            // Get the extension to check and the mod path to process.
            var _fetchExtension = Path.GetExtension(fileName).ToLower();
            var _fetchCurrentModDir = Path.Combine(_fetchModPath, "local", Path.GetFileNameWithoutExtension(fileName));

            // If the directory don't exist, create it.
            if (!Directory.Exists(_fetchCurrentModDir))
                Directory.CreateDirectory(_fetchCurrentModDir);

            // If it does:
            else
            {
                // Ask the user if they want to overwrite the mod.
                var _fetchResult = await DialogService.ShowQuestion(null, "Overwrite Existing Mod?", "The mod you are trying to install already exists. Do you wish to overwrite it?");

                // If they don't, cancel.
                if (_fetchResult == false)
                    return 0x03;

                // If they do, delete the old mod.
                await Task.Run(() =>
                {
                    Directory.Delete(_fetchCurrentModDir, true);
                    Directory.CreateDirectory(_fetchCurrentModDir);
                }, CancellationToken.None);
            }

            // If the file is a ZIP Archive:
            if (_fetchExtension.Contains("zip"))
            {
                // I hate that I have to use streams to use a file class.
                // Just have the stream internally, kappa.
                using (var _fileStream = new FileStream(fileName, FileMode.Open))
                {
                    // Fetch the archive on read-only to prevent... issues.
                    var _fetchArchive = new ZipArchive(_fileStream, ZipArchiveMode.Read);

                    // If the archive does not contain a YAML, abort.
                    if (_fetchArchive.Entries.FirstOrDefault(x => x.Name == "mod.yml") == null)
                        return 0x01;

                    // Now we gettin' to the nitty gritty.
                    await Task.Run(() =>
                    {
                        // Why am I iterating every file and handling it separately instead of using ExtractAll?
                        // Because Microsoft is a piece of shit compant that did not implement progress feedback to that function :^)
                        for (int i = 0; i < _fetchArchive.Entries.Count; i++)
                        {
                            // Fetch the current entry.
                            var _fetchEntry = _fetchArchive.Entries[i];
                            var _normalizePath = _fetchEntry.FullName.Replace("\\", "/");

                            // If the entry is a directory (yes, really): Move on to the next one.
                            if (_normalizePath.EndsWith('/'))
                                continue;

                            if (_normalizePath.Contains("../") || _normalizePath.Contains("/.."))
                                continue;

                            // Construct the target paths for the file and the directory.
                            var _fetchFileTarget = Path.Combine(_fetchCurrentModDir, _fetchEntry.FullName);
                            var _fetchDirectory = Path.Combine(_fetchCurrentModDir, Path.GetDirectoryName(_fetchEntry.FullName));

                            // Should the target directory not exist, we make it exist.
                            if (!Directory.Exists(_fetchDirectory))
                                Directory.CreateDirectory(_fetchDirectory);

                            // Extract the file.
                            _fetchEntry.ExtractToFile(_fetchFileTarget, true);

                            // If the progress feedback exists:
                            if (reportProgress != null)
                            {
                                // Feedback to the progress and see the result.
                                var _fetchProgress = reportProgress(i + 1, _fetchArchive.Entries.Count);

                                // If the result is false, meaning cancellation requested, break out immediately.
                                if (!_fetchProgress)
                                    break;
                            }

                            // Otherwise manually check for cancellation and break out if it's requested.
                            else if (CancelToken.IsCancellationRequested)
                                break;
                        }
                    }, CancelToken);

                    // If the task ended/aborted and cancellation was requested, abort.
                    if (CancelToken.IsCancellationRequested)
                    {
                        Directory.Delete(_fetchCurrentModDir, true);
                        return 0x03;
                    }
                }
            }

            // If the file is a LUA Script, handle it.
            // This is code that was YANKED from the old mod manager, cleaned up and brought up to standard.
            // I am not commenting this yet.
            else if (_fetchExtension.Contains("lua"))
            {
                var _fetchCurrentLuaName = Path.Combine(_fetchCurrentModDir, Path.GetFileName(fileName));

                File.Copy(fileName, _fetchCurrentLuaName, true);

                var _createMetadata = new Metadata
                {
                    Title = Path.GetFileNameWithoutExtension(fileName) + " (Lua)",
                    Description = "This Metadata has been automatically generated for this Lua Modification.",
                    IsValid = true,
                    Assets = new List<AssetFile>()
                };

                var _createSource = new AssetFile() { Name = Path.GetFileName(fileName) };
                var _createFile = new AssetFile()
                {
                    Name = "scripts/" + Path.GetFileName(fileName),
                    Method = "copy",
                    Source = new List<AssetFile>() { _createSource },
                };

                _createMetadata.Assets.Add(_createFile);

                using (var _strReader = new StreamReader(_fetchCurrentLuaName))
                {
                    while (!_strReader.EndOfStream)
                    {
                        string _fetchLine = await _strReader.ReadLineAsync();

                        if (_fetchLine.Contains("LUAGUI"))
                        {
                            string _lineGib = "";
                            string _lineLead = "";

                            _lineGib = _fetchLine.Substring(_fetchLine.IndexOf("=") + 1).Replace("\"", "").Replace("\'", "").Trim();
                            _lineLead = string.Concat(_fetchLine.Take(11));

                            switch (_lineLead)
                            {
                                case "LUAGUI_NAME":
                                    _createMetadata.Title = _lineGib;
                                    break;
                                case "LUAGUI_AUTH":
                                    _createMetadata.OriginalAuthor = _lineGib;
                                    break;
                                case "LUAGUI_DESC":
                                    _createMetadata.Description = _lineGib;
                                    break;
                            }
                        }
                    }
                }

                var _yamlPath = Path.Combine(_fetchCurrentModDir, "mod.yml");
                await File.WriteAllTextAsync(_yamlPath, _createMetadata.ToString(), CancellationToken.None);
            }

            // If the file is ANY PCPatch Package, handle it.
            // This is code that was YANKED from the old mod manager, cleaned up and brought up to standard.
            // I am not commenting this yet.
            else if (_fetchExtension.Contains("pcpatch"))
            {
                using (var _fileStream = new FileStream(fileName, FileMode.Open))
                {
                    var _fetchArchive = new ZipArchive(_fileStream, ZipArchiveMode.Read);

                    await Task.Run(() =>
                    {
                        for (int i = 0; i < _fetchArchive.Entries.Count; i++)
                        {
                            var _fetchEntry = _fetchArchive.Entries[i];
                            var _normalizePath = _fetchEntry.FullName.Replace("\\", "/");

                            // If the entry is a directory (yes, really): Move on to the next one.
                            if (_normalizePath.EndsWith('/'))
                                continue;

                            if (_normalizePath.Contains("../") || _normalizePath.Contains("/.."))
                                continue;


                            var _pathSplit = _fetchEntry.FullName.Split(_fetchEntry.FullName.IndexOf('/') > -1 ? "/" : "\\");
                            var _pathPackage = _pathSplit[0];

                            var _accommodatePath = _fetchEntry.FullName.Replace("original/", "").Replace(_pathPackage + "/", "");

                            var _fetchFileTarget = Path.Combine(_fetchCurrentModDir, _accommodatePath);
                            var _fetchDirectory = Path.Combine(_fetchCurrentModDir, Path.GetDirectoryName(_accommodatePath));

                            if (!Directory.Exists(_fetchDirectory))
                                Directory.CreateDirectory(_fetchDirectory);

                            _fetchEntry.ExtractToFile(_fetchFileTarget, true);

                            var _fetchProgress = reportProgress(i + 1, _fetchArchive.Entries.Count);

                            if (!_fetchProgress)
                                break;
                        }
                    }, CancelToken);

                    if (CancelToken.IsCancellationRequested)
                    {
                        Directory.Delete(_fetchCurrentModDir, true);
                        return 0x03;
                    }

                    var _fetchMetadata = new Metadata
                    {
                        Title = Path.GetFileNameWithoutExtension(fileName) + $" ({_fetchExtension.ToUpper()})",
                        Description = $"This Metadata has been automatically generated for this {_fetchExtension.ToUpper()} Modification.",
                        Assets = new List<AssetFile>()
                    };

                    await Task.Run(() =>
                    {
                        foreach (var _fetchEntry in _fetchArchive.Entries.Where(x => !string.IsNullOrEmpty(x.Name)))
                        {
                            var _pathSplit = _fetchEntry.FullName.Split(_fetchEntry.FullName.IndexOf('/') > -1 ? "/" : "\\");
                            var _pathPackage = _pathSplit[0];

                            var _accommodatePath = _fetchEntry.FullName.Replace("original/", "").Replace(_pathPackage + "/", "");

                            var _createSource = new AssetFile() { Name = _accommodatePath };

                            var _createFile = new AssetFile()
                            {
                                Name = _accommodatePath,
                                Method = "copy",
                                Source = new List<AssetFile>() { _createSource },
                                Platform = "pc",
                                Package = _pathPackage
                            };

                            _fetchMetadata.Assets.Add(_createFile);

                            if (CancelToken.IsCancellationRequested)
                                break;
                        }
                    }, CancelToken);

                    if (CancelToken.IsCancellationRequested)
                    {
                        Directory.Delete(_fetchCurrentModDir, true);
                        return 0x03;
                    }

                    var _yamlPath = Path.Combine(_fetchCurrentModDir, "mod.yml");
                    await File.WriteAllTextAsync(_yamlPath, _fetchMetadata.ToString(), CancellationToken.None);
                }
            }

            // All is good, return success.
            return 0x00;
        }

        /// <summary>
        /// Builds the mods given according to the config given and reports back progress.
        /// </summary>
        /// <param name="modsList">The mods to build.</param>
        /// <param name="currentConfig">The config to base the build on.</param>
        /// <param name="reportModProgress">[Optional] Callback to the mod build progress.</param>
        /// <param name="reportAssetProgress">[Optional] Callback to the asset build progress per mod.</param>
        /// <returns> 0x00 if it succeeds, 0x01 if it fails, 0x03 if it is cancelled.</returns>
        public static async Task<byte> Build(IEnumerable<ModModel> modsList, Config currentConfig, Func<string, int, int, bool>? reportModProgress = null, Func<int, int, bool>? reportAssetProgress = null)
        {
            // Reset the cancel token if it was called prior.
            RenewCancelToken();

            LogService.Show();

            // Create the patcher processor and the package map dictionary.
            var _fetchPatcher = new PatcherProcessor();
            var _fetchPackageMap = new ConcurrentDictionary<string, string>();

            // Fetch the game name and its ID from the cshorthand list.
            var _fetchGameName = currentConfig.Frontend.TargetGame.ToString().ToLower();
            var _fetchGameId = PatcherProcessor.GameShorthand[(int)currentConfig.Frontend.TargetGame];

            // Resolve all the necessary paths.

            var _fetchDataPath = PathService.ResolveData(currentConfig);
            var _fetchGamePath = PathService.ResolveGame(currentConfig);
            var _fetchBuildPath = PathService.ResolveBuild(currentConfig);

            var _fetchPKGMapPath = Path.Combine(_fetchBuildPath, "patch-package-map.txt");

            // Run the cleanup task for the build.
            await Task.Run(async () =>
            {
                if (Directory.Exists(_fetchBuildPath))
                {
                    Loggers.ForContext(typeof(ModService)).Verbose("Cleaning up the build directory...");

                    Directory.Delete(_fetchBuildPath, true);
                    Directory.CreateDirectory(_fetchBuildPath);
                }
            }, CancellationToken.None);

            // Declare all the progress variabled.
            var _currentModName = "";
            var _currentModIndex = 0;
            var _currentModCount = modsList.Where(x => x.ModValid && x.ModActive).Count();

            // Check if the game we are patching is the Japanese version of KHPC.
            var _fetchJapanese = PathService.ResolveRegionJP(currentConfig) ?? false;

            // If the callback function isn't null, create a feedback task.
            // This task should NOT be awaited.
            if (reportModProgress != null)
            {
                Task.Run(async () =>
                {
                    while (!CancelToken.IsCancellationRequested)
                    {
                        var _fetchProgress = reportModProgress(_currentModName, _currentModIndex, _currentModCount);

                        if (!_fetchProgress)
                            break;

                        await Task.Delay(TimeSpan.FromMilliseconds(5), CancelToken);
                    }
                }, CancelToken);
            }

            // Run the actual patcher task:
            await Task.Run(async () =>
            {
                try
                {
                    // The mods will be handles sequentially to ensure their build is sound.
                    // This operation CANNOT be done in parallel.
                    for (int _modIdx = modsList.Count() - 1; _modIdx >= 0; _modIdx--)
                    {
                        // Fetch the target mod.
                        var _fetchMod = modsList.ElementAt(_modIdx);

                        // Reset asset progress on the mod.

                        if (reportAssetProgress != null)
                            reportAssetProgress(0, 0);

                        // Increase handles mod progress.
                        _currentModIndex++;

                        // If the mod isn't valid or isn't active, skip it. 
                        if (!_fetchMod.ModValid || !_fetchMod.ModActive)
                            continue;

                        // Feed the title of the mod to the callback.
                        _currentModName = _fetchMod.ModTitle;

                        Loggers.ForContext(typeof(ModService)).Verbose($"Staring build of: {_currentModName}...");

                        // Fetch the [mod.yml] file for the mod and read it as Metadata.
                        var _fetchYamlPath = Path.Combine(_fetchMod.ModPath, "mod.yml");
                        var _fetchMetadata = Metadata.Read(_fetchYamlPath);

                        if (_fetchMod.ModPreferences != null)
                        {
                            Loggers.ForContext(typeof(ModService)).Verbose($"[{_currentModName}] - Recording mod preferences...");

                            var _fetchListMetadata = new List<Preference>();

                            foreach (var _fetchPref in _fetchMod.ModPreferences)
                            {
                                var _fetchPrefMetadata = new Preference()
                                {
                                    Title = _fetchPref.Title,
                                    Key = _fetchPref.Key,
                                    Description = _fetchPref.Description,
                                    Options = _fetchPref.Options,
                                    Category = _fetchPref.Category,
                                    Value = _fetchPref.Value
                                };

                                _fetchListMetadata.Add(_fetchPrefMetadata);
                            }

                            _fetchMetadata.Preferences = _fetchListMetadata;
                        }

                        var _fetchTargetPlatform = (int)currentConfig.Frontend.TargetPlatform;
                        var _fetchTargetGame = (int)currentConfig.Frontend.TargetGame;
                        var _finalGamePath = _fetchTargetPlatform == 0x00 ? currentConfig.Emulator.RomPath[_fetchTargetGame] : _fetchGamePath;

                        _fetchMetadata.Assets = _fetchMetadata.Assets.Where(x => x.Platform == (_fetchTargetPlatform == 0x00 ? "ps2" : "pc") || String.IsNullOrEmpty(x.Platform)).ToList();

                        Loggers.ForContext(typeof(ModService)).Verbose($"[{_currentModName}] - Processing files...");

                        // Await the patcher process for this mod.
                        await _fetchPatcher.Patch
                        (
                            _fetchDataPath,
                            _fetchBuildPath,
                            _fetchMetadata,
                            _fetchMod.ModPath,
                            _finalGamePath,
                            _fetchTargetPlatform,
                            _fetchTargetGame,
                            _fetchJapanese,
                            CancelToken,
                            _fetchPackageMap,
                            reportProgress: reportAssetProgress
                        );

                        // If cancellation is requested, break out.
                        if (CancelToken.IsCancellationRequested)
                            break;
                    }
                }
                catch (TaskCanceledException) { }

            }, CancelToken);

            LogService.Dismiss();

            // If cancellation is requested, cancel the process, but clean the build folder beforehand.
            if (CancelToken.IsCancellationRequested)
            {
                Directory.Delete(_fetchBuildPath, true);
                return 0x03;
            }

            // If we came this far, we must have succeeded. Commit the package map to the build folder.

            using (var _writePackageMap = new StreamWriter(_fetchPKGMapPath))
                foreach (var _mapEntry in _fetchPackageMap)
                    await _writePackageMap.WriteLineAsync(_mapEntry.Key + " $$$$ " + _mapEntry.Value);

            return 0x00;
        }

        /// <summary>
        /// Runs the game. Requires to be called from a View because of Avalonia's cross-platform launch service.
        /// </summary>
        /// <param name="currentConfig">The config to base the launch on.</param>
        /// <param name="topLevelObject">The top-level fetched from the calling View.</param>
        /// <returns>Only true for now.</returns>
        public static async Task<bool> Run(Config currentConfig, TopLevel topLevelObject)
        {
            var _fetchTargetGame = currentConfig.Frontend.TargetGame;
            var _fetchTargetPlatform = currentConfig.Frontend.TargetPlatform;

            if (_fetchTargetPlatform != Platform.PCSX2)
            {
                var _fetchGamePath = PathService.ResolveGame(currentConfig);

                var _fetchAPIFilePath = Path.Combine(_fetchGamePath, "steam_appid.txt");

                var _fetchLauncherPath = Path.Combine(AppContext.BaseDirectory, "assembly", "OpenKh.Command.Interceptor.exe");
                var _fetchTargetGamePath = Path.Combine(_fetchGamePath, Config.GameExecutable[_fetchTargetGame]);

                var _fetchAPIExists = _fetchTargetPlatform == Platform.STEAM ? File.Exists(_fetchAPIFilePath) : false;

                var _fetchSteamId = currentConfig.Frontend.TargetGame == Game.DREAM_DROP_DISTANCE ? 2552440 : 2552430;
                var _fetchReMIXFilePath = currentConfig.Frontend.TargetGame == Game.DREAM_DROP_DISTANCE ? Path.Combine(_fetchGamePath, "KINGDOM HEARTS HD 2.8 Final Chapter Prologue.exe") : Path.Combine(_fetchGamePath, "KINGDOM HEARTS HD 1.5+2.5 ReMIX.exe");

                var _fetchArguments = currentConfig.Frontend.LaunchArguments;

                var _fetchDeletePath = Path.Combine(_fetchGamePath, "delete.this");
                var _fetchArgumentPath = Path.Combine(_fetchGamePath, "launch_args.txt");

                if (_fetchAPIExists)
                    _fetchArguments = $"{Config.GameShorthand[_fetchTargetGame]} {_fetchArguments}";

                Uri _fetchTargetUri = null;
                FileInfo _fetchTargetFile = null;

                if (File.Exists(_fetchDeletePath))
                    File.Delete(_fetchDeletePath);

                if (!OperatingSystem.IsWindows() || !_fetchAPIExists)
                {
                    switch (_fetchTargetPlatform)
                    {
                        case Platform.STEAM:
                            _fetchTargetUri = new Uri($"steam://rungameid/{_fetchSteamId}");
                            break;
                        case Platform.EPIC_GAMES_STORE:
                            _fetchTargetUri = new Uri("com.epicgames.launcher://apps/4158b699dd70447a981fee752d970a3e%3A5aac304f0e8948268ddfd404334dbdc7%3A68c214c58f694ae88c2dab6f209b43e4?action=launch");
                            break;
                    }
                }

                if (topLevelObject.Launcher != null && _fetchTargetUri != null)
                {
                    if (!String.IsNullOrEmpty(_fetchArguments))
                        await File.WriteAllTextAsync(_fetchArgumentPath, _fetchArguments, CancelToken);

                    if (_fetchAPIExists)
                    {
                        var _fetchReMIXBackup = Path.ChangeExtension(_fetchReMIXFilePath, ".bak");

                        if (!File.Exists(_fetchReMIXBackup))
                        {
                            File.Move(_fetchReMIXFilePath, _fetchReMIXBackup);
                            File.Copy(_fetchLauncherPath, _fetchReMIXFilePath, true);
                        }
                    }

                    await topLevelObject.Launcher.LaunchUriAsync(_fetchTargetUri);
                }

                else
                {
                    var _fetchProcessInfo = new ProcessStartInfo
                    {
                        FileName = _fetchTargetGamePath,
                        WorkingDirectory = _fetchGamePath,
                        UseShellExecute = true
                    };

                    Process.Start(_fetchProcessInfo);
                }

                return true;
            }

            else
            {
                var _fetchService = new InjectorService(currentConfig);

                return true;
            }
        }
    }
}

#pragma warning restore S4790
#pragma warning restore CS4014
