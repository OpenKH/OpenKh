using OpenKh.Engine.Parsers;

namespace OpenKh.Engine.MonoGame
{
    public class MeshDesc
    {
        public PositionColoredTextured[] Vertices;
        public int[] Indices;
        public int TextureIndex;
        public bool IsOpaque;
    }
}
