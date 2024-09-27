using Godot;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.Nodes
{
    public partial class KH2Entity : CharacterBody3D
    {
        [Export]
        public KH2ObjectEntry ObjectEntry
        {
            get => _entry;
            set
            {
                _entry = value;
            }
        }

        [Export] public KH2Mdlx Model;
        [Export] public CollisionShape3D CollisionShape;
        [Export] public CapsuleShape3D CapsuleShape;
        [Export] public Vector2 Input;
        [Export] public float Rotation;

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            var deltaf = (float)delta;
            
            

            MoveAndSlide();
        }

        private KH2ObjectEntry _entry;
    }
}
