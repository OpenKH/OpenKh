using System.Runtime.InteropServices;

namespace OpenKh.Engine.Parsers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionColoredTextured
    {
        public float X, Y, Z;
        public float U, V;
        public int Color;
    }

    public class Model
    {
        public class Segment
        {
            public PositionColoredTextured[] Vertices { get; set; }
        }

        public class Part
        {
            public int[] Indices { get; set; }
            public int TextureId { get; set; }
            public int SegmentId { get; set; }
        }

        public Segment[] Segments { get; set; }
        public Part[] Parts { get; set; }
    }
}
