using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static OpenKh.Kh2.Dpd;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public class EffectTest_VM : NotifyPropertyChangedBase
    {
        public List<Texture> textures { get; set; }
        public int currentTexture { get; set; }
        public BitmapSource _bitmapImage { get; set; }
        public BitmapSource BitmapImage
        {
            get { return _bitmapImage; }
            set
            {
                _bitmapImage = value;
                OnPropertyChanged("BitmapImage");
            }
        }

        public void loadTextures()
        {
            foreach (var iDpd in ApdxService.Instance.PaxFile.DpxPackage.DpdList)
            {
                List<Texture> tempList = iDpd.TexturesList;
                if (tempList.Count > 0)
                {
                    textures.AddRange(tempList);
                }
            }

            loadTexture(0);
        }

        public void loadTexture(int i)
        {
            const double dpi = 96.0;

            if (i < 0)
            {
                currentTexture = 0;
            }
            else if (i >= textures.Count)
            {
                currentTexture = textures.Count - 1;
            }
            else
            {
                currentTexture = i;
            }

            BitmapImage = BitmapSource.Create(textures[currentTexture].Size.Width, textures[currentTexture].Size.Height, dpi, dpi, PixelFormats.Bgra32, null, textures[currentTexture].GetBitmap(), textures[currentTexture].Size.Width * 4);
        }

        public EffectTest_VM()
        {
            textures = new List<Texture>();
        }

        /*public Texture_VM() { }
        public Texture_VM(ModelTexture.Texture texture)
        {
            this.texture = texture;
            bitmapImage = texture.GetBimapSource();
        }

        public void ExportImage()
        {
            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Export Image as PNG";
            sfd.FileName = "Texture";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                AssimpUtils.AssimpGeneric.ExportBitmapSourceAsPng(this.bitmapImage, sfd.FileName);
            }
        }*/
    }
}
