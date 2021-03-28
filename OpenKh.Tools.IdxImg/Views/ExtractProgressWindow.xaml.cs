using OpenKh.Tools.IdxImg.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace OpenKh.Tools.IdxImg.Views
{
    /// <summary>
    /// Interaction logic for ExtractProgressWindow.xaml
    /// </summary>
    public partial class ExtractProgressWindow : Window, IExtractProgress, INotifyPropertyChanged
    {
        public ExtractProgressWindow(CancellationTokenSource cts)
        {
            InitializeComponent();
            DataContext = this;
            CancellationToken = cts.Token;
        }

        public CancellationToken CancellationToken { get; }
        public string FileName { get; set; }

        public void SetExtractedName(string name)
        {
            FileName = name;
            Application.Current.Dispatcher.Invoke(() =>
                OnPropertyChanged(nameof(FileName)));
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
