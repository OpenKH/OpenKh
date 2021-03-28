using OpenKh.Common;
using OpenKh.Tools.Kh2SystemEditor.ViewModels;
using System.IO;
using System.Windows;

namespace OpenKh.Tools.Kh2SystemEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SystemEditorView : Window
    {
        public SystemEditorView()
        {
            InitializeComponent();
            DataContext = new SystemEditorViewModel();
        }
    }
}
