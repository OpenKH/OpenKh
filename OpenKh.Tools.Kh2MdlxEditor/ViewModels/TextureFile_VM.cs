using OpenKh.Kh2;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using System.Collections.ObjectModel;
using System.Windows;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    internal class TextureFile_VM
    {
        public ModelTexture textureData { get; set; }
        public ObservableCollection<ModelTexture.Texture> textures { get; set; }

        public TextureFile_VM() { }
        public TextureFile_VM(ModelTexture textureFile)
        {
            textureData = textureFile;
            textures = new ObservableCollection<ModelTexture.Texture>(textureData.Images);
        }

        public void removeTextureAt(int index)
        {
            textures.RemoveAt(index);
            textureData.Images.RemoveAt(index);
        }
        public void addTexture(string filename)
        {
            textures.Add(ImageUtils.pngToTexture(filename));
            textureData.Images.Add(ImageUtils.pngToTexture(filename));
        }
    }
}
