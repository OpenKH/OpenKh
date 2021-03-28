using LibGit2Sharp;
using OpenKh.Common;
using OpenKh.Patcher;
using OpenKh.Tools.ModsManager.Exceptions;
using OpenKh.Tools.ModsManager.Models;
using System;
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

        public static bool IsModBlocked(string repositoryName) =>
            ConfigurationService.BlacklistedMods.Any(x => x.Equals(repositoryName, StringComparison.InvariantCultureIgnoreCase));

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
            var isValidMod = zipFile.GetEntry(ModMetadata) != null;
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
                progressOutput?.Invoke($"Extracting '{entry.FullName}'...");
                progressNumber?.Invoke((float)entryExtractCount / entryCount);
                var dstFileName = Path.Combine(modPath, entry.FullName);
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

        public static Task<bool> RunPacherAsync() => Task.Run(() => Handle(() =>
        {
            if (Directory.Exists(ConfigurationService.GameModPath))
            {
                try
                {
                    Directory.Delete(ConfigurationService.GameModPath, true);
                }
                catch (Exception ex)
                {
                    Log.Warn("Unable to fully clean the mod directory:\n{0}", ex.Message);
                }
            }
            Directory.CreateDirectory(ConfigurationService.GameModPath);

            var patcherProcessor = new PatcherProcessor();
            var modsList = GetMods(EnabledMods).ToList();
            for (var i = modsList.Count - 1; i >= 0; i--)
            {
                var mod = modsList[i];
                Log.Info($"Patching using {mod.Name} from {mod.Path}");

                patcherProcessor.Patch(
                    ConfigurationService.GameDataLocation,
                    ConfigurationService.GameModPath,
                    mod.Metadata,
                    mod.Path);
            }

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
