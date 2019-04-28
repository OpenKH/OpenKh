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
                .Select(x => LoadImage(x))
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

        private ISurface LoadImage(Imgd image) => Drawing
            .CreateSurface(image.Size.Width,
                image.Size.Height,
                Xe.Drawing.PixelFormat.Format32bppArgb,
                SurfaceType.Input,
                GetDataResource(image));

        private static DataResource GetDataResource(IImageRead image)
        {
            byte[] data;
            switch (image.PixelFormat)
            {
                case Imaging.PixelFormat.Indexed4:
                    data = GetDataResource4bpp(image);
                    break;
                case Imaging.PixelFormat.Indexed8:
                    data = GetDataResource8bpp(image);
                    break;
                default:
                    throw new ArgumentException($"The pixel format {image.PixelFormat} is not supported.");
            }

            return new DataResource
            {
                Data = data,
                Stride = image.Size.Width * 4
            };
        }

        private unsafe static byte[] GetDataResource4bpp(IImageRead image)
        {
            var size = image.Size;
            var data = image.GetData();
            var clut = image.GetClut();
            var dstData = new byte[size.Width * size.Height * sizeof(uint)];
            var srcIndex = 0;
            var dstIndex = 0;

            for (var y = 0; y < size.Height; y++)
            {
                for (var i = 0; i < size.Width / 2; i++)
                {
                    var ch = data[srcIndex++];
                    var palIndex1 = (ch & 15);
                    var palIndex2 = (ch >> 4);
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 2];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 1];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 0];
                    dstData[dstIndex++] = clut[palIndex1 * 4 + 3];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 2];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 1];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 0];
                    dstData[dstIndex++] = clut[palIndex2 * 4 + 3];
                }
            }

            return dstData;
        }

        private unsafe static byte[] GetDataResource8bpp(IImageRead image)
        {
            var size = image.Size;
            var data = image.GetData();
            var clut = image.GetClut();
            var dstData = new byte[size.Width * size.Height * sizeof(uint)];
            var srcIndex = 0;
            var dstIndex = 0;

            for (var y = 0; y < size.Height; y++)
            {
                for (var i = 0; i < size.Width; i++)
                {
                    var palIndex = data[srcIndex++];
                    dstData[dstIndex++] = clut[palIndex * 4 + 2];
                    dstData[dstIndex++] = clut[palIndex * 4 + 1];
                    dstData[dstIndex++] = clut[palIndex * 4 + 0];
                    dstData[dstIndex++] = clut[palIndex * 4 + 3];
                }
            }

            return dstData;
        }
    }
}
