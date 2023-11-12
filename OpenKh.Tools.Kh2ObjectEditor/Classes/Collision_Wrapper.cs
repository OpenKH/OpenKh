using OpenKh.Kh2;
using System.ComponentModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Classes
{
    public class Collision_Wrapper : INotifyPropertyChanged
    {
        public bool selected { get; set; }
        public bool Selected_VM { get => selected; set { selected = value; NotifyPropertyChanged("Selected_VM"); } }
        public string Name { get; set; }
        public ObjectCollision Collision { get; set; }

        public Collision_Wrapper(string name, ObjectCollision collision)
        {
            this.Name = name;
            this.Collision = collision;
            this.Selected_VM = false;
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        internal void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
