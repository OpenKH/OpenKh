using OpenKh.Common;
using System;
using System.Drawing;
using System.Linq;

namespace OpenKh.Kh2.Extensions
{
    public static class SequenceExtensions
    {
        public static Rectangle GetVisibilityRectangleFromAnimationGroup(
            this Sequence sequence, Sequence.AnimationGroup animGroup) =>
            animGroup.Animations.Aggregate(new Rectangle(), (rect, x) => rect.Union(sequence.GetVisibilityRectangleFromAnimation(x)));

        public static int GetFrameLength(this Sequence.AnimationGroup animGroup) =>
            animGroup.Animations.Aggregate(0, (length, anim) => Math.Max(length, anim.FrameEnd));

        public static Rectangle GetVisibilityRectangleFromAnimation(
            this Sequence sequence, Sequence.Animation animation)
        {
            var rect = sequence.GetVisibilityRectangleForFrameGroup(animation.FrameGroupIndex);

            var minXPos = animation.Xa0;
            int maxXPos = animation.Xa1;
            var minYPos = animation.Ya0;
            int maxYPos = animation.Ya1;
            var minXScale = animation.ScaleStart * animation.ScaleXStart;
            var maxXScale = animation.ScaleEnd * animation.ScaleXEnd;
            var minYScale = animation.ScaleStart * animation.ScaleYStart;
            var maxYScale = animation.ScaleEnd * animation.ScaleYEnd;

            var minRect = rect
                .Multiply(minXScale, minYScale)
                .Traslate(minXPos, minYPos);

            var maxRect = rect
                .Multiply(maxXScale, maxYScale)
                .Traslate(maxXPos, maxYPos);

            return minRect.Union(maxRect);
        }

        public static Rectangle GetVisibilityRectangleForFrameGroup(this Sequence sequence, int frameGroupIndex) =>
            sequence.FrameGroups[frameGroupIndex].Aggregate(new Rectangle(), (rect, x) => rect.Union(x.GetVisibilityRectangle()));

        public static Rectangle GetVisibilityRectangle(
            this Sequence.FrameEx frameEx) => Rectangle.FromLTRB(
                frameEx.Left,
                frameEx.Top,
                frameEx.Right,
                frameEx.Bottom);
    }
}
