using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenKh.Game.Shaders
{
    public class KingdomShader : IDisposable
    {
        private readonly EffectParameter _parameterWorldViewProjection;
        private readonly EffectParameter _parameterTexture0;

        public KingdomShader(ContentManager contentManager)
        {
            Effect = contentManager.Load<Effect>("KingdomShader");
            _parameterTexture0 = Effect.Parameters["Texture0"];
            _parameterWorldViewProjection = Effect.Parameters["WorldViewProjection"];
        }

        public Effect Effect { get; }

        public Matrix WorldViewProjection
        {
            get => _parameterWorldViewProjection.GetValueMatrix();
            set => _parameterWorldViewProjection.SetValue(value);
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
