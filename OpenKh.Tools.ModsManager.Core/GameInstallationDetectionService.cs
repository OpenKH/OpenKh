using System.Text.Json;
using System.Text.RegularExpressions;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class GameInstallationDetectionService
{
    private const string RemixFolderName = "KINGDOM HEARTS -HD 1.5+2.5 ReMIX-";
    private const string Kh3DFolderName = "KINGDOM HEARTS HD 2.8 Final Chapter Prologue";

    public GameInstallationDetectionResult Detect(string platform) =>
        platform.ToLowerInvariant() switch
        {
            "steam" => DetectSteam(),
            "egs" => DetectEpicGamesStore(),
            _ => new GameInstallationDetectionResult(
                null,
                null,
                "Automatic detection is available for Steam and Epic Games Store.")
        };

    private static GameInstallationDetectionResult DetectSteam()
    {
        var libraries = FindSteamAppsDirectories().Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var remix = FindSteamGame(libraries, RemixFolderName);
        var kh3D = FindSteamGame(libraries, Kh3DFolderName);
        return CreateResult(remix, kh3D, "Steam");
    }

    private static GameInstallationDetectionResult DetectEpicGamesStore()
    {
        if (!OperatingSystem.IsWindows())
            return new GameInstallationDetectionResult(
                null,
                null,
                "Epic Games Store automatic detection is only available on Windows.");

        var manifestsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic",
            "EpicGamesLauncher",
            "Data",
            "Manifests");
        if (!Directory.Exists(manifestsDirectory))
            return CreateResult(null, null, "Epic Games Store");

        string? remix = null;
        string? kh3D = null;
        foreach (var manifest in Directory.EnumerateFiles(manifestsDirectory, "*.item"))
        {
            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(manifest));
                var root = document.RootElement;
                if (!root.TryGetProperty("LaunchExecutable", out var executableProperty) ||
                    !root.TryGetProperty("InstallLocation", out var locationProperty))
                    continue;

                var executable = executableProperty.GetString();
                var location = locationProperty.GetString();
                if (string.IsNullOrWhiteSpace(location) || !Directory.Exists(location))
                    continue;
                if (executable?.Equals("KINGDOM HEARTS HD 1.5+2.5 ReMIX.exe", StringComparison.OrdinalIgnoreCase) == true)
                    remix = location;
                else if (executable?.Equals("KINGDOM HEARTS HD 2.8 Final Chapter Prologue.exe", StringComparison.OrdinalIgnoreCase) == true)
                    kh3D = location;
            }
            catch (JsonException)
            {
            }
            catch (IOException)
            {
            }
        }

        return CreateResult(remix, kh3D, "Epic Games Store");
    }

    private static IEnumerable<string> FindSteamAppsDirectories()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidates = new List<string>();
        if (OperatingSystem.IsWindows())
        {
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (!string.IsNullOrWhiteSpace(programFilesX86))
                candidates.Add(Path.Combine(programFilesX86, "Steam", "steamapps"));
        }
        else if (!string.IsNullOrWhiteSpace(home))
        {
            candidates.Add(Path.Combine(home, ".local", "share", "Steam", "steamapps"));
            candidates.Add(Path.Combine(home, ".steam", "steam", "steamapps"));
        }

        foreach (var steamAppsDirectory in candidates.Where(Directory.Exists))
        {
            yield return steamAppsDirectory;
            var libraryFile = Path.Combine(steamAppsDirectory, "libraryfolders.vdf");
            if (!File.Exists(libraryFile))
                continue;

            string contents;
            try
            {
                contents = File.ReadAllText(libraryFile);
            }
            catch (IOException)
            {
                continue;
            }

            foreach (Match match in Regex.Matches(contents, "\\\"path\\\"\\s*\\\"([^\\\"]+)\\\"", RegexOptions.IgnoreCase))
            {
                var libraryRoot = match.Groups[1].Value.Replace("\\\\", "\\");
                var librarySteamApps = Path.Combine(libraryRoot, "steamapps");
                if (Directory.Exists(librarySteamApps))
                    yield return librarySteamApps;
            }
        }
    }

    private static string? FindSteamGame(IEnumerable<string> steamAppsDirectories, string folderName)
    {
        foreach (var steamAppsDirectory in steamAppsDirectories)
        {
            var gameDirectory = Path.Combine(steamAppsDirectory, "common", folderName);
            if (Directory.Exists(gameDirectory) && File.Exists(Path.Combine(gameDirectory, "steam_api64.dll")))
                return gameDirectory;
        }
        return null;
    }

    private static GameInstallationDetectionResult CreateResult(string? remix, string? kh3D, string platform)
    {
        var message = (remix, kh3D) switch
        {
            (not null, not null) => $"Both game collections were found through {platform}.",
            (not null, null) => $"HD 1.5+2.5 ReMIX was found through {platform}. HD 2.8 was not found.",
            (null, not null) => $"HD 2.8 was found through {platform}. HD 1.5+2.5 ReMIX was not found.",
            _ => $"No supported game installation was found through {platform}."
        };
        return new GameInstallationDetectionResult(remix, kh3D, message);
    }
}

public sealed record GameInstallationDetectionResult(
    string? RemixDirectory,
    string? Kh3DDirectory,
    string Message);
