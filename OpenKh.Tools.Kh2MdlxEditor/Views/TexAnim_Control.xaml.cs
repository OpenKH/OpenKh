using OpenKh.Kh2.TextureFooter;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    /// <summary>
    /// Interaction logic for TexAnimControl.xaml
    /// </summary>
    public partial class TexAnim_Control : UserControl
    {
        TexAnim_VM texAnim_VM { get; set; }

        public TexAnim_Control(TextureAnimation texAnim, byte[] clutPalette)
        {
            InitializeComponent();
            texAnim_VM = new TexAnim_VM(texAnim, clutPalette);
            DataContext = texAnim_VM;
        }

        public void ListViewItem_OpenTexAnim(object sender, MouseButtonEventArgs e)
        {
            texAnim_VM.loadTextureFrameList(((ListViewItem)sender).Content as TexAnim_VM.TextureFrameGroupWrapper);
        }
    }
}
