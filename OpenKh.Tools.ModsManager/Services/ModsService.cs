using LibGit2Sharp;
using OpenKh.Common;
using OpenKh.Patcher;
using OpenKh.Tools.ModsManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public static class ModsService
    {
        public static IEnumerable<string> Mods
        {
            get
            {
                var modsPath = ConfigurationService.ModCollectionPath;
                var startPath = modsPath.Length;
                var lastChar = modsPath[startPath - 1];
                if (lastChar != '/' && lastChar != '\\')
                    startPath++;

                foreach (var dir in Directory.GetDirectories(modsPath))
                {
                    var authorName = Path.GetFileName(dir);
                    foreach (var subdir in Directory.GetDirectories(dir))
                    {
                        var repoName = Path.GetFileName(subdir);
                        if (File.Exists(Path.Combine(subdir, "mod.yml")))
                            yield return $"{authorName}/{repoName}";
                    }

                    if (File.Exists(Path.Combine(dir, "mod.yml")))
                        yield return authorName;
                }
            }
        }

        public static IEnumerable<string> EnabledMods
        {
            get
            {
                var enabledMods = ConfigurationService.EnabledMods;
                foreach (var mod in Mods)
                {
                    if (enabledMods.Contains(mod))
                        yield return mod;
                }
            }
        }

        public static async Task InstallModFromGithub(
            string repositoryName,
            Action<string> progressOutput = null,
            Action<float> progressNumber = null)
        {
            var branchName = "main";
            progressOutput?.Invoke($"Fetching file mod.yml from {branchName}");
            var isValidMod = await RepositoryService.IsFileExists(repositoryName, branchName, "mod.yml");
            if (!isValidMod)
            {
                progressOutput?.Invoke($"mod.yml not found, fetching default branch name");
                branchName = await RepositoryService.GetMainBranchFromRepository(repositoryName);
                if (branchName == null)
                    throw new Exception($"Repository '{repositoryName}' not found.");

                progressOutput?.Invoke($"Fetching file mod.yml from {branchName}");
                isValidMod = await RepositoryService.IsFileExists(repositoryName, branchName, "mod.yml");
            }

            if (!isValidMod)
                throw new Exception($"Repository '{repositoryName}' does not contain a valid OpenKH compatible mod");

            var modPath = GetModPath(repositoryName);
            Directory.CreateDirectory(modPath);

            progressOutput?.Invoke($"Mod found, initializing clonation process");
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

        public static IEnumerable<ModModel> GetMods(IEnumerable<string> repositoryNames)
        {
            var enabledMods = ConfigurationService.EnabledMods;
            foreach (var repositoryName in repositoryNames)
            {
                var modPath = GetModPath(repositoryName);
                yield return new ModModel
                {
                    Name = repositoryName,
                    Path = modPath,
                    Metadata = File.OpenRead(Path.Combine(modPath, "mod.yml")).Using(Metadata.Read),
                    IsEnabled = enabledMods.Contains(repositoryName)
                };
            }
        }

        public static Task RunPacherAsync() => Task.Run(() =>
        {
            var patcherProcessor = new PatcherProcessor();
            var modsList = GetMods(EnabledMods).ToList();
            for (var i = modsList.Count - 1; i >= 0; i--)
            {
                var mod = modsList[i];
                patcherProcessor.Patch(
                    ConfigurationService.GameDataLocation,
                    ConfigurationService.GameModPath,
                    mod.Metadata,
                    mod.Path);
            }
        });

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
