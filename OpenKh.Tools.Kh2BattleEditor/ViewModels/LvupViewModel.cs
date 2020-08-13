using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Tools.Kh2BattleEditor.Extensions;
using OpenKh.Tools.Kh2BattleEditor.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.Kh2BattleEditor.ViewModels
{
    public class LvupViewModel : GenericListModel<LvupViewModel.CharacterViewModel>, IBattleGetChanges
    {
        private const int DefaultType = 2;
        private const string entryName = "lvup";
        private readonly Lvup _lvup;
        public string EntryName => entryName;

        public LvupViewModel(IEnumerable<Bar.Entry> entries) :
            this(Lvup.Read(entries.GetBattleStream(entryName)))
        {
        }

        public LvupViewModel(Lvup Lvup) :
            this(Lvup.Characters)
        {
            _lvup = Lvup;
        }

        public LvupViewModel(IEnumerable<Lvup.PlayableCharacter> characters) : 
            base(characters.Select((x, i) => new CharacterViewModel(x, i)))
        {
        }

        public LvupViewModel() :
            this(new Lvup
            {
                MagicCode = DefaultType,
                Count = 14,
                Unknown08 = new byte[0x30],
                Characters = new List<Lvup.PlayableCharacter>()
            })
        {

        }

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            new Lvup
            {
                MagicCode = _lvup.MagicCode,
                Count = _lvup.Count,
                Unknown08 = _lvup.Unknown08,
                Characters = UnfilteredItems.Select(x => x.Character).ToList()
            }.Write(stream);

            return stream;
        }

        public class CharacterViewModel : GenericListModel<CharacterViewModel.LevelViewModel>
        {
            private readonly int _index;
            public Lvup.PlayableCharacter Character { get; set; }
            public string Name => ((Lvup.PlayableCharacterType)_index).ToString();

            public CharacterViewModel(Lvup.PlayableCharacter character, int index) :
                base(character.Levels.Select((x, i) => new LevelViewModel(x, i)))
            {
                _index = index;
                Character = character;
            }

            public class LevelViewModel
            {
                private readonly Lvup.PlayableCharacter.Level _level;
                private readonly int _index;

                public string Name => $"Level {_index + 1}";

                public LevelViewModel(Lvup.PlayableCharacter.Level level, int index)
                {
                    _level = level;
                    _index = index;
                }

                public int Exp { get => _level.Exp; set => _level.Exp = value; }
                public byte Strength { get => _level.Strength; set => _level.Strength = value; }
                public byte Magic { get => _level.Magic; set => _level.Magic = value; }
                public byte Defense { get => _level.Defense; set => _level.Defense = value; }
                public byte Ap { get => _level.Ap; set => _level.Ap = value; }
                public short SwordAbility { get => _level.SwordAbility; set => _level.SwordAbility = value; }
                public short ShieldAbility { get => _level.ShieldAbility; set => _level.ShieldAbility = value; }
                public short StaffAbility { get => _level.StaffAbility; set => _level.StaffAbility = value; }
            }
        }
    }
}
