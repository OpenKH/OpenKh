using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using Xe.Drawing;

namespace OpenKh.Game
{
    public class MonoDrawing : IDrawing
    {
        private readonly SpriteBatch spriteBatch;

        private class CSurface : ISurface
        {
            public CSurface(Texture2D texture)
            {
                Texture = texture;
            }

            public Texture2D Texture { get; }
            public int Width => Texture.Width;
            public int Height => Texture.Height;
            public Size Size => new Size(Width, Height);

            public PixelFormat PixelFormat => throw new System.NotImplementedException();


            public void Dispose() => Texture.Dispose();

            public IMappedResource Map()
            {
                throw new System.NotImplementedException();
            }

            public void Save(string filename)
            {
                throw new System.NotImplementedException();
            }
        }

        public MonoDrawing(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public GraphicsDevice GraphicsDevice { get; }

        public ISurface Surface { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public Filter Filter { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void Clear(Color color)
        {
            GraphicsDevice.Clear(ToXnaColor(color));
        }

        public ISurface CreateSurface(int width, int height, PixelFormat pixelFormat, SurfaceType type = SurfaceType.Input, DataResource dataResource = null)
        {
            var texture = new Texture2D(GraphicsDevice, width, height);
            var data = new byte[dataResource.Data.Length];
            for (int i = 0; i < data.Length; i+=4)
            {
                data[i + 2] = dataResource.Data[i + 0];
                data[i + 1] = dataResource.Data[i + 1];
                data[i + 0] = dataResource.Data[i + 2];
                data[i + 3] = dataResource.Data[i + 3];
            }

            texture.SetData(data);

            return new CSurface(texture);
        }

        public ISurface CreateSurface(string filename, Color[] filterColors = null)
        {
            throw new System.NotImplementedException();
        }
        
        public void Dispose()
        {
            spriteBatch.Dispose();
        }

        public void DrawRectangle(RectangleF rect, Color color, float width = 1)
        {
            throw new System.NotImplementedException();
        }

        public void FillRectangle(RectangleF rect, Color color)
        {
            throw new System.NotImplementedException();
        }

        public void DrawSurface(ISurface surface, Rectangle src, RectangleF dst, Flip flip = Flip.None)
        {
            throw new System.NotImplementedException();
        }

        public void DrawSurface(ISurface surface, Rectangle src, RectangleF dst, float alpha, Flip flip = Flip.None)
        {
            throw new System.NotImplementedException();
        }

        public void DrawSurface(ISurface surface, Rectangle src, RectangleF dst, ColorF color0, ColorF color1, ColorF color2, ColorF color3)
        {
            var texture = (surface as CSurface)?.Texture;
            spriteBatch.Begin();
            spriteBatch.Draw(texture,
                sourceRectangle: ToXnaRectangle(src),
                destinationRectangle: ToXnaRectangle(dst),
                color: ToXnaColor(color0));
            spriteBatch.End();
        }

        public void Flush()
        {
        }

        private static Microsoft.Xna.Framework.Rectangle ToXnaRectangle(Rectangle rectangle) =>
            new Microsoft.Xna.Framework.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        private static Microsoft.Xna.Framework.Rectangle ToXnaRectangle(RectangleF rectangle) =>
            new Microsoft.Xna.Framework.Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
        private static Microsoft.Xna.Framework.Color ToXnaColor(Color color) =>
            new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
        private static Microsoft.Xna.Framework.Color ToXnaColor(ColorF color) =>
            new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
    }
}
