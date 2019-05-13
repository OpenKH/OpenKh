using kh.Imaging;
using kh.kh2;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ImgdViewer.ViewModels
{
    public class ImageViewModel : BaseNotifyPropertyChanged
    {
		public ImageViewModel()
		{
			OpenCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, ("IMGD texture", "imd"));
				if (fd.ShowDialog() == true)
				{
					using (var stream = File.OpenRead(fd.FileName))
					{
						FileName = fd.FileName;
						LoadImgd(Imgd = Imgd.Read(stream));

						OnPropertyChanged(nameof(Image));
						OnPropertyChanged(nameof(SaveCommand));
						OnPropertyChanged(nameof(SaveAsCommand));
					}
				}
			}, x => true);

			SaveCommand = new RelayCommand(x =>
			{
				if (!string.IsNullOrEmpty(FileName))
				{
					using (var stream = File.Open(FileName, FileMode.Create))
					{
						Imgd.Write(stream);
					}
				}
				else
				{
					SaveAsCommand.Execute(x);
				}
			}, x => Imgd != null);

			SaveAsCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, ("IMGD texture", "imgd"));
				fd.DefaultFileName = FileName;

				if (fd.ShowDialog() == true)
				{
					using (var stream = File.Open(fd.FileName, FileMode.Create))
					{
						Imgd.Write(stream);
					}
				}
			}, x => Imgd != null);

			ExitCommand = new RelayCommand(x =>
			{
				Window.Close();
			}, x => true);

			AboutCommand = new RelayCommand(x =>
			{
				new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog();
			}, x => true);

			ExportCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, FileDialog.Type.ImagePng);
				fd.DefaultFileName = $"{Path.GetFileNameWithoutExtension(FileName)}.png";

				if (fd.ShowDialog() == true)
				{
					using (var fStream = File.OpenWrite(fd.FileName))
					{
						BitmapEncoder encoder = new PngBitmapEncoder();
						encoder.Frames.Add(BitmapFrame.Create(Image));
						encoder.Save(fStream);
					}
				}
			}, x => true);

			ImportCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, FileDialog.Type.ImagePng);
				fd.DefaultFileName = $"{Path.GetFileNameWithoutExtension(FileName)}.png";

				if (fd.ShowDialog() == true)
				{
					using (var fStream = File.OpenRead(fd.FileName))
					{
						throw new NotImplementedException();
					}
				}
			}, x => false);
		}

		public ImageViewModel(Stream stream) :
			this(Imgd.Read(stream))
		{
			SaveCommand = new RelayCommand(x =>
			{
				stream.Position = 0;
				stream.SetLength(0);
				Imgd.Write(stream);
			});
		}

		public ImageViewModel(Imgd imgd) :
			this()
		{
			LoadImgd(imgd);

			OpenCommand = new RelayCommand(x => { }, x => false);
		}

		private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

		public BitmapSource Image { get; set; }

		public Imgd Imgd { get; set; }

		public string FileName { get; set; }

		public RelayCommand OpenCommand { get; set; }

		public RelayCommand SaveCommand { get; set; }

		public RelayCommand SaveAsCommand { get; set; }

		public RelayCommand ExitCommand { get; set; }

		public RelayCommand AboutCommand { get; set; }

		public RelayCommand ExportCommand { get; set; }

		public RelayCommand ImportCommand { get; set; }

		private void LoadImgd(Imgd imgd)
		{
			LoadImage(imgd);
		}

		private void LoadImage(IImageRead imageRead)
		{
            var size = imageRead.Size;
            var data = imageRead.ToBgra32();
            Image = BitmapSource.Create(size.Width, size.Height, 96.0, 96.0, PixelFormats.Bgra32, null, data, size.Width * 4);
		}
	}
}
