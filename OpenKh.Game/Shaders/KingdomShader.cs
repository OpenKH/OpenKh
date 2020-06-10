using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Renders;
using System;

namespace OpenKh.Game.Shaders
{
    public class KingdomShader : IDisposable
    {
        public static readonly Vector2 DefaultTextureRegion = new Vector2(0, 1);

        private readonly EffectParameter _modelViewParameter;
        private readonly EffectParameter _worldViewParameter;
        private readonly EffectParameter _projectionViewParameter;
        private readonly EffectParameter _parameterTextureRegionU;
        private readonly EffectParameter _parameterTextureRegionV;
        private readonly EffectParameter _parameterTextureWrapModeU;
        private readonly EffectParameter _parameterTextureWrapModeV;
        private readonly EffectParameter _parameterTexture0;

        public KingdomShader(ContentManager contentManager)
        {
            Effect = contentManager.Load<Effect>("KingdomShader");
            _modelViewParameter = Effect.Parameters["ModelView"];
            _worldViewParameter = Effect.Parameters["WorldView"];
            _projectionViewParameter = Effect.Parameters["ProjectionView"];
            _parameterTextureRegionU = Effect.Parameters["TextureRegionU"];
            _parameterTextureRegionV = Effect.Parameters["TextureRegionV"];
            _parameterTextureWrapModeU = Effect.Parameters["TextureWrapModeU"];
            _parameterTextureWrapModeV = Effect.Parameters["TextureWrapModeV"];
            _parameterTexture0 = Effect.Parameters["Texture0"];

            ModelView = Matrix.Identity;
            WorldView = Matrix.Identity;
            ProjectionView = Matrix.Identity;
            TextureRegionU = DefaultTextureRegion;
            TextureRegionV = DefaultTextureRegion;
            TextureWrapModeU = TextureWrapMode.Clamp;
            TextureWrapModeV = TextureWrapMode.Clamp;
        }

        public Effect Effect { get; }

        public Matrix ModelView
        {
            get => _modelViewParameter.GetValueMatrix();
            set => _modelViewParameter.SetValue(value);
        }

        public Matrix WorldView
        {
            get => _worldViewParameter.GetValueMatrix();
            set => _worldViewParameter.SetValue(value);
        }

        public Matrix ProjectionView
        {
            get => _projectionViewParameter.GetValueMatrix();
            set => _projectionViewParameter.SetValue(value);
        }

        public Texture2D Texture0
        {
            get => _parameterTexture0.GetValueTexture2D();
            set => _parameterTexture0.SetValue(value);
        }

        public Vector2 TextureRegionU
        {
            get => _parameterTextureRegionU.GetValueVector2();
            set => _parameterTextureRegionU.SetValue(value);
        }
        public Vector2 TextureRegionV
        {
            get => _parameterTextureRegionV.GetValueVector2();
            set => _parameterTextureRegionV.SetValue(value);
        }

        public TextureWrapMode TextureWrapModeU
        {
            get => (TextureWrapMode)_parameterTextureWrapModeU.GetValueInt32();
            set => _parameterTextureWrapModeU.SetValue((int)value);
        }

        public TextureWrapMode TextureWrapModeV
        {
            get => (TextureWrapMode)_parameterTextureWrapModeV.GetValueInt32();
            set => _parameterTextureWrapModeV.SetValue((int)value);
        }

        public void Pass(Action<EffectPass> action)
        {
            foreach (var pass in Effect.CurrentTechnique.Passes)
                action(pass);
        }

        public void Dispose()
        {
            Effect?.Dispose();
        }
    }
}
