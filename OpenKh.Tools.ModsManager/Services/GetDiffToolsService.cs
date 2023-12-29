using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenKh.Tools.ModsManager.Services
{
    public class GetDiffToolsService
    {
        /// <param name="extension">`.yml`</param>
        public IEnumerable<GetDiffService> GetDiffServices(string extension)
        {
            Func<byte[], byte[], Task<byte[]>> CreateDiffAsync(string exe)
            {
                return async (rawInput, rawOutput) =>
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

                        if (process.ExitCode == 0)
                        {
                            return await File.ReadAllBytesAsync(tempOutput);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    finally
                    {
                        File.Delete(tempInput);
                        File.Delete(tempOutput);
                    }
                };
            }

            IEnumerable<GetDiffService> TryToUseExe(string name, string exe)
            {
                if (File.Exists(exe))
                {
                    yield return new GetDiffService
                    {
                        Name = name,
                        DiffAsync = CreateDiffAsync(exe),
                    };
                }
            }

            IEnumerable<GetDiffService> TryToReadReg(string fullPath)
            {
                var exe = Registry.GetValue(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath), "") + "";
                if (File.Exists(exe))
                {
                    yield return new GetDiffService
                    {
                        Name = Path.GetFileName(exe),
                        DiffAsync = CreateDiffAsync(exe),
                    };
                }
            }

            foreach (var one in new GetDiffService[0]
                .Concat(TryToUseExe("WinMerge.exe", @"C:\Program Files\WinMerge\WinMergeU.exe"))
                .Concat(TryToUseExe("WinMerge.exe", @"C:\Program Files (x86)\WinMerge\WinMergeU.exe"))
                .Concat(TryToUseExe("TortoiseGitMerge.exe", @"C:\Program Files\TortoiseGit\bin\TortoiseGitMerge.exe"))
                .Concat(TryToUseExe("TortoiseGitMerge.exe", @"C:\Program Files (x86)\TortoiseGit\bin\TortoiseGitMerge.exe"))
                .Concat(TryToUseExe("TortoiseMerge.exe", @"C:\Program Files\TortoiseSVN\bin\TortoiseMerge.exe"))
                .Concat(TryToUseExe("TortoiseMerge.exe", @"C:\Program Files (x86)\TortoiseSVN\bin\TortoiseMerge.exe"))
                .Concat(TryToReadReg(@"HKEY_LOCAL_MACHINE\SOFTWARE\TortoiseGit\TMergePath"))
                .Concat(TryToReadReg(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\TortoiseGit\TMergePath"))
                .Concat(TryToReadReg(@"HKEY_LOCAL_MACHINE\SOFTWARE\TortoiseSVN\TMergePath"))
                .Concat(TryToReadReg(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\TortoiseSVN\TMergePath"))
            )
            {
                yield return one;
            }
        }
    }
}
