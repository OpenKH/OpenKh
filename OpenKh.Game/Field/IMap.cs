using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using System;

namespace OpenKh.Game.Field
{
    public interface IMap : IDisposable
    {
        void Render(Camera camera, KingdomShader shader, EffectPass pass, bool passRenderOpaque);
    }
}
