using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public class M_EffectDpdTexture_VM
    {

        public Dpd ThisDpd { get; set; }
        public ObservableCollection<DpdTextureWrapper> TextureWrappers { get; set; }

        public M_EffectDpdTexture_VM(Dpd dpd)
        {
            ThisDpd = dpd;
            TextureWrappers = new ObservableCollection<DpdTextureWrapper>();
            loadWrappers();
        }

        public void loadWrappers()
        {
            if (ThisDpd?.TexturesList == null || ThisDpd.TexturesList.Count < 0)
                return;

            TextureWrappers.Clear();
            for (int i = 0; i < ThisDpd.TexturesList.Count; i++)
            {
                DpdTextureWrapper wrapper = new DpdTextureWrapper();
                wrapper.Id = i;
                wrapper.Name = "Texture " + i;
                wrapper.Texture = ThisDpd.TexturesList[i];
                wrapper.SizeX = wrapper.Texture.Size.Width;
                wrapper.SizeY = wrapper.Texture.Size.Height;

                TextureWrappers.Add(wrapper);
            }
        }

        public BitmapSource getTexture(int index)
        {
            const double dpi = 96.0;

            Dpd.Texture texture = TextureWrappers[index].Texture;

            return BitmapSource.Create(texture.Size.Width, texture.Size.Height, dpi, dpi, PixelFormats.Bgra32, null, texture.GetBitmap(), texture.Size.Width * 4);
        }

        public class DpdTextureWrapper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int SizeX { get; set; }
            public int SizeY { get; set; }
            public Dpd.Texture Texture { get; set; }
        }
    }
}
