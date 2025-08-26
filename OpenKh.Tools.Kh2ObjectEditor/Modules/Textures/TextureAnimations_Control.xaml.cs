using System.Windows;
using System.Windows.Controls;
using static OpenKh.Tools.Kh2ObjectEditor.Modules.Textures.TextureAnimations_VM;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Textures
{
    public partial class TextureAnimations_Control : UserControl
    {
        TextureAnimations_VM ThisVM { get; set; }
        public TextureAnimations_Control()
        {
            InitializeComponent();
            ThisVM = new TextureAnimations_VM();
            DataContext = ThisVM;
            if(ThisVM.TexAnims.Count > 0)
            {
                loadImage(0);
                List_Textures.SelectedIndex = 0;
            }
        }

        private void Animation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedItem == null)
                return;

            TexAnimWrapper item = (TexAnimWrapper)(sender as ListView).SelectedItem;
            loadImage(item.Id);
        }

        public void loadImage(int index)
        {
            SelectedAnimationFrame.Content = new TextureSelectedAnimation_Control(index);
        }
        public void Texture_Export(object sender, RoutedEventArgs e)
        {
            if (List_Textures.SelectedItem != null)
            {
                TexAnimWrapper item = (TexAnimWrapper)List_Textures.SelectedItem;
                ThisVM.exportImage(item.Id);
            }
        }
        public void Texture_Remove(object sender, RoutedEventArgs e)
        {
            if (List_Textures.SelectedItem != null)
            {
                TexAnimWrapper item = (TexAnimWrapper)List_Textures.SelectedItem;
                ThisVM.removeImage(item.Id);
            }
        }
        public void Texture_Add(object sender, RoutedEventArgs e)
        {
            TexAnimWrapper item = (TexAnimWrapper)List_Textures.SelectedItem;
            try
            {
                ThisVM.addImage();
            }
            catch (System.Exception exc)
            {
                System.Windows.Forms.MessageBox.Show("Image format is not compatible (probably)", "Couldn't load image", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
        }
        public void Texture_Replace(object sender, RoutedEventArgs e)
        {
            if (List_Textures.SelectedItem != null)
            {
                TexAnimWrapper item = (TexAnimWrapper)List_Textures.SelectedItem;
                try
                {
                    ThisVM.replaceImage(item.Id);
                    loadImage(item.Id);
                }
                catch (System.Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show("Image format is not compatible (probably)", "Couldn't load image", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }
            }
        }
    }
}
