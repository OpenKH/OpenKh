using OpenKh.Engine;
using OpenKh.Kh2.System;
using OpenKh.Tools.Kh2SystemEditor.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Tools.Kh2SystemEditor.Models.Export
{
    public class ItemExport
    {
        private readonly IMessageProvider _messageProvider;

        public Item.Entry Item { get; }

        public ItemExport(IMessageProvider messageProvider, Item.Entry item)
        {
            _messageProvider = messageProvider;
            Item = item;
        }


        [ExportTarget]
        public string Title => $"{Item.Id:X02} {_messageProvider.GetString(Item.Name)}";

        [ExportTarget]
        public ushort Id => Item.Id;
        [ExportTarget]
        public Item.Type Type => Item.Type;
        [ExportTarget]
        public byte Flag0 => Item.Flag0;
        [ExportTarget]
        public byte Flag1 => Item.Flag1;
        [ExportTarget]
        public Item.Rank Rank => Item.Rank;
        [ExportTarget]
        public ushort StatEntry => Item.StatEntry;
        [ExportTarget]
        public ushort NameId => Item.Name;
        [ExportTarget]
        public ushort DescriptionId => Item.Description;
        [ExportTarget]
        public ushort ShopBuy => Item.ShopBuy;
        [ExportTarget]
        public ushort ShopSell => Item.ShopSell;
        [ExportTarget]
        public ushort Command => Item.Command;
        [ExportTarget]
        public ushort Slot => Item.Slot;
        [ExportTarget]
        public short Picture => Item.Picture;
        [ExportTarget]
        public byte Icon1 => Item.Icon1;
        [ExportTarget]
        public byte Icon2 => Item.Icon1;

        [ExportTarget]
        public string Name => _messageProvider.GetString(Item.Name);
        [ExportTarget]
        public string Description => _messageProvider.GetString(Item.Description);
    }
}
