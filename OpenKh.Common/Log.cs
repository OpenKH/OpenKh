using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Common
{
    public static class Log
    {
        public static readonly string AppName = NormalizeName(Assembly.GetEntryAssembly().ManifestModule.ToString());
        private static readonly string LogFileName = $"{AppName}.log";
        private static readonly Task _taskLog;
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private static readonly StreamWriter _logWriter = new StreamWriter(LogFileName, false, Encoding.UTF8, 65536);
        private static readonly Queue<(long ms, string tag, string fmt, object[] args)> _logQueue =
            new Queue<(long, string, string, object[])>();
        private static readonly CancellationTokenSource _cancellationTokenSrc = new CancellationTokenSource();

        public delegate void LogDispatch(long ms, string tag, string message);
        public static event LogDispatch OnLogDispatch;

        public static void Info(string fmt, params object[] args) => LogText("INF", fmt, args);
        public static void Warn(string fmt, params object[] args) => LogText("WRN", fmt, args);
        public static void Err(string fmt, params object[] args) => LogText("ERR", fmt, args);

        static Log()
        {
            _taskLog = Task.Run(LogWriterListener, _cancellationTokenSrc.Token);
            OnLogDispatch += (long ms, string tag, string message) =>
            {
                var str = $"[{(ms / 1000):D3}.{(ms % 1000):D3}] {tag} {message}";
                Console.Error.WriteLine(str);
                _logWriter.WriteLine(str);
            };
        }

        public static void Close()
        {
            _cancellationTokenSrc.Cancel();
            _cancellationTokenSrc.Token.WaitHandle.WaitOne();
            _cancellationTokenSrc.Dispose();
            Flush();
        }

        private static void Flush()
        {
            const int Timeout = 3000;

            var timeoutStopwatch = Stopwatch.StartNew();
            do
            {
                lock (_logQueue)
                {
                    if (_logQueue.Count == 0)
                    {
                        _logWriter.Flush();
                        return;
                    }
                }
            } while (timeoutStopwatch.ElapsedMilliseconds < Timeout);

            throw new TimeoutException("Could not flush logs within the time limit");
        }

        private static void LogText(string tag, string fmt, object[] args)
        {
            var ms = _stopwatch.ElapsedMilliseconds;
            lock (_logQueue)
                _logQueue.Enqueue((ms, tag, fmt, args));
        }

        private static void LogWriterListener()
        {
            while (!_cancellationTokenSrc.IsCancellationRequested)
            {
                DequeueAllLogs();
                Thread.Sleep(1);
            }

            DequeueAllLogs();
        }

        private static void DequeueAllLogs()
        {
            lock (_logQueue)
            {
                while (_logQueue.Count > 0)
                {
                    var evt = _logQueue.Dequeue();
                    OnLogDispatch(evt.ms, evt.tag, string.Format(evt.fmt, evt.args));
                }
            }
        }

        private static string NormalizeName(string name)
        {
            if (name.EndsWith(".dll") || name.EndsWith(".exe"))
                return name.Substring(0, name.Length - 4);

            return name;
        }
    }
}
