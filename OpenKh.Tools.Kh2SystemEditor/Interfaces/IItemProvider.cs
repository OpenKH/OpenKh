using System.Collections.Generic;

namespace OpenKh.Tools.Kh2SystemEditor.Interfaces
{
    public interface IItemProvider
    {
        IEnumerable<IItemEntry> ItemEntries { get; }

        bool IsItemExists(int itemId);
        string GetItemName(int itemId);
        void InvalidateItemName(int itemId);
    }
}
