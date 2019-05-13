using OpenKh.Kh2;
using OpenKh.Tools.ImgdViewer.ViewModels;
using System.IO;
using System.Windows;

namespace OpenKh.Tools.ImgdViewer.Views
{
	/// <summary>
	/// Interaction logic for ImgdView.xaml
	/// </summary>
	public partial class ImgdView : Window
	{
		public ImgdView()
		{
			InitializeComponent();
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
			DataContext = new ImageViewModel(stream);
		}

		private void Initialize(Imgd image)
		{
			DataContext = new ImageViewModel(image);
		}
	}
}
