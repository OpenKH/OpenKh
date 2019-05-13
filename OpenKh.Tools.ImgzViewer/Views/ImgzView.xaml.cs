using OpenKh.Kh2;
using OpenKh.Tools.ImgzViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OpenKh.Tools.ImgzViewer.Views
{
	/// <summary>
	/// Interaction logic for ImgzView.xaml
	/// </summary>
	public partial class ImgzView : Window
	{
		public ImgzView()
		{
			InitializeComponent();
			DataContext = new ImagesViewModel();
		}

		public ImgzView(object[] args) :
			this()
		{
			if (args.Length > 0)
			{
				if (args[0] is Stream stream)
					Initialize(stream);
			}
		}
		
		public ImgzView(Stream stream) :
			this()
		{
			Initialize(stream);
		}

		private void Initialize(Stream stream)
		{
			DataContext = new ImagesViewModel(stream);
		}
	}
}
