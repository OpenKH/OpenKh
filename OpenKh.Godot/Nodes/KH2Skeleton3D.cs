using Godot;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Resources;
using OpenKh.Kh2;

namespace OpenKh.Godot.Nodes;

[Tool]
public partial class KH2Skeleton3D : Skeleton3D
{
    [Export] public KH2MeshInstance3D Mesh;
    [Export] public ModelCollisionResource ModelCollisions;

    [Export] public AnimationBinaryResource CurrentAnimation;
    [Export] public bool Animating = false;
    [Export] public float AnimationTime;
    [Export] public float AnimationDeltaMultiplier = 1;
    [Export] public bool AnimateLoop = true;

    public override void _Process(double delta)
    {
        base._Process(delta);
        
        if (Animating && Mesh is not null && CurrentAnimation is not null)
        {
            AnimationTime += ((float)delta) * AnimationDeltaMultiplier;
            if (AnimateLoop) AnimationTime = AnimationHelpers.Loop(CurrentAnimation.Value.MotionFile, AnimationTime);
            
            AnimationHelpers.ApplyInterpolatedMotion(CurrentAnimation.Value.MotionFile, this, AnimationTime);
        }
    }
}
