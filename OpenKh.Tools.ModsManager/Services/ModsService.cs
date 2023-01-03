using LibGit2Sharp;
using OpenKh.Common;
using OpenKh.Patcher;
using OpenKh.Tools.ModsManager.Exceptions;
using OpenKh.Tools.ModsManager.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using static OpenKh.Tools.ModsManager.Helpers;

namespace OpenKh.Tools.ModsManager.Services
{
    public static class ModsService
    {
        private const string ModMetadata = "mod.yml";
        private const string DefaultGitBranch = "main";

        private static string[] _gameList = new string[]
        {
            "OpenKH",
            "PCSX2-EX",
            "PC"
        };

        private static string[] _langList = new string[]
        {
            "Default",
            "Japanese",
            "English [US]",
            "English [UK]",
            "Italian",
            "Spanish",
            "German",
            "French",
            "Final Mix"
        };

        public static IEnumerable<string> Mods
        {
            get
            {
                var allMods = UnorderedMods.ToList();
                var enabledMods = EnabledMods.ToHashSet();
                var disabledMods = new List<string>(allMods.Count);
                foreach (var mod in allMods)
                {
                    if (!enabledMods.Contains(mod))
                        disabledMods.Add(mod);
                }

                return enabledMods.Concat(disabledMods);
            }
        }

        public static IEnumerable<string> UnorderedMods
        {
            get
            {
                var modsPath = ConfigurationService.ModCollectionPath;
                foreach (var dir in Directory.GetDirectories(modsPath))
                {
                    var authorName = Path.GetFileName(dir);
                    foreach (var subdir in Directory.GetDirectories(dir))
                    {
                        var repoName = Path.GetFileName(subdir);
                        if (File.Exists(Path.Combine(subdir, ModMetadata)))
                            yield return $"{authorName}/{repoName}";
                    }

                    if (File.Exists(Path.Combine(dir, ModMetadata)))
                        yield return authorName;
                }
            }
        }

        public static IEnumerable<string> EnabledMods
        {
            get
            {
                var mods = UnorderedMods.ToList();
                foreach (var mod in ConfigurationService.EnabledMods)
                {
                    if (mods.Contains(mod))
                        yield return mod;
                }
            }
        }

        public static bool IsModBlocked(string repositoryName)
            {
            if (ConfigurationService.BlacklistedMods != null)
                return ConfigurationService.BlacklistedMods.Any(x => x.Equals(repositoryName, StringComparison.InvariantCultureIgnoreCase));
            return false;
            }

        public static bool IsUserBlocked(string repositoryName) =>
            IsModBlocked(Path.GetDirectoryName(repositoryName));

        public static Task InstallMod(
            string name,
            bool isZipFile,
            Action<string> progressOutput = null,
            Action<float> progressNumber = null)
        {
            return isZipFile ?
                Task.Run(() => InstallModFromZip(name, progressOutput, progressNumber)) :
                InstallModFromGithub(name, progressOutput, progressNumber);
        }

