using OpenKh.Common;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;
using System.IO;
using System.Linq;

namespace OpenKh.Game.DataContent
{
    public class IdxMultipleDataContent : IDataContent
    {
        private readonly MultipleDataContent _dataContent;

        public IdxMultipleDataContent(IDataContent baseDataContent, Stream imgStream)
        {
            var dataContents = Constants.WorldIds
                .Select(worldId => $"000{worldId}.idx")
                .Select(fileName => baseDataContent.FileOpen(fileName))
                .Where(stream => stream != null)
                .Select(stream => stream.Using(s => new IdxDataContent(s, imgStream)))
                .ToArray();

            _dataContent = new MultipleDataContent(dataContents);
        }

        public bool FileExists(string fileName) => _dataContent.FileExists(fileName);

        public Stream FileOpen(string path) => _dataContent.FileOpen(path);
    }
}
