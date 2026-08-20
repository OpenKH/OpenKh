using OpenKh.Tools.Common.Avalonia;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class CopySourceFilesWindow : DialogWindowBase
    {
        public CopySourceFilesWindow()
        {
            InitializeComponent();
            DataContext = VM = new CopySourceFilesVM();
        }

        public CopySourceFilesVM VM { get; }
    }
}
