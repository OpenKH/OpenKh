using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenKh.Game.Shaders
{
    public class KingdomShader : IDisposable
    {
        private readonly EffectParameter _worldViewParameter;
        private readonly EffectParameter _projectionViewParameter;
        private readonly EffectParameter _parameterTexture0;

        public KingdomShader(ContentManager contentManager)
        {
            Effect = contentManager.Load<Effect>("KingdomShader");
            _worldViewParameter = Effect.Parameters["WorldView"];
            _projectionViewParameter = Effect.Parameters["ProjectionView"];
            _parameterTexture0 = Effect.Parameters["Texture0"];

            WorldView = Matrix.Identity;
            ProjectionView = Matrix.Identity;
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
