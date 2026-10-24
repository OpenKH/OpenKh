using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace OpenKh.Tools.ModManager.Dialogs
{
    public partial class ProgressDialog : Window
    {
        public bool? Result { get; set; }

        public string? Message { get; set; }

        public ProgressDialog()
        {
            InitializeComponent();
            Loaded += OnViewLoaded;
        }

        private void OnViewLoaded(object? sender, RoutedEventArgs e)
        {
            MainMessage.Text = Message;
        }

        private void OnCancelClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(Result = false);
    }
}
