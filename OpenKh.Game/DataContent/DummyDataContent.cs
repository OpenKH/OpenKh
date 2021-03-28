using OpenKh.Game.Infrastructure;
using System.IO;

namespace OpenKh.Game.DataContent
{
    public class DummyDataContent : IDataContent
    {
        public bool FileExists(string fileName) => false;

        public Stream FileOpen(string fileName) => null;
    }
}
