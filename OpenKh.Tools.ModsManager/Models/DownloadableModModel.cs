using System.Windows;

namespace OpenKh.Tools.ModsManager.Models
{
    public class DownloadableModModel
    {
        public string Repository { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Game { get; set; }
        public string IconImageSource { get; set; }
        public string PreviewImageSource { get; set; }
        public Visibility UpdateVisibility => Visibility.Collapsed;
    }
}
