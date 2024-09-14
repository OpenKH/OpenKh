using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;
using OpenKh.Godot.Nodes;
using OpenKh.Kh2;
using Quaternion = Godot.Quaternion;
using Vector3 = Godot.Vector3;

namespace OpenKh.Godot.Helpers
{
    public static class AnimationHelpers
    {
        public static bool IsPositionChannel(this Motion.Channel channel) => channel is Motion.Channel.TRANSLATION_X or Motion.Channel.TRANSLATION_Y or Motion.Channel.TRANSLATION_Z;
        public static bool IsRotationChannel(this Motion.Channel channel) => channel is Motion.Channel.ROTATION_X or Motion.Channel.ROTATION_Y or Motion.Channel.ROTATION_Z;
        public static bool IsScaleChannel(this Motion.Channel channel) => channel is Motion.Channel.SCALE_X or Motion.Channel.SCALE_Y or Motion.Channel.SCALE_Z;

        public static bool IsXChannel(this Motion.Channel channel) => channel is Motion.Channel.TRANSLATION_X or Motion.Channel.ROTATION_X or Motion.Channel.SCALE_X;
        public static bool IsYChannel(this Motion.Channel channel) => channel is Motion.Channel.TRANSLATION_Y or Motion.Channel.ROTATION_Y or Motion.Channel.SCALE_Y;
        public static bool IsZChannel(this Motion.Channel channel) => channel is Motion.Channel.TRANSLATION_Z or Motion.Channel.ROTATION_Z or Motion.Channel.SCALE_Z;
        
        public static float CubicHermite(float t, float p0, float p1, float m0, float m1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return (2 * t3 - 3 * t2 + 1) * p0 + (t3 - 2 * t2 + t) * m0 + (-2 * t3 + 3 * t2) * p1 + (t3 - t2) * m1;
        }
        
        public static Quaternion AnimationRotation(Vector3 rotation) => ImportHelpers.CommonRotation(rotation.X, rotation.Y, rotation.Z);
        public static Transform3D CreateTransform(Vector3 pos, Vector3 rot, Vector3 scale)
        {
            var scaleTransform = Transform3D.Identity.Scaled(scale);
            var rotationTransform = new Transform3D(new Basis(AnimationRotation(rot)), Vector3.Zero);
            var translationTransform = Transform3D.Identity.Translated(pos);

            return translationTransform * rotationTransform * scaleTransform;
        }

        private class AnimationSimulation
        {
            private Vector3[] Positions;
            private Vector3[] Rotations;
            private Vector3[] Scales;
            private int[] Parent;
            private Transform3D[] Transforms;
            private int BoneCount;
            private int IKCount;
            private int TotalCount;
            private Motion.InterpolatedMotion MotionFile;
            private float Time;
            private bool IgnoreScale;
            
            private Transform3D GetGlobalTransform(int index)
            {
                if (index < 0) return Transform3D.Identity;
                var currentTransform = Transforms[index];
                var parent = Parent[index];
                if (parent < 0) return currentTransform;
                return GetGlobalTransform(parent) * currentTransform;
            }
            private void SetGlobalTransform(int index, Transform3D value)
            {
                var parent = Parent[index];
                if (parent < 0) Transforms[index] = value;
                var parentTransform = GetGlobalTransform(parent);
                var newTransform = parentTransform.AffineInverse() * value;
                Transforms[index] = newTransform;
            }
            private void SetBoneValue(Motion.Channel channel, int index, float value)
            {
                Vector3[] array;
                if (channel.IsPositionChannel()) array = Positions;
                else if (channel.IsRotationChannel()) array = Rotations;
                else array = Scales;
                
                if (channel.IsXChannel()) array[index].X = value;
                else if (channel.IsYChannel()) array[index].Y = value;
                else array[index].Z = value;
            }
            private void ExpressionSetBoneValue(Motion.Channel channel, int index, float value)
            {
                var val = value;
                Vector3[] array;
                if (channel.IsPositionChannel()) array = Positions;
                else if (channel.IsRotationChannel())
                {
                    array = Rotations;
                    val = Mathf.DegToRad(val);
                }
                else array = Scales;
                
                if (channel.IsXChannel()) array[index].X = val;
                else if (channel.IsYChannel()) array[index].Y = val;
                else array[index].Z = val;
            }

