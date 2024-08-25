using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;
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
        
        public static Quaternion AnimationRotation(Vector3 rotation)
        {
            var rotationMatrixX = Matrix4x4.CreateRotationX(rotation.X);
            var rotationMatrixY = Matrix4x4.CreateRotationY(rotation.Y);
            var rotationMatrixZ = Matrix4x4.CreateRotationZ(rotation.Z);
            var rotationMatrix = rotationMatrixZ * rotationMatrixY * rotationMatrixX;
            Matrix4x4.Decompose(rotationMatrix, out _, out var rot, out _);

            return new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
        }

        private class AnimationSimulation
        {
            private Vector3[] Positions;
            private Vector3[] Rotations;
            private Vector3[] Scales;
            private int[] Parent;
            //private int[] Mapping;
            private int BoneCount;
            private int IKCount;
            private int TotalCount;
            private Motion.InterpolatedMotion MotionFile;
            private float Time;

            private Transform3D GetTransform(int index) => new(new Basis(AnimationRotation(Rotations[index])).Scaled(Scales[index]), Positions[index]);
            private void SetTransform(int index, Transform3D value)
            {
                Positions[index] = value.Origin;
                Rotations[index] = value.Basis.GetRotationQuaternion().GetEuler(EulerOrder.Zyx); //TODO: euler order
                Scales[index] = value.Basis.Scale;
            }
            private Transform3D GetGlobalTransform(int index)
            {
                if (index < 0) return Transform3D.Identity;
                var currentTransform = GetTransform(index);
                var parent = Parent[index];
                if (parent < 0) return currentTransform;
                return GetGlobalTransform(parent) * currentTransform;
            }
            private void SetGlobalTransform(int index, Transform3D value)
            {
                var parent = Parent[index];
                if (parent < 0) SetTransform(index, value);
                var parentTransform = GetGlobalTransform(parent);
                var newTransform = parentTransform.Inverse() * value;
                SetTransform(index, newTransform);
            }
            void SetBoneValue(Motion.Channel channel, int index, float value)
            {
                Vector3[] array;
                if (channel.IsPositionChannel()) array = Positions;
                else if (channel.IsRotationChannel()) array = Rotations;
                else array = Scales;
                
                if (channel.IsXChannel()) array[index].X = value;
                else if (channel.IsYChannel()) array[index].Y = value;
                else array[index].Z = value;
            }

            public AnimationSimulation(Motion.InterpolatedMotion motion, Skeleton3D skeleton, float time)
            {
                MotionFile = motion;
                Time = time;
                
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
                //Mapping = motion.Joints.Select(i => (int)i.JointId).ToArray();

                /*
                for (var index = 0; index < motion.Joints.Count; index++)
                {
                    var joint = motion.Joints[index];
                    var jointIndex = joint.JointId;
                    if (jointIndex >= BoneCount) continue;
                    
                    var pose = skeleton.GetBonePose(jointIndex);
                    Positions[index] = pose.Origin / ImportHelpers.KH2PositionScale;
                    Rotations[index] = pose.Basis.GetRotationQuaternion().GetEuler();
                    Scales[index] = pose.Basis.Scale;
                    Parent[index] = skeleton.GetBoneParent(jointIndex);
                }
                */
                
                for (var i = 0; i < motion.InterpolatedMotionHeader.BoneCount; i++)
                {
                    //var joint = motion.Joints[i];
                    if (i >= skeletonBoneCount) break;
                    var pose = skeleton.GetBonePose(i);
                    Positions[i] = pose.Origin / ImportHelpers.KH2PositionScale;
                    Rotations[i] = pose.Basis.GetRotationQuaternion().GetEuler();
                    Scales[i] = pose.Basis.Scale;
                    Parent[i] = skeleton.GetBoneParent(i);
                    //GD.Print($"{i}, {joint.JointId}");
                }

                for (var i = 0; i < motion.IKHelpers.Count; i++)
                {
                    var ikHelper = motion.IKHelpers[i];
                    //GD.Print(ikHelper.Index);
                    var index = /*BoneCount + i*/ ikHelper.Index;
                    //GD.Print($"Array Index: {i}, Bone Index: {ikHelper.Index}");
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
                foreach (var expression in motion.Expressions) SetBoneValue((Motion.Channel)expression.TargetChannel, expression.TargetId, Expression(motion.ExpressionNodes, expression.NodeId));

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

                            var sourceRelative = (currentParentTransform.Inverse() * sourceTransform).Origin;

                            if (sourceRelative.Normalized().Abs().IsEqualApprox(Vector3.Up))
                            {
                                //TODO: point upward or downward
                                GD.Print("IMPLEMENT: point upward");
                            }
                            else
                            {
                                var offset = type == 3 ? Quaternion.FromEuler(new Vector3(0, Mathf.Pi / 2, 0)) : Quaternion.FromEuler(new Vector3(Mathf.Pi / 2, 0, 0));
                                var newTransform = GetTransform(target).LookingAt(sourceRelative) * new Transform3D(new Basis(offset), Vector3.Zero); //TODO: is this correct?
                                SetTransform(target, newTransform);
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
                        case 13:
                            break;
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
                    DebugDraw3D.DrawPosition(GetGlobalTransform(i).ScaledLocal(Vector3.One * 0.1f).Scaled(Vector3.One * ImportHelpers.KH2PositionScale), i < motion.InterpolatedMotionHeader.BoneCount ? Colors.Aqua : Colors.DarkRed);

                    var index = /*Mapping[i]*/ i;
                    if (index < BoneCount)
                    {
                        var transform = GetTransform(index);
                        transform = transform with
                        {
                            Origin = transform.Origin * ImportHelpers.KH2PositionScale,
                        };
                        skeleton.SetBonePose(index, transform);
                    }
                }
            }
            private void HandleCurve(Motion.FCurve fCurve, bool ik = false)
            {
                var bone = ik ? fCurve.JointId + BoneCount : fCurve.JointId;
                var channel = fCurve.ChannelValue;
                Vector3[] array;
                
                if (channel.IsPositionChannel()) array = Positions;
                else if (channel.IsRotationChannel()) array = Rotations;
                else array = Scales;
                
                var keyCount = fCurve.KeyCount;
                
                var keys = Enumerable.Range(fCurve.KeyStartId, keyCount).Select(i => MotionFile.FCurveKeys[i]).ToArray();

                var lastTime = -1f;
                
                for (var i = 0; i < keyCount; i++)
                {
                    var key = keys[i];
                    var keyTime = MotionFile.KeyTimes[key.Time];
                    if (keyTime < Time || keyTime < lastTime) continue;
                    lastTime = keyTime;
                    var nextKey = i < keyCount - 1 ? keys[i + 1] : keys[0];
                    var nextKeyTime = MotionFile.KeyTimes[nextKey.Time];

                    var interp = key.Type;
                    
                    var timeDiff = keyTime - nextKeyTime;
                    var n = (Time - keyTime) / timeDiff;
                    var value = interp switch
                    {
                        Motion.Interpolation.Nearest => MotionFile.KeyValues[key.ValueId],
                        Motion.Interpolation.Linear => Mathf.Lerp(MotionFile.KeyValues[key.ValueId], MotionFile.KeyValues[nextKey.ValueId], n),
                        Motion.Interpolation.Hermite or Motion.Interpolation.Hermite3 or Motion.Interpolation.Hermite4 => CubicHermite(n, MotionFile.KeyValues[key.ValueId],
                            MotionFile.KeyValues[nextKey.ValueId], MotionFile.KeyTangents[key.LeftTangentId], MotionFile.KeyTangents[key.RightTangentId]),
                        _ => 0,
                    };
                    
                    if (channel.IsXChannel()) array[bone].X = value;
                    else if (channel.IsYChannel()) array[bone].Y = value;
                    else array[bone].Z = value;
                }
            }
            
            //GD.Print($"{(Motion.ExpressionType)node.Type}, {node.Element}, {node.IsGlobal}, {node.CAR}, {node.CDR}, {node.Value}");
            //GD.Print($"{boneCount}, {motion.InterpolatedMotionHeader.TotalBoneCount}");

            private float Expression(List<Motion.ExpressionNode> expressionNodes, int index)
            {
                if (index < 0 || index >= expressionNodes.Count) return 0;
                var node = expressionNodes[index];
                switch ((Motion.ExpressionType)node.Type)
                {
                    case Motion.ExpressionType.FUNC_SIN: return Mathf.Sin(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_COS: return Mathf.Cos(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_TAN: return Mathf.Tan(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_ASIN: return Mathf.Asin(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_ACOS: return Mathf.Acos(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_ATAN: return Mathf.Atan(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_LOG: return Mathf.Log(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_EXP: return Mathf.Exp(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_ABS: return Mathf.Abs(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_POW: return Mathf.Pow(Expression(expressionNodes, node.CAR), Expression(expressionNodes, node.CDR));
                    case Motion.ExpressionType.FUNC_SQRT: return Mathf.Sqrt(Expression(expressionNodes, node.CAR));
                    case Motion.ExpressionType.FUNC_MIN: return Mathf.Min(Expression(expressionNodes, node.CAR), Expression(expressionNodes, node.CDR));
                    case Motion.ExpressionType.FUNC_MAX: return Mathf.Max(Expression(expressionNodes, node.CAR), Expression(expressionNodes, node.CDR));
                    //case Motion.ExpressionType.FUNC_AV: return (Expression(expressionNodes, node.CAR) + Expression(expressionNodes, node.CDR)) * 0.5f;
                    case Motion.ExpressionType.FUNC_AV:
                    {
                        //TODO: this is a guess, i havent checked for examples
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
                    case Motion.ExpressionType.OP_DIV:
                    {
                        var a = Expression(expressionNodes, node.CAR);
                        var b = Expression(expressionNodes, node.CDR);
                        return b != 0 ? a / b : 0; //todo: what actually happens if you divide by zero in an expression?
                    }
                    case Motion.ExpressionType.OP_MOD: return Mathf.RoundToInt(Expression(expressionNodes, node.CAR)) % Mathf.RoundToInt(Expression(expressionNodes, node.CDR)); //mod rounds before modulo, in softimage this is described as a "cast to the nearest int", TODO is this a floor, round, or ceil?
                    case Motion.ExpressionType.OP_EQ: return Expression(expressionNodes, node.CAR) == Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_GT: return Expression(expressionNodes, node.CAR) > Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_GE: return Expression(expressionNodes, node.CAR) >= Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_LT: return Expression(expressionNodes, node.CAR) < Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_LE: return Expression(expressionNodes, node.CAR) <= Expression(expressionNodes, node.CDR) ? 1 : 0;
                    case Motion.ExpressionType.OP_AND: return Expression(expressionNodes, node.CAR) >= 1 && Expression(expressionNodes, node.CDR) >= 1 ? 1 : 0;
                    case Motion.ExpressionType.OP_OR: return Expression(expressionNodes, node.CAR) >= 1 || Expression(expressionNodes, node.CDR) >= 1 ? 1 : 0;
                    //case Motion.ExpressionType.VARIABLE_FC:
                        //is this maybe "foot collision"? otherwise i have no clue what this means
                        //break;
                    case Motion.ExpressionType.CONSTANT_NUM: return node.Value;
                    case Motion.ExpressionType.FCURVE_ETRNX: return Positions[node.Element].X; //TODO: fcurve? this just grabs the corresponding value, not sure if this is correct
                    case Motion.ExpressionType.FCURVE_ETRNY: return Positions[node.Element].Y;
                    case Motion.ExpressionType.FCURVE_ETRNZ: return Positions[node.Element].Z;
                    case Motion.ExpressionType.FCURVE_ROTX: return Rotations[node.Element].X;
                    case Motion.ExpressionType.FCURVE_ROTY: return Rotations[node.Element].Y;
                    case Motion.ExpressionType.FCURVE_ROTZ: return Rotations[node.Element].Z;
                    case Motion.ExpressionType.FCURVE_SCALX: return Scales[node.Element].X;
                    case Motion.ExpressionType.FCURVE_SCALY: return Scales[node.Element].Y;
                    case Motion.ExpressionType.FCURVE_SCALZ: return Scales[node.Element].Z;
                    case Motion.ExpressionType.LIST:
                        //this is a special case, don't handle it here, if this case is met something went wrong
                        break;
                    case Motion.ExpressionType.ELEMENT_NAME:
                        //TODO: uhhh?
                    case Motion.ExpressionType.FUNC_AT_FRAME_ROT:
                        //i don't know what this is, doesn't seem to exist in softimage, is this related to at_frame?
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
        
        public static void ApplyInterpolatedMotion(Motion.InterpolatedMotion motion, Skeleton3D skeleton, float time)
        {
            var frameTime = time * 60f;
            var actualFrame = Mathf.Wrap(frameTime, motion.InterpolatedMotionHeader.FrameCount * 2, motion.InterpolatedMotionHeader.FrameData.FrameEnd * 2);

            var sim = new AnimationSimulation(motion, skeleton, actualFrame);
        }
    }
}
