using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Game.Debugging
{
    public static class Log
    {
        private const string LogFileName = "openkh.log";
        private static Stopwatch _stopwatch = Stopwatch.StartNew();
        private static StreamWriter _logWriter = new StreamWriter(LogFileName, false, Encoding.UTF8, 65536);

        public static void Info(string text) => LogText("INF", text);
        public static void Warn(string text) => LogText("WRN", text);
        public static void Err(string text) => LogText("ERR", text);

        public static void Flush()
        {
            lock (_logWriter)
            {
                _logWriter.Flush();
            }
        }

    private static void LogText(string tag, string text)
        {
            var ms = _stopwatch.ElapsedMilliseconds;
            var str = $"[{(ms/1000):D3}.{(ms%1000):D3}] {tag} {text}";
            Task.Run(() =>
            {
                Console.WriteLine(str);
                lock (_logWriter)
                {
                    _logWriter.WriteLine(str);
                }
            });
        }
    }
}
