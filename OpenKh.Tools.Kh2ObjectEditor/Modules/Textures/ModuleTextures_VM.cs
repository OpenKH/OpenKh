using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Collections.ObjectModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Textures
{
    public class ModuleTextures_VM
    {
        public ObservableCollection<TextureWrapper> Textures { get; set; }

        public ModuleTextures_VM()
        {
            Textures = new ObservableCollection<TextureWrapper>();
            loadTextures();
        }

        public void loadTextures()
        {
            if (MdlxService.Instance.TextureFile?.Images == null || MdlxService.Instance.TextureFile.Images.Count <= 0)
                return;

            Textures.Clear();
            for (int i = 0; i < MdlxService.Instance.TextureFile.Images.Count; i++)
            {
                TextureWrapper wrapper = new TextureWrapper();
                wrapper.Id = i;
                wrapper.Name = "Texture " + i;
                wrapper.Texture = MdlxService.Instance.TextureFile.Images[i];

                Textures.Add(wrapper);
            }
        }

        public int fun_moveTextureUp(TextureWrapper wrapper)
        {
            if (wrapper.Id <= 0)
                return -1;

            moveTexture(wrapper, wrapper.Id - 1);
            loadTextures();

            return wrapper.Id - 1;
        }
        public int fun_moveTextureDown(TextureWrapper wrapper)
        {
            if (wrapper.Id >= Textures.Count - 1)
                return -1;

            moveTexture(wrapper, wrapper.Id + 1);
            loadTextures();

            return wrapper.Id + 1;
        }

        public void moveTexture(TextureWrapper wrapper, int index)
        {
            ModelTexture.Texture tempTexture = MdlxService.Instance.TextureFile.Images[wrapper.Id];
            MdlxService.Instance.TextureFile.Images.RemoveAt(wrapper.Id);
            MdlxService.Instance.TextureFile.Images.Insert(index, tempTexture);
        }

        public class TextureWrapper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int SizeX { get; set; }
            public int SizeY { get; set; }
            public ModelTexture.Texture Texture { get; set; }
        }
    }
}
