using OpenKh.Tools.Kh2ObjectEditor.Modules.Effects;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class EffectTest_Control : UserControl
    {
        public EffectTest_VM ThisVM { get; set; }

        public EffectTest_Control()
        {
            InitializeComponent();
            ThisVM = new EffectTest_VM();
            //ThisVM.loadTexture();
            DataContext = ThisVM;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.loadTextures();
        }
        private void Button_ClickL(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.loadTexture(ThisVM.currentTexture - 1);
        }
        private void Button_ClickR(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.loadTexture(ThisVM.currentTexture + 1);
        }
    }
}
