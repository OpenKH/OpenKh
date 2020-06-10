using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Game.Debugging
{
    public static class Log
    {
        private const string LogFileName = "openkh.log";
        private static Stopwatch _stopwatch = Stopwatch.StartNew();
        private static StreamWriter _logWriter = new StreamWriter(LogFileName, false, Encoding.UTF8, 65536);
        private static Queue<string> _logQueue = new Queue<string>();
        private static CancellationTokenSource _cancellationTokenSrc = new CancellationTokenSource();
        private static Task _taskLog;

        public static void Info(string text) => LogText("INF", text);
        public static void Warn(string text) => LogText("WRN", text);
        public static void Err(string text) => LogText("ERR", text);

        static Log()
        {
            _taskLog = Task.Run(LogWriterListener, _cancellationTokenSrc.Token);
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

        private static void LogText(string tag, string text)
        {
            var ms = _stopwatch.ElapsedMilliseconds;
            var str = $"[{(ms/1000):D3}.{(ms%1000):D3}] {tag} {text}";
            lock (_logQueue)
                _logQueue.Enqueue(str);
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
                    LogForReal(_logQueue.Dequeue());
            }
        }

        private static void LogForReal(string str)
        {
            Console.WriteLine(str);
            _logWriter.WriteLine(str);
        }
    }
}
