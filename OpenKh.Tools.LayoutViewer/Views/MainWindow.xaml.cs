using OpenKh.Tools.Common;
using OpenKh.Tools.LayoutViewer.ViewModels;
using System.Windows;

namespace OpenKh.Tools.LayoutViewer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel()
            {
                OnControlChanged = control =>
                {
                    content.Children.Clear();
                    content.Children.Add(control);
                }
            };

            DataContext = vm;
        }

        public MainWindow(ToolInvokeDesc desc) :
            this()
        {
            var vm = DataContext as MainViewModel;
            vm.OpenToolDesc(desc);
        }
    }
}
