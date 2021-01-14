using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Imaging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Engine.MonoGame
{
    public interface IKingdomTexture : IDisposable
    {
        Texture2D Texture2D { get; }
        ModelTexture.TextureWrapMode AddressU { get; }
        ModelTexture.TextureWrapMode AddressV { get; }
        Vector2 RegionU { get; }
        Vector2 RegionV { get; }
    }

    public class PngKingdomTexture : IKingdomTexture
    {
        private static readonly Vector2 DefaultRegion = new Vector2(0, 1);

        public PngKingdomTexture(string filePath, GraphicsDevice graphics)
        {
            Texture2D = File.OpenRead(filePath)
                .Using(x => Texture2D.FromStream(graphics, x));
        }

        public Texture2D Texture2D { get; }

        public ModelTexture.TextureWrapMode AddressU => ModelTexture.TextureWrapMode.Repeat;
        public ModelTexture.TextureWrapMode AddressV => ModelTexture.TextureWrapMode.Repeat;

        public Vector2 RegionU => DefaultRegion;
        public Vector2 RegionV => DefaultRegion;

        public void Dispose()
        {
            Texture2D?.Dispose();
        }
    }

    public class Tim2KingdomTexture : IKingdomTexture
    {
        private static readonly Vector2 DefaultRegion = new Vector2(0, 1);

        public Tim2KingdomTexture(byte[] texture, GraphicsDevice graphics)
        {
            Texture2D = Tm2.Read(new MemoryStream(texture)).First().CreateTexture(graphics);
        }

        public Texture2D Texture2D { get; }

        public ModelTexture.TextureWrapMode AddressU => ModelTexture.TextureWrapMode.Repeat;
        public ModelTexture.TextureWrapMode AddressV => ModelTexture.TextureWrapMode.Repeat;

        public Vector2 RegionU => DefaultRegion;
        public Vector2 RegionV => DefaultRegion;

        public void Dispose()
        {
            Texture2D?.Dispose();
        }
    }

    public class KingdomTexture : IKingdomTexture
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

        public ModelTexture.TextureWrapMode AddressU =>
            ModelTexture.TextureAddressMode.AddressU;

        public ModelTexture.TextureWrapMode AddressV =>
            ModelTexture.TextureAddressMode.AddressV;

        public void Dispose()
        {
            Texture2D?.Dispose();
        }
    }
}
