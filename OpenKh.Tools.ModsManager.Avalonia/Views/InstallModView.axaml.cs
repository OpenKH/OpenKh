using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using OpenKh.Tools.ModsManager.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class InstallModView : UserControl
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        private static readonly IEnumerable<Xe.Tools.Wpf.Dialogs.FileDialogFilter> _zipFilter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("Mod archive", "zip", "kh2pcpatch", "kh1pcpatch", "compcpatch", "bbspcpatch", "dddpcpatch", "lua");

        public RelayCommand CloseCommand { get; }
        public string RepositoryName { get; set; }
        public string PlatformURL { get; set; }
        public string BranchName { get; set; }
        public bool IsZipFile { get; private set; }
        public bool IsLuaFile { get; private set; } = false;
        public bool? DialogResult { get; private set; }

        private MainWindow _host;
        private DispatcherFrame _dialogFrame;

        public InstallModView()
        {
            InitializeComponent();
            DataContext = this;

            CloseCommand = new RelayCommand(_ => Close());
        }

        public bool? ShowDialog()
        {
            var desktop = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            _host = desktop?.Windows.OfType<MainWindow>().FirstOrDefault(window => window.IsActive)
                ?? desktop?.MainWindow as MainWindow;

            if (_host is null)
                return false;

            _dialogFrame = new DispatcherFrame();
            _host.ShowDialogContent(this);
            Avalonia.Threading.Dispatcher.UIThread.Post(() => txtSourceModUrl.Focus(), DispatcherPriority.Input);
            Avalonia.Threading.Dispatcher.UIThread.PushFrame(_dialogFrame);
            return DialogResult;
        }

        private void Close()
        {
            _host?.HideDialogContent(this);
            if (_dialogFrame is not null)
                _dialogFrame.Continue = false;
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
                BlockedModWarningText.Text = blockedMessage;
                InstallForm.IsVisible = false;
                BlockedModWarning.IsVisible = true;
                BlockedModWarning.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void InstallLocalFile_Click(object sender, RoutedEventArgs e)
        {
            Xe.Tools.Wpf.Dialogs.FileDialog.OnOpen(fileName =>
            {
                if (!fileName.Contains(".lua"))
                {
                    IsZipFile = true;
                    RepositoryName = fileName;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    IsZipFile = false;
                    IsLuaFile = true;
                    RepositoryName = fileName;
                    DialogResult = true;
                    Close();
                }
            }, _zipFilter);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        private void BlockedModCancel_Click(object sender, RoutedEventArgs e)
        {
            BlockedModWarning.IsVisible = false;
            InstallForm.IsVisible = true;
            txtSourceModUrl.Focus();
        }

        private void BlockedModConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void InstallModView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Install_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }

        }
    }
}
