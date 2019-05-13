using OpenKh.Imaging;
using OpenKh.Kh2;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Tools;
using Xe.Tools.Wpf;

namespace OpenKh.Tools.ImgzViewer.Models
{
	public class ImageModel : BaseNotifyPropertyChanged
	{
		private Imgd imgd;

		public ImageModel(Imgd imgd)
		{
			Imgd = imgd;
		}

		public Imgd Imgd
		{
			get => imgd;
			set => LoadImgd(imgd = value);
		}

		public BitmapSource Image { get; set; }

		public string DisplayName => $"{Image.PixelWidth}x{Image.PixelHeight}";

        private void LoadImgd(Imgd imgd)
        {
            LoadImage(imgd);
        }

        private void LoadImage(IImageRead imageRead)
        {
            var size = imageRead.Size;
            var data = imageRead.ToBgra32();
            Image = BitmapSource.Create(size.Width, size.Height, 96.0, 96.0, PixelFormats.Bgra32, null, data, size.Width * 4);
            OnPropertyChanged(nameof(Image));
        }
	}
}
