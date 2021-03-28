using OpenKh.Imaging;
using OpenKh.Tools.Common;
using System.Windows.Media.Imaging;
using Xe.Tools;

namespace OpenKh.Tools.ImageViewer.ViewModels
{
    public class ImageViewModel : BaseNotifyPropertyChanged
    {
        public ImageViewModel(IImageRead image, int index = -1)
        {
            Source = image;
            Bitmap = Source.GetBimapSource();
            Index = index;
        }

        public IImageRead Source { get; }
        public BitmapSource Bitmap { get; }
        public int Index { get; }

        public string Name => $"#{Index}";

        public int Width => Source.Size.Width;
        public int Height => Source.Size.Height;

        public string Size => $"{Width}x{Height}";
        public string Format => Source.PixelFormat.ToString();

        public string ImageSize => Source != null ? $"{Source.Size.Width}x{Source.Size.Height}" : "-";
        public string ImageFormat => Source?.PixelFormat.ToString();

        public override string ToString() => $"{Name} {Size} {Format}";
    }
}
