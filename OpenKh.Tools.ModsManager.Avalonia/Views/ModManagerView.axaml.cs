using Avalonia.Controls;
using Avalonia.Input;
using OpenKh.Tools.ModsManager.ViewModels;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class ModManagerView : UserControl
    {
        public ModManagerView()
        {
            InitializeComponent();
        }

        private void ListBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                (DataContext as MainViewModel).RemoveModCommand.Execute(null);
            if (e.Key == Key.Space)
                (DataContext as MainViewModel).SelectedValue.Enabled = !(DataContext as MainViewModel).SelectedValue.Enabled;
        }
    }
}
