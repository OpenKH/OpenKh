using OpenKh.AssimpUtils;
using OpenKh.Kh2;
using OpenKh.Kh2.TextureFooter;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Textures
{
    public class TextureImages_VM
    {
        public ObservableCollection<TextureWrapper> Textures { get; set; }

        public TextureImages_VM()
        {
            Textures = new ObservableCollection<TextureWrapper>();
            loadTextures();
        }

        public void loadTextures()
        {
            if (MdlxService.Instance.TextureFile?.Images == null || MdlxService.Instance.TextureFile.Images.Count < 0)
                return;

            Textures.Clear();
            for (int i = 0; i < MdlxService.Instance.TextureFile.Images.Count; i++)
            {
                TextureWrapper wrapper = new TextureWrapper();
                wrapper.Id = i;
                wrapper.Name = "Texture " + i;
                wrapper.Texture = MdlxService.Instance.TextureFile.Images[i];
                wrapper.SizeX = wrapper.Texture.Size.Width;
                wrapper.SizeY = wrapper.Texture.Size.Height;

                Textures.Add(wrapper);
            }
        }

        public int fun_moveTextureUp(TextureWrapper wrapper)
        {
            if (wrapper.Id <= 0)
                return -1;

            moveTexture(wrapper, wrapper.Id - 1);

            return wrapper.Id - 1;
        }
        public int fun_moveTextureDown(TextureWrapper wrapper)
        {
            if (wrapper.Id >= Textures.Count - 1)
                return -1;

            moveTexture(wrapper, wrapper.Id + 1);

            return wrapper.Id + 1;
        }

        public void moveTexture(TextureWrapper wrapper, int index)
        {
            // We have to convert to IMGDs and then build them back
            List<Imgd> imgds = getAsImgdList();

            Imgd tempImgd = imgds[wrapper.Id];
            imgds.RemoveAt(wrapper.Id);
            imgds.Insert(index, tempImgd);

            rebuildFile(imgds);
            loadTextures();
        }

        public void exportImage(int id)
        {
            BitmapSource bitmapImage = Textures[id].Texture.GetBimapSource();

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Export image as PNG";
            sfd.FileName = "Texture.png";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                MemoryStream memStream = new MemoryStream();
                AssimpGeneric.ExportBitmapSourceAsPng(bitmapImage, sfd.FileName);
            }
        }

        public void removeImage(int id)
        {
            List<Imgd> imgds = getAsImgdList();
            imgds.RemoveAt(id);
            rebuildFile(imgds);
            loadTextures();
        }

        public void replaceImage(int id)
        {
            List<Imgd> imgds = getAsImgdList();

            Imgd loadedImgd = ImageUtils.loadPngFileAsImgd();
            if (loadedImgd == null)
                return;

            imgds[id] = loadedImgd;

            rebuildFile(imgds);
            loadTextures();
        }

        public void addImage()
        {
            List<Imgd> imgds = getAsImgdList();

            Imgd loadedImgd = ImageUtils.loadPngFileAsImgd();
            if (loadedImgd == null)
                return;

            imgds.Add(loadedImgd);

            rebuildFile(imgds);
            loadTextures();
        }

        private List<Imgd> getAsImgdList()
        {
            List<Imgd> imgds = new List<Imgd>();
            foreach (ModelTexture.Texture texture in MdlxService.Instance.TextureFile.Images)
            {
                imgds.Add(Imgd.Create(texture.Size,
                                      texture.PixelFormat,
                                      texture.GetData(),
                                      texture.GetClut(),
                                      false));
            }
            return imgds;
        }

        private void rebuildFile(List<Imgd> imgds)
        {
            TextureFooterData tempFooter = MdlxService.Instance.TextureFile.TextureFooterData;

            ModelTexture modTex = new ModelTexture(imgds);
            modTex.TextureFooterData = tempFooter;

            Stream tempStream = new MemoryStream();
            modTex.Write(tempStream);

            MdlxService.Instance.TextureFile = ModelTexture.Read(tempStream);
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
