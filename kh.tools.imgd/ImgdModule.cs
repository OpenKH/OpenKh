using kh.tools.imgd.Views;
using Xe.Tools;

namespace kh.tools.imgd
{
	public class ImgdModule : IToolModule
	{
		public bool? ShowDialog(params object[] args)
		{
			return new ImgdView(args).ShowDialog();
		}
	}
}