            public AnimationSimulation(Motion.InterpolatedMotion motion, KH2Skeleton3D skeleton, float time)
            {
                MotionFile = motion;
                Time = time;

                IgnoreScale = motion.MotionHeader.SubType == 1;
                
                skeleton.ResetBonePoses();
                var skeletonBoneCount = skeleton.GetBoneCount();

                var totalCount = motion.Joints.Count;

                BoneCount = motion.InterpolatedMotionHeader.BoneCount;
                TotalCount = motion.InterpolatedMotionHeader.TotalBoneCount;
                IKCount = TotalCount - BoneCount;
                
                Positions = Enumerable.Repeat(Vector3.Zero, totalCount).ToArray();
                Rotations = Enumerable.Repeat(Vector3.Zero, totalCount).ToArray();
                Scales = Enumerable.Repeat(Vector3.One, totalCount).ToArray();
                Parent = Enumerable.Repeat(-1, totalCount).ToArray();
                Transforms = Enumerable.Repeat(Transform3D.Identity, totalCount).ToArray();
                
                for (var i = 0; i < motion.InterpolatedMotionHeader.BoneCount; i++)
                {
                    if (i >= skeletonBoneCount) break;
                    var pose = skeleton.GetBonePose(i);
                    Positions[i] = pose.Origin / ImportHelpers.KH2PositionScale;
                    Rotations[i] = pose.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Zyx);
                    Scales[i] = pose.Basis.Scale;
                    Parent[i] = skeleton.GetBoneParent(i);
                    //GD.Print($"{i}, {joint.JointId}");
                }

                for (var i = 0; i < motion.IKHelpers.Count; i++)
                {
                    var ikHelper = motion.IKHelpers[i];
                    var index = /*BoneCount + i*/ ikHelper.Index; //TODO
                    Positions[index] = new Vector3(ikHelper.TranslateX, ikHelper.TranslateY, ikHelper.TranslateZ);
                    Rotations[index] = new Vector3(ikHelper.RotateX, ikHelper.RotateY, ikHelper.RotateZ);
                    Scales[index] = new Vector3(ikHelper.ScaleX, ikHelper.ScaleY, ikHelper.ScaleZ);
                    Parent[index] = ikHelper.ParentId;
                }
                
                //apply rest values
                foreach (var rest in motion.InitialPoses) SetBoneValue(rest.ChannelValue, rest.BoneId, rest.Value);
                
                //apply fcurves
                foreach (var fCurve in motion.FCurvesForward) HandleCurve(fCurve);
                foreach (var fCurve in motion.FCurvesInverse) HandleCurve(fCurve, true);
                
                //TODO: order of operations for expressions and constraints? which goes first?
                
                //apply expressions to bones
                foreach (var expression in motion.Expressions) ExpressionSetBoneValue((Motion.Channel)expression.TargetChannel, expression.TargetId, Expression(motion.ExpressionNodes, expression.NodeId));
                
                //convert SRT to matrices
                for (var i = 0; i < totalCount; i++)
                {
                    //Transforms[i] = CreateTransform(Positions[i], Rotations[i], Scales[i]);
                    Transforms[i] = /*CreateTransform(basePositions[i], baseRotations[i], baseScales[i]) * */ CreateTransform(Positions[i], Rotations[i], Scales[i]);
                }
                
