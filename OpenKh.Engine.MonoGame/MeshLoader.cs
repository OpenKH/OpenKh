using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Motion;
using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Engine.MonoGame
{
    public static class MeshLoader
    {
        public static VertexDeclaration PositionColoredTexturedVertexDeclaration =
            new VertexDeclaration(24, new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            });

        public static IModelMotion FromKH2(Mdlx model) =>
            model != null ? new MdlxParser(model) : null;

        public static IEnumerable<KingdomTexture> LoadTextures(
            this ModelTexture texture, GraphicsDevice graphics) => texture?.Images?
                .Select(texture => new KingdomTexture(texture, graphics)).ToArray() ??
                new KingdomTexture[0];
    }
}
