using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class CustomVertex
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PositionColoredTextured
        {
            public float X, Y, Z;
            public int Color;
            public float Tu, Tv;

            public PositionColoredTextured(Vector3 v, int clr, float tu, float tv)
            {
                X = v.X;
                Y = v.Y;
                Z = v.Z;
                Color = clr;
                Tu = tu;
                Tv = tv;
            }
        }
    }
}
