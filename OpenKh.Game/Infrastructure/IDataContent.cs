using System.IO;

namespace OpenKh.Game.Infrastructure
{
    public interface IDataContent
    {
        bool FileExists(string fileName);

        Stream FileOpen(string fileName);
    }
}