        public static void InstallModFromZip(
            string fileName,
            Action<string> progressOutput = null,
            Action<float> progressNumber = null)
        {
            var modName = Path.GetFileNameWithoutExtension(fileName);
            progressOutput?.Invoke($"Opening '{modName}' zip archive...");

            using var zipFile = ZipFile.OpenRead(fileName);

            var isModPatch = fileName.Contains(".kh2pcpatch") || fileName.Contains(".kh1pcpatch") || fileName.Contains(".compcpatch") || fileName.Contains(".bbspcpatch") ? true : false;
            var isValidMod = zipFile.GetEntry(ModMetadata) != null || isModPatch;

            if (!isValidMod)
                throw new ModNotValidException(modName);

            var modPath = GetModPath(modName);
            if (Directory.Exists(modPath))
                throw new ModAlreadyExistsExceptions(modName);
            Directory.CreateDirectory(modPath);

            var entryExtractCount = 0;
            var entryCount = zipFile.Entries.Count;

            foreach (var entry in zipFile.Entries.Where(x => (x.ExternalAttributes & 0x10) != 0x10))
            {
                var _str = entry.FullName;
                var _strSplitter = _str.IndexOf('/') > -1 ? "/" : "\\";

                var _splitStr = _str.Split(_strSplitter);
                var _package = _splitStr[0];

                if (isModPatch)
                {
                    if (_str.Contains("original"))
                        _str = String.Join("\\", _splitStr.Skip(2));

                    else
                        _str = String.Join("\\", _splitStr.Skip(1));
                }

                progressOutput?.Invoke($"Extracting '{_str}'...");
                progressNumber?.Invoke((float)entryExtractCount / entryCount);
                var dstFileName = Path.Combine(modPath, _str);
                var dstFilePath = Path.GetDirectoryName(dstFileName);
                if (!Directory.Exists(dstFilePath))
                    Directory.CreateDirectory(dstFilePath);
                File.Create(dstFileName)
                    .Using(outStream =>
                    {
                        using var zipStream = entry.Open();
                        zipStream.CopyTo(outStream);
                    });

                entryExtractCount++;
            }

            if (isModPatch)
            {
                var _yamlGen = new Metadata();
                if (fileName.Contains(".kh2pcpatch"))
                {
                    _yamlGen.Title = modName + " (KH2PCPATCH)";
                    _yamlGen.Game = "kh2";
                    _yamlGen.Description = "This is an automatically generated metadata for this KH2PCPATCH Modification.";
                }
                else if (fileName.Contains(".kh1pcpatch"))
                {
                    _yamlGen.Title = modName + " (KH1PCPATCH)";
                    _yamlGen.Game = "kh1";
                    _yamlGen.Description = "This is an automatically generated metadata for this KH1PCPATCH Modification.";
                }
                else if (fileName.Contains(".compcpatch"))
                {
                    _yamlGen.Title = modName + " (COMPCPATCH)";
                    _yamlGen.Game = "Recom";
                    _yamlGen.Description = "This is an automatically generated metadata for this COMPCPATCH Modification.";
                }
                else if (fileName.Contains(".bbspcpatch"))
                {
                    _yamlGen.Title = modName + " (BBSPCPATCH)";
                    _yamlGen.Game = "bbs";
                    _yamlGen.Description = "This is an automatically generated metadata for this BBSPCPATCH Modification.";
                }
                _yamlGen.OriginalAuthor = "Unknown";
                _yamlGen.Assets = new List<AssetFile>();

                foreach (var entry in zipFile.Entries.Where(x => (x.ExternalAttributes & 0x10) != 0x10))
                {
                    var _str = entry.FullName;
                    var _strSplitter = _str.IndexOf('/') > -1 ? "/" : "\\";

                    var _splitStr = _str.Split(_strSplitter);
                    var _package = _splitStr[0];

                    if (_str.Contains("original"))
                        _str = String.Join("\\", _splitStr.Skip(2));

                    else
                        _str = String.Join("\\", _splitStr.Skip(1));

                    var _assetFile = new AssetFile();
                    var _assetSource = new AssetFile();

                    _assetSource.Name = _str;

                    _assetFile.Method = "copy";
                    _assetFile.Name = _str;
                    _assetFile.Package = _package;
                    _assetFile.Source = new List<AssetFile>() { _assetSource };
                    _assetFile.Platform = "pc";

                    _yamlGen.Assets.Add(_assetFile);
                }

                var _yamlPath = Path.Combine(modPath + "/mod.yml");

                File.WriteAllText(_yamlPath, _yamlGen.ToString());
            }
        }

        public static async Task InstallModFromGithub(
            string repositoryName,
            Action<string> progressOutput = null,
            Action<float> progressNumber = null)
        {
            var branchName = DefaultGitBranch;
            progressOutput?.Invoke($"Fetching file {ModMetadata} from {branchName}");
            var isValidMod = await RepositoryService.IsFileExists(repositoryName, branchName, ModMetadata);
            if (!isValidMod)
            {
                progressOutput?.Invoke($"{ModMetadata} not found, fetching default branch name");
                branchName = await RepositoryService.GetMainBranchFromRepository(repositoryName);
                if (branchName == null)
                    throw new RepositoryNotFoundException(repositoryName);

                progressOutput?.Invoke($"Fetching file {ModMetadata} from {branchName}");
                isValidMod = await RepositoryService.IsFileExists(repositoryName, branchName, ModMetadata);
            }

            if (!isValidMod)
                throw new ModNotValidException(repositoryName);

            var modPath = GetModPath(repositoryName);
            if (Directory.Exists(modPath))
                throw new ModAlreadyExistsExceptions(repositoryName);
            Directory.CreateDirectory(modPath);

            progressOutput?.Invoke($"Mod found, initializing cloning process");
            await Task.Run(() =>
            {
                var options = new CloneOptions
                {
                    RecurseSubmodules = true,
                    OnProgress = (serverProgressOutput) =>
                    {
                        progressOutput?.Invoke(serverProgressOutput);
                        return true;
                    },
                    OnTransferProgress = (progress) =>
                    {
                        var nProgress = (progress.IndexedObjects + progress.ReceivedObjects) / (progress.TotalObjects * 2);
                        progressNumber?.Invoke(nProgress);
                        return true;
                    }
                };

                Repository.Clone($"https://github.com/{repositoryName}", modPath, options);
            });
        }

