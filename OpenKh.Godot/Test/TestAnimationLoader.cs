using System.IO;
using System.Linq;
using Godot;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Nodes;
using OpenKh.Godot.Resources;
using OpenKh.Kh2;

namespace OpenKh.Godot.Test;

[Tool, GlobalClass]
public partial class TestAnimationLoader : Node
{
    [Export] public KH2Moveset Moveset;
    [Export] public KH2Mdlx Model;
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

        if (Model is null || Moveset is null || Animation < 0 || Animation >= Moveset.AnimationBinaries.Count) return;
        var binary = Moveset.AnimationBinaries[Animation];
        var anim = binary?.Value;
        if (anim?.MotionFile is null) return;

        if (AnimateTime) AnimationTime = Mathf.Wrap(AnimationTime + (float)delta, 0, AnimateTimeLoop);

        if (_lastAnimation != Animation)
        {
            var motion = anim.MotionFile;
            GD.Print($"Count: {motion.InterpolatedMotionHeader.FrameCount} Start: {motion.InterpolatedMotionHeader.FrameData.FrameStart} End: {motion.InterpolatedMotionHeader.FrameData.FrameEnd} FPS: {motion.InterpolatedMotionHeader.FrameData.FramesPerSecond} Ignore Scale: {motion.MotionHeader.SubType == 1}");
            GD.Print($"Joint Count: {anim.MotionFile.Joints.Count} Bone Count: {anim.MotionFile.InterpolatedMotionHeader.BoneCount} Skeleton Bone Count: {Model.Skeleton.GetBoneCount()}");
            
            GD.Print($"Initial Pose Table");
            foreach (var pose in anim.MotionFile.InitialPoses)
            {
                GD.Print($"Bone: {pose.BoneId}, Channel: {pose.ChannelValue} Value: {pose.Value}");
            }
            
            GD.Print($"Joint Table");
            for (var i = 0; i < anim.MotionFile.Joints.Count; i++)
            {
                var joint = anim.MotionFile.Joints[i];
                GD.Print($"Index: {i} Joint:{joint.JointId} Ik: {joint.IK} Translation: {joint.Trans} Rotation: {joint.Rotation} Fixed: {joint.Fixed} Calculated: {joint.Calculated} CM2R: {joint.CalcMatrix2Rot} Ext: {joint.ExtEffector} Reserved: {joint.Reserved}");
            }
            GD.Print($"FCurve Forward Table");
            foreach (var curve in anim.MotionFile.FCurvesForward)
            {
                GD.Print($"ID: {curve.JointId} Channel: {curve.ChannelValue} Pre: {curve.Pre} Post: {curve.Post}");
                if (/*curve.JointId is 13 or 43*/ true)
                {
                    foreach (var key in Enumerable.Range(curve.KeyStartId, curve.KeyCount))
                    {
                        var k = motion.FCurveKeys[key];
                        //GD.Print($"{key}, {k.Type}");
                    }
                }
            }
            GD.Print($"FCurve Inverse Table");
            foreach (var curve in anim.MotionFile.FCurvesInverse)
            {
                GD.Print($"ID: {curve.JointId} Channel: {curve.ChannelValue} Pre: {curve.Pre} Post: {curve.Post}");
                if (/*curve.JointId is 13 or 43*/ true)
                {
                    foreach (var key in Enumerable.Range(curve.KeyStartId, curve.KeyCount))
                    {
                        var k = motion.FCurveKeys[key];
                        //GD.Print($"{key}, {k.Type}");
                    }
                }
            }
            GD.Print($"Expression Table");
            foreach (var expression in anim.MotionFile.Expressions) GD.Print($"Target: {expression.TargetId} Target Channel: {(Motion.Channel)expression.TargetChannel} Starting Node: {expression.NodeId}");
            
            GD.Print($"Constraint Table");
            foreach (var c in anim.MotionFile.Constraints) GD.Print($"Source: {c.SourceJointId} Target: {c.ConstrainedJointId} Type: {c.Type} Limiter: {c.LimiterId}");
            
            GD.Print($"Limiter Table");
            for (var i = 0; i < anim.MotionFile.Limiters.Count; i++)
            {
                var limiter = anim.MotionFile.Limiters[i];
                GD.Print($"Index: {i}, Type: {limiter.Type}, Has Min: {limiter.HasXMin} {limiter.HasYMin} {limiter.HasZMin} Has Max: {limiter.HasXMax} {limiter.HasYMax} {limiter.HasZMax} Min: {limiter.MinX} {limiter.MinY} {limiter.MinZ} {limiter.MinW} Max: {limiter.MaxX} {limiter.MaxY} {limiter.MaxZ} {limiter.MaxW}");
            }
            
            GD.Print($"Expression Node Table");
            for (var index = 0; index < anim.MotionFile.ExpressionNodes.Count; index++)
            {
                var node = anim.MotionFile.ExpressionNodes[index];
                GD.Print($"{index}: {(Motion.ExpressionType)node.Type}, {node.Element}, {node.IsGlobal}, {node.CAR}, {node.CDR}, {node.Value}");
            }
            
            GD.Print($"External Effector Table");
            for (var i = 0; i < anim.MotionFile.ExternalEffectors.Count; i++)
            {
                var effector = anim.MotionFile.ExternalEffectors[i];
                GD.Print($"Index: {i}, Joint: {effector.JointId}");
            }
            _lastAnimation = Animation;
        }
        
        AnimationHelpers.ApplyInterpolatedMotion(anim.MotionFile, Model, AnimationTime * AnimationTimeMultiplier);
    }
}
