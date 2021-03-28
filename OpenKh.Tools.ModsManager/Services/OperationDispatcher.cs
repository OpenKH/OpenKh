using OpenKh.Common;
using OpenKh.Tools.ModsManager.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    public class OperationDispatcher : IOperationDispatcher
    {
        private static readonly string[] _regionFallback = new[]
        {
            "us", "fm", "jp", "uk", "it", "fr", "es", "de"
        };

        private static readonly HashSet<string> _denyList = new HashSet<string>
        {
            "ovl_title.x",
            "ovl_shop.x",
            "ovl_movie.x",
            "ovl_gumibattle.x",
            "ovl_gumimenu.x",
            "ovl_gumimenu.x",
        };

        public int LoadFile(Stream outStream, string fileName)
        {
            if (GetFinalNamePath(fileName, out var finalFileName))
            {
                Log.Info($"Load file {finalFileName}");
                return File.OpenRead(finalFileName).Using(x =>
                {
                    x.CopyTo(outStream, 512 * 1024);
                    return (int)x.Length;
                });
            }

            Log.Warn($"File {fileName} not found, falling back");
            return 0;
        }

        public int GetFileSize(string fileName)
        {
            if (GetFinalNamePath(fileName, out var finalFileName))
                return (int)new FileInfo(finalFileName).Length;

            return 0;
        }

        private bool GetFinalNamePath(string fileName, out string finalFileName)
        {
            if (_denyList.Contains(fileName))
            {
                finalFileName = null;
                return false;
            }

            finalFileName = Path.Combine(ConfigurationService.GameModPath, fileName);
            if (File.Exists(finalFileName))
                return true;

            finalFileName = Path.Combine(ConfigurationService.GameDataLocation, fileName);
            if (File.Exists(finalFileName))
                return true;

            var region = GetRegion(fileName);
            if (region == null)
                return false;

            foreach (var fallback in _regionFallback)
            {
                var temptativeRegionalFallbackFileName = fileName
                    .Replace($"/{region}/", $"/{fallback}/")
                    .Replace($".a.{region}", $".a.{fallback}")
                    .Replace($".apdx", $".a.{fallback}");
                finalFileName = Path.Combine(ConfigurationService.GameModPath, temptativeRegionalFallbackFileName);
                if (File.Exists(finalFileName))
                    return true;

                finalFileName = Path.Combine(ConfigurationService.GameDataLocation, temptativeRegionalFallbackFileName);
                if (File.Exists(finalFileName))
                    return true;
            }

            finalFileName = null;
            return false;
        }

        private static string GetRegion(string fileName)
        {
            foreach (var region in Kh2.Constants.Regions)
            {
                if (fileName.Contains($"/{region}/") ||
                    fileName.EndsWith($".a.{region}"))
                    return region;
            }

            return null;
        }
    }
}
