using OpenKh.Tools.BarEditor.Views;
using Xe.Tools;

namespace OpenKh.Tools.BarEditor
{
	public class ToolModule : IToolModule
	{
		public bool? ShowDialog(params object[] args)
		{
			return new BarView(args).ShowDialog();
		}
	}
}
