using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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

            var tempBatFile = Path.Combine(Path.GetTempPath(), $"openkh-{tempId}.bat");

            var copyTo = AppDomain.CurrentDomain.BaseDirectory;

            await CreateBatchFileAsync(
                tempBatFile: tempBatFile,
                copyFrom: Path.Combine(tempZipDir, "openkh"),
                copyTo: copyTo,
                execAfter: $"start \"\" \"{Path.Combine(copyTo, "OpenKh.Tools.ModsManager.exe")}\""
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

        private async Task CreateBatchFileAsync(string tempBatFile, string copyFrom, string copyTo, string execAfter)
        {
            var bat = new StringWriter();
            bat.WriteLine($"xcopy /d /e \"{copyFrom}\" \"{copyTo}\" || pause");
            bat.WriteLine($"{execAfter}");
            await File.WriteAllTextAsync(tempBatFile, bat.ToString(), Encoding.Default);
        }
    }
}
