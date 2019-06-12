using System.IO;
using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.DataContent
{
    public class StandardDataContent : IDataContent
    {
        public Stream FileOpen(string path) => File.OpenRead(path);
    }
}
