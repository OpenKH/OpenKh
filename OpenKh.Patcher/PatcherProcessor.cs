using OpenKh.Bbs;

using OpenKh.Common;
using OpenKh.Egs;
using OpenKh.Imaging;
using OpenKh.Kh1;
using OpenKh.Kh2;
using OpenKh.Kh2.Bdx.Models;
using OpenKh.Kh2.Bdx.Utils;
using OpenKh.Kh2.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xe.IO;
using YamlDotNet.Serialization;

using Loggers = Serilog.Log;

namespace OpenKh.Patcher
{
    public class PatcherProcessor
    {
        public class Context
        {
            public Metadata Metadata { get; set; }
            public string OriginalAssetPath { get; }
            public string SourceModAssetPath { get; set; }
            public string DestinationPath { get; set; }

            public static List<string> Regions = new List<string>()
            {
                "us",
                "jp",
                "us",
                "uk",
                "it",
                "sp",
                "gr",
                "fr",
                "fm",
            };

            public Context(
                Metadata metadata,
                string originalAssetPath,
                string sourceModAssetPath,
                string destinationPath)
            {
                Metadata = metadata;
                OriginalAssetPath = originalAssetPath;
                SourceModAssetPath = sourceModAssetPath;
                DestinationPath = destinationPath;
            }

            public string GetOriginalAssetPath(string path) => Path.Combine(OriginalAssetPath, path);
            public string GetSourceModAssetPath(string path) => Path.Combine(SourceModAssetPath, path);
            public string GetDestinationPath(string path) => Path.Combine(DestinationPath, path);
            public void EnsureDirectoryExists(string fileName) => Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            public void CopyOriginalFile(string fileName, string dstFile)
            {
                if (!File.Exists(dstFile))
                {
                    var originalFile = GetOriginalAssetPath(fileName);

                    if (File.Exists(originalFile))
                        File.Copy(originalFile, dstFile);
                }
            }
        }

        public void Patch(string originalAssets, string outputDir, string modFilePath)
        {
            var metadata = Metadata.Read(modFilePath);
            var modBasePath = Path.GetDirectoryName(modFilePath);
            Patch(originalAssets, outputDir, metadata, modBasePath);
        }

        public static Dictionary<int, string> GameShorthand = new Dictionary<int, string>()
        {
            { 0, "kh1" },
            { 1, "kh2" },
            { 2, "Recom" },
            { 3, "bbs" },
            { 4, "kh3d" }
        };

