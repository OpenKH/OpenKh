using OpenKh.Common;
using OpenKh.Tools.ModsManager.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Xceed.Wpf.Toolkit;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for SetupWizardWindow.xaml
    /// </summary>
    public partial class SetupWizardWindow : Window
    {
        private readonly SetupWizardViewModel _vm;

        public SetupWizardWindow()
        {
            InitializeComponent();
            DataContext = _vm = new SetupWizardViewModel();

            _vm.PageIsoSelection = PageIsoSelection;
            _vm.PageEosInstall = PageEosInstall;
            _vm.PageRegion = PageRegion;
            _vm.PageGameData = PageGameData;
            _vm.PageSteamAPITrick = PageSteamAPITrick;
            _vm.LastPage = LastPage;

            _vm.PageStack.OnPageChanged(wizard.CurrentPage);

            Closed += (sender, e) => _vm.SetAborted();
        }

        private void Wizard_Finish(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Wizard_PageChanged(object sender, RoutedEventArgs e)
        {
            _vm?.PageStack.OnPageChanged(((Wizard)sender).CurrentPage);
        }

        private void NavigateURL(object sender, RequestNavigateEventArgs e) =>
            new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = e.Uri.AbsoluteUri
                }
            }.Using(x => x.Start());
    }
}
