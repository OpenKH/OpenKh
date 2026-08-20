using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                        await File.WriteAllBytesAsync(tempInput, rawInput ?? new byte[0]);
                        await File.WriteAllBytesAsync(tempOutput, rawOutput ?? new byte[0]);

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

            IEnumerable<string> TryToReadReg(string fullPath)
            {
                var exe = Registry.GetValue(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath), "") + "";
                if (File.Exists(exe))
                {
                    yield return exe;
                }
            }

            IEnumerable<string> TryToFindExe(string exe)
            {
                if (File.Exists(exe))
                {
                    yield return exe;
                }
            }

            IEnumerable<string> TryToFindOnPath(string exeName)
            {
                var pathDirs = (Environment.GetEnvironmentVariable("PATH") ?? "")
                    .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var dir in pathDirs)
                {
                    var candidate = Path.Combine(dir, exeName);
                    if (File.Exists(candidate))
                    {
                        yield return candidate;
                        yield break;
                    }
                }
            }

            IEnumerable<GetDiffService> TwoFileDiff(string name, IEnumerable<string> exeFiles)
            {
                return exeFiles
                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                    .Select(
                        exe => new GetDiffService
                        {
                            Name = $"{name} ({Path.GetDirectoryName(exe)})",
                            DiffAsync = CreateDiffAsync(exe),
                        }
                    );
            }

            var tools = Enumerable.Empty<GetDiffService>();

            if (OperatingSystem.IsWindows())
            {
                tools = tools
                    .Concat(
                        TwoFileDiff(
                            "WinMerge.exe",
                            new string[0]
                                .Concat(TryToFindExe(@"C:\Program Files\WinMerge\WinMergeU.exe"))
                                .Concat(TryToFindExe(@"C:\Program Files (x86)\WinMerge\WinMergeU.exe"))
                        )
                    )
                    .Concat(
                        TwoFileDiff(
                            "TortoiseGitMerge.exe",
                            new string[0]
                                .Concat(TryToReadReg(@"HKEY_LOCAL_MACHINE\SOFTWARE\TortoiseGit\TMergePath"))
                                .Concat(TryToReadReg(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\TortoiseGit\TMergePath"))
                                .Concat(TryToFindExe(@"C:\Program Files\TortoiseGit\bin\TortoiseGitMerge.exe"))
                                .Concat(TryToFindExe(@"C:\Program Files (x86)\TortoiseGit\bin\TortoiseGitMerge.exe"))
                        )
                    )
                    .Concat(
                        TwoFileDiff(
                            "TortoiseMerge.exe",
                            new string[0]
                                .Concat(TryToReadReg(@"HKEY_LOCAL_MACHINE\SOFTWARE\TortoiseSVN\TMergePath"))
                                .Concat(TryToReadReg(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\TortoiseSVN\TMergePath"))
                                .Concat(TryToFindExe(@"C:\Program Files\TortoiseSVN\bin\TortoiseGitMerge.exe"))
                                .Concat(TryToFindExe(@"C:\Program Files (x86)\TortoiseSVN\bin\TortoiseGitMerge.exe"))
                        )
                    );
            }
            else
            {
                // No registry on Linux; look for common diff tools on PATH.
                tools = tools
                    .Concat(TwoFileDiff("meld", TryToFindOnPath("meld")))
                    .Concat(TwoFileDiff("kdiff3", TryToFindOnPath("kdiff3")))
                    .Concat(TwoFileDiff("Beyond Compare", TryToFindOnPath("bcompare")))
                    .Concat(TwoFileDiff("KDE Kompare", TryToFindOnPath("kompare")));
            }

            foreach (var one in tools)
            {
                yield return one;
            }
        }
    }
}
