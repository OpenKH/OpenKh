using OpenKh.Tools.ModsManager.ViewModels;
using System.Windows;

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
        }

        public string ConfigIsoLocation { get => _vm.IsoLocation; set => _vm.IsoLocation = value; }
        public int ConfigGameEdition { get => _vm.GameEdition; set => _vm.GameEdition = value; }
        public string ConfigOpenKhGameEngineLocation { get => _vm.OpenKhGameEngineLocation; set => _vm.OpenKhGameEngineLocation = value; }
        public string ConfigPcsx2Location { get => _vm.Pcsx2Location; set => _vm.Pcsx2Location = value; }
        public string ConfigPcReleaseLocation { get => _vm.PcReleaseLocation; set => _vm.PcReleaseLocation = value; }
        public string ConfigGameDataLocation { get => _vm.GameDataLocation; set => _vm.GameDataLocation = value; }

        private void Wizard_Finish(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
