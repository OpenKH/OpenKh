using OpenKh.Kh2;
using OpenKh.Tools.BarEditor.ViewModels;
using OpenKh.Tools.Common;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

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

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                var fileName = files?.FirstOrDefault();
                if (string.IsNullOrEmpty(fileName))
                    return;

                var vm = DataContext as BarViewModel;
                vm.OpenFileName(fileName);
                vm.FileName = fileName;
            }
        }
    }
}
