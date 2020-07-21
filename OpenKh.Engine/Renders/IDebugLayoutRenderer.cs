using System.Drawing;

namespace OpenKh.Engine.Renders
{
    public interface IDebugLayoutRenderer
    {
        bool IsSequenceGroupVisible(int index);
        bool IsSequencePropertyVisible(int index);
        Color GetSequenceGroupBlendColor(int index);
        Color GetSequencePropertyBlendColor(int index);
    }

    public interface IDebugSequenceRenderer
    {
        ColorF GetAnimationBlendColor(int index);
    }

    public class DefaultDebugLayoutRenderer : IDebugLayoutRenderer
    {
        private static readonly Color DefaultColor = Color.White;

        public bool IsSequenceGroupVisible(int index) => true;
        public bool IsSequencePropertyVisible(int index) => true;

        public Color GetSequenceGroupBlendColor(int index) => DefaultColor;
        public Color GetSequencePropertyBlendColor(int index) => DefaultColor;
    }

    public class DefaultDebugSequenceRenderer : IDebugSequenceRenderer
    {
        public ColorF GetAnimationBlendColor(int index) => ColorF.White;
    }
}
