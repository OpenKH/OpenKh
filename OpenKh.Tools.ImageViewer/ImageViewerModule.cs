using OpenKh.Tools.Common;
using OpenKh.Tools.ImageViewer.Views;
using Xe.Tools;

namespace OpenKh.Tools.ImageViewer
{
    public class ImageViewerModule : IToolModule<ToolInvokeDesc>
    {
        public bool? ShowDialog(ToolInvokeDesc desc)
        {
            return new MainWindow(desc).ShowDialog();
        }
    }
}
