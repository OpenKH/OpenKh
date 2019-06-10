using System.Windows;

namespace OpenKh.Tools.LayoutViewer.Views
{
    /// <summary>
    /// Interaction logic for LayoutSelectionDialog.xaml
    /// </summary>
    public partial class LayoutSelectionDialog : Window
    {
        public LayoutSelectionDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
