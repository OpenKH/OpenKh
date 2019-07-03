using OpenKh.Kh2;

namespace OpenKh.Tools.LayoutViewer.Models
{
    public class AnimationGroupModel
    {
        private readonly Sequence sequence;
        private readonly int index;

        public AnimationGroupModel(Sequence sequence, int index)
        {
            this.sequence = sequence;
            this.index = index;
        }

        public Sequence.AnimationGroup AnimationGroup => sequence.AnimationGroups[index];

        public string Name => $"Animation group {index}";

        // TODO fields

        public override string ToString() => Name;
    }
}
