using Microsoft.Xna.Framework.Graphics;

namespace OpenKh.Engine.MonoGame
{
    public class MeshDesc
    {
        public VertexPositionColorTexture[] Vertices;
        public int[] Indices;
        public int TextureIndex;
        public bool IsOpaque;
    }
}
