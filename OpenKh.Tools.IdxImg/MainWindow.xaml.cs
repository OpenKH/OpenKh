using OpenKh.Tools.IdxImg.ViewModels;
using System.Windows;

namespace OpenKh.Tools.IdxImg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IdxImgViewModel ViewModel
        {
            get => DataContext as IdxImgViewModel;
            set => DataContext = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new IdxImgViewModel();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {

        }
    }
}
