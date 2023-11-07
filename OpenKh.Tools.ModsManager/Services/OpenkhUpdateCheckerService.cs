using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public class OpenkhUpdateCheckerService
    {
        public record CheckResult(bool HasUpdate, string CurrentVersion, string NewVersion, string DownloadZipUrl);

        private static readonly Regex _validTag = new Regex("^release2-(?<build>\\d+)$");

        public async Task<CheckResult> CheckAsync(CancellationToken cancellation)
        {
            var gitClient = new GitHubClient(new ProductHeaderValue("OpenKh.Tools.ModsManager"));
            var releases = await gitClient.Repository.Release.GetAll(
                owner: "OpenKh",
                name: "OpenKh",
                options: new ApiOptions
                {
                    PageCount = 1,
                    PageSize = 10,
                    StartPage = 1
                }
            );
            var latestAssets = releases
                .OrderByDescending(release => release.CreatedAt)
                .Where(release => _validTag.IsMatch(release.TagName))
                .SelectMany(
                    release => release.Assets
                        .Where(asset => asset.Name == "openkh.zip" && asset.State == "uploaded")
                        .Select(asset => (Release: release, Asset: asset))
                )
                .Take(1)
                .ToArray();

            if (latestAssets.Any())
            {
                var latestAsset = latestAssets.First();

                var remoteReleaseTag = latestAsset.Release.TagName;

                var localReleaseTagFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "openkh-release");
                var localReleaseTag = File.Exists(localReleaseTagFile)
                    ? File.ReadAllLines(localReleaseTagFile).First()
                    : "(Unknown version)";

                return new CheckResult(
                    HasUpdate: localReleaseTag != remoteReleaseTag,
                    CurrentVersion: localReleaseTag,
                    NewVersion: remoteReleaseTag,
                    DownloadZipUrl: latestAsset.Asset.BrowserDownloadUrl
                );
            }

            return new CheckResult(
                HasUpdate: false,
                CurrentVersion: "",
                NewVersion: "",
                DownloadZipUrl: ""
            );
        }
    }
}
