using System.Numerics;

namespace OpenKh.Kh2.Extensions
{
    public static class Vector4Extensions
    {
        public static Vector3 ToVector3(this Vector4 vector) => new Vector3(vector.X, vector.Y, vector.Z);
    }
}
