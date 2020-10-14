using System.IO;

namespace OpenKh.Research.Kh2AnimTest.Infrastructure
{
    public interface IDataContent
    {
        bool FileExists(string fileName);

        Stream FileOpen(string fileName);
    }
}
