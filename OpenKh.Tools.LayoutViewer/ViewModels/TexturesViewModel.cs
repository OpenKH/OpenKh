using OpenKh.Kh2;
using OpenKh.Tools.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Xe.Tools.Wpf.Models;
using static OpenKh.Tools.LayoutViewer.ViewModels.TexturesViewModel;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class TexturesViewModel : GenericListModel<TextureModel>
    {
        public class TextureModel
        {
            private readonly Imgd imgd;

            public TextureModel(int index, Imgd imgd)
            {
                this.imgd = imgd;
                Index = index;
                Image = imgd.GetBimapSource();
            }

            public int Index { get; }

            public ImageSource Image { get; }

            public override string ToString() => $"{Index}: image {imgd.Size.Width}x{imgd.Size.Height}";
        }

        public TexturesViewModel(IEnumerable<Imgd> images) :
            this(images.Select((x, i) => new TextureModel(i, x)))
        {

        }

        public TexturesViewModel(IEnumerable<TextureModel> list) :
            base(list)
        {
        }

        protected override TextureModel OnNewItem()
        {
            throw new System.NotImplementedException();
        }
    }
}
