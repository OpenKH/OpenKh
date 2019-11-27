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
        public PositionColoredTextured[] Vertices { get; set; }
    }
}
