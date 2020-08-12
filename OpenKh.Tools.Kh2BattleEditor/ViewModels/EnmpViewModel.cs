using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2BattleEditor.Extensions;
using OpenKh.Tools.Kh2BattleEditor.Interfaces;
using OpenKh.Tools.Kh2BattleEditor.Services;

namespace OpenKh.Tools.Kh2BattleEditor.ViewModels
{
    public class EnmpViewModel : MyGenericListModel<EnmpViewModel.EnmpEntryViewModel>, IBattleGetChanges
    {
        public class EnmpEntryViewModel
        {
            public Enmp Enmp { get; }

            public EnmpEntryViewModel(Enmp enmp)
            {
                Enmp = enmp;
            }

            public string Name => $"{Id} {EnemyNameProvider.GetEnemyName(Enmp.Id)}";

            public string Id => $"{Enmp.Id:X02}";

            public short Level { get => Enmp.Level; set => Enmp.Level = value; }
            public short Health { get => Enmp.Health[0]; set => Enmp.Health[0] = value; }
            public short Unknown44 { get => Enmp.Unknown44; set => Enmp.Unknown44 = value; }
            public short Unknown46 { get => Enmp.Unknown46; set => Enmp.Unknown46 = value; }
            public short PhysicalWeakness { get => Enmp.PhysicalWeakness; set => Enmp.PhysicalWeakness = value; }
            public short FireWeakness { get => Enmp.FireWeakness; set => Enmp.FireWeakness = value; }
            public short IceWeakness { get => Enmp.IceWeakness; set => Enmp.IceWeakness = value; }
            public short ThunderWeakness { get => Enmp.ThunderWeakness; set => Enmp.ThunderWeakness = value; }
            public short DarkWeakness { get => Enmp.DarkWeakness; set => Enmp.DarkWeakness = value; }
            public short Unknown52 { get => Enmp.Unknown52; set => Enmp.Unknown52 = value; }
            public short ReflectWeakness { get => Enmp.ReflectWeakness; set => Enmp.ReflectWeakness = value; }
            public short Unknown56 { get => Enmp.Unknown56; set => Enmp.Unknown56 = value; }
            public short Unknown58 { get => Enmp.Unknown58; set => Enmp.Unknown58 = value; }
            public short Unknown5a { get => Enmp.Unknown5a; set => Enmp.Unknown5a = value; }

            public override string ToString() => Name;
        }

        private const string entryName = "enmp";

        private string _searchTerm;

        public EnmpViewModel(IEnumerable<Bar.Entry> entries) :
            this(Enmp.Read(entries.GetBattleStream(entryName)))
        { }

        public EnmpViewModel() :
            this(new List<Enmp>())
        { }

        private EnmpViewModel(IEnumerable<Enmp> items) :
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
            Enmp.Write(stream, UnfilteredItems.Select(x => x.Enmp));

            return stream;
        }

        protected override void OnSelectedItem(EnmpEntryViewModel item)
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

        private bool FilterNone(EnmpEntryViewModel arg) => true;

        private bool FilterByEnemy(EnmpEntryViewModel arg) =>
            arg.Name.ToUpper().Contains(SearchTerm.ToUpper());

        private static EnmpEntryViewModel Map(Enmp item) =>
            new EnmpEntryViewModel(item);
    }
}
