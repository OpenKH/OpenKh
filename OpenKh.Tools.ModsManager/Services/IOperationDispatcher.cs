using System.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    public interface IOperationDispatcher
    {
        int LoadFile(Stream outStream, string fileName);
        int GetFileSize(string fileName);
    }
}
