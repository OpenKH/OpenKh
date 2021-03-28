using OpenKh.Tools.ModsManager.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for DebuggingWindow.xaml
    /// </summary>
    public partial class DebuggingWindow : Window, IDebugging, INotifyPropertyChanged
    {
        private static readonly Brush[] _brushes = new Brush[]
        {
            new SolidColorBrush(Color.FromRgb(220, 223, 228)),
            new SolidColorBrush(Color.FromRgb(229, 192, 123)),
            new SolidColorBrush(Color.FromRgb(224, 108, 117)),
        };

        public DebuggingWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void ClearLogs()
        {
            Application.Current.Dispatcher.Invoke(LogPanel.Children.Clear);
        }

        public void HideDebugger()
        {
            Task.Run(() => Application.Current.Dispatcher.Invoke(Hide));
        }

        public void Log(long ms, string tag, string message) => Task.Run(() =>
        {
            Application.Current.Dispatcher.Invoke(() =>
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
