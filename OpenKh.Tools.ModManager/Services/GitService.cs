using LibGit2Sharp;
using Octokit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static OpenKh.Kh2.SystemData.Item;

namespace OpenKh.Tools.ModManager.Services
{
    public static class GitService
    {
        public static async Task<bool> CheckFile(string host, string author, string repository, string filePath, string? branch = "main")
        {
            try
            {
                var _fetchBaseUri = new Uri($"https://{host}");
                var _fetchRelativeUri = new Uri(_fetchBaseUri, $"{author}/{repository}");

                var _fetchRemotes = LibGit2Sharp.Repository.ListRemoteReferences(_fetchRelativeUri.ToString());
                var _fetchBranch = branch != null ? branch : _fetchRemotes.First().TargetIdentifier.Replace("refs/heads/", "");

                var _fetchGitHubAPI = $"https://raw.githubusercontent.com/{author}/{repository}/{_fetchBranch}/{filePath}";
                var _fetchForgejoAPI = $"https://{host}/{author}/{repository}/raw/{_fetchBranch}/{filePath}";

                var _fetchTargetAPI = host == "github.com" ? _fetchGitHubAPI : _fetchForgejoAPI;

                using var _makeClient = new HttpClient();
                using var _fetchResponse = await _makeClient.GetAsync(_fetchTargetAPI, HttpCompletionOption.ResponseContentRead, CancellationToken.None);

                if (_fetchResponse.StatusCode == HttpStatusCode.OK)
                    return true;

                else
                    return false;
            }

            catch (LibGit2SharpException)
            {
                return false;
            }
        }

        public static async Task<DateTime?> FetchLatestCommit(string host, string author, string repository, string? branch = "main")
        {
            try
            {
                var _fetchBaseUri = new Uri($"https://{host}");
                var _fetchRelativeUri = new Uri(_fetchBaseUri, $"{author}/{repository}");

                var _fetchBranch = branch != null ? branch : LibGit2Sharp.Repository.ListRemoteReferences(_fetchRelativeUri.ToString()).First().TargetIdentifier.Replace("refs/heads/", "");

                var _fetchGitHubAPI = $"https://api.github.com/repos/{author}/{repository}/commits?sha={_fetchBranch}";
                var _fetchForgejoAPI = $"https://{host}/api/v1/repos/{author}/{repository}/commits?sha={_fetchBranch}";

                var _fetchTargetAPI = host == "github.com" ? _fetchGitHubAPI : _fetchForgejoAPI;

                using var _makeClient = new HttpClient();

                if (_fetchTargetAPI == _fetchGitHubAPI)
                {
                    _makeClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("openkh.modmanager.gitfetcher", "no-ver"));
                    _makeClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                }

                using var _fetchResponse = await _makeClient.GetAsync(_fetchTargetAPI, HttpCompletionOption.ResponseContentRead, CancellationToken.None);

                if (_fetchResponse.StatusCode == HttpStatusCode.OK)
                {
                    var _fetchJsonRAW = await _fetchResponse.Content.ReadAsStringAsync();
                    var _fetchJson = JsonNode.Parse(_fetchJsonRAW) as JsonArray;

                    var _fetchTimeRegex = new Regex("(\\d*-\\d*-\\d*)T(\\d*:\\d*:\\d*)", RegexOptions.None, TimeSpan.FromMilliseconds(100));

                    var _fetchLatestTimestamp = _fetchTargetAPI == _fetchGitHubAPI ? _fetchTimeRegex.Match(_fetchJson.First()["commit"]["author"]["date"].ToString()) 
                                                                                   : _fetchTimeRegex.Match(_fetchJson.First()["created"].ToString());

                    var _fetchDateOnly = DateOnly.Parse(_fetchLatestTimestamp.Groups[1].Value);
                    var _fetchTimeOnly = TimeOnly.Parse(_fetchLatestTimestamp.Groups[2].Value);

                    var _fetchDateTime = new DateTime(_fetchDateOnly, _fetchTimeOnly);

                    return _fetchDateTime;
                }

                else
                    return null;
            }

            catch (LibGit2SharpException)
            {
                return null;
            }
        }

