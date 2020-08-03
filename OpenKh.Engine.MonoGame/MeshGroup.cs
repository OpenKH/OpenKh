using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace OpenKh.Engine.MonoGame
{
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
}
