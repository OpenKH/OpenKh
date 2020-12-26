using OpenKh.Tools.BarEditor.Models;

namespace OpenKh.Tools.BarEditor.Interfaces
{
    public interface IViewSettings
    {
        bool IsPlayer { get; }
        bool ShowSlotNumber { get; }
        bool ShowMovesetName { get; }

        int GetSlotIndex(BarEntryModel item);
    }
}
