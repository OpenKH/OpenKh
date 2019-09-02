using OpenKh.Common;
using System;
using System.Drawing;

namespace OpenKh.Kh2.Extensions
{
    public static class SequenceExtensions
    {
        public static T AggregateAnimationGroup<T>(
            this Sequence sequence,
            int animationGroupIndex,
            Func<T, int, T> aggregator,
            T initialValue = default)
        {
            var value = initialValue;
            var animGroup = sequence.AnimationGroups[animationGroupIndex];


            for (var i = 0; i < animGroup.Count; i++)
                value = aggregator(value, animGroup.AnimationIndex + i);

            return value;
        }

        public static T AggregateFrameGroup<T>(
            this Sequence sequence,
            int frameGroupIndex,
            Func<T, int, T> aggregator,
            T initialValue = default)
        {
            T value = initialValue;
            var frameGroup = sequence.FrameGroups[frameGroupIndex];

            for (var i = 0; i < frameGroup.Count; i++)
                value = aggregator(value, frameGroup.Start + i);

            return value;
        }

        public static Rectangle GetVisibilityRectangleFromAnimationGroup(
            this Sequence sequence, int animationGroupIndex) =>
            sequence.AggregateAnimationGroup<Rectangle>(animationGroupIndex, (x, i) =>
                x.Union(sequence.GetVisibilityRectangleFromAnimation(i)));

        public static int GetFrameLengthFromAnimationGroup(
            this Sequence sequence, int animationGroupIndex) =>
            sequence.AggregateAnimationGroup<int>(animationGroupIndex, (x, i) =>
                Math.Max(x, sequence.GetFrameLengthFromAnimation(i)));

        public static Rectangle GetVisibilityRectangleFromAnimation(
            this Sequence sequence, int animationIndex)
        {
            var animation = sequence.Animations[animationIndex];
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

        public static int GetFrameLengthFromAnimation(
            this Sequence sequence, int animationIndex) =>
            sequence.Animations[animationIndex].FrameEnd;

        public static Rectangle GetVisibilityRectangleForFrameGroup(
            this Sequence sequence, int frameGroupIndex) =>
            sequence.AggregateFrameGroup<Rectangle>(frameGroupIndex, (x, i) =>
                x.Union(sequence.FramesEx[i].GetVisibilityRectangle()));


        public static Rectangle GetVisibilityRectangle(
            this Sequence.FrameEx frameEx) => Rectangle.FromLTRB(
                frameEx.Left,
                frameEx.Top,
                frameEx.Right,
                frameEx.Bottom);
    }
}
