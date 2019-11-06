namespace OpenKh.Tools.Kh2SystemEditor.Interfaces
{
    public interface IItemProvider
    {
        bool IsItemExists(int itemId);
        string GetItemName(int itemId);
    }
}
