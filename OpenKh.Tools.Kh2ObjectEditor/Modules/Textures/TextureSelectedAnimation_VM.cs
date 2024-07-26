using ModelingToolkit.Objects;
using OpenKh.Kh2.TextureFooter;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Textures
{
    public class TextureSelectedAnimation_VM : NotifyPropertyChangedBase
    {
        TextureAnimation _texAnim { get; set; }
        public TextureAnimation TexAnim
        {
            get { return _texAnim; }
            set
            {
                _texAnim = value;
                OnPropertyChanged("TexAnim");
            }
        }

        public ObservableCollection<ScriptWrapper> Animations { get; set; }

        public TextureSelectedAnimation_VM(int index)
        {
            Animations = new ObservableCollection<ScriptWrapper>();
            TexAnim = MdlxService.Instance.TextureFile.TextureFooterData.TextureAnimationList[index];
            loadAnimations();
        }

        public void loadAnimations()
        {
            if (TexAnim?.FrameGroupList == null || TexAnim.FrameGroupList.Length <= 0)
                return;

            Animations.Clear();
            for (int i = 0; i < TexAnim.FrameGroupList.Length; i++)
            {
                ScriptWrapper wrapper = new ScriptWrapper();
                wrapper.Id = i;
                wrapper.Name = "Script " + i;
                wrapper.FrameGroup = TexAnim.FrameGroupList[i];

                Animations.Add(wrapper);
            }
        }

        public void Script_Export(int index)
        {
            TextureFrameGroup item = TexAnim.FrameGroupList[index];
            string jsonFile = System.Text.Json.JsonSerializer.Serialize(item);

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save script as json";
            sfd.FileName = "script.json";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                using (StreamWriter outputFile = new StreamWriter(sfd.FileName))
                {
                    outputFile.WriteLine(jsonFile);
                }
            }
        }
        public void Script_Import()
        {
            TextureFrameGroup script = loadTextureFrameGroup();
            List<TextureFrameGroup> scriptList = TexAnim.FrameGroupList.ToList();
            scriptList.Add(script);
            TexAnim.FrameGroupList = scriptList.ToArray();
            loadAnimations();
        }
        public void Script_Replace(int index)
        {
            TextureFrameGroup script = loadTextureFrameGroup();
            TexAnim.FrameGroupList[index] = script;
            loadAnimations();
        }
        public void Script_Remove(int index)
        {
            List<TextureFrameGroup> scriptList = TexAnim.FrameGroupList.ToList();
            scriptList.RemoveAt(index);
            TexAnim.FrameGroupList = scriptList.ToArray();
            loadAnimations();
        }

        public TextureFrameGroup loadTextureFrameGroup()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "JSON file | *.json";
            bool? success = openFileDialog.ShowDialog();

            if (success == true)
            {
                if (Directory.Exists(openFileDialog.FileName))
                {
                    return null;
                }
                else if (File.Exists(openFileDialog.FileName))
                {
                    string scriptJson = File.ReadAllText(openFileDialog.FileName);
                    return System.Text.Json.JsonSerializer.Deserialize<TextureFrameGroup>(scriptJson);
                }
            }

            return null;
        }
        public MtMaterial GetMaterial(int index)
        {
            Kh2.TextureFooter.TextureAnimation texAnim = MdlxService.Instance.TextureFile.TextureFooterData.TextureAnimationList[index];
            MtMaterial mat = new MtMaterial();
            mat.Data = texAnim.SpriteImage;
            mat.Clut = MdlxService.Instance.TextureFile.Images[texAnim.TextureIndex].GetClut();
            mat.Width = texAnim.SpriteWidth;
            mat.Height = texAnim.SpriteImage.Length / texAnim.SpriteWidth;
            mat.ColorSize = 1;
            mat.PixelHasAlpha = true;
            mat.GenerateBitmap();

            return mat;
        }

        public class ScriptWrapper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public TextureFrameGroup FrameGroup { get; set; }
        }
    }
}
