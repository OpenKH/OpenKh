using OpenKh.Engine.Renderers;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using System.Drawing;
using System.Windows;
using static OpenKh.Tools.Common.DependencyPropertyUtils;
using OpenKh.Engine.Renders;

namespace OpenKh.Tools.Common.Controls
{
    public class SequenceRendererPanel : DrawPanel
    {
        public static readonly DependencyProperty BackgroundProperty =
            GetDependencyProperty<SequenceRendererPanel, System.Windows.Media.Color>(nameof(Background), System.Windows.Media.Colors.Magenta, (o, x) => o.SetBackgroundColor(x));

        public static readonly DependencyProperty SelectedSequenceProperty =
            GetDependencyProperty<SequenceRendererPanel, Sequence>(nameof(SelectedSequence), null, (o, x) => o.TrySetSequence(), x => true);

        public static readonly DependencyProperty SelectedImagesProperty =
            GetDependencyProperty<SequenceRendererPanel, Imgd>(nameof(SelectedImage), null, (o, x) => o.LoadImage(x), x => true);

        public static readonly DependencyProperty SelectedSequenceGroupIndexProperty =
            GetDependencyProperty<SequenceRendererPanel, int>(nameof(SelectedAnimationGroupIndex), 0, (o, x) => o.SelectSequenceGroup(x), x => x >= 0);

        public static readonly DependencyProperty FrameIndexProperty =
            GetDependencyProperty<SequenceRendererPanel, int>(nameof(FrameIndex), 0, (o, x) => o.SetFrameIndex(x), x => x >= 0);

        public static readonly DependencyProperty AdjustPositionProperty =
            GetDependencyProperty<SequenceRendererPanel, bool>(nameof(AdjustPosition), false, (o, x) => { });

        private ColorF _backgroundColor;
        private Rectangle _sequenceVisibilyRectangle;

        public System.Windows.Media.Color Background
        {
            get => (System.Windows.Media.Color)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public int SelectedAnimationGroupIndex
        {
            get => (int)GetValue(SelectedSequenceGroupIndexProperty);
            set
            {
                SetValue(SelectedSequenceGroupIndexProperty, value);
                InvalidateMeasure();
            }
        }

        public Sequence SelectedSequence
        {
            get => (Sequence)GetValue(SelectedSequenceProperty);
            set
            {
                SetValue(SelectedSequenceProperty, value);
                InvalidateMeasure();
            }
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

        public bool AdjustPosition
        {
            get => (bool)GetValue(AdjustPositionProperty);
            set => SetValue(AdjustPositionProperty, value);
        }

        private ISpriteTexture surface;
        private SequenceRenderer sequenceRenderer;

        public SequenceRendererPanel()
        {
            SetBackgroundColor(Background);
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            var rect = SelectedSequence?.GetVisibilityRectangleFromAnimationGroup(SelectedSequence.AnimationGroups[SelectedAnimationGroupIndex]);
            if (rect == null)
                return base.MeasureOverride(availableSize);

            _sequenceVisibilyRectangle = rect.Value;
            var size = _sequenceVisibilyRectangle.Size;
            return new System.Windows.Size(size.Width, size.Height);
        }

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
            Drawing.Clear(_backgroundColor);

            var posX = AdjustPosition ? -_sequenceVisibilyRectangle.X : (float)(ActualWidth / 2);
            var posY = AdjustPosition ? -_sequenceVisibilyRectangle.Y : (float)(ActualHeight / 2);
            sequenceRenderer?.Draw(SelectedAnimationGroupIndex, FrameIndex, posX, posY);
            FrameIndex++;
            Drawing.Flush();
            base.OnDrawBegin();
        }

        protected override void OnDrawEnd()
        {
            base.OnDrawEnd();
        }

        private void SetBackgroundColor(System.Windows.Media.Color color) =>
            _backgroundColor = ColorF.FromRgba(color.R, color.G, color.B, color.A);

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
            surface = Drawing?.CreateSpriteTexture(image);

            TrySetSequence();
        }
    }
}
