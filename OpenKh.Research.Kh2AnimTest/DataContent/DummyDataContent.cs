using OpenKh.Research.Kh2AnimTest.Infrastructure;
using System.IO;

namespace OpenKh.Research.Kh2AnimTest.DataContent
{
    public class DummyDataContent : IDataContent
    {
        public bool FileExists(string fileName) => false;

        public Stream FileOpen(string fileName) => null;
    }
}
