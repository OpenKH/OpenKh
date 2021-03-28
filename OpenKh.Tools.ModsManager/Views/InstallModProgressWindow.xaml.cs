using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for InstallModProgressWindow.xaml
    /// </summary>
    public partial class InstallModProgressWindow : Window, INotifyPropertyChanged
    {
        private string _modName;
        private string _progressText;
        private bool _progressUnknown;
        private float _progressValue;
        private string _operationName = "Installing";

        public InstallModProgressWindow()
        {
            InitializeComponent();
            DataContext = this;
            Closed += (sender, args) =>
            {
                if (args is CancelEventArgs cancelEventArgs)
                    cancelEventArgs.Cancel = true;
            };
        }

        public string OperationName
        {
            get => _operationName;
            set
            {
                _operationName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public string DialogTitle => $"{OperationName} mod...";

        public string ModName
        {
            get => _modName;
            set
            {
                _modName = value;
                OnPropertyChanged();
            }
        }

        public string ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = value;
                OnPropertyChanged();
            }
        }

        public bool ProgressUnknown
        {
            get => _progressUnknown;
            private set
            {
                _progressUnknown = value;
                OnPropertyChanged();
            }
        }

        public float ProgressValue
        {
            get => _progressValue;
            set
            {
                ProgressUnknown = false;
                _progressValue = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
