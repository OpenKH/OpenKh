using OpenKh.Tools.ModManager.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace OpenKh.Tools.ModManager.Views
{
    /// <summary>
    /// Interaction logic for ModManagerView.xaml
    /// </summary>
    public partial class ModManagerView : UserControl
    {
        public ModManagerView()
        {
            InitializeComponent();
        }

        private void ListBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
                (DataContext as MainViewModel).RemoveModCommand.Execute(null);
            if (e.Key == System.Windows.Input.Key.Space)
                (DataContext as MainViewModel).SelectedValue.Enabled = !(DataContext as MainViewModel).SelectedValue.Enabled;
        }
    }
}
