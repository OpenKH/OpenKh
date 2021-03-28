using OpenKh.Tools.ModsManager.ViewModels;
using System;
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
            DataContext = new MainViewModel();
        }

        protected override void OnClosed(EventArgs e)
        {
            (DataContext as MainViewModel)?.CloseAllWindows();
            base.OnClosed(e);
        }
    }
}
