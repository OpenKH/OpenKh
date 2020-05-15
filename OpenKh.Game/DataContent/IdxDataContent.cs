using System.IO;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;

namespace OpenKh.Game.DataContent
{
    public class IdxDataContent : IDataContent
    {
        private readonly Idx _idx;
        private readonly Img _img;

        public IdxDataContent(Stream idxStream, Stream imgStream)
        {
            _idx = Idx.Read(idxStream);
            _img = new Img(imgStream, _idx, false);
        }

        public bool FileExists(string fileName) => _idx.TryGetEntry(fileName, out var _);

        public Stream FileOpen(string path)
        {
            if (_idx.TryGetEntry(path, out var entry))
                return _img.FileOpen(entry);

            return null;
        }
    }
}
