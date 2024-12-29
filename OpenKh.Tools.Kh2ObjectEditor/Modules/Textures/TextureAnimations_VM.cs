using ModelingToolkit.Objects;
using OpenKh.Kh2.TextureFooter;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.ObjectModel;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Textures
{
    public class TextureAnimations_VM
    {
        public TextureFooterData TextureFooterData { get; set; }
        public ObservableCollection<TexAnimWrapper> TexAnims { get; set; }
        public TextureAnimations_VM()
        {
            TextureFooterData = MdlxService.Instance.TextureFile.TextureFooterData;
            TexAnims = new ObservableCollection<TexAnimWrapper>();
            loadAnims();
        }

        public void loadAnims()
        {
            TexAnims.Clear();
            for (int i = 0; i < TextureFooterData.TextureAnimationList.Count; i++)
            {
                TexAnimWrapper texAnimWrapper = new TexAnimWrapper();
                texAnimWrapper.Id = i;
                texAnimWrapper.Name = "Animation " + i;
                texAnimWrapper.TextureAnim = TextureFooterData.TextureAnimationList[i];
                TexAnims.Add(texAnimWrapper);
            }
        }
        public void exportImage(int id)
        {
            MtMaterial mat = GetMaterial(id);

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Export image as PNG";
            sfd.FileName = Path.GetFileNameWithoutExtension(MdlxService.Instance.MdlxPath) + "_Animation_" +id+".png";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                mat.ExportAsPng(sfd.FileName);
            }
        }
        public void removeImage(int id)
        {
            MdlxService.Instance.TextureFile.TextureFooterData.TextureAnimationList.RemoveAt(id);
            MdlxService.Instance.TextureFile.TextureFooterData.UvscList.Clear();
            loadAnims();
        }
        public void addImage()
        {
            TextureAnimation texAnim = ImageUtils.loadPngFileAsTextureAnimation();

            if (texAnim == null)
                return;

            MdlxService.Instance.TextureFile.TextureFooterData.TextureAnimationList.Add(texAnim);
            MdlxService.Instance.TextureFile.TextureFooterData.UvscList.Clear();
            loadAnims();
        }
        public void replaceImage(int id)
        {
            TextureAnimation texAnim = ImageUtils.loadPngFileAsTextureAnimation();

            if (texAnim == null)
                return;

            MdlxService.Instance.TextureFile.TextureFooterData.TextureAnimationList[id].SpriteImage = texAnim.SpriteImage;

            loadAnims();
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

        public class TexAnimWrapper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public TextureAnimation TextureAnim { get; set; }
        }
    }
}
