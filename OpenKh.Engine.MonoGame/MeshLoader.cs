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
        public static IModelMotion FromKH2(Mdlx model) =>
            model != null ? new MdlxParser(model) : null;

        public static IEnumerable<KingdomTexture> LoadTextures(
            this ModelTexture texture, GraphicsDevice graphics) => texture?.Images?
                .Select(texture => new KingdomTexture(texture, graphics)).ToArray() ??
                new KingdomTexture[0];

        public static IEnumerable<MeshDesc> ToMeshDescs(this List<MeshDescriptor> meshDescriptors) =>
            meshDescriptors.Select(x => new MeshDesc
            {
                Vertices = x.Vertices
                    .Select(v => new VertexPositionColorTexture(
                        new Vector3(v.X, v.Y, v.Z),
                        new Color((v.Color >> 16) & 0xff, (v.Color >> 8) & 0xff, v.Color & 0xff, (v.Color >> 24) & 0xff),
                        new Vector2(v.Tu, v.Tv)))
                    .ToArray(),
                Indices = x.Indices,
                TextureIndex = x.TextureIndex,
                IsOpaque = x.IsOpaque
            });
    }
}
