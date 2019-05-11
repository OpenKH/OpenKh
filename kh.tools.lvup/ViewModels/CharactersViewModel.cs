using kh.kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace kh.tools.lvup.ViewModels
{
    public class CharactersViewModel : GenericListModel<CharacterViewModel>
    {
        public CharactersViewModel(IEnumerable<PlayableCharacter> list) 
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
