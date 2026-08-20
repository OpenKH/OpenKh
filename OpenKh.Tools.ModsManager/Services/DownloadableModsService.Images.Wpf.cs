using System.Windows.Media.Imaging;

namespace OpenKh.Tools.ModsManager.Services
{
    // WPF implementation of the text-avatar rendering used for mods without
    // an icon. The Avalonia build excludes this file and compiles its own
    // implementation (OpenKh.Tools.ModsManager.Avalonia/Services/
    // DownloadableModsService.Images.cs) instead.
    public partial class DownloadableModsService
    {
        private BitmapImage RenderTextAvatar(string initials, byte r, byte g, byte b)
        {
            var drawingVisual = new System.Windows.Media.DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b)),
                    null,
                    new System.Windows.Rect(0, 0, 64, 64));

                var formattedText = new System.Windows.Media.FormattedText(
                    initials,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new System.Windows.Media.Typeface("Arial"),
                    24,
                    System.Windows.Media.Brushes.White,
                    1.0);

                drawingContext.DrawText(formattedText,
                    new System.Windows.Point((64 - formattedText.Width) / 2, (64 - formattedText.Height) / 2));
            }

            var renderTarget = new System.Windows.Media.Imaging.RenderTargetBitmap(
                64, 64, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);
            renderTarget.Freeze();

            // Convert RenderTargetBitmap to BitmapImage
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTarget));

            using var memoryStream = new System.IO.MemoryStream();
            encoder.Save(memoryStream);
            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