                //apply constraints to bones
                foreach (var constraint in motion.Constraints)
                {
                    var source = constraint.SourceJointId;
                    var target = constraint.ConstrainedJointId;

                    var limiter = constraint.LimiterId == -1 ? null : motion.Limiters[constraint.LimiterId];

                    var type = constraint.Type;
                    // 0 	Position                Set global position to another bone
                    // 1 	Path                    Follow a path TODO: uhhhhh?
                    // 2 	Orientation             Set global rotation to another bone
                    // 3 	Direction               Point an object at another object, using a specified axis. TODO how do we know what axis, it's default is X
                    // 4 	Up Vector               Point an object at another object, using the Y axis.
                    // 5 	Two Points              Set global position between two other bones
                    // 6 	Scale                   Set global scale to another bone
                    // 7 	Camera                  Custom, no idea, probably put the bone directly on the camera?
                    // 8 	Camera Path             Custom, no idea
                    // 9 	Int Path                Custom, no idea
                    // 10 	Int                     Custom, no idea
                    // 11 	Camera Up Vector        Custom, no idea
                    // 12 	Position Limit
                    // 13 	Rotation Limit
                
                    switch (type)
                    {
                        case 0:
                        {
                            SetGlobalTransform(target, GetGlobalTransform(target) with { Origin = GetGlobalTransform(source).Origin });
                            break;
                        }
                        case 2:
                        {
                            var targetRot = GetGlobalTransform(source).Basis.GetRotationQuaternion();
                        
                            var current = GetGlobalTransform(target);
                            SetGlobalTransform(target, current with { Basis = new Basis(targetRot).Scaled(current.Basis.Scale) });
                            break;
                        }
                        case 3:
                        case 4:
                        {
                            var sourceTransform = GetGlobalTransform(source);

                            var currentParentTransform = GetGlobalTransform(Parent[target]);

                            var current = GetGlobalTransform(target);
                            
                            var up = (currentParentTransform.Basis * Vector3.Up).Normalized();
                            
                            if (sourceTransform.Origin.Normalized().Abs().IsEqualApprox(up))
                            {
                                //TODO: point upward or downward
                                GD.Print("IMPLEMENT: point upward");
                            }
                            else
                            {
                                /*
                                DebugDraw3D.DrawPosition(Transform3D.Identity.Scaled(Vector3.One * ImportHelpers.KH2PositionScale) * sourceTransform.ScaledLocal(
                                    (Vector3.One / ImportHelpers.KH2PositionScale) / 2), Colors.Pink, 0.5f);
                                    */
                                
                                var offset = type == 3 ? Quaternion.FromEuler(new Vector3(0, Mathf.Pi / 2, 0)) : Quaternion.FromEuler(new Vector3(Mathf.Pi / 2, 0, 0));
                                
                                var newTransform = current.LookingAt(sourceTransform.Origin, up) * new Transform3D(new Basis(offset), Vector3.Zero); //TODO: is this correct?
                                SetGlobalTransform(target, newTransform);

                                //var newTarget = GetGlobalTransform(target);
                                
                                //DebugDraw3D.DrawLine(newTarget.Origin * ImportHelpers.KH2PositionScale, (newTarget.TranslatedLocal(Vector3.Right * 2000).Origin * ImportHelpers.KH2PositionScale));
                            }
                            
                            break;
                        }
                        case 6:
                        {
                            var targetScale = GetGlobalTransform(source).Basis.Scale;

                            var current = GetGlobalTransform(target);
                            SetGlobalTransform(target, current with { Basis = new Basis(current.Basis.GetRotationQuaternion()).Scaled(targetScale) });
                            break;
                        }
                        //mute these until i actually implement them
                        case 12:
                        {
                            if (limiter is null) continue;
                            
                            var sourceTransform = GetGlobalTransform(source);
                            var targetTransform = GetGlobalTransform(target);

                            var targetRelative = sourceTransform.AffineInverse() * targetTransform;

                            if (limiter.Type == Motion.LimiterType.Box)
                            {
                                
                            }
                            else
                            {
                                
                            }
                            break;
                        }
                        case 13:
                        {
                            if (limiter is null) continue;
                            
                            var transform = Transforms[target];
                            var rotation = transform.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Zyx);
                            rotation = rotation.Clamp(new Vector3(limiter.MinX, limiter.MinY, limiter.MinZ), new Vector3(limiter.MaxX, limiter.MaxY, limiter.MaxZ));

                            Transforms[target] = CreateTransform(transform.Origin, rotation, transform.Basis.Scale);
                            
                            //TODO: ehhhh?
                            
                            break;
                        }
                        default:
                        {
                            GD.Print($"not implemented constraint: {type}");
                            break;
                        }
                    }
                }

