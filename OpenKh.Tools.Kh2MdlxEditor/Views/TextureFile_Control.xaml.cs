using OpenKh.Kh2;
using OpenKh.Kh2.TextureFooter;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    /// <summary>
    /// Interaction logic for TextureFileControl.xaml
    /// </summary>
    public partial class TextureFile_Control : UserControl
    {
        TextureFile_VM textureFileControlModel { get; set; }

        public TextureFile_Control()
        {
            InitializeComponent();
        }
        public TextureFile_Control(ModelTexture textureFile)
        {
            InitializeComponent();
            textureFileControlModel = new TextureFile_VM(textureFile);
            DataContext = textureFileControlModel;

            if (textureFileControlModel.textureData.Images != null && textureFileControlModel.textureData.Images.Count > 0)
            {
                contentFrameTexture.Content = new Texture_Control(textureFileControlModel.textureData.Images[0]);
            }
            if (textureFileControlModel.textureData.TextureFooterData != null && textureFileControlModel.textureData.TextureFooterData.TextureAnimationList.Count > 0)
            {
                TextureAnimation texAnimFirst = textureFileControlModel.textureData.TextureFooterData.TextureAnimationList[0];
                byte[] clutPalette = null;
                if (texAnimFirst.TextureIndex < textureFileControlModel.textureData.Images.Count) {
                    clutPalette = textureFileControlModel.textureData.Images[texAnimFirst.TextureIndex].GetClut();
                }
                contentFrameAnimation.Content = new TexAnim_Control(texAnimFirst, clutPalette);
            }
        }

        public void ListViewItem_OpenTexture(object sender, MouseButtonEventArgs e)
        {
            contentFrameTexture.Content = new Texture_Control(((ListViewItem)sender).Content as ModelTexture.Texture);
        }

        public void ListViewItem_OpenTexAnim(object sender, MouseButtonEventArgs e)
        {
            TextureAnimation texAnimSelected = ((ListViewItem)sender).Content as TextureAnimation;
            byte[] clutPalette = null;
            if (texAnimSelected.TextureIndex < textureFileControlModel.textureData.Images.Count)
            {
                clutPalette = textureFileControlModel.textureData.Images[texAnimSelected.TextureIndex].GetClut();
            }
            contentFrameAnimation.Content = new TexAnim_Control(texAnimSelected, clutPalette);
        }
    }
}
