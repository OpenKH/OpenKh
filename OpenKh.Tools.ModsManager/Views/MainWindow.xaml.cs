using OpenKh.Tools.ModsManager.ViewModels;
using System;
using System.Linq;
using System.Windows;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainViewModel();
            DataContext = viewModel;

            if (Environment.GetCommandLineArgs().Any(argument =>
                argument.Equals("--check-for-updates", StringComparison.OrdinalIgnoreCase)))
            {
                Loaded += (_, _) => viewModel.CheckOpenkhUpdateCommand.Execute(null);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            (DataContext as MainViewModel)?.CloseAllWindows();
            WinSettings.Default.Save();
            base.OnClosed(e);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
