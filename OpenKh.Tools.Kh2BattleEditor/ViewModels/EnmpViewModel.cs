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

            public string Name => $"{Id} > {EnemyNameProvider.GetEnemyName(Enmp.Id)}";

            public short Id { get => Enmp.Id; set => Enmp.Id = value; }
            public short Level { get => Enmp.Level; set => Enmp.Level = value; }
            public short Health { get => Enmp.Health[0]; set => Enmp.Health[0] = value; }
            // 32 Available extra health, only up to 5 are used.
            public short ExtraHealth1 { get => Enmp.Health[1]; set => Enmp.Health[1] = value; }
            public short ExtraHealth2 { get => Enmp.Health[2]; set => Enmp.Health[2] = value; }
            public short ExtraHealth3 { get => Enmp.Health[3]; set => Enmp.Health[3] = value; }
            public short ExtraHealth4 { get => Enmp.Health[4]; set => Enmp.Health[4] = value; }
            public short ExtraHealth5 { get => Enmp.Health[5]; set => Enmp.Health[5] = value; }
            public short MaxDamage { get => Enmp.MaxDamage; set => Enmp.MaxDamage = value; }
            public short MinDamage { get => Enmp.MinDamage; set => Enmp.MinDamage = value; }
            public short PhysicalWeakness { get => Enmp.PhysicalWeakness; set => Enmp.PhysicalWeakness = value; }
            public short FireWeakness { get => Enmp.FireWeakness; set => Enmp.FireWeakness = value; }
            public short IceWeakness { get => Enmp.IceWeakness; set => Enmp.IceWeakness = value; }
            public short ThunderWeakness { get => Enmp.ThunderWeakness; set => Enmp.ThunderWeakness = value; }
            public short DarkWeakness { get => Enmp.DarkWeakness; set => Enmp.DarkWeakness = value; }
            public short SpecialWeakness { get => Enmp.SpecialWeakness; set => Enmp.SpecialWeakness = value; }
            public short MaxWeakness { get => Enmp.MaxWeakness; set => Enmp.MaxWeakness = value; }
            public short Experience { get => Enmp.Experience; set => Enmp.Experience = value; }
            public short Prize { get => Enmp.Prize; set => Enmp.Prize = value; }
            public short BonusLevel { get => Enmp.BonusLevel; set => Enmp.BonusLevel = value; }

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
