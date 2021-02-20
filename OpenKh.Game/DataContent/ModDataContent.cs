using OpenKh.Common;
using OpenKh.Game.Infrastructure;
using System.IO;

namespace OpenKh.Game.DataContent
{
    public class ModDataContent : IDataContent
    {
        private readonly string _modPath;

        public ModDataContent(string modPath)
        {
            _modPath = modPath;
        }

        public bool FileExists(string fileName) => File.Exists(GetPath(fileName));

        public Stream FileOpen(string path)
        {
            var fileName = GetPath(path);
            if (File.Exists(fileName))
            {
                Log.Info("Load mod {0}", path);
                return File.OpenRead(fileName);
            }

            return null;
        }

        private string GetPath(string path) => Path.Combine(_modPath, path);
    }
}
