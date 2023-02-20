using OpenKh.AssimpUtils;
using OpenKh.Kh2;
using OpenKh.Kh2.TextureFooter;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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
            reloadContents();
        }

        public void reloadContents()
        {
            DataContext = textureFileControlModel;

            if (textureFileControlModel.textureData.Images != null && textureFileControlModel.textureData.Images.Count > 0)
            {
                contentFrameTexture.Content = new Texture_Control(textureFileControlModel.textureData.Images[0]);
            }
            if (textureFileControlModel.textureData.TextureFooterData != null && textureFileControlModel.textureData.TextureFooterData.TextureAnimationList.Count > 0)
            {
                TextureAnimation texAnimFirst = textureFileControlModel.textureData.TextureFooterData.TextureAnimationList[0];
                byte[] clutPalette = null;
                if (texAnimFirst.TextureIndex < textureFileControlModel.textureData.Images.Count)
                {
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
        public void Texture_Export(object sender, RoutedEventArgs e)
        {
            if(LV_Textures.SelectedItem != null)
            {
                BitmapSource bitmapImage = (LV_Textures.SelectedItem as ModelTexture.Texture).GetBimapSource();

                System.Windows.Forms.SaveFileDialog sfd;
                sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Title = "Export image as PNG";
                sfd.FileName = "Texture.png";
                sfd.ShowDialog();
                if (sfd.FileName != "")
                {
                    MemoryStream memStream = new MemoryStream();
                    AssimpGeneric.ExportBitmapSourceAsPng(bitmapImage, sfd.FileName);
                }
            }
        }
        // Works for the textures, but the textures are outputted from the actual data, so it only works visually on the tool
        public void Texture_Remove(object sender, RoutedEventArgs e)
        {
            if (LV_Textures.SelectedItem != null)
            {
                //textureFileControlModel.textureData.Images.RemoveAt(LV_Textures.SelectedIndex);
                textureFileControlModel.removeTextureAt(LV_Textures.SelectedIndex);
                reloadContents();
            }
        }
        // Works for the textures, but the textures are outputted from the actual data, so it only works visually on the tool
        public void Texture_Add(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog sfd;
            sfd = new System.Windows.Forms.OpenFileDialog();
            sfd.Title = "Select PNG image";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                if (sfd.FileName.ToLower().EndsWith(".png"))
                {
                    textureFileControlModel.addTexture(sfd.FileName);
                    reloadContents();
                }
            }
        }
        public void Animation_Export(object sender, RoutedEventArgs e)
        {
            if (LV_Animations.SelectedItem != null)
            {
                TextureAnimation texAnim = LV_Animations.SelectedItem as TextureAnimation;

                BitmapSource bitmapImage = ImageUtils.getBitmapSource(texAnim, textureFileControlModel.textureData.Images[texAnim.TextureIndex].GetClut());

                System.Windows.Forms.SaveFileDialog sfd;
                sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Title = "Export image as PNG";
                sfd.FileName = "Texture.png";
                sfd.ShowDialog();
                if (sfd.FileName != "")
                {
                    MemoryStream memStream = new MemoryStream();
                    AssimpGeneric.ExportBitmapSourceAsPng(bitmapImage, sfd.FileName);
                }
            }
        }
    }
}
