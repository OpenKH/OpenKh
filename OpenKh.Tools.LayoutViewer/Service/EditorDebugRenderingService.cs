using System.Drawing;
using System.Linq;
using OpenKh.Engine.Renders;

namespace OpenKh.Tools.LayoutViewer.Service
{
    public class EditorDebugRenderingService :
        IDebugLayoutRenderer
    {
        private bool[] _sequenceGroupVisibility;
        private bool[] _sequencePropertyVisibility;
        private Color[] _sequenceGroupBlendColor;
        private Color[] _sequencePropertyBlendColor;

        public EditorDebugRenderingService()
        {
            Reset();
        }

        public void Reset()
        {
            _sequenceGroupVisibility = InitializeArray(true, 100);
            _sequencePropertyVisibility = InitializeArray(true, 100);
            _sequenceGroupBlendColor = InitializeArray(Color.White, 100);
            _sequencePropertyBlendColor = InitializeArray(Color.White, 100);
        }

        public bool IsSequenceGroupVisible(int index) => _sequenceGroupVisibility[index];
        public void SetSequenceGroupVisible(int index, bool value) => _sequenceGroupVisibility[index] = value;

        public bool IsSequencePropertyVisible(int index) => _sequencePropertyVisibility[index];
        public void SetSequencePropertyVisible(int index, bool value) => _sequencePropertyVisibility[index] = value;

        public Color GetSequenceGroupBlendColor(int index) => _sequenceGroupBlendColor[index];
        public void SetSequenceGroupBlendColor(int index, Color value) => _sequenceGroupBlendColor[index] = value;

        public Color GetSequencePropertyBlendColor(int index) => _sequencePropertyBlendColor[index];
        public void GetSequencePropertyBlendColor(int index, Color value) => _sequencePropertyBlendColor[index] = value;

        private T[] InitializeArray<T>(T value, int count) =>
            Enumerable.Range(0, count).Select(x => value).ToArray();
    }
}