                //apply to skeleton
                for (var i = 0; i < TotalCount; i++)
                {
                    DebugDraw3D.DrawPosition(GetGlobalTransform(i).Scaled(Vector3.One * ImportHelpers.KH2PositionScale), i < motion.InterpolatedMotionHeader.BoneCount ? Colors.Aqua : Colors.DarkRed);

                    if (i >= BoneCount) continue;
                    
                    var transform = Transforms[i];
                    transform = transform with
                    {
                        Origin = transform.Origin * ImportHelpers.KH2PositionScale,
                    };
                    skeleton.SetBonePose(i, transform);
                }

                var collisionBones = new List<int>();

                if (skeleton.ModelCollisions is not null)
                {
                    foreach (var collision in skeleton.ModelCollisions.Value.EntryList.Where(collision => (ObjectCollision.TypeEnum)collision.Type == ObjectCollision.TypeEnum.IK))
                    {
                        //TODO: actual collision
                        var centerBone = collision.Bone;
                        var firstBone = skeleton.GetBoneParent(centerBone);
                        var tipBone = skeleton.GetBoneChildren(centerBone).First();
                    
                        collisionBones.AddRange([centerBone, firstBone, tipBone]);

                        CalculateIK(skeleton, firstBone, centerBone, tipBone, false);
                    }
                }
                
                for (var i = 0; i < motion.Joints.Count - 2; i++)
                {
                    var a = motion.Joints[i]; //first
                    var b = motion.Joints[i + 1]; //center
                    var c = motion.Joints[i + 2]; //tip
                    
                    var firstBone = a.JointId;
                    var centerBone = b.JointId;
                    var tipBone = c.JointId;
                    
                    if ((a.CalcMatrix2Rot && b.Trans && c.Calculated) && 
                        (firstBone < centerBone && centerBone < tipBone) && 
                        (!collisionBones.Contains(firstBone) && !collisionBones.Contains(centerBone) && !collisionBones.Contains(tipBone)))
                    {
                        CalculateIK(skeleton, firstBone, centerBone, tipBone, true);
                    }
                }
            }

