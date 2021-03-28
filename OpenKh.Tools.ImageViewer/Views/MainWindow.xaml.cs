using OpenKh.Tools.Common;
using OpenKh.Tools.ImageViewer.ViewModels;
using System.Windows;
using System.Linq;

namespace OpenKh.Tools.ImageViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ImageViewerViewModel();
        }

        public MainWindow(ToolInvokeDesc toolInvokeDesc) : this()
        {
            (DataContext as ImageViewerViewModel).LoadImage(toolInvokeDesc);
        }

        private void DropAction(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                var fileName = files?.FirstOrDefault();
                if (string.IsNullOrEmpty(fileName))
                    return;

                var vm = DataContext as ImageViewerViewModel;
                vm.LoadImage(fileName);
                vm.FileName = fileName;
            }
        }
    }
}
