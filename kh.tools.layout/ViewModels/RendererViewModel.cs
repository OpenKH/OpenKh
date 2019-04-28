using kh.Imaging;
using kh.kh2;
using kh.tools.common;
using kh.tools.layout.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xe.Drawing;
using Xe.Tools.Wpf.Commands;

namespace kh.tools.layout.ViewModels
{
    public class RendererViewModel
    {
        public IDrawing Drawing { get; }
        public RelayCommand DrawCreateCommand { get; }
        public RelayCommand DrawDestroyCommand { get; }
        public RelayCommand DrawBeginCommand { get; }
        public RelayCommand DrawEndCommand { get; }

        private Layout layout;
        private ISurface[] surfaces;
        private LayoutRenderer layoutRenderer;

        public RendererViewModel()
        {
            Drawing = new DrawingDirect3D();
            DrawCreateCommand = new RelayCommand<IDrawing>(x =>
            {

            });
            DrawDestroyCommand = new RelayCommand<IDrawing>(x =>
            {
                DisposeAllSurfaces();
            });
            DrawBeginCommand = new RelayCommand<IDrawing>(x =>
            {
                x.Clear(Color.Magenta);
                layoutRenderer?.Draw();
                x.Flush();
            });
            DrawEndCommand = new RelayCommand<IDrawing>(x =>
            {
            });
        }

        public void SetLayout(Layout layout, IEnumerable<Imgd> images)
        {
            DisposeAllSurfaces();

            this.layout = layout;
            surfaces = images
                .Select(x => Drawing.CreateSurface(x))
                .ToArray();

            layoutRenderer = new LayoutRenderer(layout, Drawing, surfaces);
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
