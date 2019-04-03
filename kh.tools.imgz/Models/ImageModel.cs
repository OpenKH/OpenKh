using kh.kh2;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Tools;
using Xe.Tools.Wpf;

namespace kh.tools.imgz.Models
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
			LoadImage(imgd.GetBitmap(), imgd.Size.Width, imgd.Size.Height);
		}

		private void LoadImage(byte[] data, int width, int height)
		{
			Image = BitmapSource.Create(width, height, 96.0, 96.0, PixelFormats.Bgra32, null, data, width * 4);
			OnPropertyChanged(nameof(Image));
		}
	}
}
