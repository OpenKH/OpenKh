using OpenKh.Tools.ImgdViewer.Views;
using Xe.Tools;

namespace OpenKh.Tools.ImgdViewer
{
	public class ImgdModule : IToolModule
	{
		public bool? ShowDialog(params object[] args)
		{
			return new ImgdView(args).ShowDialog();
		}
	}
}
