using System.IO;

namespace OpenKh.Tools.Kh2BattleEditor.Interfaces
{
    public interface IBattleGetChanges
    {
        string EntryName { get; }

        Stream CreateStream();
    }
}
