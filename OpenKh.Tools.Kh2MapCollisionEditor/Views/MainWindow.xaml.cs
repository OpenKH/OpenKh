using OpenKh.Tools.Kh2MapCollisionEditor.ViewModels;
using System.Windows;

namespace OpenKh.Tools.Kh2MapCollisionEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
