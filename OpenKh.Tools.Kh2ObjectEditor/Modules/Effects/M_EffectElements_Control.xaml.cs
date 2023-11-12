using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public partial class M_EffectElements_Control : UserControl
    {
        public M_EffectElements_Control()
        {
            InitializeComponent();
            loadElements();
        }

        public void loadElements()
        {
            if (ApdxService.Instance.PaxFile?.Elements == null)
                return;

            DataTable.ItemsSource = ApdxService.Instance.PaxFile.Elements;
        }
    }
}
