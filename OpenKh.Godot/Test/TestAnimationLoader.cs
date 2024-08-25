using System.IO;
using System.Linq;
using Godot;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Resources;
using OpenKh.Kh2;

namespace OpenKh.Godot.Test;

[Tool, GlobalClass]
public partial class TestAnimationLoader : Node
{
    [Export] public KH2Moveset Moveset;
    [Export] public Skeleton3D Skeleton;
    [Export] public float AnimationTime;
    [Export] public float AnimationTimeMultiplier = 1;
    [Export] public int Animation;
    [Export] private bool AnimateTime;
    [Export] private float AnimateTimeLoop = 10;

    //private bool _addedLib;

    private int _lastAnimation;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Skeleton is null || Moveset is null || Animation < 0 || Animation >= Moveset.AnimationBinaries.Count) return;
        var anim = Moveset.AnimationBinaries[Animation];
        if (anim?.MotionFile is null) return;

        if (AnimateTime) AnimationTime = Mathf.Wrap(AnimationTime + (float)delta, 0, AnimateTimeLoop);

        if (_lastAnimation != Animation)
        {
            GD.Print($"Joint Count: {anim.MotionFile.Joints.Count} Bone Count: {anim.MotionFile.InterpolatedMotionHeader.BoneCount} Skeleton Bone Count: {Skeleton.GetBoneCount()}");
            
            GD.Print($"Joint Table");
            for (var i = 0; i < anim.MotionFile.Joints.Count; i++)
            {
                var joint = anim.MotionFile.Joints[i];
                GD.Print($"{i}, {joint.JointId}");
            }
            GD.Print($"FCurve Forward Table");
            foreach (var curve in anim.MotionFile.FCurvesForward) GD.Print($"ID: {curve.JointId} Channel: {curve.ChannelValue}");
            GD.Print($"FCurve Inverse Table");
            foreach (var curve in anim.MotionFile.FCurvesInverse) GD.Print($"ID: {curve.JointId} Channel: {curve.ChannelValue}");
            GD.Print($"Expression Table");
            foreach (var expression in anim.MotionFile.Expressions) GD.Print($"Target: {expression.TargetId} Target Channel: {(Motion.Channel)expression.TargetChannel} Starting Node: {expression.NodeId}");
            
            GD.Print($"Constraint Table");
            foreach (var c in anim.MotionFile.Constraints) GD.Print($"Source: {c.SourceJointId} Target: {c.ConstrainedJointId} Type: {c.Type}");
            
            GD.Print($"Expression Node Table");
            for (var index = 0; index < anim.MotionFile.ExpressionNodes.Count; index++)
            {
                var node = anim.MotionFile.ExpressionNodes[index];
                GD.Print($"{index}: {(Motion.ExpressionType)node.Type}, {node.Element}, {node.IsGlobal}, {node.CAR}, {node.CDR}, {node.Value}");
            }
            _lastAnimation = Animation;
        }
        
        AnimationHelpers.ApplyInterpolatedMotion(anim.MotionFile, Skeleton, AnimationTime * AnimationTimeMultiplier);
    }
}
