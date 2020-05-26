using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Game.Shaders;
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

        public struct MyVertex : IVertexType
        {
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
                new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            );

            public xnaf.Vector3 Position;
            public xnaf.Vector4 Color;
            public xnaf.Vector2 TextureCoordinate;

            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

            public MyVertex(xnaf.Vector3 position, xnaf.Vector4 color, xnaf.Vector2 textureCoordinate)
            {
                Position = position;
                Color = color;
                TextureCoordinate = textureCoordinate;
            }
        }

        private const int MaxSpriteCountPerDraw = 8000; // This size should be enough to pack enough 2D graphics at once
        private readonly KingdomShader _shader;

        private readonly BlendState _blendState;
        private readonly SamplerState _samplerState;
        private readonly RasterizerState _rasterizerState;
        private readonly DepthStencilState _depthStencilState;
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;
        private readonly MyVertex[] _vertices;
        private Texture2D _texture;
        private int _currentSpriteIndex;

        public xnaf.Matrix WorldViewProjection
        {
            get => _shader.WorldViewProjection;
            set => _shader.WorldViewProjection = value;
        }
        public Texture2D Texture0
        {
            get => _shader.Texture0;
            set => _shader.Texture0 = value;
        }

        public MonoDrawing(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            GraphicsDevice = graphicsDevice;
            _blendState = BlendState.NonPremultiplied;
            _samplerState = new SamplerState
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = TextureFilter.Linear
            };
            _rasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                ScissorTestEnable = false,
                DepthClipEnable = false,
            };
            _depthStencilState = new DepthStencilState()
            {
                DepthBufferEnable = false
            };

            _shader = new KingdomShader(contentManager);

            _vertexBuffer = new VertexBuffer(graphicsDevice, MyVertex.VertexDeclaration, MaxSpriteCountPerDraw * 4, BufferUsage.WriteOnly);
            _indexBuffer = CreateIndexBufferForSprites(graphicsDevice, MaxSpriteCountPerDraw);
            _vertices = new MyVertex[MaxSpriteCountPerDraw * 4];
            _currentSpriteIndex = 0;
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
            _indexBuffer?.Dispose();
            _shader?.Dispose();
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

            var vertexIndex = PrepareVertices(texture);
            _vertices[vertexIndex + 0] = new MyVertex(new xnaf.Vector3(dst.Left, dst.Top, 0.0f), ToXnaColor(color0), new xnaf.Vector2(src.Left * tw, src.Top * th));
            _vertices[vertexIndex + 1] = new MyVertex(new xnaf.Vector3(dst.Right, dst.Top, 0.0f), ToXnaColor(color1), new xnaf.Vector2(src.Right * tw, src.Top * th));
            _vertices[vertexIndex + 2] = new MyVertex(new xnaf.Vector3(dst.Left, dst.Bottom, 0.0f), ToXnaColor(color2), new xnaf.Vector2(src.Left * tw, src.Bottom * th));
            _vertices[vertexIndex + 3] = new MyVertex(new xnaf.Vector3(dst.Right, dst.Bottom, 0.0f), ToXnaColor(color3), new xnaf.Vector2(src.Right * tw, src.Bottom * th));
            PushVertices();
        }

        public void Flush()
        {
            if (_currentSpriteIndex <= 0)
                return;
            
            GraphicsDevice.SamplerStates[0] = _samplerState;
            GraphicsDevice.RasterizerState = _rasterizerState;
            GraphicsDevice.BlendState = _blendState;
            GraphicsDevice.DepthStencilState = _depthStencilState;
            GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            GraphicsDevice.Indices = _indexBuffer;

            _vertexBuffer.SetData(_vertices);
            _shader.Pass(pass =>
            {
                Texture0 = _texture;
                WorldViewProjection = xnaf.Matrix.CreateOrthographicOffCenter(0, 512, 416, 0, -1000.0f, +1000.0f);
                pass.Apply();

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _currentSpriteIndex * 2);
            });

            _currentSpriteIndex = 0;
        }
        
        private int PrepareVertices(Texture2D texture)
        {
            if (_texture != texture)
            {
                Flush();
                _texture = texture;
            }

            if (_currentSpriteIndex >= MaxSpriteCountPerDraw)
                Flush();

            return _currentSpriteIndex++ * 4;
        }

        private void PushVertices()
        {
            // right now it does not do anything.
        }

        private static IndexBuffer CreateIndexBufferForSprites(GraphicsDevice graphicsDevice, int spriteCount)
        {
            var indexBuffer =  new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, spriteCount * 6, BufferUsage.WriteOnly);
            var indices = new int[spriteCount * 6];
            for (
                int spriteIndex = 0, vertexIndex = 0;
                spriteIndex < spriteCount;
                spriteIndex++, vertexIndex += 4)
            {
                var i = spriteIndex * 6;
                indices[i + 0] = vertexIndex + 0;
                indices[i + 1] = vertexIndex + 1;
                indices[i + 2] = vertexIndex + 2;
                indices[i + 3] = vertexIndex + 1;
                indices[i + 4] = vertexIndex + 2;
                indices[i + 5] = vertexIndex + 3;
            }

            indexBuffer.SetData(indices);

            return indexBuffer;
        }

        private static xnaf.Rectangle ToXnaRectangle(Rectangle rectangle) =>
            new xnaf.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        private static xnaf.Rectangle ToXnaRectangle(RectangleF rectangle) =>
            new xnaf.Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
        private static xnaf.Color ToXnaColor(Color color) =>
            new xnaf.Color(color.R, color.G, color.B, color.A);
        private static xnaf.Vector4 ToXnaColor(ColorF color) =>
            new xnaf.Vector4(color.R, color.G, color.B, color.A);
    }
}
