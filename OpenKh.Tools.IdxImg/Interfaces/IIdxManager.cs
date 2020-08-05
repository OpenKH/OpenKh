using System.IO;

namespace OpenKh.Tools.IdxImg.Interfaces
{
    interface IIdxManager
    {
        Stream OpenFileFromIdx(string fileName);
    }
}
