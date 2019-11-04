using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Kh2;
using OpenKh.Kh2.System;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2SystemEditor.Extensions;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;
using Xe.Tools;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class ItemViewModel : MyGenericListModel<ItemViewModel.Entry>, ISystemGetChanges
    {
        public class Entry : BaseNotifyPropertyChanged
        {
            public Item.Entry Item { get; }

            public Entry(Item.Entry item)
            {
                Item = item;
            }

            public string Name => $"{Item.Id:X02}";

            public ushort Id { get => Item.Id; set => Item.Id = value; }
            public ushort TypeShort  { get => Item.Type; set => Item.Type = value; }
            public byte Flag1  { get => Item.Flag1; set => Item.Flag1 = value; }
            public byte Flag2  { get => Item.Flag2; set => Item.Flag2 = value; }
            public ushort StatEntry  { get => Item.StatEntry; set => Item.StatEntry = value; }
            public ushort String1 { get => (ushort)(Item.String1 & 0x7FFF); set => Item.String1 = (ushort)(value | 0x8000); }
            public ushort String2 { get => (ushort)(Item.String2 & 0x7FFF); set => Item.String2 = (ushort)(value | 0x8000); }
            public ushort ShopValue1 { get => Item.ShopValue1; set => Item.ShopValue1 = value; }
            public ushort ShopValue2 { get => Item.ShopValue2; set => Item.ShopValue2 = value; }
            public ushort Command  { get => Item.Command; set => Item.Command = value; }
            public ushort Slot  { get => Item.Slot; set => Item.Slot = value; }
            public short Picture  { get => Item.Picture; set => Item.Picture = value; }
            public byte Icon1  { get => Item.Icon1; set => Item.Icon1 = value; }
            public byte Icon2  { get => Item.Icon1; set => Item.Icon1 = value; }

            public override string ToString() => Name;
        }

        private const string entryName = "item";

        public ItemViewModel(IEnumerable<Bar.Entry> entries) :
            this(Item.Read(entries.GetBinaryStream(entryName)))
        { }

        public ItemViewModel() :
            this(new Item
            {
                Items1 = new List<Item.Entry>(),
                Items2 = new List<Item.Stat>()
            })
        { }

        private ItemViewModel(Item item) :
            this(item.Items1)
        { }

        private ItemViewModel(IEnumerable<Item.Entry> items) :
            base(items.Select(Map))
        {
        }

        public string EntryName => entryName;

        public Stream CreateStream()
        {
            throw new System.NotImplementedException();
        }

        private static Entry Map(Item.Entry item) =>
            new Entry(item);
    }
}
