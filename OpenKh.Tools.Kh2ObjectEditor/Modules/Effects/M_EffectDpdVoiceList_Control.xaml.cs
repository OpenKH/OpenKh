using OpenKh.Kh2;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public partial class M_EffectDpdVoiceList_Control : UserControl
    {

        public ObservableCollection<VsfWrapper> VsfWrappers { get; set; }

        public M_EffectDpdVoiceList_Control(Dpd loadedDpd)
        {
            InitializeComponent();

            VsfWrappers = new ObservableCollection<VsfWrapper>();
            for (int i = 0; i < loadedDpd.VsfList.Count; i++)
            {
                VsfWrappers.Add(new VsfWrapper(i, loadedDpd.VsfList[i]));
            }

            DataContext = this;
        }

        public class VsfWrapper
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public DpdVsf VoiceEffect { get; set; }

            public VsfWrapper(int index, DpdVsf vsf)
            {
                Index = index;
                VoiceEffect = vsf;
                Name = "Vsf " + Index;
            }
        }

        private void ListView_Vsf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ListView_Vsf.SelectedIndex == null)
            {
                return;
            }
            Frame_Voice.Content = new M_EffectDpdVoice_Control(VsfWrappers[ListView_Vsf.SelectedIndex].VoiceEffect);
        }
    }
}
