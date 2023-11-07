using OpenKh.Kh2;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public partial class M_EffectDpdTexture_Control : UserControl
    {
        public M_EffectDpdTexture_VM ThisVM { get; set; }
        public M_EffectDpdTexture_Control(Dpd dpd)
        {
            InitializeComponent();
            ThisVM = new M_EffectDpdTexture_VM(dpd);
            DataContext = ThisVM;

            if(ThisVM.TextureWrappers.Count > 0)
            {
                loadImage(0);
                List_Textures.SelectedIndex = 0;
            }
        }

        public void loadImage(int index)
        {
            BitmapSource BitmapImage = ThisVM.getTexture(index);
            ImageFrame.Source = BitmapImage;
        }

        private void List_Textures_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            loadImage(List_Textures.SelectedIndex);
        }
    }
}
