using OpenKh.Game.Infrastructure;
using System.IO;

namespace OpenKh.Game.DataContent
{
    public class DummyDataContent : IDataContent
    {
        public Stream FileOpen(string fileName) => null;
    }
}
