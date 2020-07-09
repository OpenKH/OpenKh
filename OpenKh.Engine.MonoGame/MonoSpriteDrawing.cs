using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Imaging;
using System.Linq;

namespace OpenKh.Engine.MonoGame
{
    public class MonoSpriteDrawing : ISpriteDrawing
    {
        public class CSpriteTexture : ISpriteTexture
        {
            public CSpriteTexture(RenderTarget2D texture)
            {
                Texture = texture;
            }

            public RenderTarget2D Texture { get; }

            public int Width => Texture.Width;

            public int Height => Texture.Height;

            public void Dispose()
            {
                Texture.Dispose();
            }

            public IMappedResource Map()
            {
                throw new System.NotImplementedException();
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
        private static readonly byte[] WhiteBitmap = Enumerable.Range(0, 2 * 2 * sizeof(int)).Select(x => byte.MaxValue).ToArray();
        private static readonly BlendState BlendStateDefault = new BlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            ColorBlendFunction = BlendFunction.Add,
            AlphaBlendFunction = BlendFunction.Add,
            BlendFactor = Color.White,
            MultiSampleMask = int.MaxValue,
            IndependentBlendEnable = false
        };
        private static readonly BlendState BlendStateAdditive = new BlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,
            AlphaBlendFunction = BlendFunction.Add,
            BlendFactor = Color.White,
            MultiSampleMask = int.MaxValue,
            IndependentBlendEnable = false
        };
        private static readonly BlendState BlendStateDifference = new BlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaBlendFunction = BlendFunction.ReverseSubtract,
            BlendFactor = Color.White,
            MultiSampleMask = int.MaxValue,
            IndependentBlendEnable = false,
        };

        private readonly GraphicsDevice _graphicsDevice;
        private readonly KingdomShader _shader;

        private readonly Texture2D _defaultTexture;
        private readonly SamplerState _samplerState;
        private readonly RasterizerState _rasterizerState;
        private readonly DepthStencilState _depthStencilState;
        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;
        private readonly MyVertex[] _vertices;
        private int _currentSpriteIndex;

        private Texture2D _lastTextureUsed;
        private BlendState _lastBlendState;
        private Matrix _projectionView;

        private Vector2 _textureRegionU;
        private Vector2 _textureRegionV;
        private TextureWrapMode _textureWrapU;
        private TextureWrapMode _textureWrapV;
        private ISpriteTexture destinationTexture;

