using OpenKh.Kh2;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public partial class M_EffectsDpdTabs_Control : UserControl
    {
        Dpd ThisDpd { get; set; }
        public M_EffectsDpdTabs_Control(Dpd dpd)
        {
            InitializeComponent();
            ThisDpd = dpd;
            loadTabs();
        }

        public void loadTabs()
        {
            if (ThisDpd == null)
                return;

            Tab_Textures.Content = new M_EffectDpdTexture_Control(ThisDpd);
        }
    }
}
