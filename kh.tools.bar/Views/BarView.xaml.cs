using kh.kh2;
using kh.tools.bar.ViewModels;
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

namespace kh.tools.bar.Views
{
	/// <summary>
	/// Interaction logic for BarView.xaml
	/// </summary>
	public partial class BarView : Window
	{
		public BarView() :
			this(new object[0])
		{ }

		public BarView(params object[] args)
		{
			InitializeComponent();
			
			if (args.Length > 0)
			{
				if (args[0] is Stream stream)
				{
					DataContext = new BarViewModel(stream);
				}
				else if (args[0] is string)
				{
					using (var fStream = File.Open(args[0].ToString(), FileMode.Open))
					{
						DataContext = new BarViewModel(Bar.Open(fStream));
					}
				}
			}
			else
			{
				DataContext = new BarViewModel();
			}
		}

		private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var vm = DataContext as BarViewModel;
			vm.OpenItemCommand.Execute(vm.SelectedItem);
		}
	}
}
