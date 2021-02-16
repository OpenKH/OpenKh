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
        private const string entryName = "fmlv";
        
        public string EntryName => entryName;
        
        public FmlvViewModel() :
            this(new List<Fmlv.Level>())
        {}

        public FmlvViewModel(IEnumerable<Bar.Entry> entries) :
            this(Fmlv.Read(entries.GetBattleStream(entryName)))
        {}


        public FmlvViewModel(IEnumerable<Fmlv.Level> levels) :
            this(levels, (levels.ToList().Count == 0x2D))
        {}

        public FmlvViewModel(IEnumerable<Fmlv.Level> list, bool isFinalMix) :
            base(list.GroupBy(x => x.FormId).Select(x => new FmlvFormViewModel(x, isFinalMix)))
        {}

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            Fmlv.Write(stream, UnfilteredItems.SelectMany(form => form).Select(x => x.Level).ToList());
            return stream;
        }
    }

    public class FmlvFormViewModel : GenericListModel<FmlvFormViewModel.FmlvLevelViewModel>
    {
        private readonly IGrouping<int, Fmlv.Level> fmlvGroup;
        private readonly bool isFinalMix;

        public FmlvFormViewModel(IGrouping<int, Fmlv.Level> x, bool isFinalMix) :
            base(x.Select(y => new FmlvLevelViewModel(y)))
            
        {
            fmlvGroup = x;
            this.isFinalMix = isFinalMix;
        }

        public string Name => FormNameProvider.GetFormName(fmlvGroup.Key, isFinalMix);

        public class FmlvLevelViewModel
        {
            public Fmlv.Level Level { get; }
            public FmlvLevelViewModel(Fmlv.Level level)
            {
                Level = level;
            }

            public string Name => $"Level {Level.FormLevel}";
            public byte LevelGrowthAbility { get => Level.LevelGrowthAbility; set => Level.LevelGrowthAbility = value; }
            public short Ability { get => Level.Ability; set => Level.Ability = value; }
            public int Exp { get => Level.Exp; set => Level.Exp = value; }
        }
    }
}
