using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Media;

namespace OpenKh.Tools.ModManager.Dialogs
{
    public partial class QuestionDialog : Window
    {
        public bool? Result { get; set; }

        public QuestionDialog()
        {
            InitializeComponent();
            Loaded += OnViewLoaded;
        }

        private void OnViewLoaded(object? sender, RoutedEventArgs e)
        {
            if (OperatingSystem.IsWindows())
                SystemSounds.Exclamation.Play();
        }

        private void OnAcceptClicked(object sender, RoutedEventArgs e) => Close(Result = true);

        private void OnRejectClicked(object sender, RoutedEventArgs e) => Close(Result = false);
    }
}
