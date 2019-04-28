using kh.Imaging;
using kh.kh2;
using kh.tools.common;
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

        private Sequence sequence;
        private ISurface[] surfaces;

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
                if (surfaces?.Any() ?? false)
                {
                    x.DrawSurface(surfaces[0], 0, 0);
                }
                x.Flush();
            });
            DrawEndCommand = new RelayCommand<IDrawing>(x =>
            {
            });
        }

        public void PlaySequence(Sequence sequence, IEnumerable<Imgd> images)
        {
            DisposeAllSurfaces();

            this.sequence = sequence;
            surfaces = images
                .Select(x => Drawing.CreateSurface(x))
                .ToArray();
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
