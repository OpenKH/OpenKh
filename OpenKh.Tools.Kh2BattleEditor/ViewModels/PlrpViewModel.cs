using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Tools.Common.Wpf.Models;
using OpenKh.Tools.Kh2BattleEditor.Extensions;
using OpenKh.Tools.Kh2BattleEditor.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace OpenKh.Tools.Kh2BattleEditor.ViewModels
{
    public class PlrpViewModel : MyGenericListModel<PlrpViewModel.PlrpEntryViewModel>, IBattleGetChanges
    {
        public class PlrpEntryViewModel
        {
            public Plrp Plrp { get; }

            public PlrpEntryViewModel(Plrp plrp)
            {
                Plrp = plrp;
            }

            public string Name => $"{Plrp.Id:X02}";

            public ushort Id { get => Plrp.Id; set => Plrp.Id = value; }
            public byte Character { get => Plrp.Character; set => Plrp.Character = value; }
            public byte Hp { get => Plrp.Hp; set => Plrp.Hp = value; }
            public byte Mp { get => Plrp.Mp; set => Plrp.Mp = value; }
            public byte Ap { get => Plrp.Ap; set => Plrp.Ap = value; }
            public byte Strength { get => Plrp.Strength; set => Plrp.Strength = value; }
            public byte Magic { get => Plrp.Magic; set => Plrp.Magic = value; }
            public byte Defense { get => Plrp.Defense; set => Plrp.Defense = value; }
            public byte ArmorSlotMax { get => Plrp.ArmorSlotMax; set => Plrp.ArmorSlotMax = value; }
            public byte AccessorySlotMax { get => Plrp.AccessorySlotMax; set => Plrp.AccessorySlotMax = value; }
            public byte ItemSlotMax { get => Plrp.ItemSlotMax; set => Plrp.ItemSlotMax = value; }

            //display the starting items in a datagrid or similar...
        }

        private const string entryName = "plrp";

        private string _searchTerm;

        public PlrpViewModel(IEnumerable<Bar.Entry> entries) :
            this(Plrp.Read(entries.GetBattleStream(entryName)))
        { }

        public PlrpViewModel() :
            this(new List<Plrp>())
        { }

        private PlrpViewModel(IEnumerable<Plrp> items) :
            base(items.Select(Map))
        {
        }

        public string EntryName => entryName;

        public Visibility IsItemEditingVisible => IsItemSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsItemEditMessageVisible => !IsItemSelected ? Visibility.Visible : Visibility.Collapsed;

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            Plrp.Write(stream, UnfilteredItems.Select(x => x.Plrp));
            return stream;
        }

        protected override void OnSelectedItem(PlrpEntryViewModel item)
        {
            base.OnSelectedItem(item);

            OnPropertyChanged(nameof(IsItemEditingVisible));
            OnPropertyChanged(nameof(IsItemEditMessageVisible));
        }

        private static PlrpEntryViewModel Map(Plrp item) =>
            new PlrpEntryViewModel(item);
    }
}
