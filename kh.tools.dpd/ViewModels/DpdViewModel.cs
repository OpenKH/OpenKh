using kh.kh2;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace kh.tools.dpd.ViewModels
{
	public class DpdViewModel : BaseNotifyPropertyChanged
	{
		private Dpd dpd;

		public DpdViewModel()
		{
			OpenCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, ("DPD effect", "dpd"));
				if (fd.ShowDialog() == true)
				{
					using (var stream = File.OpenRead(fd.FileName))
					{
						FileName = fd.FileName;
						Dpd = new Dpd(stream);

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
						throw new NotImplementedException();
					}
				}
				else
				{
					SaveAsCommand.Execute(x);
				}
			}, x => true);

			SaveAsCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, ("DPD effect", "dpd"));
				if (fd.ShowDialog() == true)
				{
					using (var stream = File.Open(fd.FileName, FileMode.Create))
					{
						throw new NotImplementedException();
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
		}

		private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

		public string FileName { get; set; }

		public RelayCommand OpenCommand { get; set; }

		public RelayCommand SaveCommand { get; set; }

		public RelayCommand SaveAsCommand { get; set; }

		public RelayCommand ExitCommand { get; set; }

		public RelayCommand AboutCommand { get; set; }

		public Dpd Dpd
		{
			get => dpd;
			set
			{
				dpd = value;
				Textures = new TexturesViewModel(dpd.Textures);
				OnPropertyChanged(nameof(Textures));
			}
		}

		public TexturesViewModel Textures { get; private set; }
	}
}
