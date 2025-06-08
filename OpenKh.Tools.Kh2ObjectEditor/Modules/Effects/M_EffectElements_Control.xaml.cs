using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public partial class M_EffectElements_Control : UserControl
    {
        M_EffectElements_VM thisVM;
        public M_EffectElements_Control()
        {
            thisVM = new M_EffectElements_VM();
            InitializeComponent();
            loadElements();
            this.DataContext = thisVM;
        }

        public void loadElements()
        {
            if (ApdxService.Instance.PaxFile?.Elements == null)
                return;

            thisVM.LoadEntries(ApdxService.Instance.PaxFile.Elements);
        }

        private void Button_Save(object sender, System.Windows.RoutedEventArgs e)
        {
            thisVM.SaveEntries();
        }
    }
}
