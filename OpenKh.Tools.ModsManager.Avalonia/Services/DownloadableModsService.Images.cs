using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Globalization;
using System.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    // Avalonia implementation of the text-avatar rendering; the WPF
    // counterpart lives in OpenKh.Tools.ModsManager/Services/
    // DownloadableModsService.Images.Wpf.cs.
    public partial class DownloadableModsService
    {
        private System.Windows.Media.Imaging.BitmapImage RenderTextAvatar(string initials, byte r, byte g, byte b)
        {
            using var renderTarget = new RenderTargetBitmap(new PixelSize(64, 64), new Vector(96, 96));
            using (var context = renderTarget.CreateDrawingContext())
            {
                context.FillRectangle(
                    new SolidColorBrush(Color.FromRgb(r, g, b)),
                    new Rect(0, 0, 64, 64));

                var formattedText = new FormattedText(
                    initials,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    24,
                    Brushes.White);

                context.DrawText(formattedText,
                    new Point((64 - formattedText.Width) / 2, (64 - formattedText.Height) / 2));
            }

            using var memoryStream = new MemoryStream();
            renderTarget.Save(memoryStream);
            memoryStream.Position = 0;

            var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
