using OpenKh.Engine.Extensions;
using OpenKh.Imaging;
using Xe.Drawing;

namespace OpenKh.Engine
{
    public static partial class DrawingHelpers
    {
        public static ISurface CreateSurface(this IDrawing drawing, IImageRead image) => drawing
            .CreateSurface(image.Size.Width,
                image.Size.Height,
                Xe.Drawing.PixelFormat.Format32bppArgb,
                SurfaceType.Input,
                new DataResource
                {
                    Data = image.AsBgra8888(),
                    Stride = image.Size.Width * 4
                });
    }
}
