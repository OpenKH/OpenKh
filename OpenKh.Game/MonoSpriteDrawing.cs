using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Game.Shaders;
using OpenKh.Imaging;

namespace OpenKh.Game
{
    public class MonoSpriteDrawing : ISpriteDrawing
    {
        private class CSpriteTexture : ISpriteTexture
        {
            public CSpriteTexture(Texture2D texture)
            {
                Texture = texture;
            }

            public Texture2D Texture { get; }

            public int Width => Texture.Width;

            public int Height => Texture.Height;

            public void Dispose()
            {
                Texture.Dispose();
            }
        }

        private struct MyVertex : IVertexType
        {
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
                new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
            );

            public Vector3 Position;
            public ColorF Color;
            public Vector2 TextureCoordinate;

            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

            public MyVertex(Vector3 position, ColorF color, Vector2 textureCoordinate)
            {
                Position = position;
                Color = color;
                TextureCoordinate = textureCoordinate;
            }
        }

        // This size should be enough to pack enough 2D graphics at once
        private const int MaxSpriteCountPerDraw = 8000;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly KingdomShader _shader;

        private readonly BlendState _blendState;
        private readonly SamplerState _samplerState;
        private readonly RasterizerState _rasterizerState;
        private readonly DepthStencilState _depthStencilState;
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;
        private readonly MyVertex[] _vertices;
        private int _currentSpriteIndex;
        
        private Texture2D _lastTextureUsed;
        private Matrix _projectionView;

        public MonoSpriteDrawing(GraphicsDevice graphicsDevice, KingdomShader shader)
        {
            _graphicsDevice = graphicsDevice;
            _shader = shader;

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

            _vertexBuffer = new VertexBuffer(graphicsDevice, MyVertex.VertexDeclaration, MaxSpriteCountPerDraw * 4, BufferUsage.WriteOnly);
            _indexBuffer = CreateIndexBufferForSprites(graphicsDevice, MaxSpriteCountPerDraw);
            _vertices = new MyVertex[MaxSpriteCountPerDraw * 4];
            _currentSpriteIndex = 0;
        }

        public ISpriteTexture DestinationTexture { get; set; }

        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
        }

        public ISpriteTexture CreateSpriteTexture(IImageRead image)
        {
            var size = image.Size;
            var texture = new Texture2D(_graphicsDevice, size.Width, size.Height);
            texture.SetData(image.AsRgba8888());

            return new CSpriteTexture(texture);
        }

        public ISpriteTexture CreateSpriteTexture(int width, int height) =>
            new CSpriteTexture(new Texture2D(_graphicsDevice, width, height));

        public void SetViewport(float left, float right, float top, float bottom)
        {
            _projectionView = Matrix.CreateOrthographicOffCenter(left, right, bottom, top, -1000.0f, +1000.0f);
        }

        public void Clear(ColorF color) =>
            _graphicsDevice.Clear(new Color(color.R, color.G, color.B, color.A));

        public void AppendSprite(SpriteDrawingContext context)
        {
            var texture = (context.SpriteTexture as CSpriteTexture)?.Texture;
            var vertexIndex = PrepareVertices(texture);

            var tw = 1.0f / texture.Width;
            var th = 1.0f / texture.Height;

            _vertices[vertexIndex + 0].Position = new Vector3(context.DestinationX, context.DestinationY, 0.0f);
            _vertices[vertexIndex + 0].Color = context.Color0;
            _vertices[vertexIndex + 0].TextureCoordinate = new Vector2(context.SourceLeft * tw, context.SourceTop * th);

            _vertices[vertexIndex + 1].Position = new Vector3(context.DestinationX + context.DestinationWidth, context.DestinationY, 0.0f);
            _vertices[vertexIndex + 1].Color = context.Color1;
            _vertices[vertexIndex + 1].TextureCoordinate = new Vector2(context.SourceRight * tw, context.SourceTop * th);

            _vertices[vertexIndex + 2].Position = new Vector3(context.DestinationX, context.DestinationY + context.DestinationHeight, 0.0f);
            _vertices[vertexIndex + 2].Color = context.Color2;
            _vertices[vertexIndex + 2].TextureCoordinate = new Vector2(context.SourceLeft * tw, context.SourceBottom * th);

            _vertices[vertexIndex + 3].Position = new Vector3(context.DestinationX + context.DestinationWidth, context.DestinationY + context.DestinationHeight, 0.0f);
            _vertices[vertexIndex + 3].Color = context.Color3;
            _vertices[vertexIndex + 3].TextureCoordinate = new Vector2(context.SourceRight * tw, context.SourceBottom * th);

            PushVertices();
        }

        public void Flush()
        {
            if (_currentSpriteIndex <= 0)
                return;

            _graphicsDevice.SamplerStates[0] = _samplerState;
            _graphicsDevice.RasterizerState = _rasterizerState;
            _graphicsDevice.BlendState = _blendState;
            _graphicsDevice.DepthStencilState = _depthStencilState;
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            _vertexBuffer.SetData(_vertices);
            _shader.Pass(pass =>
            {
                _shader.Texture0 = _lastTextureUsed;
                _shader.ProjectionView = _projectionView;
                _shader.WorldView = Matrix.Identity;
                _shader.TextureRegionU = KingdomShader.DefaultTextureRegion;
                _shader.TextureRegionV = KingdomShader.DefaultTextureRegion;
                _shader.TextureWrapModeU = TextureWrapMode.Clamp;
                _shader.TextureWrapModeV = TextureWrapMode.Clamp;
                pass.Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _currentSpriteIndex * 2);
            });

            _currentSpriteIndex = 0;
        }

        private int PrepareVertices(Texture2D texture)
        {
            if (_lastTextureUsed != texture)
            {
                Flush();
                _lastTextureUsed = texture;
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
            var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, spriteCount * 6, BufferUsage.WriteOnly);
            var indices = new int[spriteCount * 6];
            for (int spriteIndex = 0, vertexIndex = 0;
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
    }
}
