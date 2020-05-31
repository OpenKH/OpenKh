using OpenKh.Kh2;
using System.Collections.Generic;

namespace OpenKh.Engine
{
    public interface IArchiveManager
    {
        T Get<T>(string resourceName) where T : class;
        void LoadArchive(List<Bar.Entry> entries);
        void LoadArchive(string fileName);
    }
}