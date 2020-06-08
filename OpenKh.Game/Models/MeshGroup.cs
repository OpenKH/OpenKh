using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;

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

    public class MeshGroup
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
            public bool IsOpaque { get; set; }
        }

        public Segment[] Segments { get; set; }
        public Part[] Parts { get; set; }
        public KingdomTexture[] Textures { get; set; }
        public List<MeshDesc> MeshDescriptors { get; set; }
    }

    public class MeshDesc
    {
        public VertexPositionColorTexture[] Vertices;
        public int[] Indices;
        public int TextureIndex;
        public bool IsOpaque;
    }
}
