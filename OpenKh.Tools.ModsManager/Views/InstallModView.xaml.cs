using System.Windows;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for InstallModView.xaml
    /// </summary>
    public partial class InstallModView : Window
    {
        public string RepositoryName { get; set; }

        public InstallModView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void txtSourceModUrl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                Install_Click(sender, e);

            e.Handled = true;
        }
    }
}
