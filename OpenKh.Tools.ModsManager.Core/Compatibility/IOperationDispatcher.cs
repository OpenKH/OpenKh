using System.IO;

namespace OpenKh.Tools.ModsManager.Core.Interfaces
{
    public interface IOperationDispatcher
    {
        int LoadFile(Stream outStream, string fileName);
        int GetFileSize(string fileName);
    }
}
