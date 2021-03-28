using OpenKh.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Tools;

namespace OpenKh.Tools.DpdViewer.Models
{
    public class TextureModel : BaseNotifyPropertyChanged
    {
        public TextureModel(IImageRead image)
        {
            MasterImage = image;
        }

        public IImageRead MasterImage { get; }

        public string DisplayName => $"{MasterImage.Size.Width}x{MasterImage.Size.Height}";

        public BitmapSource Image => BitmapSource.Create(MasterImage.Size.Width, MasterImage.Size.Height,
            96.0, 96.0, PixelFormats.Bgra32, null, MasterImage.ToBgra32(), MasterImage.Size.Width * 4);
    }
}
