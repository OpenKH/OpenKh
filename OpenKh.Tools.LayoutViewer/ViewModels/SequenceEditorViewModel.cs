using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Tools.Common.Rendering;
using OpenKh.Tools.LayoutViewer.Interfaces;
using OpenKh.Tools.LayoutViewer.Service;
using System.Collections.Generic;
using System.Diagnostics;
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

        private readonly IElementNames _elementNames;
        private readonly IEditorSettings _editorSettings;
        private int _frameIndex;
        private Sequence selectedSequence;
        private AnimationGroupListModel animationGroupList;
        private int selectedAnimationGroupIndex;


        public ISpriteDrawing Drawing { get; }
        public EditorDebugRenderingService EditorDebugRenderingService { get; }
        public System.Windows.Media.Color Background => _editorSettings.EditorBackground;

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

        public int FrameIndex
        {
            get => _frameIndex;
            set
            {
                _frameIndex = value;
                OnPropertyChanged();
            }
        }

        public int MaxFramesCount => SelectedAnimationGroupIndex >= 0 ?
            SelectedSequence.GetFrameLengthFromAnimationGroup(SelectedAnimationGroupIndex) : 0;

        public GenericListModel<SpriteViewModel> Sprites { get; }

        public SequenceEditorViewModel(
            Sequence sequence,
            Imgd texture,
            IElementNames elementNames,
            IEditorSettings editorSettings,
            EditorDebugRenderingService editorDebugRenderingService)
        {
            Drawing = new SpriteDrawingDirect3D();

            SelectedSequence = sequence;
            SelectedImage = texture;
            _elementNames = elementNames;
            _editorSettings = editorSettings;
            EditorDebugRenderingService = editorDebugRenderingService;

            Sprites = new GenericListModel<SpriteViewModel>(SelectedSequence.Frames.Select(x => new SpriteViewModel(x, texture)));

        }
    }
}
