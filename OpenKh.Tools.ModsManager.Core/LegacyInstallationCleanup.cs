using System.Diagnostics;
using System.Text;

namespace OpenKh.Tools.ModsManager.Core;

public static class LegacyInstallationCleanup
{
    private static readonly string[] PreviousApplicationDirectories =
    [
        "AdvancedTools",
        Path.Combine("Apps", "ModManager"),
    ];

    private static readonly string[] FallbackLegacyResourceDirectories =
    [
        "cs-CZ", "de", "es", "fr", "hu", "it", "ja-JP", "pt-BR", "resources",
        "ro", "ru", "runtimes", "sv", "zh-Hans",
    ];

    public static void Schedule(string installationDirectory)
    {
        if (!IsOrganizedInstallation(installationDirectory))
            return;

        var files = GetLegacyApplicationFiles(installationDirectory).Where(File.Exists).ToArray();
        var directories = GetLegacyResourceDirectories(installationDirectory)
            .Select(name => Path.Combine(installationDirectory, name))
            .Concat(PreviousApplicationDirectories.Select(name => Path.Combine(installationDirectory, name)))
            .Where(Directory.Exists)
            .ToArray();
        if (files.Length == 0 && directories.Length == 0)
            return;

        if (!OperatingSystem.IsWindows())
        {
            foreach (var file in files)
                TryDeleteFile(file);
            foreach (var directory in directories)
                TryDeleteDirectory(directory);
            return;
        }

        // The old root DLLs may still be loaded by the process that performed the update.
        var batchPath = Path.Combine(Path.GetTempPath(), $"openkh-migrate-{Guid.NewGuid():N}.bat");
        var batch = new StringBuilder()
            .AppendLine("@echo off")
            .AppendLine("chcp 65001 > nul")
            .AppendLine(":wait_for_mod_manager")
            .AppendLine($"tasklist /fi \"PID eq {Environment.ProcessId}\" 2>nul | find \"{Environment.ProcessId}\" >nul")
            .AppendLine("if not errorlevel 1 (")
            .AppendLine("    timeout /t 1 /nobreak >nul")
            .AppendLine("    goto wait_for_mod_manager")
            .AppendLine(")");
        foreach (var file in files)
        {
            var path = EscapeBatchPath(file);
            batch.AppendLine($"attrib -h -r {path} 2>nul");
            batch.AppendLine($"del /f /q {path} 2>nul");
        }
        foreach (var directory in directories)
            batch.AppendLine($"rmdir /s /q {EscapeBatchPath(directory)} 2>nul");
        batch.AppendLine("del /f /q \"%~f0\"");
        File.WriteAllText(batchPath, batch.ToString(), new UTF8Encoding(false));

        Process.Start(new ProcessStartInfo
        {
            FileName = batchPath,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        });
    }

    public static IReadOnlyList<string> GetLegacyPaths(string installationDirectory) =>
        GetLegacyApplicationFiles(installationDirectory)
            .Concat(GetLegacyResourceDirectories(installationDirectory)
                .Select(name => Path.Combine(installationDirectory, name)))
            .Concat(PreviousApplicationDirectories.Select(name => Path.Combine(installationDirectory, name)))
            .Where(path => File.Exists(path) || Directory.Exists(path))
            .ToArray();

    private static bool IsOrganizedInstallation(string installationDirectory) =>
        (File.Exists(Path.Combine(installationDirectory, "OpenKh.Launcher.exe")) ||
         File.Exists(Path.Combine(installationDirectory, "OpenKh.Launcher"))) &&
        (File.Exists(Path.Combine(installationDirectory, "Apps", "OpenKh.Tools.ModsManager.exe")) ||
         File.Exists(Path.Combine(installationDirectory, "Apps", "OpenKh.Tools.ModsManager")));

    private static IEnumerable<string> GetLegacyApplicationFiles(string installationDirectory)
    {
        var manifestPath = Path.Combine(installationDirectory, "Apps", "legacy-release-files.txt");
        if (!File.Exists(manifestPath))
        {
            return Directory.Exists(installationDirectory)
                ? Directory.EnumerateFiles(installationDirectory, "*", SearchOption.TopDirectoryOnly)
                    .Where(IsLegacyApplicationFile)
                : [];
        }

        var names = File.ReadAllLines(manifestPath)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return Directory.EnumerateFiles(installationDirectory, "*", SearchOption.TopDirectoryOnly)
            .Where(path => names.Contains(Path.GetFileName(path)) ||
                Path.GetFileName(path).StartsWith("OpenKh.Tools.ModsManager.", StringComparison.OrdinalIgnoreCase))
            .Where(path => !IsPreservedRootExecutable(path));
    }

    private static IEnumerable<string> GetLegacyResourceDirectories(string installationDirectory)
    {
        var manifestPath = Path.Combine(installationDirectory, "Apps", "legacy-release-directories.txt");
        return File.Exists(manifestPath)
            ? File.ReadAllLines(manifestPath).Where(name => !string.IsNullOrWhiteSpace(name))
            : FallbackLegacyResourceDirectories;
    }

    private static bool IsLegacyApplicationFile(string path)
    {
        if (IsPreservedRootExecutable(path))
            return false;

        var name = Path.GetFileName(path);
        if (Path.GetExtension(name).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            return true;
        if (!name.StartsWith("OpenKh.", StringComparison.OrdinalIgnoreCase))
            return false;
        return name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase) ||
               name.EndsWith(".config", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPreservedRootExecutable(string path)
    {
        var name = Path.GetFileName(path);
        return name.Equals("OpenKh.Launcher.exe", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("OpenKh.Launcher", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("OpenKh.Tools.ModsManager.exe", StringComparison.OrdinalIgnoreCase);
    }

    private static string EscapeBatchPath(string path) => $"\"{path.Replace("\"", "\"\"")}\"";

    private static void TryDeleteFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }
    }
}
