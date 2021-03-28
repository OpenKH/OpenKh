using System.IO;

namespace OpenKh.Tools.Kh2SystemEditor.Interfaces
{
    public interface ISystemGetChanges
    {
        string EntryName { get; }

        Stream CreateStream();
    }
}
