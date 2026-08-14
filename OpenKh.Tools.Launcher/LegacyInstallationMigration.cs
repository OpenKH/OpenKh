using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace OpenKh.Tools.Launcher;

internal static class LegacyInstallationMigration
{
    private const string LauncherExecutableName = "OpenKh.Launcher.exe";
    private const string CompatibilityExecutableName = "OpenKh.Tools.ModsManager.exe";

    private static readonly string[] PreviousApplicationDirectories =
    {
        "AdvancedTools",
        Path.Combine("Apps", "ModManager"),
    };

    private static readonly string[] FallbackLegacyResourceDirectories =
    {
        "cs-CZ",
        "de",
        "es",
        "fr",
        "hu",
        "it",
        "ja-JP",
        "pt-BR",
        "resources",
        "ro",
        "ru",
        "runtimes",
        "sv",
        "zh-Hans",
    };

    public static bool TryStartModManager()
    {
        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath)
            || !Path.GetFileName(processPath).Equals(CompatibilityExecutableName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var installationDirectory = AppContext.BaseDirectory.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        );
        var modManagerPath = Path.Combine(installationDirectory, "Apps", CompatibilityExecutableName);
        if (!File.Exists(modManagerPath))
        {
            modManagerPath = Path.Combine(
                installationDirectory,
                "Apps",
                "ModManager",
                CompatibilityExecutableName
            );
        }

        if (!File.Exists(modManagerPath))
        {
            MessageBox.Show(
                "The updated Mod Manager could not be found. Extract the latest OpenKH release again.",
                "OpenKH Update",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return true;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = modManagerPath,
                WorkingDirectory = Path.GetDirectoryName(modManagerPath),
                UseShellExecute = true,
            };

            foreach (var argument in Environment.GetCommandLineArgs().Skip(1))
                startInfo.ArgumentList.Add(argument);

            Process.Start(startInfo);

            ScheduleCleanupIfNeeded(installationDirectory);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"OpenKH could not complete the update.\n\n{exception.Message}",
                "OpenKH Update",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        return true;
    }

    public static void ScheduleCleanupIfNeeded()
    {
        var installationDirectory = AppContext.BaseDirectory.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        );
        ScheduleCleanupIfNeeded(installationDirectory);
    }

    private static void ScheduleCleanupIfNeeded(string installationDirectory)
    {
        if (!IsOrganizedInstallation(installationDirectory))
            return;

        var legacyFiles = GetLegacyApplicationFiles(installationDirectory)
            .Where(File.Exists)
            .ToArray();
        var legacyDirectories = GetLegacyResourceDirectories(installationDirectory)
            .Select(directoryName => Path.Combine(installationDirectory, directoryName))
            .Concat(PreviousApplicationDirectories.Select(directoryName =>
                Path.Combine(installationDirectory, directoryName)))
            .Where(Directory.Exists)
            .ToArray();

        if (legacyFiles.Length == 0 && legacyDirectories.Length == 0)
            return;

        var batchPath = Path.Combine(Path.GetTempPath(), $"openkh-migrate-{Guid.NewGuid():N}.bat");
        var batch = new StringBuilder();

        batch.AppendLine("@echo off");
        batch.AppendLine("chcp 65001 > nul");
        batch.AppendLine(":wait_for_launcher");
        batch.AppendLine($"tasklist /fi \"PID eq {Environment.ProcessId}\" 2>nul | find \"{Environment.ProcessId}\" >nul");
        batch.AppendLine("if not errorlevel 1 (");
        batch.AppendLine("    timeout /t 1 /nobreak >nul");
        batch.AppendLine("    goto wait_for_launcher");
        batch.AppendLine(")");

        foreach (var filePath in legacyFiles)
        {
            var escapedPath = EscapeBatchPath(filePath);
            batch.AppendLine($"attrib -h -r {escapedPath} 2>nul");
            batch.AppendLine($"del /f /q {escapedPath} 2>nul");
        }

        foreach (var directoryPath in legacyDirectories)
        {
            batch.AppendLine($"rmdir /s /q {EscapeBatchPath(directoryPath)} 2>nul");
        }

        batch.AppendLine("del /f /q \"%~f0\"");
        File.WriteAllText(batchPath, batch.ToString(), new UTF8Encoding(false));

        Process.Start(new ProcessStartInfo
        {
            FileName = batchPath,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        });
    }

    private static bool IsOrganizedInstallation(string installationDirectory) =>
        File.Exists(Path.Combine(installationDirectory, LauncherExecutableName))
        && File.Exists(Path.Combine(
            installationDirectory,
            "Apps",
            CompatibilityExecutableName
        ));

    private static IEnumerable<string> GetLegacyApplicationFiles(string installationDirectory)
    {
        var manifestPath = Path.Combine(installationDirectory, "Apps", "legacy-release-files.txt");
        if (!File.Exists(manifestPath))
        {
            return Directory.EnumerateFiles(installationDirectory, "*", SearchOption.TopDirectoryOnly)
                .Where(IsLegacyApplicationFile);
        }

        var legacyFileNames = File.ReadAllLines(manifestPath)
            .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return Directory.EnumerateFiles(installationDirectory, "*", SearchOption.TopDirectoryOnly)
            .Where(filePath =>
                legacyFileNames.Contains(Path.GetFileName(filePath))
                || Path.GetFileName(filePath).StartsWith("OpenKh.Tools.ModsManager.", StringComparison.OrdinalIgnoreCase)
            )
            .Where(filePath => !Path.GetFileName(filePath).Equals(LauncherExecutableName, StringComparison.OrdinalIgnoreCase))
            .Where(filePath => !Path.GetFileName(filePath).Equals(CompatibilityExecutableName, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> GetLegacyResourceDirectories(string installationDirectory)
    {
        var manifestPath = Path.Combine(installationDirectory, "Apps", "legacy-release-directories.txt");
        return File.Exists(manifestPath)
            ? File.ReadAllLines(manifestPath).Where(directoryName => !string.IsNullOrWhiteSpace(directoryName))
            : FallbackLegacyResourceDirectories;
    }

    private static bool IsLegacyApplicationFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        if (fileName.Equals(LauncherExecutableName, StringComparison.OrdinalIgnoreCase)
            || fileName.Equals(CompatibilityExecutableName, StringComparison.OrdinalIgnoreCase))
            return false;

        if (Path.GetExtension(fileName).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            return true;

        if (!fileName.StartsWith("OpenKh.", StringComparison.OrdinalIgnoreCase))
            return false;

        return fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase);
    }

    private static string EscapeBatchPath(string path) => $"\"{path.Replace("\"", "\"\"")}\"";
}
