using kh.kh2;
using kh.tools.imgd.ViewModels;
using System.IO;
using System.Windows;

namespace kh.tools.imgd.Views
{
	/// <summary>
	/// Interaction logic for ImgdView.xaml
	/// </summary>
	public partial class ImgdView : Window
	{
		public ImgdView()
		{
			DataContext = new ImageViewModel();
		}

		public ImgdView(object[] args) :
			this()
		{
			if (args.Length > 0)
			{
				if (args[0] is Imgd imgd)
					Initialize(imgd);
				else if (args[0] is Stream stream)
					Initialize(stream);
			}
		}

		public ImgdView(Imgd imgd) :
			this()
		{
			Initialize(imgd);
		}

		public ImgdView(Stream stream) :
			this()
		{
			Initialize(stream);
		}

		private void Initialize(Stream stream)
		{
			DataContext = new ImageViewModel(new Imgd(stream));
		}

		private void Initialize(Imgd image)
		{
			DataContext = new ImageViewModel(image);
		}
	}
}
