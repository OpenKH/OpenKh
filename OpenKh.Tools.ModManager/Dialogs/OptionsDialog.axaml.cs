using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModManager.Dialogs
{
    public partial class OptionsDialog : Window
    {
        public Func<Window?, Task<string?>>? MiscButtonCallback { get; set; }

        public string? Result { get; set; }
        public string? CurrentText { get; set; }
        public string? MiscButtonText { get; set; }

        public string[]? Options { get; set; }

        public OptionsDialog()
        {
            InitializeComponent();
            Loaded += OnViewLoaded;
        }

        private void OnViewLoaded(object? sender, RoutedEventArgs e)
        {
            MainOption.ItemsSource = Options;
            MainOption.SelectedIndex = 0;

            MiscButton.Content = MiscButtonText;
            MiscButton.IsVisible = MiscButtonText == null ? false : true;
        }

        private void OnAcceptClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(Result = MainOption.SelectedItem as string);

        private async void OnMiscClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (MiscButtonCallback != null)
            {
                var _fetchResult = await MiscButtonCallback(this);

                if (!String.IsNullOrEmpty(_fetchResult))
                    Close(Result = _fetchResult);
            }

            else
                Close(Result = null);
        }
    }
}
