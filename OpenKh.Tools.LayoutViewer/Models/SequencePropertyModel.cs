using OpenKh.Kh2;
using Xe.Tools;
using static OpenKh.Kh2.Layout;

namespace OpenKh.Tools.LayoutViewer.Models
{
    public class SequencePropertyModel : BaseNotifyPropertyChanged
    {
        private readonly Service.EditorDebugRenderingService editorDebugRenderingService;

        public SequencePropertyModel(int index, Layout layout, Service.EditorDebugRenderingService editorDebugRenderingService)
        {
            Index = index;
            Layout = layout;
            this.editorDebugRenderingService = editorDebugRenderingService;
        }

        public int Index { get; }
        public Layout Layout { get; }
        public SequenceProperty SequenceProperty => Layout.SequenceProperties[Index];

        public int TextureIndex
        {
            get => SequenceProperty.TextureIndex;
            set => SequenceProperty.TextureIndex = value;
        }

        public int SequenceIndex
        {
            get => SequenceProperty.SequenceIndex;
            set => SequenceProperty.SequenceIndex = value;
        }

        public int AnimationGroup
        {
            get => SequenceProperty.AnimationGroup;
            set => SequenceProperty.AnimationGroup = value;
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

        public override string ToString() =>
            $"#{Index} Seq {SequenceIndex}:{AnimationGroup}, Frame {ShowAtFrame}, XY({PositionX},{PositionY})";
    }
}
