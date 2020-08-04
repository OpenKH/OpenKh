using OpenKh.Kh2;
using OpenKh.Kh2.System;
using OpenKh.Tools.Kh2SystemEditor.Attributes;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Tools.Kh2SystemEditor.Models.Export
{
    public class TrsrExport
    {
        public TrsrExport(IItemProvider itemProvider, Trsr treasure)
        {
            ItemProvider = itemProvider;
            Treasure = treasure;
        }

        public Trsr Treasure { get; }
        public IItemProvider ItemProvider { get; }

        [ExportTarget]
        public string Title => $"{Treasure.Id:X03} {MapName} {ItemName}";

        [ExportTarget]
        public ushort Id => Treasure.Id;

        [ExportTarget]
        public ushort ItemId => Treasure.ItemId;
        [ExportTarget]
        public Trsr.TrsrType Type => Treasure.Type;
        [ExportTarget]
        public World World => (World)Treasure.World;
        [ExportTarget]
        public byte Room => Treasure.Room;
        [ExportTarget]
        public byte RoomChestIndex => Treasure.RoomChestIndex;
        [ExportTarget]
        public short EventId => Treasure.EventId;
        [ExportTarget]
        public short OverallChestIndex => Treasure.OverallChestIndex;

        [ExportTarget]
        public string IdText => $"{Id} (0x{Id:X})";
        [ExportTarget]
        public string MapName => $"{Constants.WorldIds[(int)World]}_{Room:D02}";
        [ExportTarget]
        public string ItemName => ItemProvider.GetItemName(ItemId);
    }
}