        public static async Task<byte[]> FetchZipball(string host, string author, string repository, CancellationToken cancelToken, string? branch = "main", Func<long, long, bool, bool>? progressCallback = null)
        {
            var _fetchBaseUri = new Uri($"https://{host}");
            var _fetchRelativeUri = new Uri(_fetchBaseUri, $"{author}/{repository}");

            var _fetchBranch = branch;

            if (_fetchBranch == null)
            {
                var _fetchRemotes = LibGit2Sharp.Repository.ListRemoteReferences(_fetchRelativeUri.ToString());
                _fetchBranch = branch != null ? branch : _fetchRemotes.First().TargetIdentifier.Replace("refs/heads/", "");
            }

            var _fetchForgejoAPI = $"https://{host}/{author}/{repository}/archive/{_fetchBranch}.zip";

            using var _makeClient = new HttpClient();

            _makeClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("openkh.modmanager.gitfetcher", "no-ver"));
            _makeClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var _fetchResponse = await _makeClient.GetAsync(_fetchForgejoAPI, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

            if (_fetchResponse.StatusCode == HttpStatusCode.OK)
            {
                var _fetchLength = _fetchResponse.Content.Headers.ContentLength ?? -1;

                var _fetchMemory = new byte[_fetchLength];
                var _fetchChunkSize = (long)Math.Floor(_fetchLength / 64.0);

                var _fetchTasks = new Task[64];
                long _totalReadProgress = 0;

                for (int i = 0; i < 64; i++)
                {
                    long _chunkStart = i * _fetchChunkSize;
                    long _chunkEnd = (i == 64 - 1) ? _fetchLength - 1 : _chunkStart + _fetchChunkSize - 1;

                    _fetchTasks[i] = Task.Run(async () =>
                    {
                        try
                        {
                            using (var _makeClient = new HttpClient())
                            {
                                using (var _fetchRequest = new HttpRequestMessage(HttpMethod.Get, _fetchForgejoAPI))
                                {
                                    _fetchRequest.Headers.Range = new RangeHeaderValue(_chunkStart, _chunkEnd);

                                    using var _chunkResponse = await _makeClient.SendAsync(_fetchRequest, HttpCompletionOption.ResponseHeadersRead, cancelToken);

                                    if (_chunkResponse.StatusCode == HttpStatusCode.PartialContent)
                                    {
                                        using (var _fetchContent = await _chunkResponse.Content.ReadAsStreamAsync(cancelToken))
                                        {
                                            using (var _fetchStream = new MemoryStream(_fetchMemory))
                                            {
                                                int _readProgress = 0;
                                                byte[] _fetchBuffer = new byte[262144];

                                                _fetchStream.Position = _chunkStart;

                                                while ((_readProgress = await _fetchContent.ReadAsync(_fetchBuffer, cancelToken)) != 0x00)
                                                {
                                                    await _fetchStream.WriteAsync(_fetchBuffer, 0, _readProgress, cancelToken);

                                                    _totalReadProgress += _readProgress;

                                                    if (progressCallback != null)
                                                    {
                                                        var _fetchProgress = progressCallback(_totalReadProgress, _fetchLength, false);

                                                        if (!_fetchProgress)
                                                            break;
                                                    }

                                                    if (cancelToken.IsCancellationRequested)
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        catch (TaskCanceledException) { }

                    }, cancelToken);
                }

                await Task.WhenAll(_fetchTasks);

                if (cancelToken.IsCancellationRequested)
                    return null;

                return _fetchMemory;
            }

            else
                return null;
        }

        public static async Task<IDictionary<string, byte[]>?> FetchSource(string author, string repository, CancellationToken cancelToken, string? branch = "main", Func<long, long, bool, bool>? progressCallback = null)
        {
            try
            {
                var _fetchBaseUri = new Uri($"https://github.com");
                var _fetchRelativeUri = new Uri(_fetchBaseUri, $"{author}/{repository}");

                var _fetchBranch = branch;

                if (_fetchBranch == null)
                {
                    var _fetchRemotes = LibGit2Sharp.Repository.ListRemoteReferences(_fetchRelativeUri.ToString());
                    _fetchBranch = branch != null ? branch : _fetchRemotes.First().TargetIdentifier.Replace("refs/heads/", "");
                }

                var _productHeader = new Octokit.ProductHeaderValue("openkh.modmanager.gitfetcher");
                var _octoClient = new GitHubClient(_productHeader);

                var _fetchBranchTree = await _octoClient.Git.Tree.GetRecursive(author, repository, _fetchBranch);
                var _fetchFileBlob = _fetchBranchTree.Tree.Where(t => t.Type == TreeType.Blob);

                long _fetchTotalSize = _fetchFileBlob.Sum(t => t.Size);
                long _fetchTotalRead = 0x00;

                var _fetchFileCount = _fetchFileBlob.Count();
                var _fetchFileRead = 0x00;

                var _fetchChunkSize = (int)Math.Floor(_fetchFileCount / 16.0);

                var _fetchTasks = new Task[0x10];
                var _fetchFileDict = new ConcurrentDictionary<string, byte[]>();

                for (int i = 0x00; i < 0x10; i++)
                {
                    var _startIndex = i * _fetchChunkSize;
                    var _endIndex = (i == 0x10 - 1) ? _startIndex + (_fetchFileCount - (i * _fetchChunkSize)) : _startIndex + _fetchChunkSize;

                    _fetchTasks[i] = Task.Run(async () =>
                    {
                        try
                        {
                            for (int z = _startIndex; z < _endIndex; z++)
                            {
                                var _fetchFile = _fetchFileBlob.ElementAt(z);

                                var _rawGitHubURL = $"https://raw.githubusercontent.com/{author}/{repository}/{_fetchBranch}/{_fetchFile.Path}";

                                using var _makeClient = new HttpClient();
                                using var _fetchResponse = await _makeClient.GetAsync(_rawGitHubURL, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);

                                if (_fetchResponse.StatusCode == HttpStatusCode.OK)
                                {
                                    var _fetchContent = await _fetchResponse.Content.ReadAsByteArrayAsync(cancelToken);
                                    _fetchFileDict.TryAdd(_fetchFile.Path, _fetchContent);
                                }

                                _fetchTotalRead += _fetchFile.Size;
                                _fetchFileRead++;

                                if (progressCallback != null)
                                {
                                    var _fetchResult = progressCallback(_fetchTotalRead, _fetchTotalSize, false);

                                    if (!_fetchResult)
                                        break;
                                }

                                if (cancelToken.IsCancellationRequested)
                                    break;
                            }
                        }

                        catch (TaskCanceledException) { }
                    }, cancelToken);
                }

                await Task.WhenAll(_fetchTasks);

                if (cancelToken.IsCancellationRequested)
                    return null;

                return _fetchFileDict;
            }

            catch (LibGit2SharpException)
            {
                return null;
            }
        }
    }
}
