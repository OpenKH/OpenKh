using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for ModDetailsView.xaml
    /// </summary>
    public partial class ModDetailsView : UserControl
    {
        public ModDetailsView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            using var proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = e.Uri.AbsoluteUri;
            proc.Start();
        }
    }
}
