using OpenKh.Game.Infrastructure;
using System.IO;
using System.Linq;

namespace OpenKh.Game.DataContent
{
    public class MultipleDataContent : IDataContent
    {
        private readonly IDataContent[] _dataContents;

        public MultipleDataContent(params IDataContent[] dataContents)
        {
            _dataContents = dataContents;
        }

        public bool FileExists(string fileName) =>
            _dataContents.Any(x => x.FileExists(fileName));

        public Stream FileOpen(string path)
        {
            foreach (var dataContent in _dataContents)
            {
                var stream = dataContent.FileOpen(path);
                if (stream != null)
                    return stream;
            }

            return null;
        }
    }
}
