using System;
using System.Diagnostics;
using System.IO;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class LogCrashStatusUsecase
    {
        public void Log(Exception ex, string shortAppName)
        {
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CrashLogs");
            Directory.CreateDirectory(logDir);

            File.WriteAllText(
                Path.Combine(logDir, $"{shortAppName}-{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.txt"),
                ex.ToString()
            );

            Process.Start(
                new ProcessStartInfo(logDir)
                {
                    UseShellExecute = true,
                }
            );
        }
    }
}
