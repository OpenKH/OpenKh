using OpenKh.Kh2;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public partial class M_EffectDpdVoice_Control : UserControl
    {
        public M_EffectDpdVoice_VM ThisDpdVsf { get; set; }
        public M_EffectDpdVoice_Control(DpdVsf dpdVsf)
        {
            InitializeComponent();
            ThisDpdVsf = new M_EffectDpdVoice_VM(dpdVsf);
            DataContext = ThisDpdVsf;
        }

        private void Button_SaveIndices(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisDpdVsf.SaveIndices();
        }
    }
}
