using OpenKh.Tools.BbsEventTableEditor.ViewModels;
using System.Windows;

namespace OpenKh.Tools.BbsEventTableEditor.Views
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
