using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases
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
            return $"TargetId {it.TargetId} {targetChannels.GetAtOrNull(it.TargetChannel) ?? it.TargetChannel.ToString()} Node {it.NodeId}";
        }

        public string FormatExpressionNode(ExpressionNode it)
        {
            return $"{(Motion.ExpressionType)it.Type}, {it.Value}, {it.CAR}, {it.CDR}, {it.Element}, {(it.IsGlobal ? "Global" : "Local")}";
        }
    }
}
