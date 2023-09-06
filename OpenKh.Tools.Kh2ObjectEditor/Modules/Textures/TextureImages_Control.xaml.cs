using OpenKh.Tools.Common.Wpf;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static OpenKh.Tools.Kh2ObjectEditor.Modules.Textures.TextureImages_VM;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Textures
{
    public partial class TextureImages_Control : UserControl
    {
        TextureImages_VM ThisVM { get; set; }
        public TextureImages_Control()
        {
            InitializeComponent();
            ThisVM = new TextureImages_VM();
            DataContext = ThisVM;
            if (ThisVM.Textures.Count > 0)
            {
                loadImage(0);
                List_Textures.SelectedIndex = 0;
            }
        }

        public void loadImage(int index)
        {
            BitmapSource BitmapImage = ThisVM.Textures[index].Texture.GetBimapSource();
            ImageFrame.Source = BitmapImage;
        }

        private void list_doubleCLick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextureWrapper item = (TextureWrapper)(sender as ListView).SelectedItem;
            loadImage(item.Id);
        }

        private void Button_MoveTextureUp(object sender, System.Windows.RoutedEventArgs e)
        {
            int newIndex = ThisVM.fun_moveTextureUp((TextureWrapper)List_Textures.SelectedItem);

            if (newIndex != -1)
            {
                List_Textures.SelectedIndex = newIndex;
            }
        }

        private void Button_MoveTextureDown(object sender, System.Windows.RoutedEventArgs e)
        {
            int newIndex = ThisVM.fun_moveTextureDown((TextureWrapper)List_Textures.SelectedItem);

            if (newIndex != -1)
            {
                List_Textures.SelectedIndex = newIndex;
            }
        }
        public void Texture_Export(object sender, RoutedEventArgs e)
        {
            if (List_Textures.SelectedItem != null)
            {
                TextureWrapper item = (TextureWrapper)List_Textures.SelectedItem;
                ThisVM.exportImage(item.Id);
            }
        }
        public void Texture_Replace(object sender, RoutedEventArgs e)
        {
            if (List_Textures.SelectedItem != null)
            {
                TextureWrapper item = (TextureWrapper)List_Textures.SelectedItem;
                ThisVM.replaceImage(item.Id);
                loadImage(item.Id);
            }
        }
        public void Texture_Remove(object sender, RoutedEventArgs e)
        {
            if (List_Textures.SelectedItem != null)
            {
                TextureWrapper item = (TextureWrapper)List_Textures.SelectedItem;
                ThisVM.removeImage(item.Id);
            }
        }
        public void Texture_Add(object sender, RoutedEventArgs e)
        {
            ThisVM.addImage();
        }
    }
}
