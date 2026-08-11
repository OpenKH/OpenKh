using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public class OpenkhUpdateProceederService
    {
        public async Task UpdateAsync(
            string downloadZipUrl,
            Action<float> progress,
            CancellationToken cancellation,
            string executableToRestart = ""
        )
        {
            var tempId = Guid.NewGuid().ToString("N");
            var tempZipFile = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}.zip");

            using (var client = new HttpClient())
            {
                using (var zipOutput = File.Create(tempZipFile))
                {
                    using (var resp = await client.GetAsync(downloadZipUrl, cancellation))
                    {
                        resp.EnsureSuccessStatusCode();
                        var maxLen = resp.Content.Headers.ContentLength;
                        var zipInput = await resp.Content.ReadAsStreamAsync(cancellation);
                        await CopyToAsyncWithProgress(zipInput, zipOutput, maxLen, progress, cancellation);
                    }
                }
            }

            var tempZipDir = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}");
            Directory.CreateDirectory(tempZipDir);

            using (var zip = ZipFile.OpenRead(tempZipFile))
            {
                zip.ExtractToDirectory(tempZipDir);
            }

            File.Delete(tempZipFile);
            var extractedDirectories = Directory.GetDirectories(tempZipDir);
            var extractedFiles = Directory.GetFiles(tempZipDir);
            var copyFrom = extractedDirectories.Length == 1 && extractedFiles.Length == 0
                ? extractedDirectories[0]
                : tempZipDir;
            var copyTo = OpenkhInstallation.Directory;
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
                    : OpenkhInstallation.GetModManagerExecutable(copyTo);
            var restartExecutable = string.IsNullOrWhiteSpace(executableToRestart)
                ? modManagerExecutable
                : executableToRestart;

            if (!OperatingSystem.IsWindows())
            {
                await StartUnixUpdateAsync(tempId, tempZipDir, copyFrom, copyTo, restartExecutable);
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
            string tempZipDir,
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
                .AppendLine($"rm -rf {EscapeShellArg(tempZipDir)}")
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

        private async Task CopyToAsyncWithProgress(Stream input, Stream output, long? maxLen, Action<float> progress, CancellationToken cancellation)
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
