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
            _vm.PCLaunchOption = PCLaunchOption;
            _vm.LastPage = LastPage;

            _vm.PageStack.OnPageChanged(wizard.CurrentPage);
        }

        public string ConfigIsoLocation { get => _vm.IsoLocation; set => _vm.IsoLocation = value; }
        public int ConfigGameEdition { get => _vm.GameEdition; set => _vm.GameEdition = value; }
        public string ConfigOpenKhGameEngineLocation { get => _vm.OpenKhGameEngineLocation; set => _vm.OpenKhGameEngineLocation = value; }
        public string ConfigPcsx2Location { get => _vm.Pcsx2Location; set => _vm.Pcsx2Location = value; }
        public string ConfigPcReleaseLocation { get => _vm.PcReleaseLocation; set => _vm.PcReleaseLocation = value; }
        public string ConfigPcReleaseLocationKH3D { get => _vm.PcReleaseLocationKH3D; set => _vm.PcReleaseLocationKH3D = value; }
        public string ConfigPcReleaseLanguage { get => _vm.PcReleaseLanguage; set => _vm.PcReleaseLanguage = value; }
        public string ConfigGameDataLocation { get => _vm.GameDataLocation; set => _vm.GameDataLocation = value; }
        public int ConfigRegionId { get => _vm.RegionId; set => _vm.RegionId = value; }
        public bool ConfigPanaceaInstalled { get => _vm.PanaceaInstalled; set => _vm.PanaceaInstalled = value; }
        public bool ConfigIsEGSVersion { get => _vm.IsEGSVersion; set => _vm.IsEGSVersion = value; }

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
