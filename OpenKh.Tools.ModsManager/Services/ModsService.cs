using LibGit2Sharp;
using OpenKh.Common;
using OpenKh.Patcher;
using OpenKh.Tools.ModsManager.Exceptions;
using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
                var modsPath = ConfigurationService.ModsGamePath;
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
                var collectionsPath = ConfigurationService.ModCollectionsPath;
                foreach (var dir in Directory.GetDirectories(collectionsPath))
                {
                    var authorName = Path.GetFileName(dir);
                    foreach (var subdir in Directory.GetDirectories(dir))
                    {
                        var repoName = Path.GetFileName(subdir);
                        if (File.Exists(Path.Combine(subdir, ModMetadata)))
                        {
                            yield return $"{authorName}/{repoName}";
                        }
                    }
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
            bool isLuaFile,
            Action<string> progressOutput = null,
            Action<float> progressNumber = null)
        {
            if (!isZipFile && !isLuaFile)
            {
                return Task.Run(() => InstallModFromGithub(name, progressOutput, progressNumber));
            }
            else if (isZipFile && !isLuaFile)
            {
                return Task.Run(() => InstallModFromZip(name, progressOutput, progressNumber));
            }
            else
            {
                return Task.Run(() => InstallModFromLua(name));
            }
        }

        public static void InstallModFromZip(
            string fileName,
            Action<string> progressOutput = null,
            Action<float> progressNumber = null)
        {
            var modName = Path.GetFileNameWithoutExtension(fileName);
            progressOutput?.Invoke($"Opening '{modName}' zip archive...");

            using var zipFile = ZipFile.OpenRead(fileName);

            var isModPatch = fileName.ToLower().Contains(".kh2pcpatch") || fileName.ToLower().Contains(".kh1pcpatch") || fileName.ToLower().Contains(".compcpatch") || fileName.ToLower().Contains(".bbspcpatch") || fileName.ToLower().Contains(".dddpcpatch") ? true : false;
            var isValidMod = zipFile.GetEntry(ModMetadata) != null || isModPatch;

            if (!isValidMod)
                throw new ModNotValidException(modName);

            var modPath = GetModPath(modName);
            if (Directory.Exists(modPath))
            {
                var errorMessage = MessageBox.Show($"A mod with the name '{modName}' already exists. Do you want to overwrite the mod install.", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);

                switch (errorMessage)
                {
                    case MessageBoxResult.Yes:
                        MainViewModel.overwriteMod = true;
                        Directory.Delete(modPath, true);
                        break;
                    case MessageBoxResult.No:
                        throw new ModAlreadyExistsExceptions(modName);
                }
            }

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
                if (fileName.ToLower().Contains(".kh2pcpatch"))
                {
                    _yamlGen.Title = modName + " (KH2PCPATCH)";
                    _yamlGen.Game = "kh2";
                    _yamlGen.Description = "This is an automatically generated metadata for this KH2PCPATCH Modification.";
                }
                else if (fileName.ToLower().Contains(".kh1pcpatch"))
                {
                    _yamlGen.Title = modName + " (KH1PCPATCH)";
                    _yamlGen.Game = "kh1";
                    _yamlGen.Description = "This is an automatically generated metadata for this KH1PCPATCH Modification.";
                }
                else if (fileName.ToLower().Contains(".compcpatch"))
                {
                    _yamlGen.Title = modName + " (COMPCPATCH)";
                    _yamlGen.Game = "Recom";
                    _yamlGen.Description = "This is an automatically generated metadata for this COMPCPATCH Modification.";
                }
                else if (fileName.ToLower().Contains(".bbspcpatch"))
                {
                    _yamlGen.Title = modName + " (BBSPCPATCH)";
                    _yamlGen.Game = "bbs";
                    _yamlGen.Description = "This is an automatically generated metadata for this BBSPCPATCH Modification.";
                }
                else if (fileName.ToLower().Contains(".dddpcpatch"))
                {
                    _yamlGen.Title = modName + " (DDDPCPATCH)";
                    _yamlGen.Game = "kh3d";
                    _yamlGen.Description = "This is an automatically generated metadata for this DDDPCPATCH Modification.";
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

            var modPaths = new List<string> {
                GetModPath(repositoryName),
                GetCollectionPath(repositoryName),
                GetModsGamePath(repositoryName, "kh1"),
                GetModsGamePath(repositoryName, "kh2"),
                GetModsGamePath(repositoryName, "bbs"),
                GetModsGamePath(repositoryName, "Recom"),
                GetModsGamePath(repositoryName, "kh3d"),
            };
            if (modPaths.Any(mod => Directory.Exists(mod)))
            {
                var errorMessage = MessageBox.Show($"A mod with the name '{repositoryName}' already exists. Do you want to overwrite the mod install.", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No , MessageBoxOptions.DefaultDesktopOnly);

                switch (errorMessage)
                {
                    case MessageBoxResult.Yes:
                        Handle(() =>
                        {
                            MainViewModel.overwriteMod = true;
                            ClearOldMods(repositoryName);
                        });
                        break;
                    case MessageBoxResult.No:
                        throw new ModAlreadyExistsExceptions(repositoryName);
                }
            }
            await Task.Delay(1); // fixes a bug where folder creation and deletions collide
            var modPath = modPaths[0];
            Directory.CreateDirectory(modPath);

            progressOutput?.Invoke($"Mod found, initializing cloning process");
            await Task.Run(() =>
            {
                var options = new CloneOptions
                {
                    RecurseSubmodules = true,
                };
                options.FetchOptions.OnProgress = (serverProgressOutput) =>
                {
                    progressOutput?.Invoke(serverProgressOutput);
                    return true;
                };
                options.FetchOptions.OnTransferProgress = (progress) =>
                {
                    var nProgress = ((float)progress.ReceivedObjects / (float)progress.TotalObjects);
                    progressNumber?.Invoke(nProgress);

                    progressOutput?.Invoke("Received Bytes: " + (progress.ReceivedBytes / 1048576) + " MB");
                    return true;
                };

                Repository.Clone($"https://github.com/{repositoryName}", modPath, options);
            });
            if (IsCollection(repositoryName))
                await MoveToCollection(repositoryName);
        }

        public static void InstallModFromLua(string fileName)
        {
            string modName = Path.GetFileNameWithoutExtension(fileName);
            string modAuthor = null;
            string modDescription = null;

            if (!fileName.Contains(".lua"))
                throw new ModNotValidException(modName);

            string modPath = GetModPath(modName);
            if (Directory.Exists(modPath))
            {
                var errorMessage = MessageBox.Show($"A mod with the name '{modName}' already exists. Do you want to overwrite the mod install.", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);

                switch (errorMessage)
                {
                    case MessageBoxResult.Yes:
                        MainViewModel.overwriteMod = true;
                        Directory.Delete(modPath, true);
                        break;
                    case MessageBoxResult.No:
                        throw new ModAlreadyExistsExceptions(modName);
                }
            }
            Directory.CreateDirectory(modPath);
            File.Copy(fileName, Path.Combine(modPath, Path.GetFileName(fileName)));

            StreamReader r = new StreamReader(Path.Combine(modPath, Path.GetFileName(fileName)));
            while (!r.EndOfStream)
            {
                string line = r.ReadLine();

                if (line.Contains("LUAGUI"))
                {
                    string _lineGib = "";
                    string _lineLead = "";

                    _lineGib = line.Substring(line.IndexOf("=") + 1).Replace("\"", "").Replace("\'", "").Trim();
                    _lineLead = string.Concat(line.Take(11));

                    switch (_lineLead)
                    {
                        case "LUAGUI_NAME":
                            modName = "\"" + _lineGib + "\"";
                            break;
                        case "LUAGUI_AUTH":
                            modAuthor = "\"" + _lineGib + "\"";
                            break;
                        case "LUAGUI_DESC":
                            modDescription = "\"" + _lineGib + "\"";
                            break;
                    }

                    if (modName != null && modAuthor != null && modDescription != null)
                        break;
                }
            }
            r.Close();
            modDescription ??= "This is an automatically generated metadata for a Lua Zip Mod";
            string yaml = $"title: {modName}\r\n{(modAuthor != null ? $"author: {modAuthor}\r\n" : "")}description: {modDescription}\r\n" +
                $"assets:\r\n- name: scripts/{Path.GetFileName(fileName)}\r\n  method: copy\r\n  source:\r\n  - name: {Path.GetFileName(fileName)}";
            string _yamlPath = Path.Combine(modPath + "/mod.yml");

            File.WriteAllText(_yamlPath, yaml);
        }

        public static string GetModPath(string repositoryName) =>
            Path.Combine(ConfigurationService.ModsGamePath, repositoryName);
        public static string GetModsGamePath(string repositoryName, string game) =>
            Path.Combine(ConfigurationService.ModsGamePath, "..", game, repositoryName);
        public static string GetCollectionPath(string repositoryName) =>
            Path.Combine(ConfigurationService.ModCollectionsPath, repositoryName);

        public static IEnumerable<ModModel> GetMods(IEnumerable<string> modNames)
        {
            var enabledMods = ConfigurationService.EnabledMods;
            var collectionEnabledMods = ConfigurationService.EnabledCollectionMods;
            foreach (var modName in modNames)
            {
                var collectionEnabledAssets = collectionEnabledMods.ContainsKey(modName) ? collectionEnabledMods[modName] : new Dictionary<string, bool> { };
                var modPath = GetModPath(modName);
                if (!Directory.Exists(modPath))
                    modPath = GetCollectionPath(modName);
                if (!Directory.Exists(modPath))
                    throw new ModNotValidException(modName);
                yield return new ModModel
                {
                    Name = modName,
                    Path = modPath,
                    IconImageSource = Path.Combine(modPath, "icon.png"),
                    PreviewImageSource = Path.Combine(modPath, "preview.png"),
                    Metadata = GetMetadata(modPath),
                    IsEnabled = enabledMods.Contains(modName),
                    CollectionOptionalEnabledAssets = collectionEnabledAssets
                };
            }
        }

        public static async IAsyncEnumerable<ModUpdateModel> FetchUpdates()
        {
            foreach (var modName in Mods)
            {
                var modPath = GetModPath(modName);
                var isCollection = false;
                if (!Directory.Exists(modPath))
                {
                    isCollection = true;
                    modPath = GetCollectionPath(modName);
                }
                var updateCount = await RepositoryService.FetchUpdate(modPath);
                var metadata = GetMetadata(modPath);
                if (isCollection && !metadata.IsCollection)
                    await MoveFromCollection(modName);
                else if (!isCollection && metadata.IsCollection)
                    await MoveToCollection(modName);
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
            Action<float> progressNumber = null) => Task.Run(() => Handle(() =>
            {
                var modPath = GetModPath(modName);
                var isCollection = false;
                if (!Directory.Exists(modPath))
                {
                    isCollection = true;
                    modPath = GetCollectionPath(modName);
                }
                RepositoryService.FetchAndResetUponOrigin(modPath, progressOutput, progressNumber);
                var metadata = GetMetadata(modPath);
                if (isCollection && !metadata.IsCollection)
                    MoveFromCollection(modName);
                else if (!isCollection && metadata.IsCollection)
                    MoveToCollection(modName);
            }));

        public static Task<bool> RunPatcherAsync(bool fastMode) => Task.Run(() => Handle(() =>
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
            var collectionOptionalModsList = ConfigurationService.EnabledCollectionMods;
            var enabledOptionalAssets = new Dictionary<string, bool> { };
            var packageMap = new ConcurrentDictionary<string, string>();

            for (var i = modsList.Count - 1; i >= 0; i--)
            {
                var mod = modsList[i];
                Log.Info($"Building {mod.Name} for {_gameList[ConfigurationService.GameEdition]} - {_langList[ConfigurationService.RegionId]}");
                if (collectionOptionalModsList.ContainsKey(mod.Name))
                    enabledOptionalAssets = collectionOptionalModsList[mod.Name];

                patcherProcessor.Patch(
                    Path.Combine(ConfigurationService.GameDataLocation, ConfigurationService.LaunchGame),
                    Path.Combine(ConfigurationService.GameModPath, ConfigurationService.LaunchGame),
                    mod.Metadata,
                    mod.Path,
                    ConfigurationService.GameEdition,
                    fastMode,
                    packageMap,
                    ConfigurationService.LaunchGame,
                    ConfigurationService.PcReleaseLanguage,
                    false,
                    enabledOptionalAssets);
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

        public static bool IsCollection(string modName)
        {
            var modPath = GetModPath(modName);
            if (!Directory.Exists(modPath))
                modPath = GetCollectionPath(modName);
            var metadata = GetMetadata(modPath);
            if (metadata.IsCollection)
                return true;
            return false;
        }

        public static Task MoveToCollection(string modName) => Task.Run(() => Handle(() =>
        {
            var modPath = GetModPath(modName);
            var collectionPath = GetCollectionPath(modName);
            var authorPath = GetCollectionPath(modName.Split("/")[0]);
            {
                if (!Directory.Exists(authorPath))
                    Directory.CreateDirectory(authorPath);
                Directory.Move(modPath, collectionPath);
            }
        }));

        public static Task MoveFromCollection(string modName) => Task.Run(() => Handle(() =>
        {
            var modPath = GetCollectionPath(modName);
            var metadata = GetMetadata(modPath);
            if (metadata.Game != null)
            {
                var gamePath = GetModsGamePath(modName, metadata.Game);
                var authorPath = GetModsGamePath(modName.Split("/")[0], metadata.Game);
                if (!Directory.Exists(authorPath))
                    Directory.CreateDirectory(authorPath);
                Directory.Move(modPath, gamePath);
            }
            else
            {
                ClearOldMods(modName);
                throw new ModMovedWithoutGameException(modName);
            }
        }));

        public static Task ClearOldMods(string modName) => Task.Run(() => Handle(() =>
        {
            foreach (var game in new List<string> { "kh1", "kh2", "bbs", "Recom", "kh3d" })
            {
                var modPath = GetModsGamePath(modName, game);
                if (Directory.Exists(modPath))
                    CleanModFiles(modPath);
            }
            var collectionPath = GetCollectionPath(modName);
            if (Directory.Exists(collectionPath))
                CleanModFiles(collectionPath);
        }));

        public static Task CleanModFiles(string modPath) => Task.Run(() => Handle(() =>
        {
            foreach (var filePath in Directory.GetFiles(modPath, "*", SearchOption.AllDirectories))
            {
                var attributes = File.GetAttributes(filePath);
                if (attributes.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
            }
            Directory.Delete(modPath, true);
        }));

        public static Metadata GetMetadata(string modPath)
        {
            return File.OpenRead(Path.Combine(modPath, ModMetadata)).Using(Metadata.Read);
        }

        public static IEnumerable<CollectionModModel> GetCollectionOptionalMods(ModModel mod)
        {
            var enabledMods = ConfigurationService.EnabledCollectionMods;
            foreach (var asset in mod.Metadata.Assets)
            {
                if (!asset.CollectionOptional)
                    continue;
                var enabled = false;
                if (enabledMods.ContainsKey(mod.Name))
                {
                    if (enabledMods[mod.Name].ContainsKey(asset.Name))
                        enabled = enabledMods[mod.Name][asset.Name];
                    else
                        enabledMods[mod.Name][asset.Name] = false;
                }
                yield return new CollectionModModel
                {
                    Name = asset.Name,
                    Author = mod.Metadata.OriginalAuthor,
                    IsEnabled = enabled
                };
            }
            ConfigurationService.EnabledCollectionMods = enabledMods;
        }
    }
}
