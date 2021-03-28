using OpenKh.Common.Archives;
using OpenKh.Game.Infrastructure;
using System.IO;

namespace OpenKh.Game.DataContent
{
    public class HdAssetContent : IDataContent
    {
        private readonly IDataContent _innerDataContext;

        public HdAssetContent(IDataContent innerDataContext)
        {
            _innerDataContext = innerDataContext;
        }

        public bool FileExists(string fileName) => _innerDataContext.FileExists(fileName);

        public Stream FileOpen(string fileName)
        {
            var stream = _innerDataContext.FileOpen(fileName);
            if (stream == null)
                return null;

            return HdAsset.Read(stream).Stream;
        }
    }
}
