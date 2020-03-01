using OpenKh.Kh2;
using System.Collections.Generic;
using Xe.Tools;

namespace OpenKh.Tools.Kh2MapCollisionEditor.ViewModels
{
    public class CollisionEditorViewModel : BaseNotifyPropertyChanged
    {
        private Coct _coct;
        private Coct.Co1 c1Item;
        private Coct.CollisionMesh c2Item;
        private Coct.Co3 c3Item;
        private Coct.Vector4 c4Item;
        private Coct.Co5 c5Item;
        private Coct.Co6 c6Item;
        private Coct.Co7 c7Item;

        public Coct Coct
        {
            get => _coct;
            set
            {
                _coct = value;
                OnAllPropertiesChanged();
            }
        }

        public List<Coct.Co1> C1 => _coct?.Collision1;
        public List<Coct.CollisionMesh> C2 => _coct?.Collision2;
        public List<Coct.Co3> C3 => _coct?.Collision3;
        public List<Coct.Vector4> C4 => _coct?.CollisionVertices;
        public List<Coct.Co5> C5 => _coct?.Collision5;
        public List<Coct.Co6> C6 => _coct?.Collision6;
        public List<Coct.Co7> C7 => _coct?.Collision7;

        public Coct.Co1 C1Item { get => c1Item; set { c1Item = value; OnPropertyChanged(); } }
        public Coct.CollisionMesh C2Item { get => c2Item; set { c2Item = value; OnPropertyChanged(); } }
        public Coct.Co3 C3Item { get => c3Item; set { c3Item = value; OnPropertyChanged(); } }
        public Coct.Vector4 C4Item { get => c4Item; set { c4Item = value; OnPropertyChanged(); } }
        public Coct.Co5 C5Item { get => c5Item; set { c5Item = value; OnPropertyChanged(); } }
        public Coct.Co6 C6Item { get => c6Item; set { c6Item = value; OnPropertyChanged(); } }
        public Coct.Co7 C7Item { get => c7Item; set { c7Item = value; OnPropertyChanged(); } }
    }
}
