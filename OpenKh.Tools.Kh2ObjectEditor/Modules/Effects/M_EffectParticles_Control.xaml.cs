using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public partial class M_EffectParticles_Control : UserControl
    {
        public M_EffectParticles_Control()
        {
            InitializeComponent();
            loadEffects();
        }

        public void loadEffects()
        {
            if (Apdx_Service.Instance.PaxFile?.DpxPackage?.ParticleEffects == null)
                return;

            DataTable.ItemsSource = Apdx_Service.Instance.PaxFile.DpxPackage.ParticleEffects;
        }
    }
}
