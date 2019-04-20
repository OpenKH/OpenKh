using kh.kh2;
using kh.tools.dpd.Models;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace kh.tools.dpd.ViewModels
{
	public class TexturesViewModel : GenericListModel<TextureModel>
	{
		public TexturesViewModel() :
			this(null)
		{ }

		public TexturesViewModel(IEnumerable<Dpd.Texture> textures) :
			//base(textures.Select(x => new TextureModel(x)))
            base(null)
		{

		}

		protected override TextureModel OnNewItem()
		{
			throw new System.NotImplementedException();
		}
	}
}