            private void CalculateIK(Skeleton3D skeleton, int first, int center, int tip, bool flip)
            {
                //return;
                var targetTransform = skeleton.GetBoneGlobalPose(tip);
                skeleton.SetBonePoseRotation(center, Quaternion.Identity);
                skeleton.ResetBonePose(center);
                skeleton.ResetBonePose(tip);

                var firstGlobalPose = skeleton.GetBoneGlobalPose(first);
                var centerGlobalPose = skeleton.GetBoneGlobalPose(center);
                var tipGlobalPose = skeleton.GetBoneGlobalPose(tip);
                var firstParentGlobalPose = skeleton.GetBoneGlobalPose(skeleton.GetBoneParent(first));

                var firstToTarget = firstGlobalPose.Origin.DistanceTo(targetTransform.Origin);
                var firstBoneLength = firstGlobalPose.Origin.DistanceTo(centerGlobalPose.Origin);
                var secondBoneLength = centerGlobalPose.Origin.DistanceTo(tipGlobalPose.Origin);

                if (firstBoneLength + secondBoneLength < firstToTarget) return;

                //var targetInverse = targetTransform.AffineInverse();
                var tipInverse = tipGlobalPose.AffineInverse();
                
                var centerRelativeToTip = tipInverse * centerGlobalPose;
                //var firstRelativeToTip = tipInverse * firstGlobalPose;
                //var firstParentRelativeToTip = tipInverse * firstParentGlobalPose;
                
                var aAngle = Mathf.Acos(((firstBoneLength*firstBoneLength)+(secondBoneLength*secondBoneLength)-(firstToTarget*firstToTarget)) / (2 * firstBoneLength * secondBoneLength));
                var bAngle = Mathf.Acos(((firstToTarget*firstToTarget)+(secondBoneLength*secondBoneLength)-(firstBoneLength*firstBoneLength)) / (2 * firstToTarget * secondBoneLength));
                var cAngle = Mathf.Acos(((firstToTarget*firstToTarget)+(firstBoneLength*firstBoneLength)-(secondBoneLength*secondBoneLength)) / (2 * firstToTarget * firstBoneLength));

                var firstAngle = -cAngle;
                var centerAngle = Mathf.Pi - aAngle;
                //var transformAngle = bAngle;

                if (flip)
                {
                    centerAngle *= -1;
                    firstAngle *= -1;
                    //transformAngle *= -1;
                }

                //var transformedCenterRelativeToTip = new Transform3D(new Basis(Quaternion.FromEuler(new Vector3(0,0,transformAngle))), Vector3.Zero) * centerRelativeToTip;
                
                //DebugDraw3D.DrawPosition(targetTransform);
                //var transformedFirstRelativeToTip = firstRelativeToTip.LookingAt(transformedCenterRelativeToTip.Origin, (firstParentRelativeToTip * Vector3.Up).Normalized());

                //var centerGlobal = targetTransform * transformedCenterRelativeToTip;
                //var centerRelativeToFirstParent = firstParentGlobalPose.AffineInverse() * centerGlobal;
                
                /*
                skeleton.SetBonePose(first, skeleton.GetBonePose(first).LookingAt(centerRelativeToFirstParent.Origin) * 
                                            new Transform3D(new Basis(Quaternion.FromEuler(new Vector3(-Mathf.Pi / 2, Mathf.Pi / 2, 0))), Vector3.Zero));
                                            */
                
                //skeleton.SetBoneGlobalPose(first, (targetTransform * transformedFirstRelativeToTip) * new Transform3D(new Basis(Quaternion.FromEuler(new Vector3(-Mathf.Pi / 2, 0, 0))), Vector3.Zero));
                //skeleton.SetBoneGlobalPose(center, targetTransform * transformedCenterRelativeToTip);

                //skeleton.SetBoneGlobalPose(center, skeleton.GetBoneGlobalPose(center) * new Transform3D(new Basis(Quaternion.FromEuler(new Vector3(0,0,centerAngle))), Vector3.Zero));
                skeleton.SetBonePoseRotation(center, Quaternion.FromEuler(new Vector3(0,0,centerAngle)));
                skeleton.SetBonePoseRotation(first, skeleton.GetBonePoseRotation(first) * Quaternion.FromEuler(new Vector3(0,0,firstAngle)));


                //firstGlobalPose = skeleton.GetBoneGlobalPose(first);

                //var rotationDiff = targetTransform.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Xyz).X - firstGlobalPose.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Xyz).X;
                //var rotationDiff2 = targetTransform.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Zyx).X - firstGlobalPose.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Zyx).X;

                //var rotationDiff3 = firstGlobalPose.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Xyz).X - targetTransform.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Xyz).X;
                //var rotationDiff4 = firstGlobalPose.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Zyx).X - targetTransform.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Zyx).X;

                //skeleton.SetBonePoseRotation(first, Quaternion.FromEuler(new Vector3(rotationDiff,0,0)) * skeleton.GetBonePoseRotation(first));

                skeleton.SetBoneGlobalPose(tip, targetTransform);

                //GD.Print($"{rotationDiff}, {rotationDiff2}, {rotationDiff3}, {rotationDiff4}");
            }
            private float GetCurveValue(Motion.FCurve fCurve, float time)
            {
                var keyCount = fCurve.KeyCount;
                var keys = MotionFile.FCurveKeys;
                
                for (var i = keyCount - 1; i >= 0; i--)
                {
                    var keyId = fCurve.KeyStartId + i;
                    
                    var key = keys[keyId];
                    var keyTime = MotionFile.KeyTimes[key.Time];
                    
                    if (keyTime > time) continue;
                    
                    var nextKey = i < keyCount - 1 ? keys[keyId + 1] : keys[fCurve.KeyStartId];
                    var nextKeyTime = MotionFile.KeyTimes[nextKey.Time];

                    var preType = fCurve.Pre;
                    var postType = fCurve.Post;
                    
                    var n = Mathf.Remap(time, keyTime, nextKeyTime, 0, 1);
                    
                    var value = key.Type switch
                    {
                        Motion.Interpolation.Nearest => MotionFile.KeyValues[key.ValueId],
                        Motion.Interpolation.Linear => Mathf.Lerp(MotionFile.KeyValues[key.ValueId], MotionFile.KeyValues[nextKey.ValueId], n),
                        Motion.Interpolation.Hermite or Motion.Interpolation.Hermite3 or Motion.Interpolation.Hermite4 => CubicHermite(n, MotionFile.KeyValues[key.ValueId],
                            MotionFile.KeyValues[nextKey.ValueId], MotionFile.KeyTangents[key.LeftTangentId], MotionFile.KeyTangents[key.RightTangentId]),
                        _ => 0,
                    };

                    return value;
                }
                return 0;
            }
            private void HandleCurve(Motion.FCurve fCurve, bool ik = false)
            {
                var bone = ik ? fCurve.JointId + BoneCount : fCurve.JointId;
                var channel = fCurve.ChannelValue;
                
                Vector3[] array;
                if (channel.IsPositionChannel()) array = Positions;
                else if (channel.IsRotationChannel()) array = Rotations;
                else array = Scales;

                var value = GetCurveValue(fCurve, Time);
                
                if (channel.IsXChannel()) array[bone].X = value;
                else if (channel.IsYChannel()) array[bone].Y = value;
                else array[bone].Z = value;
            }
            
