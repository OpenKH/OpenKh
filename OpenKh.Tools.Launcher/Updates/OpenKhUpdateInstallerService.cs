using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Formats.Tar;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.Launcher.Updates
{
    public class OpenKhUpdateInstallerService
    {
        private readonly string _installationDirectory;

        public OpenKhUpdateInstallerService(string installationDirectory)
        {
            _installationDirectory = installationDirectory;
        }

        public async Task UpdateAsync(
            string downloadUrl,
            Action<float> progress,
            CancellationToken cancellation,
            string executableToRestart = ""
        )
        {
            if (LauncherInstallation.IsAppImage && LauncherInstallation.AppImagePath is { } appImagePath)
            {
                await UpdateAppImageAsync(downloadUrl, appImagePath, progress, cancellation);
                return;
            }

            var tempId = Guid.NewGuid().ToString("N");
            var archiveExtension = OperatingSystem.IsWindows() ? ".zip" : ".tar.gz";
            var tempArchiveFile = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}{archiveExtension}");

            using (var client = new HttpClient())
            {
                using (var archiveOutput = File.Create(tempArchiveFile))
                {
                    using (var resp = await client.GetAsync(downloadUrl, cancellation))
                    {
                        resp.EnsureSuccessStatusCode();
                        var maxLen = resp.Content.Headers.ContentLength;
                        var archiveInput = await resp.Content.ReadAsStreamAsync(cancellation);
                        await CopyToAsyncWithProgress(archiveInput, archiveOutput, maxLen, progress, cancellation);
                    }
                }
            }

            var tempArchiveDirectory = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}");
            Directory.CreateDirectory(tempArchiveDirectory);

            ExtractArchive(tempArchiveFile, tempArchiveDirectory);

            File.Delete(tempArchiveFile);
            var extractedDirectories = Directory.GetDirectories(tempArchiveDirectory);
            var extractedFiles = Directory.GetFiles(tempArchiveDirectory);
            var copyFrom = extractedDirectories.Length == 1 && extractedFiles.Length == 0
                ? extractedDirectories[0]
                : tempArchiveDirectory;
            var copyTo = _installationDirectory;
            var packagedModManagerExecutable = Path.Combine(
                copyFrom,
                "Apps",
                "OpenKh.Tools.ModsManager.exe"
            );
            var previousPackagedModManagerExecutable = Path.Combine(
                copyFrom,
                "Apps",
                "ModManager",
                "OpenKh.Tools.ModsManager.exe"
            );
            var modManagerExecutable = File.Exists(packagedModManagerExecutable)
                ? Path.Combine(copyTo, "Apps", "OpenKh.Tools.ModsManager.exe")
                : File.Exists(previousPackagedModManagerExecutable)
                    ? Path.Combine(copyTo, "Apps", "ModManager", "OpenKh.Tools.ModsManager.exe")
                    : LauncherInstallation.FindModManagerExecutable(copyTo);
            var restartExecutable = string.IsNullOrWhiteSpace(executableToRestart)
                ? modManagerExecutable
                : executableToRestart;

            if (!OperatingSystem.IsWindows())
            {
                await StartUnixUpdateAsync(tempId, tempArchiveDirectory, copyFrom, copyTo, restartExecutable);
                return;
            }

            var tempBatFile = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}.bat");
            await CreateBatchFileAsync(
                tempBatFile: tempBatFile,
                copyFrom: copyFrom,
                copyTo: copyTo,
                processesToStop: new[]
                {
                    Path.GetFileName(restartExecutable),
                    "OpenKh.Launcher.exe",
                    "OpenKh.Tools.ModsManager.exe",
                },
                execAfter: $"start \"\" \"{restartExecutable}\""
            );

            Process.Start(
                new ProcessStartInfo(
                    tempBatFile
                )
                {
                    UseShellExecute = true,
                }
            );
        }

        private static async Task StartUnixUpdateAsync(
            string tempId,
            string temporaryDirectory,
            string copyFrom,
            string copyTo,
            string restartExecutable
        )
        {
            var scriptPath = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}.sh");
            var script = new StringBuilder()
                .AppendLine("#!/bin/sh")
                .AppendLine($"while kill -0 {Environment.ProcessId} 2>/dev/null; do sleep 0.2; done")
                .AppendLine($"mkdir -p {EscapeShellArg(copyTo)}")
                .AppendLine($"cp -a {EscapeShellArg(Path.Combine(copyFrom, "."))} {EscapeShellArg(copyTo)}")
                .AppendLine($"chmod +x {EscapeShellArg(restartExecutable)} 2>/dev/null || true")
                .AppendLine($"rm -rf {EscapeShellArg(temporaryDirectory)}")
                .AppendLine($"{EscapeShellArg(restartExecutable)} >/dev/null 2>&1 &")
                .AppendLine($"rm -f {EscapeShellArg(scriptPath)}")
                .ToString();
            await File.WriteAllTextAsync(scriptPath, script, new UTF8Encoding(false));
            if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                File.SetUnixFileMode(
                    scriptPath,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute
                );
            }
            Process.Start(new ProcessStartInfo
            {
                FileName = "/bin/sh",
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList = { scriptPath },
            });
        }

        internal static void ExtractArchive(string archivePath, string destinationDirectory)
        {
            if (archivePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
            {
                using var archive = File.OpenRead(archivePath);
                using var gzip = new GZipStream(archive, CompressionMode.Decompress);
                TarFile.ExtractToDirectory(gzip, destinationDirectory, overwriteFiles: false);
                return;
            }

            if (archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                ZipFile.ExtractToDirectory(archivePath, destinationDirectory);
                return;
            }

            throw new InvalidDataException($"Unsupported OpenKH update archive: {Path.GetFileName(archivePath)}");
        }

        private static async Task UpdateAppImageAsync(
            string downloadUrl,
            string currentAppImage,
            Action<float> progress,
            CancellationToken cancellation)
        {
            if (!OperatingSystem.IsLinux())
                throw new PlatformNotSupportedException("AppImage updates are only supported on Linux.");

            var appImageDirectory = Path.GetDirectoryName(currentAppImage)
                ?? throw new InvalidOperationException("The AppImage directory could not be determined.");
            var writeProbe = Path.Combine(appImageDirectory, $".openkh-write-test-{Guid.NewGuid():N}");
            await File.WriteAllTextAsync(writeProbe, string.Empty, cancellation);
            File.Delete(writeProbe);

            var tempId = Guid.NewGuid().ToString("N");
            var downloadedAppImage = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}.AppImage");
            using (var client = new HttpClient())
            using (var output = File.Create(downloadedAppImage))
            using (var response = await client.GetAsync(
                downloadUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellation))
            {
                response.EnsureSuccessStatusCode();
                var input = await response.Content.ReadAsStreamAsync(cancellation);
                await CopyToAsyncWithProgress(
                    input,
                    output,
                    response.Content.Headers.ContentLength,
                    progress,
                    cancellation);
            }

            File.SetUnixFileMode(
                downloadedAppImage,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);

            var scriptPath = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}.sh");
            // The mounted AppImage cannot replace its backing file until this process exits.
            var script = new StringBuilder()
                .AppendLine("#!/bin/sh")
                .AppendLine($"while kill -0 {Environment.ProcessId} 2>/dev/null; do sleep 0.2; done")
                .AppendLine($"if mv -f {EscapeShellArg(downloadedAppImage)} {EscapeShellArg(currentAppImage)}; then")
                .AppendLine($"  chmod +x {EscapeShellArg(currentAppImage)}")
                .AppendLine("else")
                .AppendLine($"  rm -f {EscapeShellArg(downloadedAppImage)}")
                .AppendLine("fi")
                .AppendLine($"{EscapeShellArg(currentAppImage)} >/dev/null 2>&1 &")
                .AppendLine($"rm -f {EscapeShellArg(scriptPath)}")
                .ToString();
            await File.WriteAllTextAsync(scriptPath, script, new UTF8Encoding(false), cancellation);
            File.SetUnixFileMode(
                scriptPath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            Process.Start(new ProcessStartInfo
            {
                FileName = "/bin/sh",
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList = { scriptPath },
            });
        }

        private static async Task CopyToAsyncWithProgress(Stream input, Stream output, long? maxLen, Action<float> progress, CancellationToken cancellation)
        {
            byte[] buffer = new byte[8192];
            var totalTransferred = 0L;
            while (true)
            {
                var read = await input.ReadAsync(buffer, cancellation);
                if (read <= 0)
                {
                    break;
                }
                await output.WriteAsync(buffer.AsMemory(0, read), cancellation);
                totalTransferred += read;
                if (maxLen != null)
                {
                    progress?.Invoke((totalTransferred * 1.0f / maxLen.Value));
                }
            }
        }

        private async Task CreateBatchFileAsync(
            string tempBatFile,
            string copyFrom,
            string copyTo,
            string[] processesToStop,
            string execAfter
        )
        {
            var bat = new StringWriter();
            bat.WriteLine($"chcp 65001");
            foreach (var processToStop in processesToStop)
                bat.WriteLine($"taskkill /f /im {EscapeRobocopyArg(processToStop)} >nul 2>&1");
            bat.WriteLine($"robocopy  {EscapeRobocopyArg(copyFrom)} {EscapeRobocopyArg(copyTo)} /e");
            bat.WriteLine($"if errorlevel 8 pause");
            bat.WriteLine($"{execAfter}");
            bat.WriteLine($"rd /s /q \"{copyFrom}\"");
            bat.WriteLine($"del %0");
            await File.WriteAllTextAsync(tempBatFile, bat.ToString(), Encoding.UTF8);
        }

        private string EscapeRobocopyArg(string arg)
        {
            if (0 <= arg.IndexOfAny(new char[] { ' ', '"' }))
            {
                var escaped1 = arg.Replace("\"", "\"\"");
                var escaped2 = escaped1.EndsWith('\\') ? $"{escaped1}\\" : escaped1;
                return $"\"{escaped2}\"";
            }
            else
            {
                return arg;
            }
        }

        private static string EscapeShellArg(string value) =>
            $"'{value.Replace("'", "'\\''")}'";
    }
}
