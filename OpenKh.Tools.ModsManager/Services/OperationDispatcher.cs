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
            // KH1
            "dkmovie.x",
            "dktitle.x",
            "gb.x",
            "wm.x",
            "xl_limit.x",
            "xs_bambi.x",
            "xs_dh_break.x",
            "xs_dumbo.x",
            "xs_genie.x",
            "xs_mushu.x",
            "xs_simba.x",
            "xs_tink.x",
            // KH2
            "ovl_title.x",
            "ovl_shop.x",
            "ovl_movie.x",
            "ovl_gumibattle.x",
            "ovl_gumimenu.x",
            "ovl_gumimenu.x",
        };

        public int LoadFile(Stream outStream, string fileName)
        {
            int status = GetFinalNamePath(fileName, out var finalFileName);
            if (status == 0)
            {
                Log.Info($"Load file {finalFileName}");
                return File.OpenRead(finalFileName).Using(x =>
                {
                    x.CopyTo(outStream, 512 * 1024);
                    return (int)x.Length;
                });
            }
            else if (status == -1)
            {
                Log.Warn($"File {fileName} is on the deny list, falling back");
            }
            else
            {
                Log.Warn($"File {fileName} not found, falling back");
            }
                return 0;
        }

        public int GetFileSize(string fileName)
        {
            if (GetFinalNamePath(fileName, out var finalFileName) == 0)
                return (int)new FileInfo(finalFileName).Length;

            return 0;
        }

        private int GetFinalNamePath(string fileName, out string finalFileName)
        {
            if (_denyList.Contains(fileName))
            {
                finalFileName = null;
                return -1;  // Error Code -1: denied
            }

            finalFileName = Path.Combine(ConfigurationService.GameModPath, ConfigurationService.LaunchGame, fileName);
            if (File.Exists(finalFileName))
                return 0;

            finalFileName = Path.Combine(ConfigurationService.GameDataLocation, ConfigurationService.LaunchGame, fileName);
            if (File.Exists(finalFileName))
                return 0;

            var region = GetRegion(fileName);
            if (region == null)
                return -2;  // Error Code -2: file not found

            foreach (var fallback in _regionFallback)
            {
                var temptativeRegionalFallbackFileName = fileName
                    .Replace($"/{region}/", $"/{fallback}/")
                    .Replace($".a.{region}", $".a.{fallback}")
                    .Replace($".apdx", $".a.{fallback}");
                finalFileName = Path.Combine(ConfigurationService.GameModPath, ConfigurationService.LaunchGame, temptativeRegionalFallbackFileName);
                if (File.Exists(finalFileName))
                    return 0;

                finalFileName = Path.Combine(ConfigurationService.GameDataLocation, ConfigurationService.LaunchGame, temptativeRegionalFallbackFileName);
                if (File.Exists(finalFileName))
                    return 0;
            }

            finalFileName = null;
            return -2;  // Error Code -2: file not found
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
