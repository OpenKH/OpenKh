using System.Windows;
using OpenKh.Tools.Kh2BattleEditor.ViewModels;

namespace OpenKh.Tools.Kh2BattleEditor.Views
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
