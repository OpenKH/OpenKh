using Avalonia.Controls.Documents;
using Avalonia.Threading;
using OpenKh.Tools.ModManager.Views;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia.Media;
using System.Linq;

namespace OpenKh.Tools.ModManager.Services
{
    public class LoggersSink : ILogEventSink
    {
        private readonly Action<LogEvent> _loggersEvent;

        public LoggersSink(Action<LogEvent> inputEvent) => _loggersEvent = inputEvent;
        public void Emit(LogEvent logEvent) => _loggersEvent(logEvent);
    }

    public static class LogService
    {
        static LogView? _logView;
        static Channel<LogEvent>? _logChannel;

        static CancellationTokenSource? _cancelSource;
        static CancellationToken _cancelToken;

        public static void Show()
        {
            _logView = new LogView();

            _cancelSource = new CancellationTokenSource();
            _cancelToken = _cancelSource.Token;

            _logChannel = Channel.CreateUnbounded<LogEvent>(new UnboundedChannelOptions { SingleReader = true });

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Async(_fetchAction => _fetchAction.Sink(new LoggersSink(_fetchEvent => _logChannel.Writer.TryWrite(_fetchEvent))))
                .CreateLogger();

            Dispatcher.UIThread.Post(() =>
            {
                _logView.Loggers.Inlines = new InlineCollection();
                _logView.Show();
            });

            Task.Run(async () =>
            {
                var _fetchBatch = new List<LogEvent>();

                while (!_cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        var _hasEventRead = await _logChannel.Reader.WaitToReadAsync(_cancelToken);

                        if (_hasEventRead)
                        {
                            // Batch process all available queued logs at once
                            while (_logChannel.Reader.TryRead(out var logEvent))
                            {
                                _fetchBatch.Add(logEvent);
                                if (_fetchBatch.Count >= 50)
                                    break;
                            }

                            if (_fetchBatch.Count > 0)
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    // Prepare text and styles off the UI thread
                                    var _fetchInline = new List<Run>(_fetchBatch.Count);

                                    foreach (var _fetchEvent in _fetchBatch.ToList())
                                    {
                                        var _fetchTimestamp = _fetchEvent.Timestamp.ToString("HH:mm:ss");
                                        var _fetchMessage = string.Format("[{0}] - {1}\n", _fetchTimestamp, _fetchEvent.MessageTemplate.Text);

                                        var _fetchColor = _fetchEvent.Level == LogEventLevel.Error ? "#CC4C4C" : (_fetchEvent.Level == LogEventLevel.Warning ? "#CCCC4C" : "#4CCCCC");

                                        _fetchInline.Add(new Run
                                        {
                                            Text = _fetchMessage,
                                            Foreground = Brush.Parse(_fetchColor)
                                        });
                                    }

                                    _fetchBatch.Clear();

                                    _logView.Loggers.Inlines.AddRange(_fetchInline);
                                    _logView.Scroller.ScrollToEnd();
                                }, DispatcherPriority.Background);
                            }

                            await Task.Delay(10, _cancelToken);
                        }
                    }

                    catch (OperationCanceledException) { break; }
                }
            });
        }

        public static void Dismiss()
        {
            _cancelSource.Cancel();
            Log.CloseAndFlush();

            Dispatcher.UIThread.Post(() =>
            {
                if (_logView != null)
                {
                    _logView.Loggers.Inlines = new InlineCollection();
                    _logView.Close();
                }
            });
        }
    }
}
