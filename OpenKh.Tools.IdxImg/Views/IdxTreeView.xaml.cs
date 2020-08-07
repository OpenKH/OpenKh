using OpenKh.Tools.IdxImg.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace OpenKh.Tools.IdxImg.Views
{
    /// <summary>
    /// Interaction logic for IdxTreeView.xaml
    /// </summary>
    public partial class IdxTreeView : UserControl
    {
        public IdxTreeView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ITreeSelectedItem treeSelectedItem)
            {
                treeSelectedItem.TreeSelectedItem = e.NewValue;
            }
        }
    }
}
