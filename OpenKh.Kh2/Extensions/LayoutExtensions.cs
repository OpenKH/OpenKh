using OpenKh.Common;
using System;
using System.Drawing;

namespace OpenKh.Kh2.Extensions
{
    public static class LayoutExtensions
    {
        public static T AggregateSequenceGroup<T>(
            this Layout layout,
            int sequenceGroupIndex,
            Func<T, int, T> aggregator,
            T initialValue = default)
        {
            var value = initialValue;
            var animGroup = layout.SequenceGroups[sequenceGroupIndex];

            for (var i = 0; i < animGroup.L1Count; i++)
                value = aggregator(value, animGroup.L1Index + i);

            return value;
        }

        public static Rectangle GetVisibilityRectangleFromSequenceGroup(
            this Layout layout, int sequenceGroupIndex) =>
            layout.AggregateSequenceGroup<Rectangle>(sequenceGroupIndex, (x, i) =>
                x.Union(layout.GetVisibilityRectangleFromSequenceProperty(i)));

        public static int GetFrameLengthFromSequenceGroup(
            this Layout layout, int sequenceGroupIndex) =>
            layout.AggregateSequenceGroup<int>(sequenceGroupIndex, (x, i) =>
                Math.Max(x, layout.GetFrameLengthFromSequenceProperty(i)));

        public static Rectangle GetVisibilityRectangleFromSequenceProperty(
            this Layout layout, int sequencePropertyIndex)
        {
            var sequenceProperty = layout.SequenceProperties[sequencePropertyIndex];
            var sequence = layout.SequenceItems[sequenceProperty.SequenceIndex];
            var animGroup = sequence.AnimationGroups[sequenceProperty.AnimationGroup];

            return sequence.GetVisibilityRectangleFromAnimationGroup(animGroup)
                .Traslate(sequenceProperty.PositionX, sequenceProperty.PositionY);
        }

        public static int GetFrameLengthFromSequenceProperty(
            this Layout layout, int sequencePropertyIndex)
        {
            var sequenceProperty = layout.SequenceProperties[sequencePropertyIndex];
            var sequence = layout.SequenceItems[sequenceProperty.SequenceIndex];
            var animGroup = sequence.AnimationGroups[sequenceProperty.AnimationGroup];

            return animGroup.GetFrameLength() + sequenceProperty.ShowAtFrame;
        }
    }
}
