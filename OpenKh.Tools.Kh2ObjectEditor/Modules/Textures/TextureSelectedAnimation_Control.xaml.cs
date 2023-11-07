using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static OpenKh.Tools.Kh2ObjectEditor.Modules.Textures.TextureSelectedAnimation_VM;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Textures
{
    public partial class TextureSelectedAnimation_Control : UserControl
    {
        TextureSelectedAnimation_VM ThisVM { get; set; }
        public TextureSelectedAnimation_Control(int index)
        {
            InitializeComponent();
            ThisVM = new TextureSelectedAnimation_VM(index);
            DataContext = ThisVM;
            loadImage(index);
        }

        public void loadImage(int index)
        {
            List<Bitmap> bitmaps = ImageUtils.footerToImages(MdlxService.Instance.TextureFile);
            BitmapSource BitmapImage = ImageUtils.BitmapToImageSource(bitmaps[index]);
            ImageFrame.Source = BitmapImage;
        }

        private void Script_Export(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ScriptList.SelectedItem == null)
                return;

            ScriptWrapper item = (ScriptWrapper)ScriptList.SelectedItem;
            ThisVM.Script_Export(item.Id);
        }
        private void Script_Import(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.Script_Import();
        }
        private void Script_Replace(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ScriptList.SelectedItem == null)
                return;

            ScriptWrapper item = (ScriptWrapper)ScriptList.SelectedItem;
            ThisVM.Script_Replace(item.Id);
        }
        private void Script_Remove(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ScriptList.SelectedItem == null)
                return;

            ScriptWrapper item = (ScriptWrapper)ScriptList.SelectedItem;
            ThisVM.Script_Remove(item.Id);
        }
    }
}
