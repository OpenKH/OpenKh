using OpenKh.Tools.BarTool.Models;

namespace OpenKh.Tools.BarTool.Interfaces
{
    public interface IViewSettings
    {
        bool IsPlayer { get; }
        bool ShowSlotNumber { get; }
        bool ShowMovesetName { get; }

        int GetSlotIndex(EntryModel item);
    }
}
