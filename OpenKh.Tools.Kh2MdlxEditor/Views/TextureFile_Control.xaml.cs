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
using OpenKh.Kh2.Models;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    /// <summary>
    /// Interaction logic for TextureFileControl.xaml
    /// </summary>
    public partial class TextureFile_Control : UserControl
    {
        TextureFile_VM textureFileControlModel { get; set; }
        
        // this is used for the Move Texture Up/Down operator, as it needs to update indexes in other places.
        ModelSkeletal modelFile { get; set; }

        public TextureFile_Control()
        {
            InitializeComponent();
        }
        public TextureFile_Control(ModelTexture textureFile, ModelSkeletal modelFile)
        {
            InitializeComponent();
            textureFileControlModel = new TextureFile_VM(textureFile);
            this.modelFile = modelFile;
            reloadContents();
        }

        public void reloadContents()
        {
            DataContext = textureFileControlModel;

            if (textureFileControlModel.textureData.Images != null && textureFileControlModel.textureData.Images.Count > 0)
            {
                contentFrameTexture.Content = new Texture_Control(textureFileControlModel.textureData.Images[0]);
            }

            ReloadAnimationContent();
        }
        
        private void ReloadAnimationContent()
        {
            if (textureFileControlModel.textureData.TextureFooterData != null 
                && textureFileControlModel.textureData.TextureFooterData.TextureAnimationList.Count > 0)
            {
                var index = LV_Animations.SelectedItem != null ? LV_Animations.SelectedIndex : 0;
                TextureAnimation texAnimFirst = textureFileControlModel.textureData.TextureFooterData.TextureAnimationList[index];
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
        
        public void Texture_MoveUp(object sender, RoutedEventArgs e)
        {
            if (LV_Textures.SelectedItem == null) return; 

            MoveCurrentTextureBy(-1);
        }

        public void Texture_MoveDown(object sender, RoutedEventArgs e)
        {
            if (LV_Textures.SelectedItem == null) return; 

            MoveCurrentTextureBy(1);
        }

        private void MoveCurrentTextureBy(int offset)
        {
            var textureData = (DataContext as TextureFile_VM).textureData;
            
            // calculate index with a wraparound
            var swapIndex = ((LV_Textures.SelectedIndex + offset + LV_Textures.Items.Count) % LV_Textures.Items.Count); ;
            textureData.SwapTextures(LV_Textures.SelectedIndex, swapIndex);
            UpdateTextureReferences((ushort) LV_Textures.SelectedIndex, (ushort) swapIndex);

            LV_Textures.Items.Refresh();
            LV_Animations.Items.Refresh();
            ReloadAnimationContent();
        }
        
        private void UpdateTextureReferences(ushort textureIndex, ushort swapTextureIndex)
        {
            foreach (var modelFileGroup in modelFile.Groups)
            {
                var itemIndex = modelFileGroup.Header.TextureIndex;
                if (itemIndex == textureIndex)
                {
                    modelFileGroup.Header.TextureIndex = swapTextureIndex;
                } else if (itemIndex == swapTextureIndex)
                {
                    modelFileGroup.Header.TextureIndex = textureIndex;
                }
            }

            var textureAnimations = textureFileControlModel.textureData.TextureFooterData.TextureAnimationList;
            foreach (var textureAnimation in textureAnimations)
            {
                var itemIndex  = textureAnimation.TextureIndex;
                if (itemIndex  == textureIndex)
                {
                    textureAnimation.TextureIndex = swapTextureIndex;
                } else if (itemIndex  == swapTextureIndex)
                {
                    textureAnimation.TextureIndex = textureIndex;
                }
            }
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
