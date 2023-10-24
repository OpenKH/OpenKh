using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class FormatListItemUsecase
    {
        public string FormatConstraint(Constraint it)
        {
            return $"{(Motion.ConstraintType)it.Type}, {it.SourceJointId}, {it.ConstrainedJointId}";
        }

        public string FormatConstraintActivation(ConstraintActivation it)
        {
            return $"{it.Time}, {it.Active}";
        }

        public string FormatLimiter(Limiter it)
        {
            return $"({it.MinX}, {it.MinY}, {it.MinZ}), ({it.MaxX}, {it.MaxY}, {it.MaxZ})";
        }

        public string FormatExpression(Expression it, string[] targetChannels)
        {
            return $"TargetId {it.TargetId} ({targetChannels.GetAtOrNull(it.TargetChannel) ?? it.TargetChannel.ToString()}) Node {it.NodeId}";
        }

        public string FormatExpressionNode(ExpressionNode it)
        {
            return $"{(Motion.ExpressionType)it.Type}, {it.Value}, {it.CAR}, {it.CDR}, {it.Element}, {(it.IsGlobal ? "Global" : "Local")}";
        }

        public string FormatJoint(Joint it)
        {
            return $"{it.JointId} {it.IK} {(it.ExtEffector ? "Ext" : "")} {(it.CalcMatrix2Rot ? "M2R" : "")} {(it.Calculated ? "Calc" : "")} {(it.Fixed ? "Fix" : "")} {(it.Rotation ? "Rot" : "")} {(it.Trans ? "Tra" : "")}";
        }

        public string FormatFCurve(FCurve it)
        {
            return $"{it.JointId}, {it.ChannelValue}, {it.Pre}, {it.Post}, {it.KeyStartId}, {it.KeyCount}";
        }

        public string FormatKey(Key it)
        {
            return $"{it.Time}, {it.Type}, {it.LeftTangentId}, {it.RightTangentId}";
        }

        public string FormatTime(float it)
        {
            return $"{it}";
        }

        public string FormatValue(float it)
        {
            return $"{it}";
        }

        public string FormatTangent(float it)
        {
            return $"{it}";
        }
    }
}
