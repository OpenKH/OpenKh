using OpenKh.AssimpUtils;
using OpenKh.Kh2.TextureFooter;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
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
            List<Bitmap> bitmaps = ImageUtils.footerToImages(MdlxService.Instance.TextureFile);
            //BitmapSource bitmapImage = ImageUtils.BitmapToImageSource(bitmaps[id]);

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Export image as PNG";
            sfd.FileName = "Animation.png";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                bitmaps[id].Save(sfd.FileName, ImageFormat.Png);
                //MemoryStream memStream = new MemoryStream();
                //AssimpGeneric.ExportBitmapSourceAsPng(bitmapImage, sfd.FileName);
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

            MdlxService.Instance.TextureFile.TextureFooterData.TextureAnimationList[id] = texAnim;
            MdlxService.Instance.TextureFile.TextureFooterData.UvscList.Clear();
            loadAnims();
        }

        public class TexAnimWrapper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public TextureAnimation TextureAnim { get; set; }
        }
    }
}
