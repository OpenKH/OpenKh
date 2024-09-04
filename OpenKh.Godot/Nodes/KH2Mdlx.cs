using Godot;
using Godot.Collections;
using OpenKh.Godot.Resources;
using OpenKh.Kh2;

namespace OpenKh.Godot.Nodes;

[Tool]
public partial class KH2Mdlx : Node3D
{
    [Export] public Skeleton3D Skeleton;
    [Export] public ModelCollisionResource ModelCollisions;
    [Export] public Array<KH2MeshInstance3D> Meshes = new();
}
