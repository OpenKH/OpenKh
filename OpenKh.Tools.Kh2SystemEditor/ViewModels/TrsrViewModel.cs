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

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class TrsrViewModel : MyGenericListModel<TrsrViewModel.Entry>, ISystemGetChanges
    {
        public class Entry : BaseNotifyPropertyChanged
        {
            private readonly IMessageProvider _messageProvider;

            public Entry(IMessageProvider messageProvider, Trsr treasure)
            {
                _messageProvider = messageProvider;
                Treasure = treasure;
            }

            public Trsr Treasure { get; }

            public string Title => $"{Treasure.Id:X02}";

            public ushort Id { get => Treasure.Id; set => Treasure.Id = value; }

            public ushort ItemId { get => Treasure.ItemId; set => Treasure.ItemId = value; }
            public Trsr.TrsrType Type { get => Treasure.Type; set => Treasure.Type = value; }
            public Constants.Worlds World { get => Treasure.World; set => Treasure.World = value; }
            public byte Room { get => Treasure.Room; set => Treasure.Room = value; }
            public byte RoomChestIndex { get => Treasure.RoomChestIndex; set => Treasure.RoomChestIndex = value; }
            public short EventId { get => Treasure.EventId; set => Treasure.EventId = value; }
            public short OverallChestIndex { get => Treasure.OverallChestIndex; set => Treasure.OverallChestIndex = value; }

            public string IdText => $"{Id} (0x{Id:X})";

            public override string ToString() => Title;
        }

        private const string entryName = "trsr";
        private readonly IMessageProvider _messageProvider;
        private string _searchTerm;

        public TrsrViewModel(IMessageProvider messageProvider, IEnumerable<Bar.Entry> entries) :
            this(messageProvider, Trsr.Read(entries.GetBinaryStream(entryName)))
        { }

        public TrsrViewModel(IMessageProvider messageProvider) :
            this(messageProvider, new Trsr[0])
        { }

        private TrsrViewModel(IMessageProvider messageProvider, IEnumerable<Trsr> items) :
            base(items.Select(item => new Entry(messageProvider, item)))
        {
            _messageProvider = messageProvider;
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
            Trsr.Write(stream, this.Select(x => x.Treasure));

            return stream;
        }

        protected override void OnSelectedItem(Entry item)
        {
            base.OnSelectedItem(item);

            OnPropertyChanged(nameof(IsItemEditingVisible));
            OnPropertyChanged(nameof(IsItemEditMessageVisible));
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
