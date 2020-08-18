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
        private CoctLogical _coct;
        private CoctLogical.CoctCollisionMeshGroup c1Item;
        private CoctLogical.CoctCollisionMesh c2Item;
        private CoctLogical.CoctCollision c3Item;
        private CoctLogical.CoctVector4 c4Item;
        private bool _allC2;
        private bool _allC3;
        private readonly CollisionMapDrawHandler _drawHandler;

        public IDrawing DrawingContext { get; }
        public RelayCommand DrawCreate { get; }
        public RelayCommand DrawDestroy { get; }
        public RelayCommand DrawBegin { get; }
        public RelayCommand DrawEnd { get; }

        public CoctLogical Coct
        {
            get => _coct;
            set
            {
                _coct = value;
                _drawHandler.Coct = value;
                OnAllPropertiesChanged();
            }
        }

        public List<CoctLogical.CoctCollisionMeshGroup> C1 => _coct?.CollisionMeshGroupList;
        public IEnumerable<CoctLogical.CoctCollisionMesh> C2 => !AllC2 ? C1Item?.Meshes : C1.SelectMany(c1 => c1.Meshes);
        public IEnumerable<CoctLogical.CoctCollision> C3 => !AllC3 ? C2Item?.Items : C1.SelectMany(c1 => c1.Meshes.SelectMany(c2 => c2.Items));
        public IEnumerable<CoctLogical.CoctVector4> C4 { get
            {
                if (C3Item == null) yield break;
                yield return _coct?.VertexList[C3Item.Vertex1];
                yield return _coct?.VertexList[C3Item.Vertex2];
                yield return _coct?.VertexList[C3Item.Vertex3];
                if (C3Item.Vertex4 >= 0)
                    yield return _coct?.VertexList[C3Item.Vertex4];
            } }

        public CoctLogical.CoctCollisionMeshGroup C1Item { get => c1Item; set { c1Item = value; OnPropertyChanged(); OnPropertyChanged(nameof(C2)); } }
        public CoctLogical.CoctCollisionMesh C2Item { get => c2Item; set { c2Item = value; OnPropertyChanged(); OnPropertyChanged(nameof(C3)); } }
        public CoctLogical.CoctCollision C3Item { get => c3Item; set { c3Item = value; OnPropertyChanged(); OnPropertyChanged(nameof(C4)); } }
        public CoctLogical.CoctVector4 C4Item { get => c4Item; set { c4Item = value; OnPropertyChanged(); } }

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
