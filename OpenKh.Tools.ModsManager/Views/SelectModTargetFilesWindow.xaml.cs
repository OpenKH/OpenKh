using OpenKh.Tools.ModsManager.Models.ViewHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            VM.OnSearchHitsSelected(
                ((ListBox)sender).SelectedItems
                    .OfType<SearchHit>()
            );
        }
    }
}
