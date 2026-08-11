using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OpenKh.Tools.ModsManager.Core;

public sealed record OpenKhUpdateCheckResult(
    bool HasUpdate,
    string CurrentVersion,
    string LatestVersion,
    string DownloadUrl);

public sealed class OpenKhUpdateCheckerService
{
    private static readonly Regex ReleaseTagPattern = new(
        "^release2-(?<build>\\d+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HttpClient SharedClient = CreateClient();
    private readonly ModManagerConfigurationService _configuration;
    private readonly HttpClient _httpClient;

    public OpenKhUpdateCheckerService(
        ModManagerConfigurationService configuration,
        HttpClient? httpClient = null)
    {
        _configuration = configuration;
        _httpClient = httpClient ?? SharedClient;
    }

    public async Task<OpenKhUpdateCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            "https://api.github.com/repos/OpenKH/OpenKh/releases?per_page=20",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var releases = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
        var latest = FindLatestRelease(releases.RootElement);
        var currentVersion = ReadCurrentVersion();

        return latest is null
            ? new OpenKhUpdateCheckResult(false, currentVersion, string.Empty, string.Empty)
            : new OpenKhUpdateCheckResult(
                !currentVersion.Equals(latest.Value.Tag, StringComparison.OrdinalIgnoreCase),
                currentVersion,
                latest.Value.Tag,
                latest.Value.DownloadUrl);
    }

    private string ReadCurrentVersion()
    {
        var versionFile = Path.Combine(_configuration.InstallationDirectory, "openkh-release");
        if (!File.Exists(versionFile))
            return "Unknown version";

        return File.ReadLines(versionFile).FirstOrDefault()?.Trim() is { Length: > 0 } version
            ? version
            : "Unknown version";
    }

    private static ReleaseAsset? FindLatestRelease(JsonElement releases)
    {
        ReleaseAsset? latest = null;
        foreach (var release in releases.EnumerateArray())
        {
            var tag = release.GetProperty("tag_name").GetString() ?? string.Empty;
            var match = ReleaseTagPattern.Match(tag);
            if (!match.Success || !int.TryParse(match.Groups["build"].Value, out var build))
                continue;

            foreach (var asset in release.GetProperty("assets").EnumerateArray())
            {
                if (!string.Equals(asset.GetProperty("name").GetString(), "openkh.zip", StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(asset.GetProperty("state").GetString(), "uploaded", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var downloadUrl = asset.GetProperty("browser_download_url").GetString();
                if (string.IsNullOrWhiteSpace(downloadUrl))
                    continue;

                if (latest is null || build > latest.Value.Build)
                    latest = new ReleaseAsset(build, tag, downloadUrl);
            }
        }

        return latest;
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("OpenKh-ModManager", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }

    private readonly record struct ReleaseAsset(int Build, string Tag, string DownloadUrl);
}
