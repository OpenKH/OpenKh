using kh.tools.imgz.Views;
using Xe.Tools;

namespace kh.tools.imgd
{
	public class ImgzModule : IToolModule
	{
		public bool? ShowDialog(params object[] args)
		{
			return new ImgzView(args).ShowDialog();
		}
	}
}
