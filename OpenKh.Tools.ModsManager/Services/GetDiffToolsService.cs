using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public class GetDiffToolsService
    {
        /// <param name="extension">`.yml`</param>
        public IEnumerable<GetDiffService> GetDiffServices(string extension)
        {
            IEnumerable<GetDiffService> TryToUse(string name, string exe)
            {
                if (File.Exists(exe))
                {
                    yield return new GetDiffService
                    {
                        Name = name,
                        DiffAsync = async (rawInput, rawOutput) =>
                        {
                            var tempInput = Path.GetTempFileName() + extension;
                            var tempOutput = Path.GetTempFileName() + extension;
                            try
                            {
                                await File.WriteAllBytesAsync(tempInput, rawInput);
                                await File.WriteAllBytesAsync(tempOutput, rawOutput);

                                var process = System.Diagnostics.Process.Start(
                                    exe,
                                    $"\"{tempInput}\" \"{tempOutput}\""
                                );
                                await process.WaitForExitAsync();

                                return null;
                            }
                            finally
                            {
                                File.Delete(tempInput);
                                File.Delete(tempOutput);
                            }
                        }
                    };
                }
            }

            foreach (var one in new GetDiffService[0]
                .Concat(TryToUse("WinMerge.exe", @"C:\Program Files\WinMerge\WinMergeU.exe"))
                .Concat(TryToUse("TortoiseGitMerge.exe", @"C:\Program Files\TortoiseGit\bin\TortoiseGitMerge.exe"))
                .Concat(TryToUse("TortoiseMerge.exe", @"C:\Program Files\TortoiseSVN\bin\TortoiseMerge.exe"))
            )
            {
                yield return one;
            }
        }
    }
}
