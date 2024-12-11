using OpenKh.Tools.ModsManager.Models.ViewHelper;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for SelectModTargetFilesWindow.xaml
    /// </summary>
    public partial class SelectModTargetFilesWindow : Window
    {
        public SelectModTargetFilesWindow()
        {
            InitializeComponent();
            DataContext = VM = new SelectModTargetFilesVM();
        }

        public SelectModTargetFilesVM VM { get; }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VM.SearchHitSelectedList = ((ListBox)sender).SelectedItems
                .OfType<SearchHit>()
                .ToArray();
        }
    }
}
