using OpenKh.Tools.BarEditor.Views;
using OpenKh.Tools.Common;
using Xe.Tools;

namespace OpenKh.Tools.BarEditor
{
    public class ToolModule : IToolModule<ToolInvokeDesc>
    {
        public bool? ShowDialog(ToolInvokeDesc args) => new BarView(args).ShowDialog();
    }
}
