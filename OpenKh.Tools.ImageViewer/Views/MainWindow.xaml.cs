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
            (DataContext as ImageViewerViewModel).LoadImage(@"D:\Hacking\KHBBS\DUMP_EU\BBS\arc\system\fonten.arc");
        }
    }
}
