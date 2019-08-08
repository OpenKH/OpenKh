using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

            public override string ToString() => Name;
        }

        private const int DefaultType = 2;
        private const string entryName = "enmp";

        private readonly int _type;
        private string _searchTerm;

        public EnmpViewModel(IEnumerable<Bar.Entry> entries) :
            this(Enmp.Read(entries.GetBattleStream(entryName)))
        { }

        public EnmpViewModel() :
            this(new BaseBattle<Enmp>
            {
                Id = DefaultType,
                Items = new List<Enmp>()
            })
        { }

        private EnmpViewModel(BaseBattle<Enmp> enmp) :
            this(enmp.Id, enmp.Items)
        { }

        private EnmpViewModel(int type, IEnumerable<Enmp> items) :
            base(items.Select(Map))
        {
            _type = type;
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
            new BaseBattle<Enmp>
            {
                Id = _type,
                Items = Items.Select(x => x.Enmp).ToList()
            }.Write(stream);

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
