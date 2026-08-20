using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using OpenKh.Tools.ModsManager.Interfaces;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class DebuggingWindow : Window, IDebugging
    {
        private static readonly IBrush[] _brushes = new IBrush[]
        {
            new SolidColorBrush(Color.FromRgb(220, 223, 228)),
            new SolidColorBrush(Color.FromRgb(229, 192, 123)),
            new SolidColorBrush(Color.FromRgb(224, 108, 117)),
        };

        public DebuggingWindow()
        {
            InitializeComponent();
        }

        public void ClearLogs()
        {
            Dispatcher.UIThread.Invoke(LogPanel.Children.Clear);
        }

        public void HideDebugger()
        {
            Task.Run(() => Dispatcher.UIThread.Invoke(Hide));
        }

        public void Log(long ms, string tag, string message) => Task.Run(() =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var str = $"[{(ms / 1000):D3}.{(ms % 1000):D3}] {tag} {message}";
                var brush = tag switch
                {
                    "INF" => _brushes[0],
                    "WRN" => _brushes[1],
                    "ERR" => _brushes[2],
                    _ => _brushes[0],
                };
                LogPanel.Children.Insert(0, new TextBlock
                {
                    Text = str,
                    Foreground = brush,
                    TextWrapping = TextWrapping.Wrap
                });
            });
        });
    }
}
