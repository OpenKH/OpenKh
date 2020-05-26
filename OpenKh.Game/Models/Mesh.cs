using Microsoft.Xna.Framework.Graphics;
using OpenKh.Kh2;
using System;
using System.Drawing;

namespace OpenKh.Game.Models
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
        public RectangleF Region => RectangleF.FromLTRB(
            (float)Math.Min(ModelTexture.TextureAddressMode.Left, ModelTexture.TextureAddressMode.Right) / Texture2D.Width,
            (float)Math.Min(ModelTexture.TextureAddressMode.Top, ModelTexture.TextureAddressMode.Bottom) / Texture2D.Height,
            (float)Math.Max(ModelTexture.TextureAddressMode.Left, ModelTexture.TextureAddressMode.Right) / Texture2D.Width,
            (float)Math.Max(ModelTexture.TextureAddressMode.Top, ModelTexture.TextureAddressMode.Bottom) / Texture2D.Height
            );

        public void Dispose()
        {
            Texture2D?.Dispose();
        }
    }

    public class Mesh
    {
        public class Segment
        {
            public VertexPositionColorTexture[] Vertices { get; set; }
        }

        public class Part
        {
            public int[] Indices { get; set; }
            public int TextureId { get; set; }
            public int SegmentId { get; set; }
        }

        public Segment[] Segments { get; set; }
        public Part[] Parts { get; set; }
        public KingdomTexture[] Textures { get; set; }
    }
}
