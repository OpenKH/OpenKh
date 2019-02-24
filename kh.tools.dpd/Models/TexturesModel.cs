using kh.kh2;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Tools.Wpf;

namespace kh.tools.dpd.Models
{
	public class TextureModel : BaseNotifyPropertyChanged
	{
		public TextureModel(IImage image)
		{
			MasterImage = image;
		}

		public IImage MasterImage { get; }

		public string DisplayName => $"{MasterImage.Size.Width}x{MasterImage.Size.Height}";

		public BitmapSource Image => BitmapSource.Create(MasterImage.Size.Width, MasterImage.Size.Height,
			96.0, 96.0, PixelFormats.Bgra32, null, MasterImage.GetBitmap(), MasterImage.Size.Width * 4);
	}
}
