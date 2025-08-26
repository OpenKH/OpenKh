using System.Windows.Media.Imaging;

namespace OpenKh.Tools.ModsManager.Models
{
    public class DownloadableModModel
    {
        public string Repo { get; set; }
        public string Title { get; set; }
        public string OriginalAuthor { get; set; }
        public string Description { get; set; }
        public string Game { get; set; }
        public BitmapImage IconImage { get; set; }
        public BitmapImage ScreenshotImageSource { get; set; }
        public bool IsInstalled { get; set; }

        public string RepoOwner => Repo?.Split('/')[0];
        public string RepoName => Repo?.Split('/').Length > 1 ? Repo?.Split('/')[1] : string.Empty;
    }
}
