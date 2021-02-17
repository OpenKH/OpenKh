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
    }
}
