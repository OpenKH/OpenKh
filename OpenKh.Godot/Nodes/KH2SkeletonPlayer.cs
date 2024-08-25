using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace OpenKh.Godot.Nodes;

[Tool]
public partial class KH2SkeletonPlayer : Node
{
    [Export]
    public Skeleton3D Skeleton
    {
        get => _skeleton;
        set
        {
            _skeleton = value;
            var newCount = _skeleton.GetBoneCount();
            if (newCount != _boneCount)
            {
                _boneCount = newCount;
                Positions = Enumerable.Repeat(Vector3.Zero, _boneCount).ToList();
                Rotations = Enumerable.Repeat(Vector3.Zero, _boneCount).ToList();
                Scales = Enumerable.Repeat(Vector3.One, _boneCount).ToList();
                
                for (var i = 0; i < _boneCount; i++)
                {
                    var transform = _skeleton.GetBoneGlobalPose(i);

                    Positions[i] = transform.Origin;
                    Rotations[i] = transform.Basis.GetRotationQuaternion().GetEuler();
                    Scales[i] = transform.Basis.Scale;
                }
            }
        }
    }

    private Skeleton3D _skeleton;
    public List<Vector3> Positions = new();
    public List<Vector3> Rotations = new();
    public List<Vector3> Scales = new();
    private int _boneCount;

    [Export]
    public int IKHelperCount
    {
        get => _ikHelperCount;
        set
        {
            if (_ikHelperCount == value) return;
            _ikHelperCount = value;
            
            IKHelperPositions = Enumerable.Repeat(Vector3.Zero, _ikHelperCount).ToList();
            IKHelperRotations = Enumerable.Repeat(Vector3.Zero, _ikHelperCount).ToList();
            IKHelperScales = Enumerable.Repeat(Vector3.One, _ikHelperCount).ToList();
        }
    }
    private int _ikHelperCount;
    public List<Vector3> IKHelperPositions = new();
    public List<Vector3> IKHelperRotations = new();
    public List<Vector3> IKHelperScales = new();

    public void Apply()
    {
        for (var i = 0; i < _boneCount; i++)
            Skeleton.SetBoneGlobalPose(i, new Transform3D(new Basis(Quaternion.FromEuler(Rotations[i])).Scaled(Scales[i]), Positions[i]));
    }

    public override void _Ready()
    {
        base._Ready();
        NotifyPropertyListChanged();
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Skeleton is not null) Apply();
    }
}
