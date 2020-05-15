using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using Xe.Drawing;

namespace OpenKh.Game
{
    using xnaf = Microsoft.Xna.Framework;

    public class MonoDrawing : IDrawing
    {
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

        private readonly BasicEffect _effect;

        public MonoDrawing(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            _effect = new BasicEffect(graphicsDevice);
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
            var tw = 1.0f / texture.Width;
            var th = 1.0f / texture.Height;

            var indices = new int[] { 0, 1, 2, 1, 2, 3 };
            var vertices = new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture()
                {
                    Position = new xnaf.Vector3(dst.Left, dst.Top, 0.0f),
                    TextureCoordinate = new xnaf.Vector2(src.Left * tw, src.Top * th),
                    Color = ToXnaColor(color0)
                },
                new VertexPositionColorTexture()
                {
                    Position = new xnaf.Vector3(dst.Right, dst.Top, 0.0f),
                    TextureCoordinate = new xnaf.Vector2(src.Right * tw, src.Top * th),
                    Color = ToXnaColor(color1)
                },
                new VertexPositionColorTexture()
                {
                    Position = new xnaf.Vector3(dst.Left, dst.Bottom, 0.0f),
                    TextureCoordinate = new xnaf.Vector2(src.Left * tw, src.Bottom * th),
                    Color = ToXnaColor(color2)
                },
                new VertexPositionColorTexture()
                {
                    Position = new xnaf.Vector3(dst.Right, dst.Bottom, 0.0f),
                    TextureCoordinate = new xnaf.Vector2(src.Right * tw, src.Bottom * th),
                    Color = ToXnaColor(color3)
                },
            };

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                _effect.Texture = texture;
                _effect.TextureEnabled = true;
                _effect.VertexColorEnabled = true;
                _effect.Projection = xnaf.Matrix.CreateOrthographicOffCenter(0, 512, 416, 0, -1000.0f, +1000.0f);
                pass.Apply();

                GraphicsDevice.RasterizerState = new RasterizerState()
                {
                    CullMode = CullMode.None,
                };

                GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertices,
                    0,
                    vertices.Length,
                    indices,
                    0,
                    indices.Length / 3);
            }
        }

        public void Flush()
        {
        }

        private static xnaf.Rectangle ToXnaRectangle(Rectangle rectangle) =>
            new xnaf.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        private static xnaf.Rectangle ToXnaRectangle(RectangleF rectangle) =>
            new xnaf.Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
        private static xnaf.Color ToXnaColor(Color color) =>
            new xnaf.Color(color.R, color.G, color.B, color.A);
        private static xnaf.Color ToXnaColor(ColorF color) =>
            new xnaf.Color(color.R, color.G, color.B, color.A);
    }
}
