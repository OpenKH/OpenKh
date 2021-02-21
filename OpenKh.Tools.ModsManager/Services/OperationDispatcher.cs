using OpenKh.Common;
using System.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    public class OperationDispatcher : IOperationDispatcher
    {
        private static readonly string[] _regionFallback = new[]
        {
            "us", "fm", "jp", "uk", "it", "fr", "es", "de"
        };

        public bool LoadFile(Stream outStream, string fileName)
        {
            //fileName = fileName.Replace("menu/it/title", "menu/fm/title");
            //fileName = fileName.Replace("menu/it/pause", "menu/fm/pause");
            fileName = fileName.Replace("menu/it/save", "menu/fm/save");

            if (LoadFileInternal(outStream, fileName))
                return true;

            var region = GetRegion(fileName);
            if (region == null)
                return false;

            foreach (var fallback in _regionFallback)
            {
                if (LoadFileInternal(outStream, fileName.Replace($"/{region}/", $"/{fallback}/")))
                    return true;
            }

            return false;
        }

        public int GetFileSize(string fileName)
        {
            var realFileName = Path.Combine(ConfigurationService.GameModPath, fileName);
            if (!File.Exists(realFileName))
                realFileName = Path.Combine(ConfigurationService.GameDataLocation, fileName);
            if (!File.Exists(realFileName))
                return 0;

            return (int)new FileInfo(realFileName).Length;
        }

        private bool LoadFileInternal(Stream outStream, string fileName)
        {
            var realFileName = Path.Combine(ConfigurationService.GameModPath, fileName);
            if (!File.Exists(realFileName))
                realFileName = Path.Combine(ConfigurationService.GameDataLocation, fileName);
            if (!File.Exists(realFileName))
                return false;

            File.OpenRead(realFileName).Using(x => x.CopyTo(outStream));
            return true;
        }

        private static string GetRegion(string fileName)
        {
            foreach (var region in Kh2.Constants.Regions)
            {
                var indexOfRegion = fileName.IndexOf($"/{region}/");
                if (indexOfRegion >= 0)
                    return region;
            }

            return null;
        }
    }
}
