using kh.kh2;
using System.Collections.ObjectModel;
using Xe.Tools;

namespace kh.tools.lvup.ViewModels
{
    public class CharacterViewModel : BaseNotifyPropertyChanged
    {
        private Lvup.PlayableCharacter _char;
        private Lvup.LevelUpEntry selectedItem;
        private int selectedIndex;

        public CharacterViewModel(Lvup.PlayableCharacter character)
        {
            _char = character;
        }

        public Lvup.LevelUpEntry SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged();
            }
        }

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _char.Name;
        }

        public ObservableCollection<Lvup.LevelUpEntry> Items
        {
            get => _char.Levels;
            set
            {
                _char.Levels = value;
                OnPropertyChanged();
            }
        }
    }
}
