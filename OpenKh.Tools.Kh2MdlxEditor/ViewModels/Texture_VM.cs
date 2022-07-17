using OpenKh.Kh2;
using OpenKh.Tools.Common.Wpf;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    internal class Texture_VM
    {
        public ModelTexture.Texture texture { get; set; }
        public BitmapSource bitmapImage { get; set; }

        public Texture_VM() { }
        public Texture_VM(ModelTexture.Texture texture)
        {
            this.texture = texture;
            bitmapImage = texture.GetBimapSource();
        }
    }
}
