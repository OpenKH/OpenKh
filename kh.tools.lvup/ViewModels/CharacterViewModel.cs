using kh.kh2;
using System.Collections.ObjectModel;
using Xe.Tools;

namespace kh.tools.lvup.ViewModels
{
    public class CharacterViewModel : BaseNotifyPropertyChanged
    {
        public Lvup.PlayableCharacter Character { get; }
        private Lvup.LevelUpEntry selectedItem;
        private int selectedIndex;

        public CharacterViewModel(Lvup.PlayableCharacter character)
        {
            Character = character;
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
            get => Character.Name;
        }

        public ObservableCollection<Lvup.LevelUpEntry> Items
        {
            get => Character.Levels;
            set
            {
                Character.Levels = value;
                OnPropertyChanged();
            }
        }
    }
}
