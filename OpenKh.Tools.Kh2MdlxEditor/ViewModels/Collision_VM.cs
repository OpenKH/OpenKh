using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    internal class Collision_VM
    {
        public ModelCollision CollisionFile { get; set; }
        public Collision_VM() { }
        public Collision_VM(ModelCollision collisionFile)
        {
            this.CollisionFile = collisionFile;
        }
    }
}
