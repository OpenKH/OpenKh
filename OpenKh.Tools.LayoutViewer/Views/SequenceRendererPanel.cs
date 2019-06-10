using kh.tools.common.Controls;
using OpenKh.Engine;
using OpenKh.Engine.Renderers;
using OpenKh.Kh2;
using System.Drawing;
using System.Windows;
using Xe.Drawing;
using static kh.tools.common.DependencyPropertyUtils;

namespace OpenKh.Tools.LayoutViewer.Views
{
    public class SequenceRendererPanel : DrawPanel
    {
        public static readonly DependencyProperty SelectedSequenceProperty =
            GetDependencyProperty<SequenceRendererPanel, Sequence>(nameof(SelectedSequence), null, (o, x) => o.TrySetSequence(), x => true);

        public static readonly DependencyProperty SelectedImagesProperty =
            GetDependencyProperty<SequenceRendererPanel, Imgd>(nameof(SelectedImage), null, (o, x) => o.LoadImage(x), x => true);

        public static readonly DependencyProperty SelectedSequenceGroupIndexProperty =
            GetDependencyProperty<SequenceRendererPanel, int>(nameof(SelectedAnimationGroupIndex), 0, (o, x) => o.SelectSequenceGroup(x), x => x >= 0);

        public static readonly DependencyProperty FrameIndexProperty =
            GetDependencyProperty<SequenceRendererPanel, int>(nameof(FrameIndex), 0, (o, x) => o.SetFrameIndex(x), x => x >= 0);

        public int SelectedAnimationGroupIndex
        {
            get => (int)GetValue(SelectedSequenceGroupIndexProperty);
            set => SetValue(SelectedSequenceGroupIndexProperty, value);
        }

        public Sequence SelectedSequence
        {
            get => (Sequence)GetValue(SelectedSequenceProperty);
            set => SetValue(SelectedSequenceProperty, value);
        }

        public Imgd SelectedImage
        {
            get => (Imgd)GetValue(SelectedImagesProperty);
            set => SetValue(SelectedImagesProperty, value);
        }

        public int FrameIndex
        {
            get => (int)GetValue(FrameIndexProperty);
            set => SetValue(FrameIndexProperty, value);
        }

        private ISurface surface;
        private SequenceRenderer sequenceRenderer;

        protected override void OnDrawCreate()
        {
            base.OnDrawCreate();
        }

        protected override void OnDrawDestroy()
        {
            surface?.Dispose();
            base.OnDrawDestroy();
        }

        protected override void OnDrawBegin()
        {
            Drawing.Clear(Color.Magenta);
            sequenceRenderer?.Draw(SelectedAnimationGroupIndex, FrameIndex, 0, 0);
            FrameIndex++;
            Drawing.Flush();
            base.OnDrawBegin();
        }

        protected override void OnDrawEnd()
        {
            base.OnDrawEnd();
        }

        private void SelectSequenceGroup(int index)
        {
            FrameIndex = 0;
        }

        private void SetFrameIndex(int frameIndex)
        {
        }

        private void TrySetSequence()
        {
            if (SelectedSequence != null && surface != null)
                sequenceRenderer = new SequenceRenderer(SelectedSequence, Drawing, surface);
            else
                sequenceRenderer = null;
        }

        private void LoadImage(Imgd image)
        {
            surface?.Dispose();
            surface = Drawing.CreateSurface(image);

            TrySetSequence();
        }
    }
}