        public async Task Patch(
            string extractDataPath,
            string buildOutputPath,
            Metadata modMetadata,
            string modFilesPath,
            string gameFilesPath = null,
            int targetPlatform = 0x00,            
            int targetGame = 0x01,
            bool isJapanese = false,
            CancellationToken? cancelToken = null,
            IDictionary<string, string> packageMap = null,
            Dictionary<string, bool> collectionOptionalEnabledMods = null,
            Func<int, int, bool> reportProgress = null,
            bool Tests = false)
        {
            var _isExtraction = extractDataPath != null;
            var _fetchGameId = GameShorthand[targetGame];

            var _protectPackageMap = new object();

            var _fetchCollectionMods = new Dictionary<string, bool> { };
            var _fetchFileDictionary = new ConcurrentDictionary<string, string>();

            if (collectionOptionalEnabledMods != null)
                _fetchCollectionMods = collectionOptionalEnabledMods;

            try
            {
                // A lot of this class relies on this existing. I am in no mood to rewrite that much code yet.
                var _fetchContext = new Context(modMetadata, extractDataPath, modFilesPath, buildOutputPath);
                // If the mod has a game declared and it doesn't match the current game, don't build it.
                if (modMetadata.Game != null && modMetadata.Game.ToLower() != _fetchGameId)
                {
                    Loggers.ForContext<PatcherProcessor>().Error($"[{modMetadata.Title}] - This mod does not support the target game (How did this get installed?) - Aborting build...");
                    return;
                }

                // If the mod is part of a collection and the collection doesn't support the current game, don't build it.
                if (modMetadata.IsCollection && !modMetadata.CollectionGames.Contains(_fetchGameId))
                {
                    Loggers.ForContext<PatcherProcessor>().Error($"[{modMetadata.Title}] - The collection this mod is in does not support the target game. Aborting build...");
                    return;
                }

                // If the mod does not have any assets, throw an error. This should not happen.
                if (modMetadata.Assets == null)
                    throw new NullReferenceException("The mod doesn't contain any assets.");

                var _assetProcessCount = 0;
                var _assetTotalCount = modMetadata.Assets.Count;

                // If the build is not extraction-based:
                if (!_isExtraction && targetPlatform != 0x00)
                {
                    // Fetch a list of all header files for the targeted game.
                    var _fetchDataPath = Path.Combine(gameFilesPath, "Image", targetPlatform == 0x01 ? "dt" : (isJapanese ? "jp" : "en"));
                    var _fetchHeaderFiles = Directory.GetFiles(_fetchDataPath).Where(x => x.Contains(_fetchGameId) && x.EndsWith(".hed"));

                    // Start a parallel for every header file.
                    await Parallel.ForEachAsync(_fetchHeaderFiles.AsParallel(), new ParallelOptions() { CancellationToken = cancelToken ?? new CancellationToken() }, async (_fetchHeader, _fetchCancelToken) =>
                    {
                        // Load up the header file and read it as a HED class.
                        using (var _fetchHeaderStream = File.OpenRead(_fetchHeader))
                        {
                            var _fetchPackageName = Path.GetFileNameWithoutExtension(_fetchHeader);
                            var _fetchHeaderChildren = Hed.Read(_fetchHeaderStream);

                            // Record every hash with its corresponding package name for speedier handling.
                            // This eliminates the need to inquire every package for every single file.
                            foreach (var _fetchEntry in _fetchHeaderChildren)
                            {
                                _fetchFileDictionary.TryAdd(Convert.ToHexString(_fetchEntry.MD5), _fetchPackageName);

                                if (_fetchCancelToken.IsCancellationRequested)
                                    break;
                            }
                        }
                    });
                }

                // Start a parallel for all the assets:
                await Parallel.ForEachAsync(modMetadata.Assets.AsParallel(), new ParallelOptions() { CancellationToken = cancelToken ?? new CancellationToken() }, async (_fetchAsset, _fetchCancelToken) =>
                {
                    // Process callback for progress.
                    _assetProcessCount++;

                    // If the asset has a game declared and it doesn't match the current game, skip it.
                    if (_fetchAsset.Game != null && _fetchAsset.Game != _fetchGameId)
                    {
                        Loggers.ForContext<PatcherProcessor>().Warning($"[{modMetadata.Title}] - The file \"{_fetchAsset.Name}\" does not support the current game (Literally how?). Skipping...");
                        return;
                    }

                    // I don't know what this does but it is here for compat.
                    if (_fetchAsset.CollectionOptional == true && (!_fetchCollectionMods.ContainsKey(_fetchAsset.Name) || !_fetchCollectionMods[_fetchAsset.Name]))
                        return;

                    if (_fetchAsset.Condition != null)
                    {
                        var _fetchConditionKey = _fetchAsset.Condition.Key;
                        var _fetchConditionValue = _fetchAsset.Condition.Value;

                        var _fetchConditionMain = modMetadata.Preferences.FirstOrDefault(x => x.Key == _fetchConditionKey);

                        if (_fetchConditionMain != null && !Object.Equals(_fetchConditionValue, _fetchConditionMain.Value))
                        {
                            Loggers.ForContext<PatcherProcessor>().Warning($"[{modMetadata.Title}] - The file \"{_fetchAsset.Name}\" does not satisfy the required condition. Skipping...");
                            return;
                        }
                    }

                    // Fetch all the target names to be used.
                    var _fetchFileNames = new List<string>();
                    var _fetchMultiNames = _fetchAsset.Multi != null ? _fetchAsset.Multi.Select(x => x.Name)
                                                                                        .Where(x => !string.IsNullOrEmpty(x)) : null;

                    _fetchFileNames.Add(_fetchAsset.Name);

                    if (_fetchMultiNames != null)
                        _fetchFileNames.AddRange(_fetchMultiNames);

                    // If the platform for the asset isn't declared, default it to be both.
                    _fetchAsset.Platform = _fetchAsset.Platform != null ? _fetchAsset.Platform.ToLower() : "both";

                    foreach (var _fetchName in _fetchFileNames)
                    {
                        // Check discrepancy for JP-specific files.
                        var _doesRegionMatch = (!isJapanese && !_fetchName.Contains(".a.jp") && !_fetchName.Contains("/jp/")) || (isJapanese && (_fetchName.Contains(".a.jp") || _fetchName.Contains("/jp/")));
                        var _isAssetExclusivePS2 = _fetchName.Contains(".a.fm") || _fetchName.Contains("/fm/");

                        // Declare whether the asset is patchable or not.
                        var _isAssetPatchable = _fetchAsset.Method != "copy" && _fetchAsset.Method != "imd";

                        // This is used for package map purposes.
                        var _fetchPackageMap = "";

                        // Construct the many paths that will be needed for the asset.

                        /*
                         * -> _fetchOriginalPath = Path to the target file from the game data. Constructed and used only if extraction exists.
                         * -> _fetchVariablePath = Path to the target file as it exists in the build folder. Only used if the file is patchable. [Type isn't COPY or IMD]
                         * -> _fetchExternalPath = Path to the source file as it exists in the mod's source folder. Used if the file is neither patchable nor internal.
                         * -> _fetchInternalPath = Path to the source file from the game data. Used if the file is not patchable but is internal. Constructed and used only if extraction exists.
                         */

                        var _fetchOriginalPath = _isExtraction ? Path.Combine(extractDataPath, _fetchName) : null;
                        var _fetchVariablePath = _isAssetPatchable && _fetchAsset.Source[0].Type != "internal" ? Path.Combine(buildOutputPath, _fetchName) : null;
                        var _fetchExternalPath = !_isAssetPatchable && (_fetchAsset.Source[0].Type != "internal" && _fetchAsset.Source[0].Name != null) ? Path.Combine(modFilesPath, _fetchAsset.Source[0].Name) : null;
                        var _fetchInternalPath = !_isAssetPatchable && (_fetchAsset.Source[0].Type == "internal" && _fetchAsset.Source[0].Name != null && _isExtraction) ? Path.Combine(extractDataPath, _fetchAsset.Source[0].Name) : null;

                        // Fetch the parent directory of the file.
                        var _fetchFileParent = _fetchName.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]).FirstOrDefault();

                        // Construct the target path and directory for the file.

                        var _fetchTargetPath = Path.Combine(buildOutputPath, _fetchName);
                        var _fetchTargetDir = Path.GetDirectoryName(_fetchTargetPath);

                        // If declared, fetch the package name for the asset.
                        // This is used in hard-patching and for package mapping.
                        var _fetchPackage = _fetchAsset.Package != null ? _fetchAsset.Package : $"{_fetchGameId}_first";

                        // Fetch certain information about the file.

                        var _isFileRAW = _fetchFileParent == "raw";
                        var _isFileRemastered = _fetchFileParent == "remastered";
                        var _isFileSpecial = _fetchFileParent == "scripts" || _fetchFileParent == "dll";

                        var _isFilePC = _isFileRemastered || _isFileRAW;

                        // If the file is special, commit it to the package map as such.
                        if (_isFileSpecial)
                            _fetchPackageMap = "special/" + _fetchName;

                        else
                        {
                            // If the target platform is the PS2 and the file is PC-specific, skip it.
                            if (targetPlatform == 0x00 && _isFilePC && (_fetchFileParent == "dll" || _fetchAsset.Platform == "pc"))
                            {
                                Loggers.ForContext<PatcherProcessor>().Warning($"[{modMetadata.Title}] - The file \"{_fetchAsset.Name}\" is expecting PC and can't be built for PS2. Skipping...");
                                continue;
                            }

                            // If the target platform is not PS2:
                            else if (targetPlatform != 0x00)
                            {
                                // But the asset is, skip it.
                                if (_fetchAsset.Platform == "ps2" || _isAssetExclusivePS2)
                                {
                                    Loggers.ForContext<PatcherProcessor>().Warning($"[{modMetadata.Title}] - The file \"{_fetchAsset.Name}\" is expecting PS2 and can't be built for PC. Skipping...");
                                    continue;
                                }

                                if (!_doesRegionMatch && (_isAssetPatchable || _fetchAsset.Source[0].Type == "internal"))
                                {
                                    Loggers.ForContext<PatcherProcessor>().Warning($"[{modMetadata.Title}] - The file \"{_fetchAsset.Name}\" does not have a matching region for this game. Skipping...");
                                    continue;
                                }

                                // Otherwise, commit it to the package map.
                                _fetchPackageMap = Path.Combine(_fetchPackage, _isFilePC ? "" : "original/", _fetchName);
                            }
                        }

                        // Lock the package map and commit the constructed string.
                        if (packageMap != null && _fetchPackageMap.Length > 0)
                        {
                            lock (_protectPackageMap)
                                packageMap[_fetchName.Replace("\\", "/")] = _fetchPackageMap;
                        }

                        // If the target directory does not exist, create it.
                        if (!Directory.Exists(_fetchTargetDir))
                            Directory.CreateDirectory(_fetchTargetDir);

                        // If a built version of the file already exists, patch that one.
                        if (_fetchVariablePath != null && File.Exists(_fetchVariablePath))
                        {
                            using (var _fileStream = File.Open(_fetchVariablePath, FileMode.OpenOrCreate))
                                PatchFile(_fetchContext, _fetchAsset, _fileStream);
                        }

                        // Otherwise;
                        else
                        {
                            byte[] _fetchAssetData = null;

                            // If the asset is not patchable, meaning it has to be copied from somewhere:
                            if (!_isAssetPatchable)
                            {
                                // If the file type is internal, the internal path is valid, and the file exists: Copy it to target.
                                if (_fetchAsset.Source[0].Type == "internal" && _fetchInternalPath != null && File.Exists(_fetchInternalPath))
                                    File.Copy(_fetchInternalPath, _fetchTargetPath, true);

                                // If the file type is NOT internal, the external path is valid, and the file exists: Copy it to target.
                                else if (_fetchExternalPath != null && File.Exists(_fetchExternalPath))
                                    File.Copy(_fetchExternalPath, _fetchTargetPath, true);
                            }

                            // If the asset is patchable, the original path is valid, and the file exists: Copy it to target.
                            else if (_isAssetPatchable && _fetchOriginalPath != null && File.Exists(_fetchOriginalPath))
                                File.Copy(_fetchOriginalPath, _fetchTargetPath, true);

                            // If after the above the target file STILL does not exists, try to fetch it from the game data.
                            if (!File.Exists(_fetchTargetPath))
                            {
                                Loggers.ForContext<PatcherProcessor>().Verbose($"[{modMetadata.Title}] - Processing the file \"{_fetchAsset.Name}\" from game data...");

                                if (targetPlatform != 0x00)
                                {
                                    var _fetchTarget = _isAssetPatchable ? _fetchName : _fetchAsset.Source[0].Name;

                                    var _fetchParent = "";
                                    var _fetchChild = "";

                                    if (_isFileRAW)
                                        _fetchTarget = _fetchTarget.Replace("raw/", "");

                                    if (_isFileRemastered)
                                    {
                                        var _fetchMatch = Regex.Match(_fetchTarget, "[\\w,-]+\\.[a-zA-Z0-9]{2,4}", RegexOptions.None, TimeSpan.FromMilliseconds(500));

                                        var _fetchIndex = _fetchTarget.IndexOf(_fetchMatch.Value);
                                        var _fetchEndPoint = _fetchIndex + _fetchMatch.Value.Length;

                                        var _fetchSubFirst = _fetchTarget.Substring(0, _fetchEndPoint);
                                        var _fetchSubSecond = _fetchTarget.Substring(_fetchEndPoint, _fetchTarget.Length - _fetchEndPoint);

                                        _fetchParent = _fetchSubFirst.Replace("remastered/", "");
                                        _fetchChild = _fetchSubSecond.Trim('/');
                                    }

                                    var _fetchNameHash = Egs.Helpers.CreateMD5(_isFileRemastered ? _fetchParent : _fetchTarget);
                                    var _fetchKeyExists = _fetchFileDictionary.ContainsKey(_fetchNameHash);

                                    if (_fetchKeyExists)
                                    {
                                        var _fetchFilePack = _fetchFileDictionary[_fetchNameHash];

                                        if (!String.IsNullOrEmpty(_fetchFilePack))
                                        {
                                            var _fetchDataPath = Path.Combine(gameFilesPath, "Image", targetPlatform == 0x01 ? "dt" : (isJapanese ? "jp" : "en"));

                                            var _fetchHeaderName = Path.Combine(_fetchDataPath, _fetchFilePack + ".hed");
                                            var _fetchPackageName = Path.Combine(_fetchDataPath, _fetchFilePack + ".pkg");

                                            using (var _fetchHeaderStream = File.OpenRead(_fetchHeaderName))
                                            {
                                                var _fetchEntries = Hed.Read(_fetchHeaderStream);
                                                var _fetchTargetEntry = _fetchEntries.FirstOrDefault(x => Convert.ToHexString(x.MD5) == _fetchNameHash);

                                                Loggers.ForContext<PatcherProcessor>().Verbose($"[{modMetadata.Title}] - Found the file \"{_fetchAsset.Name}\" at \"{_fetchFilePack}.pkg\"! Processing...");

                                                using (var _fetchPackageStream = File.OpenRead(_fetchPackageName))
                                                {
                                                    var _fetchTargetAsset = new EgsHdAsset(_fetchPackageStream.SetPosition(_fetchTargetEntry.Offset));

                                                    if (_isFileRemastered)
                                                        _fetchTargetAsset.RemasteredAssetsDecompressedData.TryGetValue(_fetchChild, out _fetchAssetData);

                                                    else
                                                        _fetchAssetData = _isFileRAW ? _fetchTargetAsset.OriginalRawData : _fetchTargetAsset.OriginalData;

                                                    if (_fetchAssetData != null)
                                                        await File.WriteAllBytesAsync(_fetchTargetPath, _fetchAssetData, CancellationToken.None);

                                                    Loggers.ForContext<PatcherProcessor>().Verbose($"[{modMetadata.Title}] - Processed file \"{_fetchAsset.Name}\" successfully!");
                                                }
                                            }
                                        }
                                    }
                                }

                                else
                                {
                                    var _fetchTarget = _isAssetPatchable ? _fetchName : _fetchAsset.Source[0].Name;

                                    using (var _fetchStreamISO = new FileStream(gameFilesPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        switch (targetGame)
                                        {
                                            case 0x00:
                                            {
                                                var _fetchOffsetIDX = IsoUtility.GetFileOffset(_fetchStreamISO, "KINGDOM.IDX;1");
                                                var _fetchOffsetIMG = IsoUtility.GetFileOffset(_fetchStreamISO, "SYSTEM.CNF;1");

                                                var _fetchSubStrIDX = new SubStream(_fetchStreamISO, _fetchOffsetIDX * 0x800, _fetchStreamISO.Length - (_fetchOffsetIDX * 0x800));
                                                var _fetchSubStrIMG = new SubStream(_fetchStreamISO, _fetchOffsetIMG * 0x800, _fetchStreamISO.Length - (_fetchOffsetIMG * 0x800));

                                                var _fetchIDX = Idx1.Read(_fetchSubStrIDX);
                                                var _fetchIMG = new Img1(_fetchSubStrIMG, _fetchIDX, 0x00);

                                                var _fetchEntry = _fetchIMG.Entries.FirstOrDefault(x => Idx1Name.Lookup(x.Value) == _fetchTarget).Value;

                                                if (_fetchEntry != null)
                                                {
                                                    using (var _fetchStreamTarget = _fetchIMG.FileOpen(_fetchEntry))
                                                        File.Create(_fetchTargetPath).Using(_tempStr => _fetchStreamTarget.CopyTo(_tempStr));

                                                    Loggers.ForContext<PatcherProcessor>().Verbose($"[{modMetadata.Title}] - Processed file \"{_fetchAsset.Name}\" successfully from KINGDOM.IDX!");
                                                }

                                                else
                                                    Loggers.ForContext<PatcherProcessor>().Verbose($"[{modMetadata.Title}] - The file \"{_fetchAsset.Name}\" does not exist in the ISO! Was this file meant to be COPY? Skipping...");
                                            }
                                            break;
                                            case 0x01:
                                            {
                                                var _fetchOffsetIDX = IsoUtility.GetFileOffset(_fetchStreamISO, "KH2.IDX;1");
                                                var _fetchOffsetIMG = IsoUtility.GetFileOffset(_fetchStreamISO, "KH2.IMG;1");

                                                var _fetchSubStrIDX = new SubStream(_fetchStreamISO, _fetchOffsetIDX * 0x800, _fetchStreamISO.Length - (_fetchOffsetIDX * 0x800));
                                                var _fetchSubStrIMG = new SubStream(_fetchStreamISO, _fetchOffsetIMG * 0x800, _fetchStreamISO.Length - (_fetchOffsetIMG * 0x800));

                                                var _fetchIDX = Idx.Read(_fetchSubStrIDX);
                                                var _fetchIMG = new Img(_fetchSubStrIMG, _fetchIDX, true);

                                                var _fetchEntry = _fetchIMG.Entries.FirstOrDefault(x => IdxName.Lookup(x) == _fetchTarget);

                                                if (_fetchEntry != null)
                                                {
                                                    using (var _fetchStreamTarget = _fetchIMG.FileOpen(_fetchEntry))
                                                        File.Create(_fetchTargetPath).Using(_tempStr => _fetchStreamTarget.CopyTo(_tempStr));

                                                    Loggers.ForContext<PatcherProcessor>().Verbose($"[{modMetadata.Title}] - Processed file \"{_fetchAsset.Name}\" successfully from KH2.IDX!");
                                                }

                                                else
                                                    Loggers.ForContext<PatcherProcessor>().Verbose($"[{modMetadata.Title}] - The file \"{_fetchAsset.Name}\" does not exist in the ISO! Was this file meant to be COPY? Skipping...");

                                            }
                                            break;
                                        }
                                    }
                                }
                            }

                            // After all that, if the target file actually exists now: Patch it.
                            if (File.Exists(_fetchTargetPath))
                            {
                                using (var _fileStream = File.Open(_fetchTargetPath, FileMode.OpenOrCreate))
                                {
                                    Loggers.ForContext<PatcherProcessor>().Verbose($"[{modMetadata.Title}] - Patching file \"{_fetchAsset.Name}\"...");
                                    PatchFile(_fetchContext, _fetchAsset, _fileStream, _fetchAssetData);
                                }
                            }
                        }
                    }

                    if (reportProgress != null)
                        reportProgress(_assetProcessCount, _assetTotalCount);
                });
            }

