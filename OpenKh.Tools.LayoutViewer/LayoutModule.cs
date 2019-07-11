using OpenKh.Tools.Common;
using OpenKh.Tools.LayoutViewer.Views;
using Xe.Tools;

namespace OpenKh.Tools.ImgdViewer
{
    public class LayoutModule : IToolModule<ToolInvokeDesc>
    {
        public bool? ShowDialog(ToolInvokeDesc desc)
        {
            return new MainWindow(desc).ShowDialog();
        }
    }
}
