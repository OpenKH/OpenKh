using OpenKh.Kh2;

namespace OpenKh.Tools.LayoutViewer.Models
{
    public class SequenceGroupModel
    {
        private readonly Layout layout;
        private readonly int index;

        public SequenceGroupModel(Layout layout, int index)
        {
            this.layout = layout;
            this.index = index;
        }

        public Layout.SequenceGroup SequenceGroup => layout.SequenceGroups[index];

        public string Name => $"Sequence {index}";

        public short L1Index
        {
            get => SequenceGroup.L1Index;
            set => SequenceGroup.L1Index = value;
        }

        public short L1Count
        {
            get => SequenceGroup.L1Count;
            set => SequenceGroup.L1Count = value;
        }

        public int Unknown04
        {
            get => SequenceGroup.Unknown04;
            set => SequenceGroup.Unknown04 = value;
        }

        public int Unknown08
        {
            get => SequenceGroup.Unknown08;
            set => SequenceGroup.Unknown08 = value;
        }

        public int Unknown0c
        {
            get => SequenceGroup.Unknown0c;
            set => SequenceGroup.Unknown0c = value;
        }

        public int Unknown10
        {
            get => SequenceGroup.Unknown10;
            set => SequenceGroup.Unknown10 = value;
        }

        public override string ToString() => Name;
    }
}
