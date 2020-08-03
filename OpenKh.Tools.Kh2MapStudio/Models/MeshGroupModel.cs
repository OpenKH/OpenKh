using OpenKh.Engine.MonoGame;

namespace OpenKh.Tools.Kh2MapStudio.Models
{
    class MeshGroupModel
    {
        public MeshGroupModel(string name, MeshGroup meshGroup)
        {
            Name = name;
            MeshGroup = meshGroup;
            IsVisible = true;
        }

        public string Name { get; }
        public MeshGroup MeshGroup { get; }
        public bool IsVisible { get; set; }
    }
}
