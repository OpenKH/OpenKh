using kh.tools.bar.Views;
using Xe.Tools;

namespace kh.tools.bar
{
	public class ToolModule : IToolModule
	{
		public bool? ShowDialog(params object[] args)
		{
			return new BarView(args).ShowDialog();
		}
	}
}
