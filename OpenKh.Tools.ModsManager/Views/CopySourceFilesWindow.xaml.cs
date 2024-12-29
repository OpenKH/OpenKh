using System.Windows;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for CopySourceFilesWindow.xaml
    /// </summary>
    public partial class CopySourceFilesWindow : Window
    {
        public CopySourceFilesWindow()
        {
            InitializeComponent();
            DataContext = VM = new CopySourceFilesVM();
        }

        public CopySourceFilesVM VM { get; }
    }
}
