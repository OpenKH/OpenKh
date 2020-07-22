using OpenKh.Tools.Common;
using OpenKh.Tools.Kh2MapCollisionEditor.Services;
using OpenKh.Tools.Kh2MapCollisionEditor.ViewModels;
using System.IO;
using System.Windows;

namespace OpenKh.Tools.Kh2MapCollisionEditor.Views
{
    /// <summary>
    /// Interaction logic for OpenProcessDialog.xaml
    /// </summary>
    public partial class OpenProcessDialog : Window
    {
        public OpenProcessDialog()
        {
            InitializeComponent();
            DataContext = new OpenProcessViewModel();
        }

        private OpenProcessViewModel ViewModel =>
            DataContext as OpenProcessViewModel;

        public ProcessStream SelectedProcessStream =>
            ProcessService.OpenPcsx2ProcessStream(ViewModel.SelectedProcess, ViewModel.Address);

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
