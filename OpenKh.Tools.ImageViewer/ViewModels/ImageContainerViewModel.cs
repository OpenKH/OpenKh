using OpenKh.Tools.ImageViewer.Services;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.ImageViewer.ViewModels
{
    public class ImageContainerViewModel : GenericListModel<ImageViewModel>
    {
        private readonly IImageContainer imageContainer;

        public ImageContainerViewModel(IImageContainer imageContainer) :
            this(imageContainer.Images.Select((image, index) => new ImageViewModel(image, index)))
        {
            this.imageContainer = imageContainer;
        }

        private ImageContainerViewModel(IEnumerable<ImageViewModel> imageViewModels) :
            base(imageViewModels)
        {

        }

        protected override ImageViewModel OnNewItem()
        {
            throw new System.NotImplementedException();
        }
    }
}
