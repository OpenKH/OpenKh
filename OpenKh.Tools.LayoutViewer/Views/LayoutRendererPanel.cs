using kh.tools.common.Controls;
using OpenKh.Kh2;
using OpenKh.Tools.LayoutViewer.Renderer;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using Xe.Drawing;
using static kh.tools.common.DependencyPropertyUtils;

namespace OpenKh.Tools.LayoutViewer.Views
{
    public class LayoutRendererPanel : DrawPanel
    {
        public static readonly DependencyProperty SelectedLayoutProperty =
            GetDependencyProperty<LayoutRendererPanel, Layout>("SelectedLayout", null, (o, x) => o.TrySetLayout(), x => true);

        public static readonly DependencyProperty SelectedImagesProperty =
            GetDependencyProperty<LayoutRendererPanel, IEnumerable<Imgd>>("SelectedImages", null, (o, x) => o.LoadImages(x), x => true);

        public static readonly DependencyProperty SelectedSequenceGroupIndexProperty =
            GetDependencyProperty<LayoutRendererPanel, int>("SelectedSequenceGroupIndex", 0, (o, x) => o.SelectSequenceGroup(x), x => x >= 0);

        public static readonly DependencyProperty FrameIndexProperty =
            GetDependencyProperty<LayoutRendererPanel, int>("FrameIndex", 0, (o, x) => o.SetFrameIndex(x), x => x >= 0);

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

        private ISurface[] surfaces;
        private LayoutRenderer layoutRenderer;

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
            Drawing.Clear(Color.Magenta);
            layoutRenderer?.Draw();
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

        private void TrySetLayout()
        {
            if (SelectedLayout != null && surfaces != null)
                layoutRenderer = new LayoutRenderer(SelectedLayout, Drawing, surfaces);
            else
                layoutRenderer = null;
        }

        private void LoadImages(IEnumerable<Imgd> images)
        {
            DisposeAllSurfaces();

            surfaces = images
                .Select(x => Drawing.CreateSurface(x))
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
