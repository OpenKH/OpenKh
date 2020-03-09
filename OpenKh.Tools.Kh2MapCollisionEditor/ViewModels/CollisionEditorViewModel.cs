using Microsoft.Xna.Framework.Graphics;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MapCollisionEditor.Services;
using System.Collections.Generic;
using System.Linq;
using Xe.Drawing;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

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
        private bool _allC2;
        private bool _allC3;
        private readonly CollisionMapDrawHandler _drawHandler;

        public IDrawing DrawingContext { get; }
        public RelayCommand DrawCreate { get; }
        public RelayCommand DrawDestroy { get; }
        public RelayCommand DrawBegin { get; }
        public RelayCommand DrawEnd { get; }

        public Coct Coct
        {
            get => _coct;
            set
            {
                _coct = value;
                _drawHandler.Coct = value;
                OnAllPropertiesChanged();
            }
        }

        public List<Coct.Co1> C1 => _coct?.Collision1;
        public IEnumerable<Coct.CollisionMesh> C2 => !AllC2 ? C1Item?.Meshes : C1.SelectMany(c1 => c1.Meshes);
        public IEnumerable<Coct.Co3> C3 => !AllC3 ? C2Item?.Items : C1.SelectMany(c1 => c1.Meshes.SelectMany(c2 => c2.Items));
        public IEnumerable<Coct.Vector4> C4 { get
            {
                if (C3Item == null) yield break;
                yield return _coct?.CollisionVertices[C3Item.Vertex1];
                yield return _coct?.CollisionVertices[C3Item.Vertex2];
                yield return _coct?.CollisionVertices[C3Item.Vertex3];
                if (C3Item.Vertex4 >= 0)
                    yield return _coct?.CollisionVertices[C3Item.Vertex4];
            } }
        public List<Coct.Co5> C5 => _coct?.Collision5;
        public List<Coct.Co6> C6 => _coct?.Collision6;
        public List<Coct.Co7> C7 => _coct?.Collision7;

        public Coct.Co1 C1Item { get => c1Item; set { c1Item = value; OnPropertyChanged(); OnPropertyChanged(nameof(C2)); } }
        public Coct.CollisionMesh C2Item { get => c2Item; set { c2Item = value; OnPropertyChanged(); OnPropertyChanged(nameof(C3)); } }
        public Coct.Co3 C3Item { get => c3Item; set { c3Item = value; OnPropertyChanged(); OnPropertyChanged(nameof(C4)); } }
        public Coct.Vector4 C4Item { get => c4Item; set { c4Item = value; OnPropertyChanged(); } }
        public Coct.Co5 C5Item { get => c5Item; set { c5Item = value; OnPropertyChanged(); } }
        public Coct.Co6 C6Item { get => c6Item; set { c6Item = value; OnPropertyChanged(); } }
        public Coct.Co7 C7Item { get => c7Item; set { c7Item = value; OnPropertyChanged(); } }

        public bool AllC2 { get => _allC2; set { _allC2 = value; OnPropertyChanged(nameof(C2)); } }
        public bool AllC3 { get => _allC3; set { _allC3 = value; OnPropertyChanged(nameof(C3)); } }

        public CollisionEditorViewModel(GraphicsDevice graphicsDevice, IDrawing drawing)
        {
            _drawHandler = new CollisionMapDrawHandler();
            _drawHandler.Initialize(graphicsDevice);

            DrawingContext = drawing;

            DrawCreate = new RelayCommand(_ => { });
            DrawDestroy = new RelayCommand(_ => _drawHandler.Destroy());
            DrawBegin = new RelayCommand(_ => _drawHandler.DrawBegin());
            DrawEnd = new RelayCommand(_ => _drawHandler.DrawEnd());
        }
    }
}
