using System.IO;
using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.DataContent
{
    public class StandardDataContent : IDataContent
    {
        private readonly string _baseDirectory;

        public StandardDataContent(string baseDirectory = ".")
        {
            _baseDirectory = baseDirectory;
        }

        public Stream FileOpen(string path)
        {
            var fileName = Path.Combine(_baseDirectory, path);
            if (File.Exists(fileName))
                return File.OpenRead(fileName);

            return null;
        }
    }
}