            catch (TaskCanceledException) { }

            catch (Exception ex)
            {
                throw new PatcherException(modMetadata, ex);
            }
        }

        private static void PatchFile(Context context, AssetFile assetFile, Stream stream, byte[] originalData = null)
        {
            if (assetFile == null)
                throw new Exception("Asset file is null.");

            switch (assetFile.Method)
            {
                case "copy":
                    CopyFile(context, assetFile, stream, originalData);
                    break;
                case "bar":
                case "binarc":
                    PatchBinarc(context, assetFile, stream);
                    break;
                case "bbsarc":
                    PatchBBSArc(context, assetFile, stream);
                    break;
                case "imd":
                case "imgd":
                    CreateImageImd(context, assetFile.Source[0]).Write(stream);
                    break;
                case "imz":
                case "imgz":
                    PatchImageImz(context, assetFile, stream);
                    break;
                case "fac":
                    Imgd.WriteAsFac(stream, assetFile.Source.Select(x => CreateImageImd(context, x)));
                    break;
                case "kh2msg":
                    PatchKh2Msg(context, assetFile.Source, stream);
                    break;
                case "areadatascript":
                    PatchAreaDataScript(context, assetFile.Source, stream);
                    break;
                case "kh1ardresource":
                    PatchKh1ArdResource(context, assetFile, stream);
                    break;
                case "bdscript":
                    PatchBdscript(context, assetFile, stream);
                    break;
                case "spawnpoint":
                    PatchSpawnPoint(context, assetFile, stream);
                    break;
                case "listpatch":
                    PatchList(context, assetFile.Source, stream);
                    break;
                case "synthpatch":
                    PatchSynth(context, assetFile.Source, stream);
                    break;
                default:
                    Log.Warn($"Method '{assetFile.Method}' not recognized for '{assetFile.Name}'. Falling back to 'copy'");
                    CopyFile(context, assetFile, stream);
                    break;
            }

            stream.SetLength(stream.Position);
        }

        private static void CopyFile(Context context, AssetFile assetFile, Stream stream, byte[] originalData = null)
        {
            if (assetFile.Source == null || assetFile.Source.Count == 0)
                throw new Exception($"File '{assetFile.Name}' does not contain any source");

            if (assetFile.Source[0].Type == "internal")
            {
                if (context.OriginalAssetPath != null)
                {
                    var _fetchSource = Path.Combine(context.OriginalAssetPath, assetFile.Source[0].Name);

                    using (var _fetchStream = File.OpenRead(_fetchSource))
                        _fetchStream.CopyTo(stream);
                }

                else if (originalData != null)
                {
                    using (var _memoryRead = new MemoryStream(originalData))
                        _memoryRead.CopyTo(stream);
                }
            }

            else
            {
                var _fetchSource = Path.Combine(context.SourceModAssetPath, assetFile.Source[0].Name);

                if (!File.Exists(_fetchSource))
                    Console.WriteLine($"The mod does not contain the file {assetFile.Source[0].Name} in {context.SourceModAssetPath}");

                else
                {
                    using (var _fetchStream = File.OpenRead(_fetchSource))
                        _fetchStream.CopyTo(stream);
                }
            }
        }

        //Binarc Update: Specify by Name OR Index. Some files in BARS may have the same name but different indexes, and you want to patch a later index only.
        private static void PatchBinarc(Context context, AssetFile assetFile, Stream stream)
        {
            var binarc = Bar.IsValid(stream) ? Bar.Read(stream) :
                new Bar()
                {
                    Motionset = assetFile.MotionsetType
                };

            foreach (var file in assetFile.Source)
            {
                if (file.Condition != null)
                {
                    var _fetchConditionKey = file.Condition.Key;
                    var _fetchConditionValue = file.Condition.Value;

                    var _fetchConditionMain = context.Metadata.Preferences.FirstOrDefault(x => x.Key == _fetchConditionKey);

                    if (_fetchConditionMain != null && !Object.Equals(_fetchConditionValue, _fetchConditionMain.Value))
                        return;
                }

                if (!Enum.TryParse<Bar.EntryType>(file.Type, true, out var barEntryType))
                    throw new Exception($"BinArc type {file.Type} not recognized");

                Bar.Entry entry = null;

                // Check if the name is specified
                if (!string.IsNullOrEmpty(file.Name))
                {
                    entry = binarc.FirstOrDefault(x => x.Name == file.Name && x.Type == barEntryType);
                }
                // If name is not specified but index is
                else if (file.Index >= 0 && file.Index < binarc.Count)
                {
                    entry = binarc[file.Index];
                }

                //If entry is not found by name or index, create a new one
                if (entry == null)
                {
                    entry = new Bar.Entry
                    {
                        Name = file.Name,
                        Type = barEntryType,
                        Stream = new MemoryStream()
                    };
                    binarc.Add(entry);
                }

                PatchFile(context, file, entry.Stream);
            }

            Bar.Write(stream.SetPosition(0), binarc);
            foreach (var entry in binarc)
                entry.Stream?.Dispose();
        }

        private static void PatchBBSArc(Context context, AssetFile assetFile, Stream stream)
        {
            var entryList = Arc.IsValid(stream) ? Arc.Read(stream).ToList() : new List<Arc.Entry>();
            foreach (var file in assetFile.Source)
            {
                var entry = entryList.FirstOrDefault(e => e.Name == file.Name);

                if (entry == null)
                {
                    entry = new Arc.Entry()
                    {
                        Name = file.Name
                    };
                    entryList.Add(entry);
                }
                else if (entry.IsLink)
                {
                    throw new Exception("Cannot patch an arc link!");
                }

                MemoryStream data = new MemoryStream();
                if (entry.Data != null)
                {
                    data.Write(entry.Data);
                    data.SetPosition(0);
                }
                PatchFile(context, file, data);
                entry.Data = data.ToArray();
            }

            OpenKh.Bbs.Arc.Write(entryList, stream.SetPosition(0));
        }

        private static Imgd CreateImageImd(Context context, AssetFile source)
        {
            var srcFile = context.GetSourceModAssetPath(source.Name);
            using var srcStream = File.OpenRead(srcFile);
            if (PngImage.IsValid(srcStream))
            {
                try
                {
                    var png = PngImage.Read(srcStream);
                    return Imgd.Create(png.Size, png.PixelFormat, png.GetData(), png.GetClut(), source.IsSwizzled);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to decode the PNG '{source.Name}': {ex.Message}");
                }
            }
            else if (Imgd.IsValid(srcStream))
                return Imgd.Read(srcStream);

            throw new Exception($"Image source '{source.Name}' not recognized");
        }

        private static void PatchImageImz(Context context, AssetFile assetFile, Stream stream)
        {
            var index = 0;
            var images = Imgz.IsValid(stream) ? Imgz.Read(stream).ToList() : new List<Imgd>();
            foreach (var source in assetFile.Source)
            {
                if (source.Index > 0)
                    index = source.Index;

                var imd = CreateImageImd(context, source);
                if (images.Count <= index)
                    images.Add(imd);
                else
                    images[index] = imd;

                index++;
            }

            Imgz.Write(stream.SetPosition(0), images);
        }

