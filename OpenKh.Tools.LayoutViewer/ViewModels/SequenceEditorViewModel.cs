using OpenKh.Kh2;
using OpenKh.Tools.LayoutViewer.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Xe.Drawing;
using Xe.Tools;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class SequenceEditorViewModel : BaseNotifyPropertyChanged
    {
        private class AnimationGroupEntryModel
        {
            public AnimationGroupEntryModel(int index, Sequence.AnimationGroup animationGroup)
            {
                Index = index;
                AnimationGroup = animationGroup;
            }

            public int Index { get; }
            public Sequence.AnimationGroup AnimationGroup { get; }

            public override string ToString() => $"Animation group {Index}";
        }

        private class AnimationGroupListModel : GenericListModel<AnimationGroupEntryModel>
        {
            public AnimationGroupListModel(Sequence sequence) :
                this(sequence.AnimationGroups.Select((x, i) => new AnimationGroupEntryModel(i, x)))
            {
            }

            public AnimationGroupListModel(IEnumerable<AnimationGroupEntryModel> list) :
                base(list)
            {
            }

            protected override AnimationGroupEntryModel OnNewItem()
            {
                throw new System.NotImplementedException();
            }
        }

        private Sequence selectedSequence;
        private AnimationGroupListModel animationGroupList;
        private int selectedAnimationGroupIndex;

        public IDrawing Drawing { get; }
        public ISequencePlayer SequencePlayer { get; }

        public object AnimationGroupList
        {
            get => animationGroupList;
            private set
            {
                animationGroupList = value as AnimationGroupListModel;
                OnPropertyChanged();
            }
        }

        public Sequence SelectedSequence
        {
            get => selectedSequence;
            set
            {
                selectedSequence = value;
                AnimationGroupList = new AnimationGroupListModel(value);
            }
        }

        public Imgd SelectedImage { get; set; }
        public int SelectedAnimationGroupIndex
        {
            get => selectedAnimationGroupIndex;
            set
            {
                selectedAnimationGroupIndex = value;
                OnPropertyChanged();
            }
        }

        public SequenceEditorViewModel(IDrawing drawing, ISequencePlayer sequencePlayer)
        {
            Drawing = drawing;
            SequencePlayer = sequencePlayer;
        }
    }
}
