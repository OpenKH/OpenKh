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
    public class SumnViewModel : MyGenericListModel<SumnViewModel.SumnEntryViewModel>, IBattleGetChanges
    {
        public class SumnEntryViewModel
        {
            public Sumn Sumn { get; }

            public SumnEntryViewModel(Sumn sumn)
            {
                Sumn = sumn;
            }

            public ushort Command { get => Sumn.Command; set => Sumn.Command = value; }
            public ushort Item { get => Sumn.Item; set => Sumn.Item = value; }
            public uint Entity1 { get => Sumn.Entity1; set => Sumn.Entity1 = value; }
            public uint Entity2 { get => Sumn.Entity2; set => Sumn.Entity2 = value; }
            public ushort LimitCommand { get => Sumn.LimitCommand; set => Sumn.LimitCommand = value; }
        }

        private const string entryName = "sumn";

        private string _searchTerm;

        public SumnViewModel(IEnumerable<Bar.Entry> entries) :
            this(Sumn.Read(entries.GetBattleStream(entryName)))
        { }

        public SumnViewModel() :
            this(new List<Sumn>())
        { }

        private SumnViewModel(IEnumerable<Sumn> items) :
            base(items.Select(Map))
        {
        }

        public string EntryName => entryName;

        public Visibility IsItemEditingVisible => IsItemSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsItemEditMessageVisible => !IsItemSelected ? Visibility.Visible : Visibility.Collapsed;
        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            Sumn.Write(stream, UnfilteredItems.Select(x => x.Sumn));
            return stream;
        }

        protected override void OnSelectedItem(SumnEntryViewModel item)
        {
            base.OnSelectedItem(item);

            OnPropertyChanged(nameof(IsItemEditingVisible));
            OnPropertyChanged(nameof(IsItemEditMessageVisible));
        }

        private static SumnEntryViewModel Map(Sumn item) =>
            new SumnEntryViewModel(item);
    }
}
