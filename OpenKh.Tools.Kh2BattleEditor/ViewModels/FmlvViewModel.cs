using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Tools.Kh2BattleEditor.Extensions;
using OpenKh.Tools.Kh2BattleEditor.Interfaces;
using OpenKh.Tools.Kh2BattleEditor.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.Kh2BattleEditor.ViewModels
{
    public class FmlvViewModel : GenericListModel<FmlvFormViewModel>, IBattleGetChanges
    {
        private const int DefaultType = 2;
        private const string entryName = "fmlv";
        
        
        private readonly int _type;
        //private static bool isFinalMix = false;
        public string EntryName => entryName;
        
        public FmlvViewModel() :
            this(new BaseBattle<Fmlv2>
            {
                Id = DefaultType,
                Items = new List<Fmlv2>()
            })
        {}

        public FmlvViewModel(IEnumerable<Bar.Entry> entries) :
            this(Fmlv2.Read(entries.GetBattleStream(entryName)))
        {}


        public FmlvViewModel(BaseBattle<Fmlv2> fmlv) :
            this(fmlv.Id, fmlv.Items, (fmlv.Items.Count == 0x2D))
        {}

        public FmlvViewModel(int type, IEnumerable<Fmlv2> list, bool isFinalMix) :
            base(list.GroupBy(x => x.FormId).Select(x => new FmlvFormViewModel(x, isFinalMix)))
        {
            _type = type;
        }

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            new BaseBattle<Fmlv2>
            {
                Id = _type,
                Items = Items.SelectMany(form => form).Select(x => x.Fmlv).ToList()

            }.Write(stream);

            return stream;
        }
    }

    public class FmlvFormViewModel : GenericListModel<FmlvFormViewModel.FmlvEntryViewModel>
    {
        private readonly IGrouping<int, Fmlv2> fmlvGroup;
        private readonly bool isFinalMix;

        public FmlvFormViewModel(IGrouping<int, Fmlv2> x, bool isFinalMix) :
            base(x.Select(y => new FmlvEntryViewModel(y)))
            
        {
            fmlvGroup = x;
            this.isFinalMix = isFinalMix;
        }

        public string Name => FormNameProvider.GetFormName(fmlvGroup.Key, isFinalMix);

        public class FmlvEntryViewModel
        {
            public Fmlv2 Fmlv { get; }
            public FmlvEntryViewModel(Fmlv2 fmlv)
            {
                Fmlv = fmlv;
            }

            public string Name => $"{Fmlv.FormLevel}";
            public byte LevelOfMovementAbility { get => Fmlv.LevelOfMovementAbility; set => Fmlv.LevelOfMovementAbility = value; }
            public short AbilityId { get => Fmlv.AbilityId; set => Fmlv.AbilityId = value; }
            public int Exp { get => Fmlv.Exp; set => Fmlv.Exp = value; }

            public override string ToString() => Name;
        }
    }
}
