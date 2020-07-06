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
            var rect = sequence.GetVisibilityRectangleForFrameGroup(animation.SpriteGroupIndex);

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
            sequence.SpriteGroups[frameGroupIndex].Aggregate(new Rectangle(), (rect, x) => rect.Union(x.GetVisibilityRectangle()));

        public static Rectangle GetVisibilityRectangle(
            this Sequence.SpritePart frameEx) => Rectangle.FromLTRB(
                frameEx.Left,
                frameEx.Top,
                frameEx.Right,
                frameEx.Bottom);

        public static Sequence.Animation Clone(this Sequence.Animation anim) => new Sequence.Animation
        {
            Flags = anim.Flags,
            SpriteGroupIndex = anim.SpriteGroupIndex,
            FrameStart = anim.FrameStart,
            FrameEnd = anim.FrameEnd,
            Xa0 = anim.Xa0,
            Xa1 = anim.Xa1,
            Ya0 = anim.Ya0,
            Ya1 = anim.Ya1,
            Xb0 = anim.Xb0,
            Xb1 = anim.Xb1,
            Yb0 = anim.Yb0,
            Yb1 = anim.Yb1,
            Unknown30 = anim.Unknown30,
            Unknown34 = anim.Unknown34,
            Unknown38 = anim.Unknown38,
            Unknown3c = anim.Unknown3c,
            RotationStart = anim.RotationStart,
            RotationEnd = anim.RotationEnd,
            ScaleStart = anim.ScaleStart,
            ScaleEnd = anim.ScaleEnd,
            ScaleXStart = anim.ScaleXStart,
            ScaleXEnd = anim.ScaleXEnd,
            ScaleYStart = anim.ScaleYStart,
            ScaleYEnd = anim.ScaleYEnd,
            Unknown60 = anim.Unknown60,
            Unknown64 = anim.Unknown64,
            Unknown68 = anim.Unknown68,
            Unknown6c = anim.Unknown6c,
            BounceXStart = anim.BounceXStart,
            BounceXEnd = anim.BounceXEnd,
            BounceYStart = anim.BounceYStart,
            BounceYEnd = anim.BounceYEnd,
            Unknwon80 = anim.Unknwon80,
            ColorBlend = anim.ColorBlend,
            ColorStart = anim.ColorStart,
            ColorEnd = anim.ColorEnd,
        };
    }
}