        private static void PatchKh2Msg(Context context, List<AssetFile> sources, Stream stream)
        {
            var msgs = Msg.IsValid(stream) ? Msg.Read(stream) : new List<Msg.Entry>();

            foreach (var source in sources)
            {
                if (string.IsNullOrEmpty(source.Language))
                    throw new Exception($"No language specified in '{source.Name}'");

                var content = File.ReadAllText(context.GetSourceModAssetPath(source.Name));
                var patchedMsgs = new Deserializer().Deserialize<List<Dictionary<string, string>>>(content);
                foreach (var msg in patchedMsgs)
                {
                    if (!msg.TryGetValue("id", out var strId))
                        throw new Exception($"Source '{source.Name}' contains a message without an ID");
                    if (!ushort.TryParse(strId, out var id))
                    {
                        if (strId.Length > 2 && strId[1] == 'x')
                        {
                            if (!ushort.TryParse(strId.Substring(2), NumberStyles.HexNumber, null, out id))
                                throw new Exception($"Message ID '{strId} in '{source.Name}' must be between 0 and 65535");
                        }
                        else
                            throw new Exception($"Message ID '{strId} in '{source.Name}' must be between 0 and 65535");
                    }
                    if (!msg.TryGetValue(source.Language, out var text))
                        continue;

                    var encoder = source.Language switch
                    {
                        "jp" => Encoders.JapaneseSystem,
                        "je" => Encoders.JapaneseEvent,
                        "tr" => Encoders.TurkishSystem,
                        _ => Encoders.InternationalSystem,
                    };

                    var data = encoder.Encode(MsgSerializer.DeserializeText(text ?? string.Empty).ToList());
                    var originalMsg = msgs.FirstOrDefault(x => x.Id == id);
                    if (originalMsg == null)
                        msgs.Add(new Msg.Entry
                        {
                            Id = id,
                            Data = data
                        });
                    else
                        originalMsg.Data = data;
                }
            }

            Msg.WriteOptimized(stream.SetPosition(0), msgs);
        }

        private static void PatchAreaDataScript(Context context, List<AssetFile> sources, Stream stream)
        {
            var scripts = Kh2.Ard.AreaDataScript.IsValid(stream) ?
                Kh2.Ard.AreaDataScript.Read(stream).ToDictionary(x => x.ProgramId, x => x) :
                new Dictionary<short, Kh2.Ard.AreaDataScript>();
            foreach (var source in sources)
            {
                var programsInput = File.ReadAllText(context.GetSourceModAssetPath(source.Name));
                foreach (var newScript in Kh2.Ard.AreaDataScript.Compile(programsInput))
                    scripts[newScript.ProgramId] = newScript;
            }

            Kh2.Ard.AreaDataScript.Write(stream.SetPosition(0), scripts.Values);
        }

        private static void PatchKh1ArdResource(Context context, AssetFile assetFile, Stream stream)
        {
            if (assetFile.Source == null || assetFile.Source.Count == 0)
                throw new Exception($"File '{assetFile.Name}' does not contain any source");

            if (!Kh1.Ard.IsValid(stream))
                throw new InvalidDataException($"'{assetFile.Name}' is not a valid KH1 .ard archive");

            var resources = Kh1.Ard.ReadResourceList(stream);
            foreach (var source in assetFile.Source)
            {
                var srcFile = context.GetSourceModAssetPath(source.Name);
                if (!File.Exists(srcFile))
                    throw new FileNotFoundException($"The mod does not contain the file {source.Name}", srcFile);

                var replacements = deserializer.Deserialize<Dictionary<int, string>>(File.ReadAllText(srcFile))
                    ?? new Dictionary<int, string>();

                foreach (var replacement in replacements)
                {
                    if (replacement.Key < 0 || replacement.Key >= resources.Count)
                        throw new IndexOutOfRangeException(
                            $"'{source.Name}' sets resource index {replacement.Key}, but '{assetFile.Name}' only has {resources.Count} entries (0 to {resources.Count - 1})");

                    if (string.IsNullOrEmpty(replacement.Value))
                        throw new Exception($"'{source.Name}' does not give a name for resource index {replacement.Key}");

                    resources[replacement.Key] = replacement.Value;
                }
            }

            Kh1.Ard.WriteResourceList(stream, resources);

            // The resource list is edited in place; keep the rest of the archive intact,
            // as PatchFile truncates the stream to whatever position it is left at.
            stream.Position = stream.Length;
        }

        private static void PatchBdscript(Context context, AssetFile assetFile, Stream stream)
        {

            if (assetFile.Source == null || assetFile.Source.Count == 0)
                throw new Exception($"File '{assetFile.Name}' does not contain any source");

            var scriptName = assetFile.Source[0].Name;
            var srcFile = context.GetSourceModAssetPath(scriptName);
            if (!File.Exists(srcFile))
                throw new FileNotFoundException($"The mod does not contain the file {scriptName}", srcFile);


            var programsInput = File.ReadAllText(context.GetSourceModAssetPath(scriptName));
            var ascii = BdxAsciiModel.ParseText(programsInput);
            var decoder = new BdxEncoder(
                header: new YamlDotNet.Serialization.DeserializerBuilder()
                    .Build()
                    .Deserialize<BdxHeader>(
                        ascii.Header ?? ""
                    ),
                script: ascii.GetLineNumberRetainedScriptBody(),
                scriptName: scriptName,
                loadScript: programsInput => programsInput
                );

            stream.SetPosition(0);
            stream.Write(decoder.Content);

        }

        private static void PatchSpawnPoint(Context context, AssetFile assetFile, Stream stream)
        {
            if (assetFile.Source == null || assetFile.Source.Count == 0)
                throw new Exception($"File '{assetFile.Name}' does not contain any source");

            var srcFile = context.GetSourceModAssetPath(assetFile.Source[0].Name);
            if (!File.Exists(srcFile))
                throw new FileNotFoundException($"The mod does not contain the file {assetFile.Source[0].Name}", srcFile);

            var spawnPoint = OpenKh.Common.Helpers.YamlDeserialize<List<Kh2.Ard.SpawnPoint>>(File.ReadAllText(srcFile));

            Kh2.Ard.SpawnPoint.Write(stream.SetPosition(0), spawnPoint);
        }

        private static readonly Dictionary<string, byte> characterMap = new Dictionary<string, byte>(){
            { "Sora", 1 }, { "Donald", 2 }, { "Goofy", 3 },  { "Mickey", 4 },  { "Auron", 5 }, { "PingMulan",6 }, { "Aladdin", 7 },  { "Sparrow", 8 }, { "Beast", 9 },  { "Jack", 10 },  { "Simba", 11 }, { "Tron", 12 }, { "Riku", 13 }, { "Roxas", 14}, {"Ping", 15}
        };

