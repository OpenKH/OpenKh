using Avalonia.Controls;
using System.Windows.Input;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class WorkInProgressWindow : Window
    {
        public record TViewModel(
            string DialogTitle,
            string OperationName,
            bool ProgressUnknown,
            float ProgressValue,
            ICommand Cancel,
            bool CancelEnabled
        );

        public WorkInProgressWindow()
        {
            InitializeComponent();
            Closed += (sender, args) =>
            {
                ViewModel?.Cancel?.Execute(null);
            };
        }

        public TViewModel ViewModel
        {
            get => (TViewModel)DataContext;
            set => DataContext = value;
        }
    }
}
