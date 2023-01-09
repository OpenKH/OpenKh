using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for WorkInProgressWindow.xaml
    /// </summary>
    public partial class WorkInProgressWindow : Window
    {
        public record TViewModel(
            string DialogTitle,
            string OperationName,
            string ModName,
            bool ProgressUnknown,
            float ProgressValue,
            string ProgressText,
            Action Cancel
        );

        public WorkInProgressWindow()
        {
            InitializeComponent();
            Closed += (sender, args) =>
            {
                if (args is CancelEventArgs cancelEventArgs)
                {
                    cancelEventArgs.Cancel = true;
                }

                ViewModel?.Cancel?.Invoke();
            };
        }

        public TViewModel ViewModel
        {
            get => (TViewModel)DataContext;
            set => DataContext = value;
        }
    }
}
