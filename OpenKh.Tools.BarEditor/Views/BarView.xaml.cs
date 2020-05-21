using OpenKh.Kh2;
using OpenKh.Tools.BarEditor.ViewModels;
using OpenKh.Tools.Common;
using System.Windows;
using System.Windows.Input;

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
            DataContext = new BarViewModel(desc);
        }

		private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var vm = DataContext as BarViewModel;
			vm.OpenItemCommand.Execute(vm.SelectedItem);
		}
	}
}
