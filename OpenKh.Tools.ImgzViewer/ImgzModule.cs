using OpenKh.Tools.ImgzViewer.Views;
using Xe.Tools;

namespace OpenKh.Tools.ImgdViewer
{
	public class ImgzModule : IToolModule
	{
		public bool? ShowDialog(params object[] args)
		{
			return new ImgzView(args).ShowDialog();
		}
	}
}
