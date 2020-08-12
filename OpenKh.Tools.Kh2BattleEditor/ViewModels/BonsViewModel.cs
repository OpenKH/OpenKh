using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2BattleEditor.Extensions;
using OpenKh.Tools.Kh2BattleEditor.Interfaces;
using OpenKh.Tools.Kh2BattleEditor.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace OpenKh.Tools.Kh2BattleEditor.ViewModels
{
    public class BonsViewModel : MyGenericListModel<BonsViewModel.BonsEntryViewModel>, IBattleGetChanges
    {
        public class BonsEntryViewModel
        {
            public Bons Bons { get; }

            public BonsEntryViewModel(Bons bons)
            {
                Bons = bons;
            }

            public string Name => $"{Id} {CharacterNameProvider.GetCharacterName(Bons.CharacterId)}";

            public string Id => $"{Bons.RewardId:X02}";

            public byte RewardId { get => Bons.RewardId; set => Bons.RewardId = value; }
            public byte CharacterId { get => Bons.CharacterId; set => Bons.CharacterId = value; }
            public byte HpIncrease { get => Bons.HpIncrease; set => Bons.HpIncrease = value; }
            public byte MpIncrease { get => Bons.MpIncrease; set => Bons.MpIncrease = value; }
            public byte DriveGaugeUpgrade { get => Bons.DriveGaugeUpgrade; set => Bons.DriveGaugeUpgrade = value; }
            public byte ItemSlotUpgrade { get => Bons.ItemSlotUpgrade; set => Bons.ItemSlotUpgrade = value; }
            public byte AccessorySlotUpgrade { get => Bons.AccessorySlotUpgrade; set => Bons.AccessorySlotUpgrade = value; }
            public byte ArmorSlotUpgrade { get => Bons.ArmorSlotUpgrade; set => Bons.ArmorSlotUpgrade = value; }
            public short BonusItem1 { get => Bons.BonusItem1; set => Bons.BonusItem1 = value; }
            public short BonusItem2 { get => Bons.BonusItem2; set => Bons.BonusItem2 = value; }
            public int Unknown0c { get => Bons.Unknown0c; set => Bons.Unknown0c = value; }

            public override string ToString() => Name;
        }

        private const string entryName = "bons";

        private string _searchTerm;

        public BonsViewModel(IEnumerable<Bar.Entry> entries) :
            this(Bons.Read(entries.GetBattleStream(entryName)))
        { }

        public BonsViewModel() :
            this(new List<Bons>())
        { }

        private BonsViewModel(IEnumerable<Bons> items) :
            base(items.Select(Map))
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
            Bons.Write(stream, UnfilteredItems.Select(x => x.Bons));

            return stream;
        }
        protected override void OnSelectedItem(BonsEntryViewModel item)
        {
            base.OnSelectedItem(item);

            OnPropertyChanged(nameof(IsItemEditingVisible));
            OnPropertyChanged(nameof(IsItemEditMessageVisible));
        }

        private void PerformFiltering()
        {
            if (string.IsNullOrWhiteSpace(_searchTerm))
                Filter(FilterNone);
            else
                Filter(FilterByCharacter);
        }

        private bool FilterNone(BonsEntryViewModel arg) => true;

        private bool FilterByCharacter(BonsEntryViewModel arg) =>
            arg.Name.ToUpper().Contains(SearchTerm.ToUpper());

        private static BonsEntryViewModel Map(Bons item) =>
            new BonsEntryViewModel(item);
    }
}