            //GD.Print($"{(Motion.ExpressionType)node.Type}, {node.Element}, {node.IsGlobal}, {node.CAR}, {node.CDR}, {node.Value}");
            //GD.Print($"{boneCount}, {motion.InterpolatedMotionHeader.TotalBoneCount}");

            private float Expression(List<Motion.ExpressionNode> expressionNodes, int index)
            {
                if (index < 0 || index >= expressionNodes.Count) return 0;
                var node = expressionNodes[index];
                switch ((Motion.ExpressionType)node.Type)
                {
                    case Motion.ExpressionType.FUNC_SIN: return Mathf.Sin(Mathf.DegToRad(Expression(expressionNodes, node.CAR)));
                    case Motion.ExpressionType.FUNC_COS: return Mathf.Cos(Mathf.DegToRad(Expression(expressionNodes, node.CAR)));
                    case Motion.ExpressionType.FUNC_TAN: return Mathf.Tan(Mathf.DegToRad(Expression(expressionNodes, node.CAR)));
                    case Motion.ExpressionType.FUNC_ASIN:
                    {
                        var val = Expression(expressionNodes, node.CAR);
                        return val switch
                        {
                            >= 1 => 90,
                            <= -1 => -90,
                            _ => Mathf.RadToDeg(Mathf.Asin(val)),
                        };
                    }
                    case Motion.ExpressionType.FUNC_ACOS:
                    {
                        var val = Expression(expressionNodes, node.CAR);
                        return val switch
                        {
                            >= 1 => 0,
                            <= -1 => 180,
                            _ => Mathf.RadToDeg(Mathf.Acos(val)),
                        };
                    }
                    case Motion.ExpressionType.FUNC_ATAN: return Mathf.RadToDeg(Mathf.Atan(Expression(expressionNodes, node.CAR)));
                    case Motion.ExpressionType.FUNC_LOG: return Mathf.Log(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_EXP: return Mathf.Exp(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_ABS: return Mathf.Abs(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_POW: return Mathf.Pow(Expression(expressionNodes, node.CAR), Expression(expressionNodes, node.CDR));
                    case Motion.ExpressionType.FUNC_SQRT: return Mathf.Sqrt(Mathf.Abs(Expression(expressionNodes, node.CAR))); //according to debug symbols, we abs before doing sqrt
                    case Motion.ExpressionType.FUNC_MIN: return Mathf.Min(Expression(expressionNodes, node.CAR), Expression(expressionNodes, node.CDR));
                    case Motion.ExpressionType.FUNC_MAX: return Mathf.Max(Expression(expressionNodes, node.CAR), Expression(expressionNodes, node.CDR));
                    //case Motion.ExpressionType.FUNC_AV: return (Expression(expressionNodes, node.CAR) + Expression(expressionNodes, node.CDR)) * 0.5f;
                    case Motion.ExpressionType.FUNC_AV:
                    {
                        var a = node.CAR;
                        var b = node.CDR;
                        if (a >= 0 && (Motion.ExpressionType)expressionNodes[a].Type == Motion.ExpressionType.LIST && b < 0)
                        {
                            var list = ListExpression(expressionNodes, a);
                            return list.Sum() / list.Count;
                        }
                        break;
                    }
                    case Motion.ExpressionType.FUNC_COND:
                    {
                        var a = node.CAR;
                        var b = node.CDR;
                        if (a >= 0 && (Motion.ExpressionType)expressionNodes[a].Type == Motion.ExpressionType.LIST && b < 0)
                        {
                            var list = ListExpression(expressionNodes, a);
                            if (list.Count == 3) return list[0] >= 0 ? list[1] : list[2];
                            GD.Print($"List count 3 expected, actual: {list.Count}");
                        }
                        break;
                    }
                    //case Motion.ExpressionType.FUNC_AT_FRAME:
                        //in softimage, at frame gets the value of the expression at the frame # (Frame Offset function in docs)
                        //this is uh
                        //literal time travel?
                        //i have no idea how to implement this send help
                        //break;
                    //case Motion.ExpressionType.FUNC_CTR_DIST:
                        //in softimage, center to center returns the distance between two objects (Center to Center function in docs)
                        //i presume here it would be distance between two bones, with A and B being
                        //the indices of the bones, but i don't have examples yet
                        //break;
                    case Motion.ExpressionType.FUNC_FMOD: return Expression(expressionNodes, node.CAR) % Expression(expressionNodes, node.CDR); //fmod returns the mod without casting
                    case Motion.ExpressionType.OP_PLUS: return Expression(expressionNodes, node.CAR) + Expression(expressionNodes, node.CDR);
                    case Motion.ExpressionType.OP_MINUS: return Expression(expressionNodes, node.CAR) - Expression(expressionNodes, node.CDR);
                    case Motion.ExpressionType.OP_MUL: return Expression(expressionNodes, node.CAR) * Expression(expressionNodes, node.CDR);
                    case Motion.ExpressionType.OP_DIV: return Expression(expressionNodes, node.CAR) / Expression(expressionNodes, node.CDR); //according to debug symbols, throw on divide by 0
                    case Motion.ExpressionType.OP_MOD: return Mathf.RoundToInt(Expression(expressionNodes, node.CAR)) % Mathf.RoundToInt(Expression(expressionNodes, node.CDR)); //mod rounds before modulo, in softimage this is described as a "cast to the nearest int", TODO is this a floor, round, or ceil?
                    case Motion.ExpressionType.OP_EQ: return Expression(expressionNodes, node.CAR) == Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_GT: return Expression(expressionNodes, node.CAR) > Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_GE: return Expression(expressionNodes, node.CAR) >= Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_LT: return Expression(expressionNodes, node.CAR) < Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_LE: return Expression(expressionNodes, node.CAR) <= Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_AND: return Expression(expressionNodes, node.CAR) >= 1 && Expression(expressionNodes, node.CDR) >= 1 ? 1 : 0;
                    case Motion.ExpressionType.OP_OR: return Expression(expressionNodes, node.CAR) >= 1 || Expression(expressionNodes, node.CDR) >= 1 ? 1 : 0;
                    case Motion.ExpressionType.VARIABLE_FC: return Time; //im a little bit stupid, "frame current"
                    case Motion.ExpressionType.CONSTANT_NUM: return node.Value;
                    //TODO: are these in local or global space?
                    /*
                    case Motion.ExpressionType.FCURVE_ETRNX: return Positions[node.Element].X;
                    case Motion.ExpressionType.FCURVE_ETRNY: return Positions[node.Element].Y;
                    case Motion.ExpressionType.FCURVE_ETRNZ: return Positions[node.Element].Z;
                    case Motion.ExpressionType.FCURVE_ROTX: return Mathf.RadToDeg(Rotations[node.Element].X);
                    case Motion.ExpressionType.FCURVE_ROTY: return Mathf.RadToDeg(Rotations[node.Element].Y);
                    case Motion.ExpressionType.FCURVE_ROTZ: return Mathf.RadToDeg(Rotations[node.Element].Z);
                    case Motion.ExpressionType.FCURVE_SCALX: return Scales[node.Element].X;
                    case Motion.ExpressionType.FCURVE_SCALY: return Scales[node.Element].Y;
                    case Motion.ExpressionType.FCURVE_SCALZ: return Scales[node.Element].Z;
                    */
                    case Motion.ExpressionType.FCURVE_ETRNX: return GetGlobalTransform(node.Element).Origin.X;
                    case Motion.ExpressionType.FCURVE_ETRNY: return GetGlobalTransform(node.Element).Origin.Y;
                    case Motion.ExpressionType.FCURVE_ETRNZ: return GetGlobalTransform(node.Element).Origin.Z;
                    case Motion.ExpressionType.FCURVE_ROTX: return Mathf.RadToDeg(GetGlobalTransform(node.Element).Basis.GetEuler(EulerOrder.Zyx).X);
                    case Motion.ExpressionType.FCURVE_ROTY: return Mathf.RadToDeg(GetGlobalTransform(node.Element).Basis.GetEuler(EulerOrder.Zyx).Y);
                    case Motion.ExpressionType.FCURVE_ROTZ: return Mathf.RadToDeg(GetGlobalTransform(node.Element).Basis.GetEuler(EulerOrder.Zyx).Z);
                    case Motion.ExpressionType.FCURVE_SCALX: return GetGlobalTransform(node.Element).Basis.Scale.X;
                    case Motion.ExpressionType.FCURVE_SCALY: return GetGlobalTransform(node.Element).Basis.Scale.Y;
                    case Motion.ExpressionType.FCURVE_SCALZ: return GetGlobalTransform(node.Element).Basis.Scale.Z;
                    case Motion.ExpressionType.LIST:
                        //this is a special case, don't handle it here, if this case is met something went wrong
                        break;
                    case Motion.ExpressionType.ELEMENT_NAME: return 0; //this actually does literally nothing lmao
                    case Motion.ExpressionType.FUNC_AT_FRAME_ROT:
                        //i don't know what this is, doesn't seem to exist in softimage, is this related to at_frame?

                        //this is indeed related to at_frame, both do time travel
                    default:
                    {
                        GD.Print(
                            $"Unknown node: (Type: {(Motion.ExpressionType)node.Type}, Element Value: {node.Element}, Global: {node.IsGlobal}, First connection index: {node.CAR}, second connection index: {node.CDR}, float value: {node.Value})");
                        break;
                    }
                }
                return 0;
            }
            private List<float> ListExpression(List<Motion.ExpressionNode> expressionNodes, int index)
            {
                var node = expressionNodes[index];
                var a = node.CAR;
                var b = node.CDR;
                var list = new List<float>();
                if (a >= 0)
                {
                    var aNode = expressionNodes[a];
                    if ((Motion.ExpressionType)aNode.Type == Motion.ExpressionType.LIST) list.AddRange(ListExpression(expressionNodes, a));
                    else list.Add(Expression(expressionNodes, a));
                }
                if (b >= 0)
                {
                    var bNode = expressionNodes[b];
                    if ((Motion.ExpressionType)bNode.Type == Motion.ExpressionType.LIST) list.AddRange(ListExpression(expressionNodes, b));
                    else list.Add(Expression(expressionNodes, b));
                }
                return list;
            }
        }

        public static float Loop(Motion.InterpolatedMotion motion, float time)
        {
            var frameTime = time * motion.InterpolatedMotionHeader.FrameData.FramesPerSecond;
            var diff = 60 / motion.InterpolatedMotionHeader.FrameData.FramesPerSecond;
            
            var actualFrame = Mathf.Wrap(frameTime * diff, motion.InterpolatedMotionHeader.FrameData.FrameStart * diff, motion.InterpolatedMotionHeader.FrameData.FrameEnd * diff);

            return (actualFrame / diff) / motion.InterpolatedMotionHeader.FrameData.FramesPerSecond;
        }
        public static void ApplyInterpolatedMotion(Motion.InterpolatedMotion motion, KH2Skeleton3D skeleton, float time)
        {
            var frameTime = time * motion.InterpolatedMotionHeader.FrameData.FramesPerSecond;
            var diff = 60 / motion.InterpolatedMotionHeader.FrameData.FramesPerSecond;
            
            var actualFrame = Mathf.Wrap(frameTime * diff, motion.InterpolatedMotionHeader.FrameData.FrameStart * diff, motion.InterpolatedMotionHeader.FrameData.FrameEnd * diff);

            var sim = new AnimationSimulation(motion, skeleton, actualFrame);
        }
    }
}
