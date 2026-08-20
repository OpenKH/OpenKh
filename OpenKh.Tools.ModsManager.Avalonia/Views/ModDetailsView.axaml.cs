using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class ModDetailsView : UserControl
    {
        public ModDetailsView()
        {
            InitializeComponent();
        }

        private void Link_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if ((sender as Control)?.Tag is string url && !string.IsNullOrEmpty(url))
            {
                using var proc = new Process();
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.FileName = url;
                proc.Start();
            }
        }
    }
}
