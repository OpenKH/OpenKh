using OpenKh.Kh2;
using OpenKh.Tools.BarEditor.ViewModels;
using OpenKh.Tools.Common;
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

namespace OpenKh.Tools.BarEditor.Views
{
	/// <summary>
	/// Interaction logic for BarView.xaml
	/// </summary>
	public partial class BarView : Window
	{
		public BarView()
		{
			InitializeComponent();
            DataContext = new BarViewModel();
        }

        public BarView(ToolInvokeDesc desc) :
            base()
        {
            var vm = DataContext as BarViewModel;
            DataContext = new BarViewModel(Bar.Read(desc.SelectedEntry.Stream));
        }

		private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var vm = DataContext as BarViewModel;
			vm.OpenItemCommand.Execute(vm.SelectedItem);
		}
	}
}
