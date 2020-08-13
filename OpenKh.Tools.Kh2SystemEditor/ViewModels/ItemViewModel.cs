using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using OpenKh.Engine;
using OpenKh.Kh2;
using OpenKh.Kh2.System;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2SystemEditor.Extensions;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;
using Xe.Tools;
using Xe.Tools.Models;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class ItemViewModel : MyGenericListModel<ItemViewModel.Entry>, ISystemGetChanges, IItemProvider
    {
        public class Entry : BaseNotifyPropertyChanged, IItemEntry
        {
            private readonly IMessageProvider _messageProvider;

            public Entry(IMessageProvider messageProvider, Item.Entry item)
            {
                _messageProvider = messageProvider;
                Item = item;
                Types = new EnumModel<Item.Type>();
                Ranks = new EnumModel<Item.Rank>();
            }

            public Item.Entry Item { get; }

            public string Title => $"{Item.Id:X02} {_messageProvider.GetString(Item.Name)}";

            public ushort Id { get => Item.Id; set => Item.Id = value; }
            public Item.Type Type  { get => Item.Type; set => Item.Type = value; }
            public byte Flag0  { get => Item.Flag0; set => Item.Flag0 = value; }
            public byte Flag1  { get => Item.Flag1; set => Item.Flag1 = value; }
            public Item.Rank Rank { get => Item.Rank; set => Item.Rank = value; }
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
            public string Name { get => _messageProvider.GetString(Item.Name); set => _messageProvider.SetString(Item.Name, value); }
            public string Description { get => _messageProvider.GetString(Item.Description); set => _messageProvider.SetString(Item.Description, value); }
            public EnumModel<Item.Type> Types { get; }
            public EnumModel<Item.Rank> Ranks { get; }

            public override string ToString() => Title;

            public void RefreshMessages()
            {
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Description));
            }
        }

        private const string entryName = "item";
        private readonly IMessageProvider _messageProvider;
        private readonly List<Item.Stat> _item2;
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
        {
            _messageProvider = messageProvider;
            _item2 = item.Items2;
        }

        private ItemViewModel(IMessageProvider messageProvider, IEnumerable<Item.Entry> items) :
            base(items.Select(item => new Entry(messageProvider, item)))
        {
        }

        public string EntryName => entryName;
        
        public Visibility IsItemEditingVisible => IsItemSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsItemEditMessageVisible => !IsItemSelected ? Visibility.Visible : Visibility.Collapsed;


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
                Items1 = UnfilteredItems.Select(x => x.Item).ToList(),
                Items2 = _item2
            }.Write(stream);

            return stream;
        }

        public IEnumerable<IItemEntry> ItemEntries => this;
        public bool IsItemExists(int itemId) => this.Any(x => x.Id == itemId);
        public string GetItemName(int itemId) => this.FirstOrDefault(x => x.Id == itemId)?.Name;
        public void InvalidateItemName(int itemId)
        {
            OnPropertyChanged(nameof(ItemEntries));
        }

        protected override void OnSelectedItem(Entry item)
        {
            base.OnSelectedItem(item);

            OnPropertyChanged(nameof(IsItemEditingVisible));
            OnPropertyChanged(nameof(IsItemEditMessageVisible));
        }

        protected override Entry OnNewItem()
        {
            ushort smallestUnusedId = 0;
            foreach (var item in UnfilteredItems.OrderBy(x => x.Id))
            {
                if (smallestUnusedId++ + 1 != item.Id)
                    break;
            }

            return SelectedItem = new Entry(_messageProvider, new Item.Entry
            {
                Id = smallestUnusedId
            });
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

        public void RefreshAllMessages()
        {
            foreach (var item in Items)
            {
                item.RefreshMessages();
            }
        }
    }
}
