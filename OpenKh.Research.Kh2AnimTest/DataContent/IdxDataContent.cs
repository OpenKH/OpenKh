using System.IO;
using OpenKh.Research.Kh2AnimTest.Debugging;
using OpenKh.Research.Kh2AnimTest.Infrastructure;
using OpenKh.Kh2;

namespace OpenKh.Research.Kh2AnimTest.DataContent
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
