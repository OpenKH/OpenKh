using OpenKh.Tools.DpdViewer.ViewModels;
using System.Windows;

namespace OpenKh.Tools.DpdViewer.Views
{
    /// <summary>
    /// Interaction logic for DpdView.xaml
    /// </summary>
    public partial class DpdView : Window
    {
        public DpdView()
        {
            InitializeComponent();
            DataContext = new DpdViewModel();
        }
    }
}
