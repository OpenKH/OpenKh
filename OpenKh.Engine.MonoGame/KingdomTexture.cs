using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Kh2;
using System;

namespace OpenKh.Engine.MonoGame
{
    public class KingdomTexture : IDisposable
    {
        public KingdomTexture(ModelTexture.Texture texture, GraphicsDevice graphics)
        {
            ModelTexture = texture;
            Texture2D = texture.CreateTexture(graphics);
        }

        public ModelTexture.Texture ModelTexture { get; }
        public Texture2D Texture2D { get; }

        public Vector2 RegionU => new Vector2(
            (float)Math.Min(ModelTexture.TextureAddressMode.Left, ModelTexture.TextureAddressMode.Right) / Texture2D.Width,
            (float)Math.Max(ModelTexture.TextureAddressMode.Left, ModelTexture.TextureAddressMode.Right) / Texture2D.Width);

        public Vector2 RegionV => new Vector2(
            (float)Math.Min(ModelTexture.TextureAddressMode.Top, ModelTexture.TextureAddressMode.Bottom) / Texture2D.Height,
            (float)Math.Max(ModelTexture.TextureAddressMode.Top, ModelTexture.TextureAddressMode.Bottom) / Texture2D.Height);

        public void Dispose()
        {
            Texture2D?.Dispose();
        }
    }
}
