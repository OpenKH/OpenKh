using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Kh2;
using OpenKh.Kh2.System;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2SystemEditor.Extensions;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;
using Xe.Tools;
using Xe.Tools.Models;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class ItemViewModel : MyGenericListModel<ItemViewModel.Entry>, ISystemGetChanges
    {
        public class Entry : BaseNotifyPropertyChanged
        {
            private readonly IMessageProvider _messageProvider;

            public Entry(IMessageProvider messageProvider, Item.Entry item)
            {
                _messageProvider = messageProvider;
                Item = item;
                Types = new EnumModel<Item.Type>();
            }

            public Item.Entry Item { get; }

            public string Title => $"{Item.Id:X02} {_messageProvider.GetMessage(Item.Name)}";

            public ushort Id { get => Item.Id; set => Item.Id = value; }
            public Item.Type Type  { get => Item.Type; set => Item.Type = value; }
            public byte Flag0  { get => Item.Flag0; set => Item.Flag0 = value; }
            public byte Flag1  { get => Item.Flag1; set => Item.Flag1 = value; }
            public byte Flag2  { get => Item.Flag2; set => Item.Flag2 = value; }
            public ushort StatEntry  { get => Item.StatEntry; set => Item.StatEntry = value; }
            public ushort NameId
            {
                get => Item.Name;
                set
                {
                    Item.Name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
            public ushort DescriptionId
            {
                get => Item.Description;
                set
                {
                    Item.Description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
            public ushort ShopBuy { get => Item.ShopBuy; set => Item.ShopBuy = value; }
            public ushort ShopSell { get => Item.ShopSell; set => Item.ShopSell = value; }
            public ushort Command  { get => Item.Command; set => Item.Command = value; }
            public ushort Slot  { get => Item.Slot; set => Item.Slot = value; }
            public short Picture  { get => Item.Picture; set => Item.Picture = value; }
            public byte Icon1  { get => Item.Icon1; set => Item.Icon1 = value; }
            public byte Icon2  { get => Item.Icon1; set => Item.Icon1 = value; }

            public string IdText => $"{Id} (0x{Id:X})";
            public string Name { get => _messageProvider.GetMessage(Item.Name); set => _messageProvider.SetMessage(Item.Name, value); }
            public string Description { get => _messageProvider.GetMessage(Item.Description); set => _messageProvider.SetMessage(Item.Description, value); }
            public EnumModel<Item.Type> Types { get; }

            public override string ToString() => Title;
        }

        private const string entryName = "item";
        private readonly IMessageProvider _messageProvider;
        private string _searchTerm;

        public ItemViewModel(IMessageProvider messageProvider, IEnumerable<Bar.Entry> entries) :
            this(messageProvider, Item.Read(entries.GetBinaryStream(entryName)))
        { }

        public ItemViewModel(IMessageProvider messageProvider) :
            this(messageProvider, new Item
            {
                Items1 = new List<Item.Entry>(),
                Items2 = new List<Item.Stat>()
            })
        { }

        private ItemViewModel(IMessageProvider messageProvider, Item item) :
            this(messageProvider, item.Items1)
        { }

        private ItemViewModel(IMessageProvider messageProvider, IEnumerable<Item.Entry> items) :
            base(items.Select(item => new Entry(messageProvider, item)))
        {
            _messageProvider = messageProvider;
        }

        public string EntryName => entryName;

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                PerformFiltering();
            }
        }

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            new Item
            {
                Items1 = this.Select(x => x.Item).ToList(),
                Items2 = new List<Item.Stat>()
            }.Write(stream);

            return stream;
        }

        private void PerformFiltering()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
                Filter(FilterNone);
            else
                Filter(FilterByName);
        }

        private bool FilterNone(Entry arg) => true;

        private bool FilterByName(Entry arg) =>
            arg.Title.ToUpper().Contains(SearchTerm.ToUpper());
    }
}
