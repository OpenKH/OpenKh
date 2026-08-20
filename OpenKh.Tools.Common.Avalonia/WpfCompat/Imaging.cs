// WPF compatibility shims for System.Windows.Media imaging types used by the
// shared ViewModels (ImageSource, BitmapImage with its staged
// BeginInit/EndInit protocol). The shimmed ImageSource implements Avalonia's
// IImage by delegating to the decoded Avalonia bitmap, so bindings like
// <Image Source="{Binding IconImage}"/> work without converters.
// See WpfEnums.cs for why these live in WPF's namespaces.

using Avalonia;
using Avalonia.Media;
using System.IO;
using AvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace System.Windows.Media
{
    public abstract class ImageSource : IImage
    {
        internal abstract AvaloniaBitmap Bitmap { get; }

        public Size Size => Bitmap?.Size ?? default;

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect) =>
            ((IImage)Bitmap)?.Draw(context, sourceRect, destRect);
    }
}

namespace System.Windows.Media.Imaging
{
    public enum BitmapCacheOption
    {
        Default = 0,
        OnDemand = 0,
        OnLoad = 1,
        None = 2,
    }

    [System.Flags]
    public enum BitmapCreateOptions
    {
        None = 0,
        PreservePixelFormat = 1,
        DelayCreation = 2,
        IgnoreColorProfile = 4,
        IgnoreImageCache = 8,
    }

    public class BitmapImage : ImageSource
    {
        private AvaloniaBitmap _bitmap;

        public BitmapImage()
        {
        }

        public Stream StreamSource { get; set; }
        public Uri UriSource { get; set; }
        public BitmapCacheOption CacheOption { get; set; }
        public BitmapCreateOptions CreateOptions { get; set; }

        internal override AvaloniaBitmap Bitmap => _bitmap;

        public void BeginInit()
        {
        }

        public void EndInit()
        {
            // Avalonia decodes eagerly, which matches BitmapCacheOption.OnLoad,
            // the only mode the shared sources use.
            if (StreamSource != null)
            {
                if (StreamSource.CanSeek)
                    StreamSource.Position = 0;
                _bitmap = new AvaloniaBitmap(StreamSource);
            }
            else if (UriSource != null)
            {
                _bitmap = new AvaloniaBitmap(UriSource.LocalPath);
            }
        }

        public void Freeze()
        {
            // Avalonia bitmaps are immutable and thread-safe already.
        }
    }
}
