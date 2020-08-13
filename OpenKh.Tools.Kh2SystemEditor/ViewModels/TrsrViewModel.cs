using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using OpenKh.Kh2;
using OpenKh.Kh2.System;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2SystemEditor.Extensions;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;
using Xe.Tools;
using Xe.Tools.Models;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class TrsrViewModel : MyGenericListModel<TrsrViewModel.Entry>, ISystemGetChanges
    {
        public class Entry : BaseNotifyPropertyChanged
        {
            public Entry(IItemProvider itemProvider, Trsr treasure)
            {
                ItemProvider = itemProvider;
                Treasure = treasure;
                Worlds = new Kh2WorldsList();
                Types = new EnumModel<Trsr.TrsrType>();
            }

            public Trsr Treasure { get; }
            public IItemProvider ItemProvider { get; }

            public string Title => $"{Treasure.Id:X03} {MapName} {ItemName}";
            public string Query => $"{Id} {Title} {Type} {World} {RoomChestIndex} {EventId}";

            public ushort Id { get => Treasure.Id; set => Treasure.Id = value; }

            public ushort ItemId
            {
                get => Treasure.ItemId;
                set
                {
                    Treasure.ItemId = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
            public Trsr.TrsrType Type { get => Treasure.Type; set => Treasure.Type = value; }
            public World World
            {
                get => (World)Treasure.World;
                set
                {
                    Treasure.World = (byte)value;
                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(MapName));
                }
            }
            public byte Room
            {
                get => Treasure.Room;
                set
                {
                    Treasure.Room = value;
                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(MapName));
                }
            }
            public byte RoomChestIndex { get => Treasure.RoomChestIndex; set => Treasure.RoomChestIndex = value; }
            public short EventId { get => Treasure.EventId; set => Treasure.EventId = value; }
            public short OverallChestIndex { get => Treasure.OverallChestIndex; set => Treasure.OverallChestIndex = value; }

            public string IdText => $"{Id} (0x{Id:X})";
            public string MapName => $"{Constants.WorldIds[(int)World]}_{Room:D02}";
            public string ItemName => ItemProvider.GetItemName(ItemId);

            public Kh2WorldsList Worlds { get; }
            public EnumModel<Trsr.TrsrType> Types { get; }

            public override string ToString() => Title;

            public void RefreshMessages()
            {
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Query));
                OnPropertyChanged(nameof(IdText));
                OnPropertyChanged(nameof(MapName));
                OnPropertyChanged(nameof(ItemName));
            }
        }

        private const string entryName = "trsr";
        private readonly IItemProvider _itemProvider;
        private string _searchTerm;

        public TrsrViewModel(IItemProvider itemProvider, IEnumerable<Bar.Entry> entries) :
            this(itemProvider, Trsr.Read(entries.GetBinaryStream(entryName)))
        { }

        public TrsrViewModel(IItemProvider itemProvider) :
            this(itemProvider, new Trsr[0])
        { }

        private TrsrViewModel(IItemProvider itemProvider, IEnumerable<Trsr> items) :
            base(items.Select(item => new Entry(itemProvider, item)))
        {
            _itemProvider = itemProvider;
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
            Trsr.Write(stream, UnfilteredItems.Select(x => x.Treasure));

            return stream;
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

            return SelectedItem = new Entry(_itemProvider, new Trsr
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

        private bool FilterByName(Entry arg)
        {
            var query = arg.Query.ToUpper();
            return SearchTerm.ToUpper().Split(new char[] { ',', ' ' }).All(term => query.Contains(term));
        }

        public void RefreshAllMessages()
        {
            foreach (var item in Items)
            {
                item.RefreshMessages();
            }
        }
    }
}
