using OpenKh.Kh2;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    /// <summary>
    /// Interaction logic for TextureControl.xaml
    /// </summary>
    public partial class Texture_Control : UserControl
    {
        Texture_VM textureControlVM { get; set; }
        public Texture_Control()
        {
            InitializeComponent();
        }
        public Texture_Control(ModelTexture.Texture texture)
        {
            InitializeComponent();
            textureControlVM = new Texture_VM(texture);
            DataContext = textureControlVM;
        }
    }
}
