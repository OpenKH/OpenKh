using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2BattleEditor.Extensions;
using OpenKh.Tools.Kh2BattleEditor.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace OpenKh.Tools.Kh2BattleEditor.ViewModels
{
    public class BtlvViewModel : MyGenericListModel<BtlvViewModel.BtlvEntryViewModel>, IBattleGetChanges
    {
        public class BtlvEntryViewModel
        {
            public Btlv Btlv { get; }

            public BtlvEntryViewModel(Btlv btlv)
            {
                Btlv = btlv;
            }

            public string Name => $"{Btlv.Id:X02}";
            
            public int Id { get => Btlv.Id; set => Btlv.Id = value; }
            public int ProgressFlag { get => Btlv.ProgressFlag; set => Btlv.ProgressFlag = value; }
            public byte WorldZZ { get => Btlv.WorldZZ; set => Btlv.WorldZZ = value; }
            public byte WorldOfDarkness { get => Btlv.WorldOfDarkness; set => Btlv.WorldOfDarkness = value; }
            public byte TwilightTown { get => Btlv.TwilightTown; set => Btlv.TwilightTown = value; }
            public byte DestinyIslands { get => Btlv.DestinyIslands; set => Btlv.DestinyIslands = value; }
            public byte HollowBastion { get => Btlv.HollowBastion; set => Btlv.HollowBastion = value; }
            public byte BeastCastle { get => Btlv.BeastCastle; set => Btlv.BeastCastle = value; }
            public byte OlympusColiseum { get => Btlv.OlympusColiseum; set => Btlv.OlympusColiseum = value; }
            public byte Agrabah { get => Btlv.Agrabah; set => Btlv.Agrabah = value; }
            public byte LandOfDragons { get => Btlv.LandOfDragons; set => Btlv.LandOfDragons = value; }
            public byte HundredAcreWoods { get => Btlv.HundredAcreWoods; set => Btlv.HundredAcreWoods = value; }
            public byte PrideLands { get => Btlv.PrideLands; set => Btlv.PrideLands = value; }
            public byte Atlantica { get => Btlv.Atlantica; set => Btlv.Atlantica = value; }
            public byte DisneyCastle { get => Btlv.DisneyCastle; set => Btlv.DisneyCastle = value; }
            public byte TimelessRiver { get => Btlv.TimelessRiver; set => Btlv.TimelessRiver = value; }
            public byte HalloweenTown { get => Btlv.HalloweenTown; set => Btlv.HalloweenTown = value; }
            public byte WorldMap { get => Btlv.WorldMap; set => Btlv.WorldMap = value; }
            public byte PortRoyal { get => Btlv.PortRoyal; set => Btlv.PortRoyal = value; }
            public byte SpaceParanoids { get => Btlv.SpaceParanoids; set => Btlv.SpaceParanoids = value; }
            public byte TheWorldThatNeverWas { get => Btlv.TheWorldThatNeverWas; set => Btlv.TheWorldThatNeverWas = value; }
        }

        private const string entryName = "btlv";

        private string _searchTerm;

        public BtlvViewModel(IEnumerable<Bar.Entry> entries) :
            this(Btlv.Read(entries.GetBattleStream(entryName)))
        { }

        public BtlvViewModel() :
            this(new List<Btlv>())
        { }

        private BtlvViewModel(IEnumerable<Btlv> items) :
            base(items.Select(Map))
        {
        }

        public string EntryName => entryName;

        public Visibility IsItemEditingVisible => IsItemSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsItemEditMessageVisible => !IsItemSelected ? Visibility.Visible : Visibility.Collapsed;

        //public string SearchTerm
        //{
        //    get => _searchTerm;
        //    set
        //    {
        //        _searchTerm = value;
        //        PerformFiltering();
        //    }
        //}

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            Btlv.Write(stream, UnfilteredItems.Select(x => x.Btlv));
            return stream;
        }

        protected override void OnSelectedItem(BtlvEntryViewModel item)
        {
            base.OnSelectedItem(item);

            OnPropertyChanged(nameof(IsItemEditingVisible));
            OnPropertyChanged(nameof(IsItemEditMessageVisible));
        }

        //private void PerformFiltering()
        //{
        //    if (string.IsNullOrWhiteSpace(_searchTerm))
        //        Filter(FilterNone);
        //    else
        //        Filter(FilterByEnemy);
        //}

        //private bool FilterNone(BtlvEntryViewModel arg) => true;

        //private bool FilterByEnemy(BtlvEntryViewModel arg) =>
        //    arg.Name.ToUpper().Contains(SearchTerm.ToUpper());

        private static BtlvEntryViewModel Map(Btlv item) =>
            new BtlvEntryViewModel(item);
    }
}
