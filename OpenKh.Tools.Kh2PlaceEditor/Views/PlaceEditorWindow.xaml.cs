using OpenKh.Tools.Kh2PlaceEditor.ViewModels;
using System.Windows;

namespace OpenKh.Tools.Kh2PlaceEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PlaceEditorWindow : Window
    {
        public PlaceEditorWindow()
        {
            InitializeComponent();
            DataContext = new PlaceEditorViewModel();
        }
    }
}