        private static readonly Dictionary<string, byte> worldIndexMap = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase){
    { "worldzz", 0 }, { "endofsea", 1 }, { "twilighttown", 2 },  { "destinyisland", 3 },  { "hollowbastion", 4 }, { "beastscastle", 5 }, { "olympuscoliseum", 6 },  { "agrabah", 7 }, { "thelandofdragons", 8 },  { "100acrewood", 9 },  { "prideland", 10 }, { "atlantica", 11 }, { "disneycastle", 12 }, { "timelessriver", 13}, {"halloweentown", 14}, { "worldmap", 15 }, { "portroyal", 16 }, { "spaceparanoids", 17 }, { "theworldthatneverwas", 18 }
    };

        private static readonly Dictionary<string, int> wentNameMap =
    new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {        ["Sora"] = 1, ["SoraNM"] = 11, ["Donald"] = 17, ["DonaldNM"] = 19, ["Goofy"] = 20, ["GoofyLK"] = 21, ["GoofyNM"] = 22, ["Aladdin"] = 23, ["Auron"] = 24, ["Mulan"] = 25, ["Ping"] = 26, ["Tron"] = 27, ["Mickey"] = 28, ["Beast"] = 31, ["Jack"] = 32, ["Simba"] = 33, ["Sparrow"] = 34, ["Riku"] = 35, ["SparrowHuman"] = 36, ["SoraTR"] = 37, ["SoraWI"] = 43, ["DonaldTR"] = 49, ["DonaldWI"] = 50, ["GoofyTR"] = 51, ["GoofyWI"] = 52 };

        private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

        private static void PatchList(Context context, List<AssetFile> sources, Stream stream)
        {
            foreach (var source in sources)
            {
                string sourceText = File.ReadAllText(context.GetSourceModAssetPath(source.Name));
                switch (source.Type)
                {
                    case "trsr":
                        var trsrList = Kh2.SystemData.Trsr.Read(stream).ToDictionary(x => x.Id, x => x);
                        var moddedTrsr = deserializer.Deserialize<Dictionary<ushort, Kh2.SystemData.Trsr>>(sourceText);
                        foreach (var treasure in moddedTrsr)
                        {
                            if (trsrList.ContainsKey(treasure.Key))
                            {
                                if (treasure.Value.World == 0)
                                {
                                    trsrList[treasure.Key].ItemId = treasure.Value.ItemId;
                                }
                                else
                                {
                                    trsrList[treasure.Key] = treasure.Value;
                                }
                            }
                            else
                            {
                                trsrList.Add(treasure.Key, treasure.Value);
                            }

                        }
                        Kh2.SystemData.Trsr.Write(stream.SetPosition(0), trsrList.Values);
                        break;

                    case "item":
                        var itemList = Kh2.SystemData.Item.Read(stream);
                        var moddedItem = deserializer.Deserialize<Kh2.SystemData.Item>(sourceText);

                        if (moddedItem.Items != null)
                        {
                            foreach (var item in moddedItem.Items)
                            {
                                var itemToUpdate = itemList.Items.FirstOrDefault(x => x.Id == item.Id);
                                if (itemToUpdate != null)
                                {
                                    itemList.Items[itemList.Items.IndexOf(itemToUpdate)] = item;
                                }
                                else
                                {
                                    if (item.InsertBefore != 0) // Prioritize InsertBefore
                                    {
                                        var index = itemList.Items.FindIndex(x => x.Id == item.InsertBefore);
                                        if (index >= 0)
                                        {
                                            itemList.Items.Insert(index, item);
                                            continue;
                                        }
                                    }
                                    else if (item.InsertAfter != 0) // If InsertBefore not set, check InsertAfter
                                    {
                                        var index = itemList.Items.FindIndex(x => x.Id == item.InsertAfter);
                                        if (index >= 0)
                                        {
                                            itemList.Items.Insert(index + 1, item);
                                            continue;
                                        }
                                    }

                                    // Default case: append
                                    itemList.Items.Add(item);
                                }
                            }
                        }

                        if (moddedItem.Stats != null)
                        {
                            foreach (var item in moddedItem.Stats)
                            {
                                var itemToUpdate = itemList.Stats.FirstOrDefault(x => x.Id == item.Id);
                                if (itemToUpdate != null)
                                {
                                    itemList.Stats[itemList.Stats.IndexOf(itemToUpdate)] = item;
                                }
                                else
                                {
                                    itemList.Stats.Add(item);
                                }
                            }
                        }

                        itemList.Write(stream.SetPosition(0));
                        break;

                    case "fmlv":
                        var formRaw = Kh2.Battle.Fmlv.Read(stream).ToList();
                        var formList = new Dictionary<Kh2.Battle.Fmlv.FormFm, List<Kh2.Battle.Fmlv>>();
                        foreach (var form in formRaw)
                        {
                            if (!formList.ContainsKey(form.FinalMixForm))
                            {
                                formList.Add(form.FinalMixForm, new List<Kh2.Battle.Fmlv>());
                            }
                            formList[form.FinalMixForm].Add(form);
                        }
                        var moddedForms = deserializer.Deserialize<Dictionary<Kh2.Battle.Fmlv.FormFm, List<FmlvDTO>>>(sourceText);
                        foreach (var form in moddedForms)
                        {
                            foreach (var level in form.Value)
                            {
                                formList[form.Key][level.FormLevel - 1].Ability = (ushort)level.Ability;
                                formList[form.Key][level.FormLevel - 1].Exp = level.Experience;
                                formList[form.Key][level.FormLevel - 1].Unk1 = level.GrowthAbilityLevel;

                            }

                        }
                        Kh2.Battle.Fmlv.Write(stream.SetPosition(0), formList.Values.SelectMany(x => x).ToList());
                        break;

                    case "lvup":
                        var levelList = Kh2.Battle.Lvup.Read(stream);
                        var moddedLevels = deserializer.Deserialize<Dictionary<string, Dictionary<int, Kh2.Battle.Lvup.PlayableCharacter.Level>>>(sourceText);

                        foreach (var character in moddedLevels)
                        {
                            var charIndex = characterMap[character.Key] - 1;
                            var characterData = levelList.Characters[charIndex];

                            foreach (var level in character.Value)
                            {
                                int levelIndex = level.Key - 1;
                                while (characterData.Levels.Count <= levelIndex)
                                {
                                    characterData.Levels.Add(new Kh2.Battle.Lvup.PlayableCharacter.Level());
                                }
                                characterData.Levels[levelIndex] = level.Value;
                            }
                            characterData.NumLevels = characterData.Levels.Count;
                        }

                        levelList.Write(stream.SetPosition(0));
                        break;
                        
                    case "went":
                    {
                        var went = Kh2.SystemData.Went.Read(stream);

                        var mod = deserializer.Deserialize<
                            Dictionary<string, Dictionary<int, uint>>
                        >(sourceText);

                        foreach (var headerEntry in mod)
                        {
                            if (!wentNameMap.TryGetValue(headerEntry.Key, out int headerIndex))
                                throw new Exception($"Invalid Went name: {headerEntry.Key}");

                            uint offset = went.Offsets[headerIndex];
                            if (offset == 0)
                                continue;

                            var set = went.Sets
                                .Find(x => x.OriginalOffset == offset);

                            if (set == null)
                                throw new Exception($"Set not found for offset {offset}");

                            foreach (var entry in headerEntry.Value)
                            {
                                int weaponIndex = entry.Key;
                                uint weaponId = entry.Value;

                                while (set.WeaponIds.Count <= weaponIndex)
                                    set.WeaponIds.Add(0);

                                set.WeaponIds[weaponIndex] = weaponId;
                            }
                        }

                        stream.SetLength(0);
                        stream.Position = 0;

                        went.Write(stream);
                        break;
                    }

                    case "sstm":
                    {
                        var sstm = Kh2.SystemData.Sstm.Read(stream);

                        var mod = deserializer.Deserialize<Dictionary<string, object>>(sourceText);

                        foreach (var entry in mod)
                        {
                            var prop = typeof(Kh2.SystemData.Sstm).GetProperty(entry.Key);
                            if (prop == null)
                                throw new Exception($"Invalid SSTM field: {entry.Key}");

                            var converted = Convert.ChangeType(
                                entry.Value,
                                Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType
                            );

                            prop.SetValue(sstm, converted);
                        }

                        sstm.Write(stream);
                        break;
                    }

                    case "prty":
                    {
                        var file = Kh2.SystemData.PrtyFile.Read(stream);

                        var mod = deserializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(sourceText);

                        foreach (var character in mod)
                        {
                            if (!Kh2.SystemData.Prty.CharacterMap.TryGetValue(character.Key, out int index))
                                throw new Exception($"Invalid PRTY character: {character.Key}");

                            int realIndex = index + 1;
                            int offset = file.Offsets[realIndex];

                            var entry = file.UniqueEntries[offset];

                            foreach (var field in character.Value)
                            {
                                var prop = typeof(Kh2.SystemData.Prty).GetProperty(field.Key);
                                if (prop == null)
                                    throw new Exception($"Invalid PRTY field: {field.Key}");

                                var converted = Convert.ChangeType(
                                    field.Value,
                                    Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType
                                );

                                prop.SetValue(entry, converted);
                            }
                        }

                        file.Write(stream);
                        break;
                    }
                        
                    case "bons":
                        var bonusRaw = Kh2.Battle.Bons.Read(stream);
                        var bonusList = new Dictionary<byte, Dictionary<string, Kh2.Battle.Bons>>();
                        foreach (var bonus in bonusRaw)
                        {
                            if (!bonusList.ContainsKey(bonus.RewardId))
                            {
                                bonusList.Add(bonus.RewardId, new Dictionary<string, Kh2.Battle.Bons>());
                            }
                            var character = characterMap.First(x => x.Value == bonus.CharacterId).Key;
                            if (!bonusList[bonus.RewardId].ContainsKey(character))
                            {
                                bonusList[bonus.RewardId].Add(character, bonus);
                            }
                        }
                        var moddedBonus = deserializer.Deserialize<Dictionary<byte, Dictionary<string, Kh2.Battle.Bons>>>(sourceText);
                        foreach (var bonus in moddedBonus)
                        {
                            if (!bonusList.ContainsKey(bonus.Key))
                            {
                                bonusList.Add(bonus.Key, new Dictionary<string, Kh2.Battle.Bons>());
                            }
                            foreach (var character in moddedBonus[bonus.Key])
                            {
                                if (!bonusList[bonus.Key].ContainsKey(character.Key))
                                {
                                    bonusList[bonus.Key].Add(character.Key, new Kh2.Battle.Bons());
                                }
                                bonusList[bonus.Key][character.Key] = character.Value;
                            }
                        }

                        Kh2.Battle.Bons.Write(stream.SetPosition(0), bonusList.Values.SelectMany(x => x.Values));
                        break;

                    case "atkp":
                        var atkpList = Kh2.Battle.Atkp.Read(stream);
                        var moddedAtkp = deserializer.Deserialize<List<Kh2.Battle.Atkp>>(sourceText);

                        foreach (var attack in moddedAtkp)
                        {
                            //Same general template used for cmd, enmp, and przt.
                            // Check if the attack exists in atkpList based on Id, SubId, and Switch
                            var existingAttack = atkpList.FirstOrDefault(x => x.Id == attack.Id && x.SubId == attack.SubId && x.Switch == attack.Switch);

                            if (existingAttack != null)
                            {
                                // Update existing attack in atkpList
                                atkpList[atkpList.IndexOf(existingAttack)] = attack;
                            }
                            else
                            {
                                // Add the attack to atkpList if it doesn't exist
                                atkpList.Add(attack);
                            }
                        }

                        Kh2.Battle.Atkp.Write(stream.SetPosition(0), atkpList);
                        break;

                    case "lvpm":
                        var lvpmList = Kh2.Battle.Lvpm.Read(stream);
                        var moddedLvpm = deserializer.Deserialize<List<Kh2.Battle.LvpmHelper>>(sourceText);
                        var helperList = Kh2.Battle.LvpmHelper.ConvertLvpmListToHelper(lvpmList);

                        foreach (var level in moddedLvpm)
                        {
                            var oldLvpm = helperList.First(x => x.Level == level.Level);
                            lvpmList[helperList.IndexOf(oldLvpm)] = Kh2.Battle.LvpmHelper.ConvertLvpmHelperToLvpm(level);
                        }
                        Kh2.Battle.Lvpm.Write(stream.SetPosition(0), lvpmList);
                        break;

                    case "objentry":
                        var objEntryList = Kh2.Objentry.Read(stream).ToDictionary(x => x.ObjectId, x => x);
                        var moddedObjEntry = deserializer.Deserialize<Dictionary<uint, Kh2.Objentry>>(sourceText);

                        foreach (var objEntry in moddedObjEntry)
                        {
                            objEntryList[objEntry.Key] = objEntry.Value; // Add or overwrite
                        }

                        // Sort final list by ObjId before writing
                        var sortedEntries = objEntryList.Values.OrderBy(x => x.ObjectId).ToList();

                        Kh2.Objentry.Write(stream.SetPosition(0), sortedEntries);
                        break;

                    case "plrp":
                        var plrpList = Kh2.Battle.Plrp.Read(stream);
                        var moddedPlrp = deserializer.Deserialize<List<Kh2.Battle.Plrp>>(sourceText);
                        foreach (var plrp in moddedPlrp)
                        {
                            var oldPlrp = plrpList.First(x => x.Character == plrp.Character && x.Id == plrp.Id);
                            if (oldPlrp != null)
                            {
                                plrpList[plrpList.IndexOf(oldPlrp)] = plrp;
                            }
                            else
                            {
                                plrpList.Add(plrp);
                            }
                        }
                        Kh2.Battle.Plrp.Write(stream.SetPosition(0), plrpList);
                        break;

                    case "cmd":
                        var cmdList = Kh2.SystemData.Cmd.Read(stream);
                        var moddedCmd = deserializer.Deserialize<List<Kh2.SystemData.Cmd>>(sourceText);

                        foreach (var commands in moddedCmd)
                        {
                            var existingCommand = cmdList.FirstOrDefault(x => x.Id == commands.Id);

                            if (existingCommand != null)
                            {
                                cmdList[cmdList.IndexOf(existingCommand)] = commands;
                            }
                            else
                            {
                                cmdList.Add(commands);
                            }
                        }

                        Kh2.SystemData.Cmd.Write(stream.SetPosition(0), cmdList);
                        break;

                    case "slct":
                        var slctList = Kh2.Slct.Read(stream);
                        var moddedSlct = deserializer.Deserialize<List<Kh2.Slct>>(sourceText);

                        foreach (var entries in moddedSlct)
                        {
                            var existingEntry = slctList.FirstOrDefault(x => x.Id == entries.Id);

                            if (existingEntry != null)
                            {
                                slctList[slctList.IndexOf(existingEntry)] = entries;
                            }
                            else
                            {
                                slctList.Add(entries);
                            }
                        }

                        Kh2.Slct.Write(stream.SetPosition(0), slctList);
                        break;
                        
                    case "localset":
                        var localList = Kh2.Localset.Read(stream);
                        var moddedLocal = deserializer.Deserialize<List<Kh2.Localset>>(sourceText);

                        foreach (var set in moddedLocal)
                        {
                            var existingSet = localList.FirstOrDefault(x => x.ProgramId == set.ProgramId);

                            if (existingSet != null)
                            {
                                localList[localList.IndexOf(existingSet)] = set;
                            }
                            else
                            {
                                localList.Add(set);
                            }
                        }

                        Kh2.Localset.Write(stream.SetPosition(0), localList);
                        break;

                    case "jigsaw":
                        var jigsawList = Kh2.Jigsaw.Read(stream);
                        var moddedJigsaw = deserializer.Deserialize<List<Kh2.Jigsaw>>(sourceText);

                        foreach (var piece in moddedJigsaw)
                        {
                            //Allow variations of capitalizations with World spellings; can't handle an extra space unfortunately, making it inconsistent with Arif.
                            //Can just use ID's for worlds, though.
                            if (worldIndexMap.TryGetValue(piece.World.ToString().Replace(" ", "").ToLower(), out var worldValue))
                            {
                                piece.World = (Kh2.Jigsaw.WorldList)worldValue;
                            }

                            var existingPiece = jigsawList.FirstOrDefault(x => x.Picture == piece.Picture && x.Part == piece.Part); //Identify a puzzle by its Picture+Part.

                            if (existingPiece != null)
                            {
                                jigsawList[jigsawList.IndexOf(existingPiece)] = piece;
                            }
                            else
                            {
                                jigsawList.Add(piece);
                            }
                        }

                        Kh2.Jigsaw.Write(stream.SetPosition(0), jigsawList);
                        break;



                    case "enmp":
                        var enmpList = Kh2.Battle.Enmp.Read(stream);
                        var moddedEnmp = deserializer.Deserialize<List<Kh2.Battle.Enmp>>(sourceText);

                        foreach (var enmp in moddedEnmp)
                        {
                            var existingEnmp = enmpList.FirstOrDefault(x => x.Id == enmp.Id);

                            if (existingEnmp != null)
                            {
                                enmpList[enmpList.IndexOf(existingEnmp)] = enmp;
                            }
                            else
                            {
                                enmpList.Add(enmp);
                            }
                        }

                        Kh2.Battle.Enmp.Write(stream.SetPosition(0), enmpList);
                        break;

                    case "shop":
                        var shop = Kh2.SystemData.Shop.Read(stream);
                        var moddedShop = deserializer.Deserialize<Kh2.SystemData.Shop.ShopHelper>(sourceText);
                        ushort inventoriesBaseOffset = (ushort)(Kh2.SystemData.Shop.HeaderSize + shop.ShopEntries.Count * Kh2.SystemData.Shop.ShopEntrySize);
                        ushort productsBaseOffset = (ushort)(inventoriesBaseOffset + shop.InventoryEntries.Count * Kh2.SystemData.Shop.InventoryEntrySize);
                        List<Kh2.SystemData.Shop.ShopEntryHelper> moddedShopEntryHelpers = shop.ShopEntries.Select(x => x.ToShopEntryHelper(inventoriesBaseOffset)).ToList();
                        if (moddedShop?.ShopEntryHelpers != null)
                        {
                            foreach (var shopEntryHelper in moddedShop.ShopEntryHelpers)
                            {
                                int entryIndex = moddedShopEntryHelpers.FindIndex(x => x.ShopID == shopEntryHelper.ShopID);
                                if (entryIndex < 0)
                                {
                                    moddedShopEntryHelpers.Add(shopEntryHelper);
                                }
                                else
                                {
                                    moddedShopEntryHelpers[entryIndex] = shopEntryHelper;
                                }
                            }
                        }
                        inventoriesBaseOffset = (ushort)(Kh2.SystemData.Shop.HeaderSize + moddedShopEntryHelpers.Count * Kh2.SystemData.Shop.ShopEntrySize);
                        shop.ShopEntries = moddedShopEntryHelpers.Select(x => x.ToShopEntry(inventoriesBaseOffset)).ToList();
                        List<Kh2.SystemData.Shop.InventoryEntryHelper> moddedInventoryEntryHelpers = shop.InventoryEntries.Select((x, index) =>
                            x.ToInventoryEntryHelper(index, productsBaseOffset)
                        ).ToList();
                        if (moddedShop?.InventoryEntryHelpers != null)
                        {
                            foreach (var inventoryEntryHelper in moddedShop.InventoryEntryHelpers)
                            {
                                int invIndex = inventoryEntryHelper.InventoryIndex;
                                if (invIndex < 0)
                                    throw new InvalidDataException($"Shop listpatch: InventoryIndex {invIndex} is an invalid index.");
                                if (invIndex >= moddedInventoryEntryHelpers.Count)
                                {
                                    int dummiesToAdd = invIndex - moddedInventoryEntryHelpers.Count;
                                    for (int i = 0; i < dummiesToAdd; i++)
                                    {
                                        moddedInventoryEntryHelpers.Add(new Kh2.SystemData.Shop.InventoryEntryHelper {
                                            InventoryIndex = moddedInventoryEntryHelpers.Count + i,
                                            UnlockEventID = 0,
                                            ProductCount = 0,
                                            ProductStartIndex = 0
                                        });
                                    }
                                    moddedInventoryEntryHelpers.Add(inventoryEntryHelper);
                                }
                                else
                                {
                                    moddedInventoryEntryHelpers[invIndex] = inventoryEntryHelper;
                                }
                            }
                        }
                        productsBaseOffset = (ushort)(inventoriesBaseOffset + moddedInventoryEntryHelpers.Count * Kh2.SystemData.Shop.InventoryEntrySize);
                        shop.InventoryEntries = moddedInventoryEntryHelpers.Select(x => x.ToInventoryEntry(productsBaseOffset)).ToList();
                        List<Kh2.SystemData.Shop.ProductEntryHelper> moddedProductEntryHelpers = shop.ProductEntries.Select((x, index) => x.ToProductEntryHelper(index)).ToList();
                        if (moddedShop?.ProductEntryHelpers != null)
                        {
                            foreach (var productEntryHelper in moddedShop.ProductEntryHelpers)
                            {
                                int prodIndex = productEntryHelper.ProductIndex;
                                if (prodIndex < 0)
                                    throw new InvalidDataException($"Shop listpatch: ProductIndex {prodIndex} is an invalid index.");
                                if (prodIndex >= moddedProductEntryHelpers.Count)
                                {
                                    int dummiesToAdd = prodIndex - moddedProductEntryHelpers.Count;
                                    for (int i = 0; i < dummiesToAdd; i++)
                                    {
                                        moddedProductEntryHelpers.Add(new Kh2.SystemData.Shop.ProductEntryHelper
                                        {
                                            ProductIndex = moddedProductEntryHelpers.Count + i,
                                            ItemID = 0
                                        });
                                    }
                                    moddedProductEntryHelpers.Add(productEntryHelper);
                                }
                                else
                                {
                                    moddedProductEntryHelpers[prodIndex] = productEntryHelper;
                                }
                            }
                        }
                        shop.ProductEntries = moddedProductEntryHelpers.Select(x => x.ToProductEntry()).ToList();
                        List<Kh2.SystemData.Shop.ProductEntryHelper> moddedValidProductEntryHelpers = shop.ValidProductEntries.Select((x, index) => x.ToProductEntryHelper(index)).ToList();
                        if (moddedShop?.ValidProductEntryHelpers != null)
                        {
                            foreach (var productEntryHelper in moddedShop.ValidProductEntryHelpers)
                            {
                                int prodIndex = productEntryHelper.ProductIndex;
                                if (prodIndex < 0)
                                    throw new InvalidDataException($"Shop listpatch: ValidProductIndex {prodIndex} is an invalid index.");
                                if (prodIndex >= moddedValidProductEntryHelpers.Count)
                                {
                                    int dummiesToAdd = prodIndex - moddedValidProductEntryHelpers.Count;
                                    for (int i = 0; i < dummiesToAdd; i++)
                                    {
                                        moddedValidProductEntryHelpers.Add(new Kh2.SystemData.Shop.ProductEntryHelper
                                        {
                                            ProductIndex = moddedValidProductEntryHelpers.Count + i,
                                            ItemID = 0
                                        });
                                    }
                                    moddedValidProductEntryHelpers.Add(productEntryHelper);
                                }
                                else
                                {
                                    moddedValidProductEntryHelpers[prodIndex] = productEntryHelper;
                                }
                            }
                        }
                        shop.ValidProductEntries = moddedValidProductEntryHelpers.Select(x => x.ToProductEntry()).ToList();
                        Kh2.SystemData.Shop.Write(stream.SetPosition(0), shop);
                        break;

                    case "sklt":
                        var skltList = Kh2.SystemData.Sklt.Read(stream);
                        var moddedSklt = deserializer.Deserialize<List<Kh2.SystemData.Sklt>>(sourceText);

                        foreach (var sklt in moddedSklt)
                        {
                            var existingSklt = skltList.FirstOrDefault(x => x.CharacterId == sklt.CharacterId);

                            if (existingSklt != null)
                            {
                                skltList[skltList.IndexOf(existingSklt)] = sklt;
                            }
                            else
                            {
                                skltList.Add(sklt);
                            }
                        }

                        Kh2.SystemData.Sklt.Write(stream.SetPosition(0), skltList);
                        break;

                    case "przt":
                        var prztList = Kh2.Battle.Przt.Read(stream);
                        var moddedPrzt = deserializer.Deserialize<List<Kh2.Battle.Przt>>(sourceText);

                        foreach (var przt in moddedPrzt)
                        {
                            var existingPrzt = prztList.FirstOrDefault(x => x.Id == przt.Id);

                            if (existingPrzt != null)
                            {
                                prztList[prztList.IndexOf(existingPrzt)] = przt;
                            }
                            else
                            {
                                prztList.Add(przt);
                            }
                        }
                        Kh2.Battle.Przt.Write(stream.SetPosition(0), prztList);
                        break;

                    case "magc":
                        var magcList = Kh2.Battle.Magc.Read(stream);
                        var moddedMagc = deserializer.Deserialize<List<Kh2.Battle.Magc>>(sourceText);
                        foreach (var magc in moddedMagc)
                        {
                            var existingMagc = magcList.First(x => x.Id == magc.Id && x.Level == magc.Level);
                            if (existingMagc != null)
                            {
                                //existingMagc = MergeHelper.Merge(existingMagc, magc);
                                magcList[magcList.IndexOf(existingMagc)] = magc;
                            }
                            else
                            {
                                magcList.Add(magc);
                            }
                        }
                        Kh2.Battle.Magc.Write(stream.SetPosition(0), magcList);
                        break;

                    case "btlv":
                        var btlvList = Kh2.Battle.Btlv.Read(stream);
                        var moddedBtlv = deserializer.Deserialize<List<Kh2.Battle.Btlv>>(sourceText);

                        foreach (var btlv in moddedBtlv)
                        {
                            var existingBtlv = btlvList.FirstOrDefault(x => x.Id == btlv.Id);

                            if (existingBtlv != null)
                            {
                                btlvList[btlvList.IndexOf(existingBtlv)] = btlv;
                            }
                            else
                            {
                                btlvList.Add(btlv);
                            }
                        }

                        Kh2.Battle.Btlv.Write(stream.SetPosition(0), btlvList);
                        break;

                    case "vtbl":
                        var vtblList = Kh2.Battle.Vtbl.Read(stream);
                        var moddedVtbl = deserializer.Deserialize<List<Kh2.Battle.Vtbl>>(sourceText);

                        foreach (var vtbl in moddedVtbl)
                        {
                            var existingVtbl = vtblList.FirstOrDefault(x => x.Id == vtbl.Id && x.CharacterId == vtbl.CharacterId);
                            //Search for CharacterID & "Action" ID.
                            if (existingVtbl != null)
                            {
                                vtblList[vtblList.IndexOf(existingVtbl)] = vtbl;

                            }
                            else
                            {
                                vtblList.Add(vtbl);
                            }
                        }

                        Kh2.Battle.Vtbl.Write(stream.SetPosition(0), vtblList);
                        break;

                    //Add new limits. ID found -> Continue.
                    case "limt":
                        var limtList = Kh2.Battle.Limt.Read(stream);
                        var moddedLimt = deserializer.Deserialize<List<Kh2.Battle.Limt>>(sourceText);

                        foreach (var limt in moddedLimt)
                        {
                            var existingLimt = limtList.FirstOrDefault(x => x.Id == limt.Id);

                            if (existingLimt != null)
                            {
                                limtList[limtList.IndexOf(existingLimt)] = limt;
                            }
                            else
                            {
                                limtList.Add(limt);
                            }
                        }
                        Kh2.Battle.Limt.Write(stream.SetPosition(0), limtList);
                        break;

                    //Listpatch for Arif. Takes a string as an input for the WorldIndex to use, and ints for the roomIndex to edit.
                    //Strings do not need to worry about capitalization, etc. so long as they have the same characters.
                    case "arif":
                        var originalData = Kh2.SystemData.Arif.Read(stream);
                        var patches = deserializer.Deserialize<Dictionary<string, Dictionary<int, Kh2.SystemData.Arif>>>(sourceText);

                        foreach (var worldPatch in patches)
                        {
                            if (!worldIndexMap.TryGetValue(worldPatch.Key.ToLower().Replace(" ", ""), out var worldIndex))
                            {
                                Log.Warn($"Invalid world index: {worldPatch.Key}");
                            }

                            if (worldIndex >= 0 && worldIndex < originalData.Count)
                            {
                                var worldData = originalData[worldIndex];

                                foreach (var areaPatch in worldPatch.Value)
                                {
                                    int areaIndex = areaPatch.Key;
                                    var patch = areaPatch.Value;

                                    // Add new areas.
                                    while (areaIndex >= worldData.Count)
                                    {
                                        worldData.Add(new Kh2.SystemData.Arif
                                        {
                                            Bgms = new Kh2.SystemData.BgmSet[8],
                                            Reserved = new byte[11]
                                        });

                                        // Initialize the BgmSet elements within the Bgms array
                                        for (int i = 0; i < 8; i++)
                                        {
                                            worldData[worldData.Count - 1].Bgms[i] = new Kh2.SystemData.BgmSet();
                                        }
                                    }
                                    // End of adding new areas.

                                    if (areaIndex >= 0 && areaIndex < worldData.Count)
                                    {
                                        var areaData = worldData[areaIndex];
                                        //Below: Compares each field to see if it's specified in the YML.
                                        //If yes, update w/ YML value.
                                        //If no, retain original value.
                                        areaData.Flags = patch.Flags != 0 ? patch.Flags : areaData.Flags;
                                        areaData.Reverb = patch.Reverb != 0 ? patch.Reverb : areaData.Reverb;
                                        areaData.SoundEffectBank1 = patch.SoundEffectBank1 != 0 ? patch.SoundEffectBank1 : areaData.SoundEffectBank1;
                                        areaData.SoundEffectBank2 = patch.SoundEffectBank2 != 0 ? patch.SoundEffectBank2 : areaData.SoundEffectBank2;
                                        for (int i = 0; i < patch.Bgms.Length && i < areaData.Bgms.Length; i++)
                                        {
                                            areaData.Bgms[i].BgmField = patch.Bgms[i].BgmField != 0 ? (ushort)patch.Bgms[i].BgmField : areaData.Bgms[i].BgmField;
                                            areaData.Bgms[i].BgmBattle = patch.Bgms[i].BgmBattle != 0 ? (ushort)patch.Bgms[i].BgmBattle : areaData.Bgms[i].BgmBattle;
                                        }
                                        areaData.Voice = patch.Voice != 0 ? patch.Voice : areaData.Voice;
                                        areaData.NavigationMapItem = patch.NavigationMapItem != 0 ? patch.NavigationMapItem : areaData.NavigationMapItem;
                                        areaData.Command = patch.Command != 0 ? patch.Command : areaData.Command;
                                        areaData.Reserved = patch.Reserved != null ? patch.Reserved : areaData.Reserved;
                                    }
                                }
                            }
                        }


                        Kh2.SystemData.Arif.Write(stream.SetPosition(0), originalData);
                        break;

                    case "place":
                        var originalPlace = Kh2.Places.Read(stream);
                        var moddedPlace = deserializer.Deserialize<List<Kh2.Places.PlacePatch>>(sourceText);

                        foreach (var place in moddedPlace)
                        {
                            if (place.Index >= 0 && place.Index < originalPlace.Count)
                            {
                                // Update existing entry
                                originalPlace[place.Index].MessageId = place.MessageId;
                                originalPlace[place.Index].Padding = place.Padding;
                            }
                            else if (place.Index == originalPlace.Count)
                            {
                                // Add new entry
                                originalPlace.Add(new Places { MessageId = place.MessageId, Padding = place.Padding });
                            }
                            else
                            {
                                // Expand the list and add the new entry at the specified index
                                while (originalPlace.Count < place.Index)
                                {
                                    originalPlace.Add(new Places { MessageId = 0, Padding = 0 });
                                }
                                originalPlace.Add(new Places { MessageId = place.MessageId, Padding = place.Padding });
                            }
                        }

                        Kh2.Places.Write(stream.SetPosition(0), originalPlace);
                        break;

                    case "soundinfo":
                        var originalSoundInfo = Kh2.Soundinfo.Read(stream);
                        var moddedSoundInfo = deserializer.Deserialize<List<Kh2.Soundinfo.SoundinfoPatch>>(sourceText);

                        foreach (var info in moddedSoundInfo)
                        {
                            while (originalSoundInfo.Count <= info.Index)
                            {
                                originalSoundInfo.Add(new Soundinfo
                                {
                                    Reverb = 0,
                                    Rate = 0,
                                    EnvironmentWAV = 0,
                                    EnvironmentSEB = 0,
                                    EnvironmentNUMBER = 0,
                                    EnvironmentSPOT = 0,
                                    FootstepWAV = 0,
                                    FootstepSORA = 0,
                                    FootstepDONALD = 0,
                                    FootstepGOOFY = 0,
                                    FootstepWORLDFRIEND = 0,
                                    FootstepOTHER = 0
                                });
                            }

                            originalSoundInfo[info.Index].Reverb = info.Reverb;
                            originalSoundInfo[info.Index].Rate = info.Rate;
                            originalSoundInfo[info.Index].EnvironmentWAV = info.EnvironmentWAV;
                            originalSoundInfo[info.Index].EnvironmentSEB = info.EnvironmentSEB;
                            originalSoundInfo[info.Index].EnvironmentNUMBER = info.EnvironmentNUMBER;
                            originalSoundInfo[info.Index].EnvironmentSPOT = info.EnvironmentSPOT;
                            originalSoundInfo[info.Index].FootstepWAV = info.FootstepWAV;
                            originalSoundInfo[info.Index].FootstepSORA = info.FootstepSORA;
                            originalSoundInfo[info.Index].FootstepDONALD = info.FootstepDONALD;
                            originalSoundInfo[info.Index].FootstepGOOFY = info.FootstepGOOFY;
                            originalSoundInfo[info.Index].FootstepWORLDFRIEND = info.FootstepWORLDFRIEND;
                            originalSoundInfo[info.Index].FootstepOTHER = info.FootstepOTHER;
                        }

                        // Write the updated list back to the stream
                        Kh2.Soundinfo.Write(stream.SetPosition(0), originalSoundInfo);
                        break;

                    case "libretto":
                        var originalLibretto = Kh2.Libretto.Read(stream);

                        var patches2 = deserializer.Deserialize<List<Kh2.Libretto.TalkMessagePatch>>(sourceText);

                        foreach (var patch in patches2)
                        {
                            var definition = originalLibretto.Definitions.FirstOrDefault(def => def.TalkMessageId == patch.TalkMessageId);

                            if (definition != null)
                            {
                                definition.Type = patch.Type;

                                var contentList = new List<Libretto.TalkMessageContent>();
                                foreach (var contentPatch in patch.Contents)
                                {
                                    contentList.Add(new Libretto.TalkMessageContent
                                    {
                                        CodeType = contentPatch.CodeType,
                                        Unknown = contentPatch.Unknown,
                                        TextId = contentPatch.TextId
                                    });
                                }
                                originalLibretto.Contents[originalLibretto.Definitions.IndexOf(definition)] = contentList;
                            }
                            else
                            {
                                var newDefinition = new Libretto.TalkMessageDefinition
                                {
                                    TalkMessageId = patch.TalkMessageId,
                                    Type = patch.Type,
                                    ContentPointer = 0 // Will update this later after adding content entries
                                };

                                originalLibretto.Definitions.Add(newDefinition);
                                originalLibretto.Count++;

                                var contentList = new List<Libretto.TalkMessageContent>();
                                foreach (var contentPatch in patch.Contents)
                                {
                                    contentList.Add(new Libretto.TalkMessageContent
                                    {
                                        CodeType = contentPatch.CodeType,
                                        Unknown = contentPatch.Unknown,
                                        TextId = contentPatch.TextId
                                    });
                                }
                                originalLibretto.Contents.Add(contentList);
                            }
                        }

                        stream.Position = 0;
                        Kh2.Libretto.Write(stream, originalLibretto);
                        break;


                    case "memt":
                        var memt = Kh2.SystemData.Memt.Read(stream);
                        var memtEntries = memt.Entries.Cast<Kh2.SystemData.Memt.EntryFinalMix>().ToList();
                        var memtPatches = deserializer.Deserialize<Kh2.SystemData.Memt.MemtPatches>(sourceText);

                        if (memtPatches.MemtEntries != null)
                        {
                            foreach (var patch in memtPatches.MemtEntries)
                            {
                                if (patch.Index < 0)
                                    throw new IndexOutOfRangeException($"Invalid index {patch.Index} for Memt.");

                                if (patch.Index >= memtEntries.Count)
                                {
                                    // Index is beyond current entries, append new entries up to the patch index
                                    while (memtEntries.Count <= patch.Index)
                                    {
                                        memtEntries.Add(new Kh2.SystemData.Memt.EntryFinalMix());
                                    }
                                }

                                var memtEntry = memtEntries[patch.Index];
                                memtEntry.WorldId = patch.WorldId;
                                memtEntry.CheckStoryFlag = patch.CheckStoryFlag;
                                if (!string.IsNullOrWhiteSpace(patch.FlagForWorld))
                                {
                                    if (!worldIndexMap.TryGetValue(patch.FlagForWorld.Replace(" ", "").ToLower(), out var worldId))
                                        throw new Exception($"Unknown world name '{patch.FlagForWorld}' in CheckStoryFlagWorld.");

                                    memtEntry.CheckStoryFlag += (short)(worldId * 1024);
                                }

                                memtEntry.CheckStoryFlagNegation = patch.CheckStoryFlagNegation;
                                if (!string.IsNullOrWhiteSpace(patch.NegationFlagForWorld))
                                {
                                    if (!worldIndexMap.TryGetValue(patch.NegationFlagForWorld.Replace(" ", "").ToLower(), out var worldId))
                                        throw new Exception($"Unknown world name '{patch.NegationFlagForWorld}' in CheckStoryFlagNegationWorld.");

                                    memtEntry.CheckStoryFlagNegation += (short)(worldId * 1024);
                                }
                                memtEntry.CheckArea = patch.CheckArea;
                                memtEntry.Padding = patch.Padding;
                                memtEntry.PlayerSize = patch.PlayerSize;
                                memtEntry.FriendSize = patch.FriendSize;

                                memtEntry.Members = patch.Members.ToArray();
                                memtEntries[patch.Index] = memtEntry;
                            }

                            memt.Entries.Clear();
                            memt.Entries.AddRange(memtEntries);

                            stream.Position = 0;
                            Kh2.SystemData.Memt.Write(stream, memt);
                        }

                        if (memtPatches.MemberIndices != null)
                        {
                            foreach (var patch in memtPatches.MemberIndices)
                            {
                                if (patch.Index < 0 || patch.Index >= memt.MemberIndexCollection.Length)
                                    throw new IndexOutOfRangeException($"Invalid MemberIndices index {patch.Index}.");

                                var memberIndices = memt.MemberIndexCollection[patch.Index];
                                memberIndices.Player = patch.Player;
                                memberIndices.Friend1 = patch.Friend1;
                                memberIndices.Friend2 = patch.Friend2;
                                memberIndices.FriendWorld = patch.FriendWorld;

                                memt.MemberIndexCollection[patch.Index] = memberIndices;
                            }

                            stream.Position = 0;
                            Kh2.SystemData.Memt.Write(stream, memt);
                        }
                        break;

                    //New: Pref listpatches. More rigid as they're mostly offset-based. Can be updated to eventually support addition though.
                    case "fmab":
                        var fmabList = Kh2.SystemData.Fmab.Read(stream);

                        var moddedFmab = deserializer.Deserialize<Kh2.SystemData.Fmab.FmabEntries>(sourceText);

                        foreach (var patch in moddedFmab.Entries)
                        {
                            if (patch.Key >= 0 && patch.Key < fmabList.Count)
                            {
                                fmabList[patch.Key] = patch.Value;
                            }
                        }

                        stream.SetLength(0);
                        Kh2.SystemData.Fmab.Write(stream, fmabList);
                        break;

                    default:
                        break;
                }
            }
        }
        private static void PatchSynth(Context context, List<AssetFile> sources, Stream stream)
        {
            foreach (var source in sources)
            {
                string sourceText = File.ReadAllText(context.GetSourceModAssetPath(source.Name));
                switch (source.Type)
                {
                    case "recipe":
                        var recipeList = Kh2.Mixdata.ReciLP.Read(stream); // Read existing Reci list
                        var moddedRecipes = deserializer.Deserialize<List<Kh2.Mixdata.ReciLP>>(sourceText); // Deserialize modded recipes

                        foreach (var moddedRecipe in moddedRecipes)
                        {
                            var existingRecipe = recipeList.FirstOrDefault(x => x.Id == moddedRecipe.Id);

                            if (existingRecipe != null)
                            {
                                // Update existing recipe in the list
                                recipeList[recipeList.IndexOf(existingRecipe)] = moddedRecipe;

                                // Update other properties as needed
                            }
                            else
                            {
                                // Add new recipe to the list
                                recipeList.Add(moddedRecipe);
                            }
                        }

                        // Write the updated recipe list back to the stream
                        Kh2.Mixdata.ReciLP.Write(stream, recipeList); // Pass IEnumerable<Reci>
                        break;

                    case "condition":
                        var conditionList = Kh2.Mixdata.CondLP.Read(stream);
                        var moddedConditions = deserializer.Deserialize<List<Kh2.Mixdata.CondLP>>(sourceText);

                        foreach (var moddedCondition in moddedConditions)
                        {
                            var existingCondition = conditionList.FirstOrDefault(x => x.TextId == moddedCondition.TextId);

                            if (existingCondition != null)
                            {
                                conditionList[conditionList.IndexOf(existingCondition)] = moddedCondition;

                            }
                            else
                            {
                                conditionList.Add(moddedCondition);
                            }
                        }

                        Kh2.Mixdata.CondLP.Write(stream, conditionList); // Pass IEnumerable<Reci>
                        break;

                    case "level":
                        var levelList = Kh2.Mixdata.LeveLP.Read(stream);
                        var moddedLevels = deserializer.Deserialize<List<Kh2.Mixdata.LeveLP>>(sourceText);

                        foreach (var moddedLevel in moddedLevels)
                        {
                            var existingLevel = levelList.FirstOrDefault(x => x.Title == moddedLevel.Title);

                            if (existingLevel != null)
                            {
                                levelList[levelList.IndexOf(existingLevel)] = moddedLevel;
                            }
                            else
                            {
                                levelList.Add(moddedLevel);
                            }
                        }

                        Kh2.Mixdata.LeveLP.Write(stream, levelList);
                        break;
                }
            }
        }
    }
}



