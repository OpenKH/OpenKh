using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    internal class TextureFile_VM
    {
        public ModelTexture textureData { get; set; }

        public TextureFile_VM() { }
        public TextureFile_VM(ModelTexture textureFile)
        {
            textureData = textureFile;
        }
    }
}
