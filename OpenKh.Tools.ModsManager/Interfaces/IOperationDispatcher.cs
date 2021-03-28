using System.IO;

namespace OpenKh.Tools.ModsManager.Interfaces
{
    public interface IOperationDispatcher
    {
        int LoadFile(Stream outStream, string fileName);
        int GetFileSize(string fileName);
    }
}
