using OpenKh.Common;
using System;
using System.Collections.Generic;
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

            var minXPos = animation.TranslateXStart;
            int maxXPos = animation.TranslateXEnd;
            var minYPos = animation.TranslateYStart;
            int maxYPos = animation.TranslateYEnd;
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
            sequence.SpriteGroups[frameGroupIndex].GetVisibilityRectangleForFrameGroup();

        public static Rectangle GetVisibilityRectangleForFrameGroup(this List<Sequence.SpritePart> spriteGroup) =>
            spriteGroup.Aggregate(new Rectangle(), (rect, x) => rect.Union(x.GetVisibilityRectangle()));

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
            TranslateXStart = anim.TranslateXStart,
            TranslateXEnd = anim.TranslateXEnd,
            TranslateYStart = anim.TranslateYStart,
            TranslateYEnd = anim.TranslateYEnd,
            PivotXStart = anim.PivotXStart,
            PivotXEnd = anim.PivotXEnd,
            PivotYStart = anim.PivotYStart,
            PivotYEnd = anim.PivotYEnd,
            RotationXStart = anim.RotationXStart,
            RotationXEnd = anim.RotationXEnd,
            RotationYStart = anim.RotationYStart,
            RotationYEnd = anim.RotationYEnd,
            RotationZStart = anim.RotationZStart,
            RotationZEnd = anim.RotationZEnd,
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
            BounceXSpeed = anim.BounceXSpeed,
            BounceYSpeed = anim.BounceYSpeed,
            ColorBlend = anim.ColorBlend,
            ColorStart = anim.ColorStart,
            ColorEnd = anim.ColorEnd,
        };
    }
}
