using System.IO;

namespace OpenKh.Game.Infrastructure
{
    public interface IDataContent
    {
        Stream FileOpen(string fileName);
    }
}
