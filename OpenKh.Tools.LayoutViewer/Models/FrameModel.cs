using OpenKh.Kh2;

namespace OpenKh.Tools.LayoutViewer.Models
{
    public class FrameModel
    {
        private readonly Sequence.Sprite frame;

        public FrameModel(Sequence.Sprite frame)
        {
            this.frame = frame;
        }

        public int Unknown00
        {
            get => frame.Unknown00;
            set => frame.Unknown00 = value;
        }

        public int Left
        {
            get => frame.Left;
            set => frame.Left = value;
        }

        public int Top
        {
            get => frame.Top;
            set => frame.Top = value;
        }

        public int Right
        {
            get => frame.Right;
            set => frame.Right = value;
        }

        public int Bottom
        {
            get => frame.Bottom;
            set => frame.Bottom = value;
        }

        public float Unknown10
        {
            get => frame.UTranslation;
            set => frame.UTranslation = value;
        }

        public float Unknown14
        {
            get => frame.VTranslation;
            set => frame.VTranslation = value;
        }

        public uint ColorLeft
        {
            get => frame.ColorLeft;
            set => frame.ColorLeft = value;
        }

        public uint ColorTop
        {
            get => frame.ColorTop;
            set => frame.ColorTop = value;
        }

        public uint ColorRight
        {
            get => frame.ColorRight;
            set => frame.ColorRight = value;
        }

        public uint ColorBottom
        {
            get => frame.ColorBottom;
            set => frame.ColorBottom = value;
        }
    }
}
