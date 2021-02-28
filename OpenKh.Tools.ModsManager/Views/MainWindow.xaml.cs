using OpenKh.Tools.ModsManager.ViewModels;
using System;
using System.Diagnostics;
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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            using (var proc = new Process())
            {
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.FileName = e.Uri.AbsoluteUri;
                proc.Start();
            }
        }

        private void ListBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
             if (e.Key == System.Windows.Input.Key.Delete)
                (DataContext as MainViewModel).RemoveModCommand.Execute(null);
            if (e.Key == System.Windows.Input.Key.Space)
                (DataContext as MainViewModel).SelectedValue.Enabled = !(DataContext as MainViewModel).SelectedValue.Enabled;
        }
    }
}
