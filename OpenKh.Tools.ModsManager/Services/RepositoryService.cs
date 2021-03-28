using LibGit2Sharp;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public class RepositoryNotFoundException : Exception
    {
        public RepositoryNotFoundException(string repositoryName) :
            base($"There is no repository under the name of '{repositoryName}' in GitHub. Please be aware that the name is case sensitive.")
        {
            RepositoryName = repositoryName;
        }

        public string RepositoryName { get; }
    }

    public static class RepositoryService
    {
        private class ReposResponse
        {
            [JsonProperty("default_branch")]
            public string DefaultBranch { get; set; }
        }

        public static async Task<string> GetMainBranchFromRepository(string repositoryName)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{repositoryName}");
            request.Headers.UserAgent.TryParseAdd("OpenKH Mods Manager/1.0");
            using var response = await client.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new RepositoryNotFoundException(repositoryName);

            return JsonConvert.DeserializeObject<ReposResponse>(await response.Content.ReadAsStringAsync())?.DefaultBranch;
        }

        public static Task<bool> IsFileExists(string repoName, string branch, string filePath) =>
            IsFileExists($"https://raw.githubusercontent.com/{repoName}/{branch}/{filePath}");

        public static async Task<bool> IsFileExists(string url)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public static Task<int> FetchUpdate(string path) => Task.Run(() =>
        {
            if (!Repository.IsValid(path))
                return -1;

            using var repository = new Repository(path);
            if (repository.Info.IsHeadDetached)
                return -1;

            Fetch(repository);
            return repository.Head.TrackingDetails.BehindBy ?? 0;
        });

        public static Task FetchAndResetUponOrigin(string path,
            Action<string> progressOutput = null,
            Action<float> progressNumber = null) => Task.Run(() =>
        {
            if (!Repository.IsValid(path))
                return;

            using var repository = new Repository(path);
            if (repository.Info.IsHeadDetached)
                return;

            Fetch(repository);
            repository.Reset(ResetMode.Hard, repository.Head.TrackedBranch.Tip, new CheckoutOptions
            {
                CheckoutModifiers = CheckoutModifiers.Force,
                CheckoutNotifyFlags = CheckoutNotifyFlags.None,
                OnCheckoutProgress = (string path, int completedSteps, int totalSteps) =>
                {
                    progressOutput?.Invoke(path);
                    var nProgress = (float)completedSteps / totalSteps;
                    progressNumber?.Invoke(nProgress);
                }
            });
        });

        private static void Fetch(Repository repository)
        {
            if (string.IsNullOrEmpty(repository.Head.RemoteName))
                return;

            try
            {
                repository.Network.Fetch(repository.Head.RemoteName, new string[0]);
            }
            catch
            {

            }
        }
    }
}
