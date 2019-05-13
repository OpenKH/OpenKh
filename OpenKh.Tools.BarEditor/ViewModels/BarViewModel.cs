using kh.kh2;
using OpenKh.Tools.BarEditor.Models;
using OpenKh.Tools.BarEditor.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools.Models;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.BarEditor.ViewModels
{
    public class BarViewModel : GenericListModel<BarEntryModel>
	{
		public BarViewModel() : this((IEnumerable<BarEntryModel>)null) { }

		public BarViewModel(Stream stream) :
			this(Bar.Open(stream))
		{
			OpenCommand = new RelayCommand(x => { }, x => false);
			SaveCommand = new RelayCommand(x =>
			{
				stream.Position = 0;
				stream.SetLength(0);
				Bar.Save(stream, Items.Select(item => item.Entry));
			});
		}

		public BarViewModel(IEnumerable<Bar.Entry> list) :
			this(list.Select(x => new BarEntryModel(x)))
		{ }

		public BarViewModel(IEnumerable<BarEntryModel> list) :
			base(list)
		{
			Types = new EnumModel<Bar.EntryType>();

			OpenCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open, FileDialog.Type.Any);
				if (fd.ShowDialog() == true)
				{
					using (var stream = File.Open(fd.FileName, FileMode.Open))
					{
						FileName = fd.FileName;
						Items.Clear();
						foreach (var item in Bar.Open(stream))
						{
							Items.Add(new BarEntryModel(item));
						}
					}
				}
			}, x => true);

			SaveCommand = new RelayCommand(x =>
			{
				if (!string.IsNullOrEmpty(FileName))
				{
					using (var stream = File.Open(FileName, FileMode.Create))
					{
						Bar.Save(stream, Items.Select(item => item.Entry));
					}
				}
				else
				{
					SaveAsCommand.Execute(x);
				}
			}, x => true);

			SaveAsCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save, FileDialog.Type.Any);
				if (fd.ShowDialog() == true)
				{
					using (var stream = File.Open(fd.FileName, FileMode.Create))
					{
						Bar.Save(stream, Items.Select(item => item.Entry));
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

			OpenItemCommand = new RelayCommand(x =>
			{
				try
				{
					ToolsLoaderService.OpenTool(SelectedItem.Entry.Stream, SelectedItem.Type);
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				OnPropertyChanged(nameof(SelectedItem));
			}, x => true);

			ExportCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Save);
                fd.DefaultFileName = $"{SelectedItem.Entry.Name}.bin";

				if (fd.ShowDialog() == true)
				{
					using (var fStream = File.OpenWrite(fd.FileName))
					{
						SelectedItem.Entry.Stream.Position = 0;
						SelectedItem.Entry.Stream.CopyTo(fStream);
					}
				}
			}, x => IsItemSelected);

            ExportAllCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(Window, FileDialog.Behavior.Folder);

                if (fd.ShowDialog() == true)
                {
                    var basePath = fd.FileName;
                    foreach (var item in Items.Select(item => item.Entry))
                    {
                        var fileName = $"{item.Name}.bin";
                        using (var fStream = File.OpenWrite(Path.Combine(basePath, fileName)))
                        {
                            item.Stream.Position = 0;
                            item.Stream.CopyTo(fStream);
                        }
                    }
                }
            }, x => true);

            ImportCommand = new RelayCommand(x =>
			{
				var fd = FileDialog.Factory(Window, FileDialog.Behavior.Open);
				if (fd.ShowDialog() == true)
				{
					using (var fStream = File.OpenRead(fd.FileName))
					{
						var memStream = new MemoryStream((int)fStream.Length);
						fStream.CopyTo(memStream);
						SelectedItem.Entry.Stream = memStream;
					}

					OnPropertyChanged(nameof(SelectedItem.Size));
				}
			}, x => IsItemSelected);
			SearchCommand = new RelayCommand(x => { }, x => false);
		}

		private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

		private string FileName { get; set; }

		public RelayCommand OpenCommand { get; set; }

		public RelayCommand SaveCommand { get; set; }

		public RelayCommand SaveAsCommand { get; set; }

		public RelayCommand ExitCommand { get; set; }

		public RelayCommand AboutCommand { get; set; }

        public RelayCommand ExportCommand { get; set; }

        public RelayCommand ExportAllCommand { get; set; }

        public RelayCommand ImportCommand { get; set; }

		public RelayCommand OpenItemCommand { get; set; }

		public RelayCommand SearchCommand { get; set; }

		public EnumModel<Bar.EntryType> Types { get; set; }

		public string ExportFileName => IsItemSelected ? $"{SelectedItem?.DisplayName}.bin" : "(no file selected)";

		protected override BarEntryModel OnNewItem()
		{
			return new BarEntryModel(new Bar.Entry()
			{
				Stream = new MemoryStream()
			});
		}

		protected override void OnSelectedItem(BarEntryModel item)
		{
			base.OnSelectedItem(item);

			ExportCommand.CanExecute(SelectedItem);
			ImportCommand.CanExecute(SelectedItem);
			OpenItemCommand.CanExecute(SelectedItem);

			OnPropertyChanged(nameof(ExportFileName));
		}
	}
}
