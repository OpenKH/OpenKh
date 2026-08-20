using Avalonia.Controls;
using OpenKh.Tools.Common.Avalonia;
using OpenKh.Tools.ModsManager.Models.ViewHelper;
using System.Linq;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class SelectModTargetFilesWindow : DialogWindowBase
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