        public MonoSpriteDrawing(GraphicsDevice graphicsDevice, KingdomShader shader)
        {
            _graphicsDevice = graphicsDevice;
            _shader = shader;

            _defaultTexture = new Texture2D(_graphicsDevice, 2, 2);
            _defaultTexture.SetData(WhiteBitmap);

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

        public ISpriteTexture DestinationTexture
        {
            get => destinationTexture;
            set
            {
                destinationTexture = value;
                if (value != null)
                {
                    var myTextureWrapper = value as CSpriteTexture;
                    _graphicsDevice.SetRenderTarget(myTextureWrapper.Texture);
                }
                else
                    _graphicsDevice.SetRenderTarget(null);
            }
        }

        public void Dispose()
        {
            _defaultTexture?.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
        }

        public ISpriteTexture CreateSpriteTexture(IImageRead image)
        {
            var size = image.Size;
            var texture = new RenderTarget2D(_graphicsDevice, size.Width, size.Height);
            texture.SetData(image.AsRgba8888());

            return new CSpriteTexture(texture);
        }

        public ISpriteTexture CreateSpriteTexture(int width, int height) =>
            new CSpriteTexture(new RenderTarget2D(_graphicsDevice, width, height));

        public void SetViewport(float left, float right, float top, float bottom)
        {
            _projectionView = Matrix.CreateOrthographicOffCenter(left, right, bottom, top, -1000.0f, +1000.0f);
        }

        public void Clear(ColorF color) =>
            _graphicsDevice.Clear(new Color(color.R, color.G, color.B, color.A));

        public void AppendSprite(SpriteDrawingContext context)
        {
            switch (context.BlendMode)
            {
                case BlendMode.Default:
                    SetBlendState(BlendStateDefault);
                    break;
                case BlendMode.Add:
                    SetBlendState(BlendStateAdditive);
                    break;
                case BlendMode.Subtract:
                    SetBlendState(BlendStateDifference);
                    break;
                default:
                    SetBlendState(BlendStateDefault);
                    break;
            }

            var texture = (context.SpriteTexture as CSpriteTexture)?.Texture;
            var tw = 1f / texture?.Width ?? 1f;
            var th = 1f / texture?.Height ?? 1f;

            var textureRegionU = new Vector2(context.TextureRegionLeft * tw, context.TextureRegionRight * tw);
            var textureRegionV = new Vector2(context.TextureRegionTop * th, context.TextureRegionBottom * th);
            if (context.TextureWrapU == TextureWrapMode.Default)
                textureRegionU = new Vector2(0, 1);
            if (context.TextureWrapV == TextureWrapMode.Default)
                textureRegionV = new Vector2(0, 1);

            if (_textureRegionU != textureRegionU ||
                _textureRegionV != textureRegionV ||
                _textureWrapU != context.TextureWrapU ||
                _textureWrapV != context.TextureWrapV)
                Flush();

            var vertexIndex = PrepareVertices(texture);

            _textureRegionU = textureRegionU;
            _textureRegionV = textureRegionV;
            _textureWrapU = context.TextureWrapU;
            _textureWrapV = context.TextureWrapV;

            _vertices[vertexIndex + 0].Position = new Vector3(context.Vec0.X, context.Vec0.Y, 0.0f);
            _vertices[vertexIndex + 0].Color = context.Color0;
            _vertices[vertexIndex + 0].TextureCoordinate = new Vector2(context.SourceLeft * tw + context.TextureHorizontalShift, context.SourceTop * th + context.TextureVerticalShift);

            _vertices[vertexIndex + 1].Position = new Vector3(context.Vec1.X, context.Vec1.Y, 0.0f);
            _vertices[vertexIndex + 1].Color = context.Color1;
            _vertices[vertexIndex + 1].TextureCoordinate = new Vector2(context.SourceRight * tw + context.TextureHorizontalShift, context.SourceTop * th + context.TextureVerticalShift);

            _vertices[vertexIndex + 2].Position = new Vector3(context.Vec2.X, context.Vec2.Y, 0.0f);
            _vertices[vertexIndex + 2].Color = context.Color2;
            _vertices[vertexIndex + 2].TextureCoordinate = new Vector2(context.SourceLeft * tw + context.TextureHorizontalShift, context.SourceBottom * th + context.TextureVerticalShift);

            _vertices[vertexIndex + 3].Position = new Vector3(context.Vec3.X, context.Vec3.Y, 0.0f);
            _vertices[vertexIndex + 3].Color = context.Color3;
            _vertices[vertexIndex + 3].TextureCoordinate = new Vector2(context.SourceRight * tw + context.TextureHorizontalShift, context.SourceBottom * th + context.TextureVerticalShift);

            PushVertices();
        }

        public void Flush()
        {
            if (_currentSpriteIndex <= 0)
                return;

            _graphicsDevice.SamplerStates[0] = _samplerState;
            _graphicsDevice.RasterizerState = _rasterizerState;
            _graphicsDevice.BlendState = _lastBlendState;
            _graphicsDevice.DepthStencilState = _depthStencilState;
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);
            _graphicsDevice.Indices = _indexBuffer;

            _vertexBuffer.SetData(_vertices, 0, 4 * _currentSpriteIndex);
            _shader.Pass(pass =>
            {
                _shader.Texture0 = _lastTextureUsed;
                _shader.ProjectionView = _projectionView;
                _shader.WorldView = Matrix.Identity;
                _shader.ModelView = Matrix.Identity;
                _shader.TextureRegionU = _textureRegionU;
                _shader.TextureRegionV = _textureRegionV;
                _shader.TextureWrapModeU = _textureWrapU;
                _shader.TextureWrapModeV = _textureWrapV;
                pass.Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _currentSpriteIndex * 2);
            });

            _currentSpriteIndex = 0;
        }

        private int PrepareVertices(Texture2D texture)
        {
            texture ??= _defaultTexture;
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

        private void SetBlendState(BlendState blendState)
        {
            if (_lastBlendState != blendState)
            {
                Flush();
                _lastBlendState = blendState;
            }
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
