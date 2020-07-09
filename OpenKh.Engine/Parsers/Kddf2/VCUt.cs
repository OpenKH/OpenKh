using OpenKh.Engine.Maths;
using System.Numerics;

namespace OpenKh.Engine.Parsers.Kddf2
{
    class VCUt
    {
        public static Vector3 V4To3(Vector4 pos)
        {
            return new Vector3(pos.X, pos.Y, pos.Z);
        }
    }
}
