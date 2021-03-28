using OpenKh.Tools.ModsManager.Services;
using System.Collections.Generic;
using System.Windows;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for InstallModView.xaml
    /// </summary>
    public partial class InstallModView : Window
    {
        private static readonly IEnumerable<FileDialogFilter> _zipFilter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("OpenKH mod as ZIP file", "zip");

        public RelayCommand CloseCommand { get; }
        public string RepositoryName { get; set; }
        public bool IsZipFile { get; private set; }

        public InstallModView()
        {
            InitializeComponent();
            DataContext = this;

            CloseCommand = new RelayCommand(_ => Close());
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            var isBlocked = false;
            var blockedMessage = string.Empty;
            if (ModsService.IsUserBlocked(RepositoryName))
            {
                isBlocked = true;
                blockedMessage = "The author of this mod violated OpenKH rules therefore we do not recommend their mods. Do you wish to install it anyway?";
            }
            else if (ModsService.IsModBlocked(RepositoryName))
            {
                isBlocked = true;
                blockedMessage = "The selected mod violates OpenKH rules, therefore we do not recommend its installation. Do you wish to install it anyway?";
            }

            if (isBlocked)
            {
                var result = MessageBox.Show(blockedMessage, $"Warning on installing {RepositoryName}", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                DialogResult = result == MessageBoxResult.Yes;
            }
            else
                DialogResult = true;

            Close();
        }

        private void InstallZip_Click(object sender, RoutedEventArgs e)
        {
            FileDialog.OnOpen(fileName =>
            {
                IsZipFile = true;
                RepositoryName = fileName;
                DialogResult = true;
                Close();
            }, _zipFilter);
        }

        private void txtSourceModUrl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                Install_Click(sender, e);

            e.Handled = true;
        }
    }
}
