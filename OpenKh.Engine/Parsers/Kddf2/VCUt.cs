using OpenKh.Engine.Maths;

namespace OpenKh.Engine.Parsers.Kddf2
{
    class VCUt
    {
        public static Vector3 V4To3(Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}
