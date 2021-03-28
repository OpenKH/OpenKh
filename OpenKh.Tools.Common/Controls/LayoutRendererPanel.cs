using OpenKh.Engine;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static OpenKh.Tools.Common.DependencyPropertyUtils;

namespace OpenKh.Tools.Common.Controls
{
    public class LayoutRendererPanel : DrawPanel
    {
        public static readonly DependencyProperty BackgroundProperty =
            GetDependencyProperty<LayoutRendererPanel, System.Windows.Media.Color>(nameof(Background), System.Windows.Media.Colors.Magenta, (o, x) => o.SetBackgroundColor(x));

        public static readonly DependencyProperty SelectedLayoutProperty =
            GetDependencyProperty<LayoutRendererPanel, Layout>(nameof(SelectedLayout), null, (o, x) => o.TrySetLayout(), x => true);

        public static readonly DependencyProperty SelectedImagesProperty =
            GetDependencyProperty<LayoutRendererPanel, IEnumerable<Imgd>>(nameof(SelectedImages), null, (o, x) => o.LoadImages(x), x => true);

        public static readonly DependencyProperty SelectedSequenceGroupIndexProperty =
            GetDependencyProperty<LayoutRendererPanel, int>(nameof(SelectedSequenceGroupIndex), 0, (o, x) => o.SelectSequenceGroup(x), x => x >= 0);

        public static readonly DependencyProperty FrameIndexProperty =
            GetDependencyProperty<LayoutRendererPanel, int>(nameof(FrameIndex), 0, (o, x) => o.SetFrameIndex(x), x => x >= 0);

        public static readonly DependencyProperty IsPlayingProperty =
            GetDependencyProperty<LayoutRendererPanel, bool>(nameof(IsPlaying), true, (o, x) => o.SetIsPlaying(x));

        public static readonly DependencyProperty DebugLayoutRendererProperty =
            GetDependencyProperty<LayoutRendererPanel, IDebugLayoutRenderer>(nameof(DebugLayoutRenderer), null, (o, x) => o.SetDebugLayoutRenderer(x));

        private ColorF _backgroundColor;
        private IDebugLayoutRenderer _debugLayoutRenderer;

        public System.Windows.Media.Color Background
        {
            get => (System.Windows.Media.Color)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public int SelectedSequenceGroupIndex
        {
            get => (int)GetValue(SelectedSequenceGroupIndexProperty);
            set => SetValue(SelectedSequenceGroupIndexProperty, value);
        }

        public Layout SelectedLayout
        {
            get => (Layout)GetValue(SelectedLayoutProperty);
            set => SetValue(SelectedLayoutProperty, value);
        }

        public IEnumerable<Imgd> SelectedImages
        {
            get => (IEnumerable<Imgd>)GetValue(SelectedImagesProperty);
            set => SetValue(SelectedImagesProperty, value);
        }

        public int FrameIndex
        {
            get => (int)GetValue(FrameIndexProperty);
            set => SetValue(FrameIndexProperty, value);
        }

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set => SetValue(IsPlayingProperty, value);
        }

        public IDebugLayoutRenderer DebugLayoutRenderer
        {
            get => (IDebugLayoutRenderer)GetValue(DebugLayoutRendererProperty);
            set => SetValue(DebugLayoutRendererProperty, value);
        }

        private ISpriteTexture[] surfaces;
        private LayoutRenderer layoutRenderer;

        public LayoutRendererPanel()
        {
            SetBackgroundColor(Background);
        }

        protected override void OnDrawCreate()
        {
            base.OnDrawCreate();
        }

        protected override void OnDrawDestroy()
        {
            DisposeAllSurfaces();
            base.OnDrawDestroy();
        }

        protected override void OnDrawBegin()
        {
            Drawing.Clear(_backgroundColor);
            layoutRenderer?.Draw();

            if (IsPlaying)
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
            layoutRenderer.SelectedSequenceGroupIndex = index;
            FrameIndex = 0;
        }

        private void SetFrameIndex(int frameIndex)
        {
            if (layoutRenderer != null)
                layoutRenderer.FrameIndex = frameIndex;
        }

        private void SetIsPlaying(bool isPlaying) { }

        private void SetDebugLayoutRenderer(IDebugLayoutRenderer debugLayoutRenderer)
        {
            _debugLayoutRenderer = debugLayoutRenderer;
            layoutRenderer?.SetDebugLayoutRenderer(debugLayoutRenderer);
        }

        private void TrySetLayout()
        {
            if (SelectedLayout != null && surfaces != null)
            {
                layoutRenderer = new LayoutRenderer(SelectedLayout, Drawing, surfaces);
                layoutRenderer.SetDebugLayoutRenderer(_debugLayoutRenderer);
            }
            else
                layoutRenderer = null;
        }

        private void SetBackgroundColor(System.Windows.Media.Color color) =>
            _backgroundColor = ColorF.FromRgba(color.R, color.G, color.B, color.A);

        private void LoadImages(IEnumerable<Imgd> images)
        {
            DisposeAllSurfaces();

            surfaces = images
                .Select(x => Drawing.CreateSpriteTexture(x))
                .ToArray();

            TrySetLayout();
        }

        private void DisposeAllSurfaces()
        {
            if (surfaces == null)
                return;

            foreach (var surface in surfaces)
                surface.Dispose();

            surfaces = null;
        }
    }
}
