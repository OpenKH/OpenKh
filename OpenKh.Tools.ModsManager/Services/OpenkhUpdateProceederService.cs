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
        public async Task UpdateAsync(string downloadZipUrl, Action<float> progress, CancellationToken cancellation)
        {
            var tempId = Guid.NewGuid().ToString("N");
            var tempZipFile = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}.zip");

            using (var client = new HttpClient())
            {
                using (var zipOutput = File.Create(tempZipFile))
                {
                    using (var resp = await client.GetAsync(downloadZipUrl, cancellation))
                    {
                        var maxLen = resp.Content.Headers.ContentLength;
                        var zipInput = await resp.Content.ReadAsStreamAsync();
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
            var tempBatFile = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}.bat");

            var copyFrom = Path.Combine(tempZipDir, "openkh");
            var copyTo = OpenkhInstallation.Directory;
            var packagedModManagerExecutable = Path.Combine(
                copyFrom,
                "Apps",
                "ModManager",
                "OpenKh.Tools.ModsManager.exe"
            );
            var modManagerExecutable = File.Exists(packagedModManagerExecutable)
                ? Path.Combine(copyTo, "Apps", "ModManager", "OpenKh.Tools.ModsManager.exe")
                : OpenkhInstallation.GetModManagerExecutable(copyTo);
            var compatibilityExecutable = File.Exists(packagedModManagerExecutable)
                ? Path.Combine(copyTo, "OpenKh.Tools.ModsManager.exe")
                : null;

            await CreateBatchFileAsync(
                tempBatFile: tempBatFile,
                copyFrom: copyFrom,
                copyTo: copyTo,
                deleteAfterCopy: compatibilityExecutable,
                execAfter: $"start \"\" \"{modManagerExecutable}\""
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
            string deleteAfterCopy,
            string execAfter
        )
        {
            var bat = new StringWriter();
            bat.WriteLine($"chcp 65001");
            bat.WriteLine($"taskkill /im OpenKh.Tools.ModsManager.exe");
            bat.WriteLine($"robocopy  {EscapeRobocopyArg(copyFrom)} {EscapeRobocopyArg(copyTo)} /e");
            bat.WriteLine($"if errorlevel 8 pause");
            if (!string.IsNullOrWhiteSpace(deleteAfterCopy))
            {
                bat.WriteLine($"attrib -h -r {EscapeRobocopyArg(deleteAfterCopy)}");
                bat.WriteLine($"del /f /q {EscapeRobocopyArg(deleteAfterCopy)}");
            }
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
    }
}
