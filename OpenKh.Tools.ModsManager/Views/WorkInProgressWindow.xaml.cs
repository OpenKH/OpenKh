using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

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
                if (args is CancelEventArgs cancelEventArgs)
                {
                    cancelEventArgs.Cancel = true;
                }

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
