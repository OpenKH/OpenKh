using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class LuaBackendService
{
    private static readonly HttpClient HttpClient = CreateHttpClient();
    private readonly ModManagerConfigurationService _configuration;

    public LuaBackendService(ModManagerConfigurationService configuration)
    {
        _configuration = configuration;
    }

    public bool IsInstalled(string? gameDirectory) =>
        !string.IsNullOrWhiteSpace(gameDirectory) &&
        File.Exists(Path.Combine(gameDirectory, "LuaBackend.dll")) &&
        File.Exists(Path.Combine(gameDirectory, "LuaBackend.toml"));

    public async Task InstallAsync(
        string gameDirectory,
        IReadOnlyCollection<GameInfo> games,
        bool useSteamDocuments,
        IProgress<ModOperationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ValidateGameDirectory(gameDirectory);
        progress?.Report(new ModOperationProgress("Finding the latest Lua Backend release"));
        var downloadUrl = await GetLatestDownloadUrlAsync(cancellationToken);
        var temporaryRoot = Path.Combine(Path.GetTempPath(), $"openkh-lua-backend-{Guid.NewGuid():N}");
        var archivePath = Path.Combine(temporaryRoot, "LuaBackend.zip");
        var extractionPath = Path.Combine(temporaryRoot, "extracted");

        Directory.CreateDirectory(temporaryRoot);
        try
        {
            progress?.Report(new ModOperationProgress("Downloading Lua Backend", 0));
            using (var response = await HttpClient.GetAsync(
                       downloadUrl,
                       HttpCompletionOption.ResponseHeadersRead,
                       cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using var output = File.Create(archivePath);
                await CopyWithProgressAsync(
                    input,
                    output,
                    response.Content.Headers.ContentLength,
                    progress,
                    cancellationToken);
            }

            Directory.CreateDirectory(extractionPath);
            ZipFile.ExtractToDirectory(archivePath, extractionPath, true);
            var sourceDll = Directory.EnumerateFiles(extractionPath, "DBGHELP.dll", SearchOption.AllDirectories)
                .FirstOrDefault() ?? Directory.EnumerateFiles(extractionPath, "LuaBackend.dll", SearchOption.AllDirectories)
                .FirstOrDefault();
            var sourceConfiguration = Directory.EnumerateFiles(extractionPath, "LuaBackend.toml", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (sourceDll is null || sourceConfiguration is null)
                throw new InvalidDataException("The Lua Backend release does not contain DBGHELP.dll and LuaBackend.toml.");

            File.Copy(sourceDll, Path.Combine(gameDirectory, "LuaBackend.dll"), true);
            File.Copy(sourceConfiguration, Path.Combine(gameDirectory, "LuaBackend.toml"), true);
            Configure(gameDirectory, games, useSteamDocuments);
            progress?.Report(new ModOperationProgress("Lua Backend was installed and configured", 1));
        }
        finally
        {
            if (Directory.Exists(temporaryRoot))
                Directory.Delete(temporaryRoot, true);
        }
    }

    public void Configure(
        string gameDirectory,
        IReadOnlyCollection<GameInfo> games,
        bool useSteamDocuments)
    {
        ValidateGameDirectory(gameDirectory);
        var configurationPath = Path.Combine(gameDirectory, "LuaBackend.toml");
        if (!File.Exists(configurationPath))
            throw new FileNotFoundException("LuaBackend.toml was not found in the selected game directory.", configurationPath);

        var text = File.ReadAllText(configurationPath).Replace("\\", "/");
        foreach (var game in games)
        {
            var sectionName = game.Id.Equals("Recom", StringComparison.OrdinalIgnoreCase)
                ? "recom"
                : game.Id.ToLowerInvariant();
            var scriptPath = Path.Combine(_configuration.GetGameModOutputDirectory(game), "scripts")
                .Replace("\\", "/");
            text = SetScriptsPath(text, sectionName, scriptPath);
        }

        if (useSteamDocuments)
            text = SelectSteamDocumentsPaths(text);

        File.WriteAllText(configurationPath, text);
    }

    public void Remove(string gameDirectory)
    {
        ValidateGameDirectory(gameDirectory);
        File.Delete(Path.Combine(gameDirectory, "LuaBackend.dll"));
        File.Delete(Path.Combine(gameDirectory, "LuaBackend.toml"));
    }

    private static string SetScriptsPath(string text, string sectionName, string scriptPath)
    {
        var newLine = DetectNewLine(text);
        var escapedSectionName = Regex.Escape(sectionName);
        var joinedHeaderPattern = $@"(?m)^(?<header>[^\S\r\n]*\[{escapedSectionName}\][^\S\r\n]*)(?=scripts[^\S\r\n]*=)";
        text = new Regex(joinedHeaderPattern).Replace(
            text,
            match => match.Groups["header"].Value.TrimEnd(' ', '\t') + newLine,
            1);

        var sectionPattern = $@"(?ms)(?<header>^[^\S\r\n]*\[{escapedSectionName}\][^\S\r\n]*(?:\r\n|\n|\r))(?<body>.*?)(?=^[^\S\r\n]*\[|\z)";
        return new Regex(sectionPattern).Replace(text, match =>
        {
            var body = match.Groups["body"].Value;
            var scriptsLine = $"scripts = [{{ path = \"scripts/{sectionName}/\", relative = true }}, {{ path = \"{scriptPath}\", relative = false }}]";
            body = ReplaceScriptsAssignment(body, scriptsLine, newLine);
            return match.Groups["header"].Value + body;
        }, 1);
    }

    private static string ReplaceScriptsAssignment(string body, string scriptsLine, string newLine)
    {
        const string scriptsPattern = @"(?m)^(?<indent>[^\S\r\n]*)scripts[^\S\r\n]*=";
        var match = Regex.Match(body, scriptsPattern);
        if (!match.Success)
            return scriptsLine + newLine + body;

        var assignmentEnd = FindAssignmentEnd(body, match.Index + match.Length);
        return body[..match.Index] +
            match.Groups["indent"].Value +
            scriptsLine +
            body[assignmentEnd..];
    }

    private static int FindAssignmentEnd(string text, int valueStart)
    {
        var arrayStart = text.IndexOf('[', valueStart);
        var lineEnd = IndexOfLineEnd(text, valueStart);
        if (arrayStart < 0 || arrayStart > lineEnd)
            return lineEnd;

        var depth = 0;
        var inString = false;
        var quote = '\0';
        var escaped = false;
        for (var index = arrayStart; index < text.Length; index++)
        {
            var character = text[index];
            if (inString)
            {
                if (quote == '"' && character == '\\' && !escaped)
                {
                    escaped = true;
                    continue;
                }

                if (character == quote && !escaped)
                    inString = false;
                escaped = false;
                continue;
            }

            if (character is '"' or '\'')
            {
                inString = true;
                quote = character;
            }
            else if (character == '[')
            {
                depth++;
            }
            else if (character == ']' && --depth == 0)
            {
                return index + 1;
            }
        }

        return lineEnd;
    }

    private static int IndexOfLineEnd(string text, int startIndex)
    {
        for (var index = startIndex; index < text.Length; index++)
        {
            if (text[index] is '\r' or '\n')
                return index;
        }

        return text.Length;
    }

    private static string DetectNewLine(string text)
    {
        var lineFeed = text.IndexOf('\n');
        if (lineFeed >= 0)
            return lineFeed > 0 && text[lineFeed - 1] == '\r' ? "\r\n" : "\n";
        return text.Contains('\r') ? "\r" : Environment.NewLine;
    }

    private static string SelectSteamDocumentsPaths(string text)
    {
        var lines = text.Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            if (!lines[index].Contains("game_docs", StringComparison.Ordinal))
                continue;

            if (lines[index].Contains("My Games/", StringComparison.Ordinal))
                lines[index] = Regex.Replace(lines[index], @"^(\s*)#\s?", "$1");
            else if (!lines[index].TrimStart().StartsWith('#'))
                lines[index] = Regex.Replace(lines[index], @"^(\s*)", "$1# ");
        }

        return string.Join('\n', lines);
    }

    private static async Task<string> GetLatestDownloadUrlAsync(CancellationToken cancellationToken)
    {
        using var response = await HttpClient.GetAsync(
            "https://api.github.com/repos/Sirius902/LuaBackend/releases/latest",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var asset = document.RootElement.GetProperty("assets")
            .EnumerateArray()
            .Select(element => new
            {
                Name = element.GetProperty("name").GetString(),
                Url = element.GetProperty("browser_download_url").GetString()
            })
            .FirstOrDefault(item =>
                item.Name?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true &&
                !string.IsNullOrWhiteSpace(item.Url));
        return asset?.Url ?? throw new InvalidDataException("The latest Lua Backend release has no ZIP asset.");
    }

    private static async Task CopyWithProgressAsync(
        Stream input,
        Stream output,
        long? contentLength,
        IProgress<ModOperationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[81920];
        long copied = 0;
        while (true)
        {
            var read = await input.ReadAsync(buffer, cancellationToken);
            if (read == 0)
                break;
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            copied += read;
            progress?.Report(new ModOperationProgress(
                "Downloading Lua Backend",
                contentLength is > 0 ? (double)copied / contentLength.Value : null));
        }
    }

    private static void ValidateGameDirectory(string? gameDirectory)
    {
        if (string.IsNullOrWhiteSpace(gameDirectory) || !Directory.Exists(gameDirectory))
            throw new DirectoryNotFoundException("Configure a valid PC game collection directory first.");
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("OpenKh-ModManager", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }
}
