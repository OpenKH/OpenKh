using OpenKh.Kh2;
using OpenKh.Tools.ImgzViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.ImgzViewer.ViewModels
{
	public class ImagesViewModel : GenericListModel<ImageModel>
	{
		public ImagesViewModel() :
			this((IEnumerable<ImageModel>)null)
		{ }

		public ImagesViewModel(Stream stream) :
			this(Imgz.Read(stream))
		{
			OpenCommand = new RelayCommand(x => { }, x => false);
			SaveCommand = new RelayCommand(x =>
			{
				stream.Position = 0;
				stream.SetLength(0);
				Imgz.Save(stream, Items.Select(image => image.Imgd));
			});
		}

		public ImagesViewModel(IEnumerable<Imgd> images) :
			this(images.Select(x => new ImageModel(x)))
		{ }

		public ImagesViewModel(IEnumerable<ImageModel> images) :
			base(images)
		{
			OpenCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, ("IMGZ texture", "imz"));
				if (fd.ShowDialog() == true)
				{
					using (var stream = File.OpenRead(fd.FileName))
					{
						FileName = fd.FileName;
						Items.Clear();
						foreach (var item in Imgz.Read(stream))
						{
							Items.Add(new ImageModel(item));
						}

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
						Imgz.Save(stream, Items.Select(model => model.Imgd));
					}
				}
				else
				{
					SaveAsCommand.Execute(x);
				}
			}, x => true);

			SaveAsCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, ("IMGZ texture", "imz"));
				if (fd.ShowDialog() == true)
				{
					using (var stream = File.Open(fd.FileName, FileMode.Create))
					{
						Imgz.Save(stream, Items.Select(model => model.Imgd));
					}
				}
			}, x => true);

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
				fd.DefaultFileName = $"{Path.GetFileNameWithoutExtension(FileName)}_{SelectedIndex}.png";

				if (fd.ShowDialog() == true)
				{
					using (var fStream = File.OpenWrite(fd.FileName))
					{
						BitmapEncoder encoder = new PngBitmapEncoder();
						encoder.Frames.Add(BitmapFrame.Create(SelectedItem.Image));
						encoder.Save(fStream);
					}
				}
			}, x => IsItemSelected);

			ExportAllCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Folder, FileDialog.Type.ImagePng);

				if (fd.ShowDialog() == true)
				{
					var index = 0;
					foreach (var item in Items)
					{
						var defaultFileName = $"{Path.GetFileNameWithoutExtension(FileName)}_{index++}.png";
						var path = Path.Combine(fd.FileName, defaultFileName);

						using (var fStream = File.OpenWrite(path))
						{
							BitmapEncoder encoder = new PngBitmapEncoder();
							encoder.Frames.Add(BitmapFrame.Create(item.Image));
							encoder.Save(fStream);
						}
					}
				}
			}, x => IsItemSelected);

			ImportCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, FileDialog.Type.ImagePng);
				fd.DefaultFileName = $"{Path.GetFileNameWithoutExtension(FileName)}_{SelectedIndex}.png";

				if (fd.ShowDialog() == true)
				{
					using (var fStream = File.OpenRead(fd.FileName))
					{
						throw new NotImplementedException();
					}
				}
			}, x => false);
		}


		private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

		public string FileName { get; set; }

		public RelayCommand OpenCommand { get; set; }

		public RelayCommand SaveCommand { get; set; }

		public RelayCommand SaveAsCommand { get; set; }

		public RelayCommand ExitCommand { get; set; }

		public RelayCommand AboutCommand { get; set; }

		public RelayCommand AddItemCommand { get; set; }

		public RelayCommand ExportCommand { get; set; }

		public RelayCommand ExportAllCommand { get; set; }

		public RelayCommand ImportCommand { get; set; }

		protected override ImageModel OnNewItem()
		{
			throw new NotImplementedException();
		}
	}
}
