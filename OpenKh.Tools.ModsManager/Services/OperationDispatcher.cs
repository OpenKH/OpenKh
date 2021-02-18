using OpenKh.Common;
using System.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    public class OperationDispatcher : IOperationDispatcher
    {
        public bool LoadFile(Stream outStream, string fileName)
        {
            var realFileName = Path.Combine(ConfigurationService.OutputModPath, fileName);
            if (!File.Exists(realFileName))
                realFileName = Path.Combine(ConfigurationService.GameAssetPath, fileName);
            if (!File.Exists(realFileName))
                return false;

            File.OpenRead(realFileName).Using(x => x.CopyTo(outStream));
            return true;
        }

        public int GetFileSize(string fileName)
        {
            var realFileName = Path.Combine(ConfigurationService.OutputModPath, fileName);
            if (!File.Exists(realFileName))
                realFileName = Path.Combine(ConfigurationService.GameAssetPath, fileName);
            if (!File.Exists(realFileName))
                return 0;

            return (int)new FileInfo(realFileName).Length;
        }
    }
}
