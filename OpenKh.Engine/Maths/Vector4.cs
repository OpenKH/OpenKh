using System;

namespace OpenKh.Engine.Maths
{
    public struct Vector4
    {
        public float X, Y, Z, W;

        public Vector4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public float Length()
        {
            var a = X * (double)X;
            var b = Y * (double)Y;
            var c = a + b;
            var d = Z * (double)Z;
            var e = c + d;
            var f = W * (double)W;
            return (float)Math.Sqrt(e + f);
        }

        public static Vector4 Transform(Vector4 vector, Matrix transformation)
        {
            return new Vector4()
            {
                X = (float)((double)transformation.M21 * vector.Y + transformation.M11 * vector.X + transformation.M31 * vector.Z + transformation.M41 * vector.W),
                Y = (float)((double)transformation.M22 * vector.Y + transformation.M12 * vector.X + transformation.M32 * vector.Z + transformation.M42 * vector.W),
                Z = (float)((double)transformation.M23 * vector.Y + transformation.M13 * vector.X + transformation.M33 * vector.Z + transformation.M43 * vector.W),
                W = (float)((double)transformation.M24 * vector.Y + transformation.M14 * vector.X + transformation.M34 * vector.Z + transformation.M44 * vector.W)
            };
        }
    }
}
