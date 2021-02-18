using System.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    public interface IOperationDispatcher
    {
        bool LoadFile(Stream outStream, string fileName);
        int GetFileSize(string fileName);
    }
}
