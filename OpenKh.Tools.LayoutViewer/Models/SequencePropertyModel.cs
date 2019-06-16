using OpenKh.Kh2;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.LayoutViewer.ViewModels;
using System;
using System.Linq;
using System.Windows.Media;
using Xe.Drawing;
using Xe.Tools;
using static OpenKh.Kh2.Layout;

namespace OpenKh.Tools.LayoutViewer.Models
{
    public class SequencePropertyModel : BaseNotifyPropertyChanged
    {
        public class SequenceModel
        {
            private readonly int index;
            private readonly Sequence sequence;

            public SequenceModel(int index, Sequence sequence)
            {
                this.index = index;
                this.sequence = sequence;
            }

            public override string ToString() => $"Sequence {index}";
        }

        public class AnimationGroupModel
        {
            private readonly int index;
            private readonly Sequence sequence;

            public AnimationGroupModel(int index, Sequence sequence)
            {
                this.index = index;
                this.sequence = sequence;
            }

            public override string ToString() => $"Animation group {index}";
        }

        private readonly Service.EditorDebugRenderingService editorDebugRenderingService;

        public SequencePropertyModel(int index, Layout layout, ViewModels.TexturesViewModel texturesViewModel, Service.EditorDebugRenderingService editorDebugRenderingService)
        {
            Index = index;
            Layout = layout;
            Textures = texturesViewModel;
            this.editorDebugRenderingService = editorDebugRenderingService;
        }

        public int Index { get; }
        public Layout Layout { get; }
        public SequenceProperty SequenceProperty => Layout.SequenceProperties[Index];

        public TexturesViewModel Textures { get; }
        public ImageSource TextureImage => Textures.Items[TextureIndex].Image;

        public MyGenericListModel<SequenceModel> SequenceItems =>
            new MyGenericListModel<SequenceModel>(Layout.SequenceItems.Select((x, i) => new SequenceModel(i, x)));

        public MyGenericListModel<AnimationGroupModel> AnimationGroupItems =>
            new MyGenericListModel<AnimationGroupModel>(Layout.SequenceItems[SequenceIndex].AnimationGroups.Select((x, i) =>
                new AnimationGroupModel(i, Layout.SequenceItems[SequenceIndex])));

        public int TextureIndex
        {
            get => SequenceProperty.TextureIndex;
            set
            {
                SequenceProperty.TextureIndex = value;
                OnPropertyChanged(nameof(SelectedImage));
            }
        }

        public int SequenceIndex
        {
            get => SequenceProperty.SequenceIndex;
            set
            {
                AnimationGroup = 0;
                SequenceProperty.SequenceIndex = value;
                OnPropertyChanged(nameof(SelectedSequence));
                OnPropertyChanged(nameof(AnimationGroupItems));
            }
        }

        public int AnimationGroup
        {
            get => SequenceProperty.AnimationGroup;
            set
            {
                SequenceProperty.AnimationGroup = Math.Max(0, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedAnimationGroupIndex));
            }
        }

        public int ShowAtFrame
        {
            get => SequenceProperty.ShowAtFrame;
            set => SequenceProperty.ShowAtFrame = value;
        }

        public int PositionX
        {
            get => SequenceProperty.PositionX;
            set => SequenceProperty.PositionX = value;
        }

        public int PositionY
        {
            get => SequenceProperty.PositionY;
            set => SequenceProperty.PositionY = value;
        }

        public bool IsVisible
        {
            get => editorDebugRenderingService.IsSequencePropertyVisible(Index);
            set => editorDebugRenderingService.SetSequencePropertyVisible(Index, value);
        }

        public IDrawing Drawing => new DrawingDirect3D();
        public int FrameIndex => 0;
        public Sequence SelectedSequence => Layout.SequenceItems[SequenceIndex];
        public Imgd SelectedImage => Textures.Items[TextureIndex].Texture;
        public int SelectedAnimationGroupIndex => AnimationGroup;

        public override string ToString() =>
            $"#{Index} Seq {SequenceIndex}:{AnimationGroup}, Frame {ShowAtFrame}, XY({PositionX},{PositionY})";
    }
}
