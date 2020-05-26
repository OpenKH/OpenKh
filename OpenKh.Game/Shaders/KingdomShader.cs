using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;

namespace OpenKh.Game.Shaders
{
    public class KingdomShader : IDisposable
    {
        public static readonly RectangleF DefaultTextureRegion = new RectangleF(0, 0, 1, 1);

        private readonly EffectParameter _worldViewParameter;
        private readonly EffectParameter _projectionViewParameter;
        private readonly EffectParameter _parameterTextureRegion;
        private readonly EffectParameter _parameterTexture0;

        public KingdomShader(ContentManager contentManager)
        {
            Effect = contentManager.Load<Effect>("KingdomShader");
            _worldViewParameter = Effect.Parameters["WorldView"];
            _projectionViewParameter = Effect.Parameters["ProjectionView"];
            _parameterTextureRegion = Effect.Parameters["TextureRegion"];
            _parameterTexture0 = Effect.Parameters["Texture0"];

            WorldView = Matrix.Identity;
            ProjectionView = Matrix.Identity;
            TextureRegion = DefaultTextureRegion;
        }

        public Effect Effect { get; }

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

        public RectangleF TextureRegion
        {
            get
            {
                var value = _parameterTextureRegion.GetValueVector4();
                return RectangleF.FromLTRB(value.X, value.Y, value.Z, value.W);
            }

            set => _parameterTextureRegion.SetValue(
                new Vector4(value.Left, value.Top, value.Right, value.Bottom));
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
