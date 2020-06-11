using System.IO;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.DataContent
{
    public class StandardDataContent : IDataContent
    {
        private readonly string _baseDirectory;

        public StandardDataContent(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public bool FileExists(string fileName) => File.Exists(GetPath(fileName));

        public Stream FileOpen(string path)
        {
            Log.Info($"Load file {path}");
            var fileName = GetPath(path);
            if (File.Exists(fileName))
                return File.OpenRead(fileName);

            return null;
        }

        private string GetPath(string path) => Path.Combine(_baseDirectory, path);
    }
}
