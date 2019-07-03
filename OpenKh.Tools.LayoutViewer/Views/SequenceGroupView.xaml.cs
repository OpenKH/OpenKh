using System.Windows.Controls;
using System.Windows.Input;
using Xe.Tools.Wpf.Extensions;

namespace OpenKh.Tools.LayoutViewer.Views
{
    /// <summary>
    /// Interaction logic for SequenceGroupView.xaml
    /// </summary>
    public partial class SequenceGroupView : UserControl
    {
        public SequenceGroupView()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var parent = this.GetParent<TabControl>("SequenceEditorTabControl");
            if (parent != null)
                parent.GetControl<TabItem>("SequencePropertyTab")?.Focus();
        }
    }
}
