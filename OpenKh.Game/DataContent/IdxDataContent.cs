using System.IO;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;

namespace OpenKh.Game.DataContent
{
    public class IdxDataContent : IDataContent
    {
        private readonly Img _img;

        public IdxDataContent(Stream idxStream, Stream imgStream)
        {
            _img = new Img(imgStream, Idx.Read(idxStream), false);
        }

        public bool FileExists(string fileName) => _img.FileExists(fileName);

        public Stream FileOpen(string path)
        {
            Log.Info($"Load IDX file {path}");
            return _img.TryFileOpen(path, out var stream) ? stream : null;
        }
    }
}
