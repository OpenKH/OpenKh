using OpenKh.Kh2.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.LevelUpEditor.ViewModels
{
    public class CharactersViewModel : GenericListModel<CharacterViewModel>
    {
        public CharactersViewModel(IEnumerable<Lvup.PlayableCharacter> list) 
            : base(list.Select(x => new CharacterViewModel(x)))
        {
        }

        public CharactersViewModel(IEnumerable<CharacterViewModel> list) : base(list)
        {
        }

        protected override CharacterViewModel OnNewItem()
        {
            throw new NotImplementedException();
        }
    }
}
