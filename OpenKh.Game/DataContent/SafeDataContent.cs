using OpenKh.Game.Infrastructure;
using System.IO;

namespace OpenKh.Game.DataContent
{
    public class SafeDataContent : IDataContent
    {
        private readonly IDataContent _innerDataContext;

        public SafeDataContent(IDataContent innerDataContext)
        {
            _innerDataContext = innerDataContext;
        }

        public bool FileExists(string fileName) => _innerDataContext.FileExists(fileName);

        public Stream FileOpen(string fileName)
        {
            var stream = _innerDataContext.FileOpen(fileName);
            if (stream == null)
                throw new FileNotFoundException(fileName);

            return stream;
        }
    }
}
