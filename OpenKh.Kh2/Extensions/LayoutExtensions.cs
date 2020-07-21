using OpenKh.Common;
using System;
using System.Drawing;
using System.Linq;

namespace OpenKh.Kh2.Extensions
{
    public static class LayoutExtensions
    {
        public static Rectangle GetVisibilityRectangleFromSequenceGroup(
            this Layout layout, int sequenceGroupIndex)
        {
            var sgi = layout.SequenceGroups[sequenceGroupIndex];
            return sgi.Sequences.Aggregate(new Rectangle(),
                (rect, x) => rect.Union(layout.GetVisibilityRectangleFromSequenceProperty(x)));
        }

        public static int GetFrameLengthFromSequenceGroup(
            this Layout layout, int sequenceGroupIndex)
        {
            var sgi = layout.SequenceGroups[sequenceGroupIndex];
            return sgi.Sequences.Aggregate(0,
                (len, x) => Math.Max(len, layout.GetFrameLengthFromSequenceProperty(x)));
        }

        public static Rectangle GetVisibilityRectangleFromSequenceProperty(
            this Layout layout, Layout.SequenceProperty sequenceProperty)
        {
            var sequence = layout.SequenceItems[sequenceProperty.SequenceIndex];
            var animGroup = sequence.AnimationGroups[sequenceProperty.AnimationGroup];

            return sequence.GetVisibilityRectangleFromAnimationGroup(animGroup)
                .Traslate(sequenceProperty.PositionX, sequenceProperty.PositionY);
        }

        public static int GetFrameLengthFromSequenceProperty(
            this Layout layout, Layout.SequenceProperty sequenceProperty)
        {
            var sequence = layout.SequenceItems[sequenceProperty.SequenceIndex];
            var animGroup = sequence.AnimationGroups[sequenceProperty.AnimationGroup];

            return animGroup.GetFrameLength() + sequenceProperty.ShowAtFrame;
        }
    }
}
