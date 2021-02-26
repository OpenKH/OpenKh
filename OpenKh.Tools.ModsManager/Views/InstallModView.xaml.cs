using System.Collections.Generic;
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
            var isBlocked = false;
            var blockedMessage = string.Empty;
            if (isBlocked |= ModsService.IsModBlocked(RepositoryName))
                blockedMessage = "The selected mod violates OpenKH rules, therefore we do not recommend its installation. Do you wish to install it anyway?";
            if (isBlocked |= ModsService.IsUserBlocked(RepositoryName))
                blockedMessage = "The user of this mod violated OpenKH rules therefore we do not recommend their mods. Do you wish to install it anyway?";

            if (isBlocked)
            {
                var result = MessageBox.Show(blockedMessage, $"Warning on installing {RepositoryName}", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                DialogResult = result == MessageBoxResult.Yes;
            }
            else
                DialogResult = true;

            Close();
        }
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
