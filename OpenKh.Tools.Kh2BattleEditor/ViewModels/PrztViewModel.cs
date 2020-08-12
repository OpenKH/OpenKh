using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Tools.Kh2BattleEditor.Extensions;
using OpenKh.Tools.Kh2BattleEditor.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.Kh2BattleEditor.ViewModels
{
    public class PrztViewModel : GenericListModel<PrztViewModel.PrztEntryViewModel>, IBattleGetChanges
    {
        public class PrztEntryViewModel
        {
            public Przt Przt { get; }
            public PrztEntryViewModel(Przt przt)
            {
                Przt = przt;
            }
            public string Name => $"{Id}";
            public string Id => $"{Przt.Id:X02}";

            public ushort DropId { get => Przt.Id; set => Przt.Id = value; }
            public byte SmallHpOrbs { get => Przt.SmallHpOrbs; set => Przt.SmallHpOrbs = value; }
            public byte BigHpOrbs { get => Przt.BigHpOrbs; set => Przt.BigHpOrbs = value; }
            public byte BigMoneyOrbs { get => Przt.BigMoneyOrbs; set => Przt.BigMoneyOrbs = value; }
            public byte MediumMoneyOrbs { get => Przt.MediumMoneyOrbs; set => Przt.MediumMoneyOrbs = value; }
            public byte SmallMoneyOrbs { get => Przt.SmallMoneyOrbs; set => Przt.SmallMoneyOrbs = value; }
            public byte SmallMpOrbs { get => Przt.SmallMpOrbs; set => Przt.SmallMpOrbs = value; }
            public byte BigMpOrbs { get => Przt.BigMpOrbs; set => Przt.BigMpOrbs = value; }
            public byte SmallDriveOrbs { get => Przt.SmallDriveOrbs; set => Przt.SmallDriveOrbs = value; }
            public byte BigDriveOrbs { get => Przt.BigDriveOrbs; set => Przt.BigDriveOrbs = value; }
            public byte Unknown0a { get => Przt.Unknown0a; set => Przt.Unknown0a = value; }
            public ushort Item1 { get => Przt.Item1; set => Przt.Item1 = value; }
            public short Item1Percentage { get => Przt.Item1Percentage; set => Przt.Item1Percentage = value; }
            public ushort Item2 { get => Przt.Item2; set => Przt.Item2 = value; }
            public short Item2Percentage { get => Przt.Item2Percentage; set => Przt.Item2Percentage = value; }
            public ushort Item3 { get => Przt.Item3; set => Przt.Item3 = value; }
            public short Item3Percentage { get => Przt.Item3Percentage; set => Przt.Item3Percentage = value; }

            public override string ToString() => Name;
        }

        private const string entryName = "przt";

        private string _searchTerm;
        public string EntryName => entryName;

        public PrztViewModel(IEnumerable<Bar.Entry> entries) :
            this(Przt.Read(entries.GetBattleStream(entryName)))
        { }

        public PrztViewModel() :
            this(new List<Przt>())
        { }

        private PrztViewModel(IEnumerable<Przt> items) :
            base(items.Select(Map))
        {
        }

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
            Przt.Write(stream, UnfilteredItems.Select(x => x.Przt));

            return stream;
        }

        protected override void OnSelectedItem(PrztEntryViewModel item)
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
                Filter(FilterByEnemy);
        }

        private bool FilterNone(PrztEntryViewModel arg) => true;

        private bool FilterByEnemy(PrztEntryViewModel arg) =>
            arg.Name.ToUpper().Contains(SearchTerm.ToUpper());

        private static PrztEntryViewModel Map(Przt item) =>
            new PrztEntryViewModel(item);
    }
}
