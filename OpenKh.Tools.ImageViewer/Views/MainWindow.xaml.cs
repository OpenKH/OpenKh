using OpenKh.Tools.Common;
using OpenKh.Tools.ImageViewer.ViewModels;
using System.Windows;

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
    }
}