        public static string GetModPath(string author, string repo) =>
            Path.Combine(ConfigurationService.ModCollectionPath, author, repo);

        public static string GetModPath(string repositoryName) =>
            Path.Combine(ConfigurationService.ModCollectionPath, repositoryName);

        public static IEnumerable<ModModel> GetMods(IEnumerable<string> modNames)
        {
            var enabledMods = ConfigurationService.EnabledMods;
            foreach (var modName in modNames)
            {
                var modPath = GetModPath(modName);
                yield return new ModModel
                {
                    Name = modName,
                    Path = modPath,
                    IconImageSource = Path.Combine(modPath, "icon.png"),
                    PreviewImageSource = Path.Combine(modPath, "preview.png"),
                    Metadata = File.OpenRead(Path.Combine(modPath, ModMetadata)).Using(Metadata.Read),
                    IsEnabled = enabledMods.Contains(modName)
                };
            }            
        }

        public static async IAsyncEnumerable<ModUpdateModel> FetchUpdates()
        {
            foreach (var modName in Mods)
            {
                var modPath = GetModPath(modName);
                var updateCount = await RepositoryService.FetchUpdate(modPath);
                if (updateCount > 0)
                    yield return new ModUpdateModel
                    {
                        Name = modName,
                        UpdateCount = updateCount
                    };
            }
        }

        public static Task Update(string modName,
            Action<string> progressOutput = null,
            Action<float> progressNumber = null) =>
            RepositoryService.FetchAndResetUponOrigin(GetModPath(modName), progressOutput, progressNumber);

        public static Task<bool> RunPacherAsync(bool fastMode) => Task.Run(() => Handle(() =>
        {
            if (Directory.Exists(Path.Combine(ConfigurationService.GameModPath, ConfigurationService.LaunchGame)))
            {
                try
                {
                    Directory.Delete(Path.Combine(ConfigurationService.GameModPath, ConfigurationService.LaunchGame), true);
                }
                catch (Exception ex)
                {
                    Log.Warn("Unable to fully clean the mod directory:\n{0}", ex.Message);
                }
            }

            Directory.CreateDirectory(Path.Combine(ConfigurationService.GameModPath, ConfigurationService.LaunchGame));

            var patcherProcessor = new PatcherProcessor();
            var modsList = GetMods(EnabledMods).ToList();
            var packageMap = new ConcurrentDictionary<string, string>();

            for (var i = modsList.Count - 1; i >= 0; i--)
            {
                var mod = modsList[i];
                Log.Info($"Building {mod.Name} for {_gameList[ConfigurationService.GameEdition]} - {_langList[ConfigurationService.RegionId]}");

                patcherProcessor.Patch(
                    Path.Combine(ConfigurationService.GameDataLocation, ConfigurationService.LaunchGame),
                    Path.Combine(ConfigurationService.GameModPath, ConfigurationService.LaunchGame),
                    mod.Metadata,
                    mod.Path,
                    ConfigurationService.GameEdition,
                    fastMode,
                    packageMap,
                    ConfigurationService.LaunchGame);
            }

            using var packageMapWriter = new StreamWriter(Path.Combine(Path.Combine(ConfigurationService.GameModPath, ConfigurationService.LaunchGame), "patch-package-map.txt"));
            foreach (var entry in packageMap)
                packageMapWriter.WriteLine(entry.Key + " $$$$ " + entry.Value);
            packageMapWriter.Flush();
            packageMapWriter.Close();

            return true;
        }));

        private static string GetSourceFromUrl(string url)
        {
            var projectNameIndex = url.LastIndexOf('/');
            if (projectNameIndex < 0)
                return null;
            var projectName = url.Substring(projectNameIndex + 1);
            if (projectName.EndsWith(".git"))
                projectName = projectName.Substring(0, projectName.Length - 4);

            var firstPart = url.Substring(0, projectNameIndex);
            var authorNameIndex = firstPart.LastIndexOf('/');
            if (authorNameIndex < 0)
                authorNameIndex = firstPart.LastIndexOf(':');
            if (authorNameIndex < 0)
                return null;
            var authorName = firstPart.Substring(authorNameIndex + 1);

            return $"{authorName}/{projectName}";
        }

        private static string GetSourceFromRepository(Repository repository)
        {
            if (repository == null)
                return null;

            var remote = repository.Network.Remotes.FirstOrDefault();
            return remote != null ? GetSourceFromUrl(remote.Url) : null;
        }
    }
}
